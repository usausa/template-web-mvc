namespace Template.WebApp.Host.Application;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.FeatureManagement;

using MiniDataProfiler;
using MiniDataProfiler.Listener.Logging;
using MiniDataProfiler.Listener.OpenTelemetry;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Serilog;

using Smart.AspNetCore;
using Smart.AspNetCore.ApplicationModels;
using Smart.Data;

using Template.WebApp.Accessors;
using Template.WebApp.Host.Application.Telemetry;
using Template.WebApp.Host.Infrastructure.ExceptionHandling;
using Template.WebApp.Host.Infrastructure.Filters;
using Template.WebApp.Host.Infrastructure.HealthChecks;
using Template.WebApp.Host.Infrastructure.Identity;
using Template.WebApp.Host.Infrastructure.Logging;
using Template.WebApp.Host.Infrastructure.Security;
using Template.WebApp.Infrastructure.Security;
using Template.WebApp.Infrastructure.Storage;

public static class ApplicationExtensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";
    private const string ApiPathPrefix = "/api";

    //--------------------------------------------------------------------------------
    // System
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureSystem(this WebApplicationBuilder builder)
    {
        // Path
        builder.Configuration.SetBasePath(AppContext.BaseDirectory);

        // Encoding
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Host
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureHost(this WebApplicationBuilder builder)
    {
        // Service
        builder.Services
            .AddWindowsService()
            .AddSystemd();

        // Feature management
        builder.Services.AddFeatureManagement();

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Logging
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureLogging(this IHostApplicationBuilder builder)
    {
        var setting = builder.Configuration.GetSection("Log").Get<LogSetting>()!;
        var useOtlpExporter = builder.Configuration.IsOtelExporterEnabled();

        // Application log
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog(
            (provider, options) =>
            {
                var accessor = provider.GetRequiredService<IHttpContextAccessor>();
                options.ReadFrom.Configuration(builder.Configuration);
                options.Enrich.With(new CallbackEnricher("RemoteIpAddress", () => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString()));
                options.Enrich.With(new CallbackEnricher("UserId", () => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)));
            },
            writeToProviders: useOtlpExporter);

        // HTTP log
        builder.Services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.RequestMethod |
                                    HttpLoggingFields.RequestPath |
                                    HttpLoggingFields.ResponseStatusCode |
                                    HttpLoggingFields.Duration;
            if (setting.HttpDump)
            {
                options.LoggingFields |= HttpLoggingFields.RequestBody | HttpLoggingFields.ResponseBody;
                options.CombineLogs = true;
                options.RequestBodyLogLimit = setting.HttpDumpLimit;
                options.ResponseBodyLogLimit = setting.HttpDumpLimit;
                options.MediaTypeOptions.Clear();
                options.MediaTypeOptions.AddText("application/json");
                options.MediaTypeOptions.AddText("application/*+json");
                options.MediaTypeOptions.AddText("application/xml");
                options.MediaTypeOptions.AddText("application/*+xml");
            }
        });

        // Access log (W3C)
        if (setting.W3CLog.Enable)
        {
            builder.Services.AddW3CLogging(options =>
            {
                options.LogDirectory = setting.W3CLog.Directory;
                options.FileName = setting.W3CLog.FileName;
                options.RetainedFileCountLimit = setting.W3CLog.RetainedFileCount;
            });
        }

        return builder;
    }

    public static WebApplication UseW3CLog(this WebApplication app)
    {
        var setting = app.Services.GetRequiredService<LogSetting>();
        if (setting.W3CLog.Enable)
        {
            app.UseW3CLogging();
        }

        return app;
    }

    public static WebApplication UseHttpLog(this WebApplication app)
    {
        var setting = app.Services.GetRequiredService<LogSetting>();
        if (setting.HttpLog)
        {
            app.UseWhen(
                static context => context.Request.Path.StartsWithSegments(ApiPathPrefix, StringComparison.OrdinalIgnoreCase),
                static b => b.UseHttpLogging());
        }

        return app;
    }

    //--------------------------------------------------------------------------------
    // Http
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureHttp(this IHostApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddHttpContextAccessor();

        // CSP nonce
        builder.Services.AddScoped<CspNonce>();

        // XForward
        builder.Services.Configure<ForwardedHeadersOptions>(static options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // Do not restrict to local network/proxy
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return builder;
    }

    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        // HSTS
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        // Headers
        app.UseMiddleware<SecurityHeadersMiddleware>();

        return app;
    }

    //--------------------------------------------------------------------------------
    // API
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureApi(this IHostApplicationBuilder builder)
    {
        // Error handler
        builder.Services.AddProblemDetails(static options =>
        {
            options.CustomizeProblemDetails = static context =>
            {
                context.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
            };
        });
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        return builder;
    }

    public static WebApplication UseErrorHandler(this WebApplication app)
    {
        // API: ProblemDetails
        app.UseWhen(
            static context => context.Request.Path.StartsWithSegments(ApiPathPrefix, StringComparison.OrdinalIgnoreCase),
            static b => b.UseExceptionHandler());

        // Page: error page
        app.UseWhen(
            static context => !context.Request.Path.StartsWithSegments(ApiPathPrefix, StringComparison.OrdinalIgnoreCase),
            static b =>
            {
                b.UseExceptionHandler("/error");
                b.UseStatusCodePagesWithReExecute("/error/{0}");
            });

        return app;
    }

    //--------------------------------------------------------------------------------
    // Authentication
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureAuthentication(this IHostApplicationBuilder builder)
    {
        var setting = builder.Configuration.GetSection("Auth").Get<AuthSetting>()!;
        var isDevelopment = builder.Environment.IsDevelopment();

        // Identity Core + 自前ストア(Smart.Data.Accessor)。EF Coreは使わない。
        // 認証の流れ(Cookie / パスキー / ロックアウト)はIdentityに任せ、利用者の保存だけをAccountStoreで行う
        builder.Services
            .AddAuthentication(static options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        builder.Services
            .AddIdentityCore<AccountEntity>(static options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                // 開発用初期パスワード(admin)を許容する緩和設定。運用時はポリシーに合わせて強化する
                options.Password.RequiredLength = 4;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddUserStore<AccountStore>()
            .AddClaimsPrincipalFactory<AccountClaimsPrincipalFactory>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // ハッシュは既存のIPasswordProviderへ委譲する(AddIdentityCoreの既定登録を上書き)
        builder.Services.AddScoped<IPasswordHasher<AccountEntity>, AccountPasswordHasher>();

        builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/error/403";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(setting.ExpireMinutes);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

                // API returns status code instead of redirect
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = static context =>
                    {
                        if (context.Request.Path.StartsWithSegments(ApiPathPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }

                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = static context =>
                    {
                        if (context.Request.Path.StartsWithSegments(ApiPathPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        // 認可。Auth:Enabled=false(既定)のときは[Authorize]を匿名でも通し、認証なしで使える状態にする
        builder.Services.AddAuthorization(options =>
        {
            if (setting.Enabled)
            {
                options.AddPolicy(Policies.Administrator, static policy => policy.RequireRole(Roles.Administrator));
            }
            else
            {
                var allowAll = new AuthorizationPolicyBuilder().RequireAssertion(static _ => true).Build();
                options.DefaultPolicy = allowAll;
                options.AddPolicy(Policies.Administrator, allowAll);
            }
        });

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Compress
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureCompression(this IHostApplicationBuilder builder)
    {
        builder.Services.AddResponseCompression(static options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        builder.Services.AddRequestDecompression();

        return builder;
    }

    public static WebApplication UseCompression(this WebApplication app)
    {
        var setting = app.Services.GetRequiredService<CompressionSetting>();
        if (setting.Response || setting.Request)
        {
            app.UseWhen(
                static context => context.Request.Path.StartsWithSegments(ApiPathPrefix, StringComparison.OrdinalIgnoreCase),
                b =>
                {
                    if (setting.Response)
                    {
                        b.UseResponseCompression();
                    }

                    if (setting.Request)
                    {
                        b.UseRequestDecompression();
                    }
                });
        }

        return app;
    }

    //--------------------------------------------------------------------------------
    // OpenApi
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureOpenApi(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi(static options =>
        {
            options.AddDocumentTransformer(static (document, _, _) =>
            {
                document.Info.Title = "Template API";
                document.Info.Version = "v1";
                document.Info.Description = "Template web application API.";
                return Task.CompletedTask;
            });
        });

        return builder;
    }

    //--------------------------------------------------------------------------------
    // MVC
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureMvc(this IHostApplicationBuilder builder)
    {
        // Filter
        builder.Services.AddTimeLogging(static options =>
        {
            options.Threshold = 10_000;
        });
        builder.Services.AddSingleton<RequestMetricsActionFilter>();

        // MVC
        builder.Services
            .AddControllersWithViews(static options =>
            {
                options.Filters.AddTimeLogging();
                options.Conventions.Add(new LowercaseControllerModelConvention());
                options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(static (_, y) => Messages.MakeInvalid(y));
            })
            .AddJsonOptions(static options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = NamingPolicy.JsonPropertyNaming;
                options.JsonSerializerOptions.DictionaryKeyPolicy = NamingPolicy.JsonDictionaryKeyNaming;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            });

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Health
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureHealth(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddHealthChecks()
            .AddCheck("self", static () => HealthCheckResult.Healthy(), ["live"])
            .AddCheck<DatabaseHealthCheck>("database");

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Telemetry
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureTelemetry(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = builder.Configuration.IsOtelExporterEnabled();

        var prometheusSection = builder.Configuration.GetSection("Prometheus");
        var prometheusUri = prometheusSection.GetValue<string>("Uri")!;
        var usePrometheusExporter = !String.IsNullOrEmpty(prometheusUri);

        var telemetry = builder.Services.AddOpenTelemetry()
            .ConfigureResource(config =>
            {
                config.AddService(
                    serviceName: builder.Environment.ApplicationName,
                    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString(),
                    serviceInstanceId: Environment.MachineName);
            });

        // Log
        if (useOtlpExporter)
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });
            builder.Services.Configure<OpenTelemetryLoggerOptions>(static logging =>
            {
                logging.AddOtlpExporter();
            });
        }

        // Metrics
        if (useOtlpExporter || usePrometheusExporter)
        {
            telemetry
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddRuntimeInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddApplicationInstrumentation();

                    if (useOtlpExporter)
                    {
                        metrics.AddOtlpExporter();
                    }

                    if (usePrometheusExporter)
                    {
                        var prometheusEndpoint = new Uri(prometheusUri);
                        metrics.AddPrometheusHttpListener(config =>
                        {
                            config.Host = prometheusEndpoint.Host;
                            config.Port = prometheusEndpoint.Port;
                        });
                    }
                });
        }

        // Trace
        if (useOtlpExporter)
        {
            telemetry
                .WithTracing(tracing =>
                {
                    tracing
                        .AddSource(builder.Environment.ApplicationName)
                        .AddAspNetCoreInstrumentation(static options =>
                        {
                            options.Filter = static context =>
                            {
                                var path = context.Request.Path;
                                return !path.StartsWithSegments(AlivenessEndpointPath, StringComparison.OrdinalIgnoreCase) &&
                                       !path.StartsWithSegments(HealthEndpointPath, StringComparison.OrdinalIgnoreCase) &&
                                       !path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase) &&
                                       !path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase) &&
                                       !path.StartsWithSegments("/redoc", StringComparison.OrdinalIgnoreCase);
                            };
                        })
                        .AddHttpClientInstrumentation()
                        .AddMiniDataProfilerInstrumentation()
                        .AddApplicationInstrumentation();

                    tracing.AddOtlpExporter();
                });
        }

        // Custom instrument
        builder.Services.AddApplicationInstrument();

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Components
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureComponents(this IHostApplicationBuilder builder)
    {
        // System
        builder.Services.AddSingleton(TimeProvider.System);

        // Data
        builder.Services.AddSingleton<IDbProvider>(static p =>
        {
            var configuration = p.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("Default");

            var listener = CreateProfileListener(p, p.GetRequiredService<ProfilerSetting>());
            if (listener is not null)
            {
                return new DelegateDbProvider(() => new ProfileDbConnection(listener, new SqliteConnection(connectionString)));
            }

            return new DelegateDbProvider(() => new SqliteConnection(connectionString));
        });
        builder.Services.AddSingleton<IDialect>(new DelegateDialect(
            static ex => ex is SqliteException { SqliteErrorCode: 19 } or SqliteException { SqliteExtendedErrorCode: 1555 or 2067 },
            static x => Regex.Replace(x, "[%_]", "[$0]")));
        builder.Services.AddDataAccessors(typeof(DataAccessor).Assembly);

        // Cache
        builder.Services.AddMemoryCache();

        // Storage
        builder.Services.AddOptions<FileStorageOptions>().BindConfiguration("Storage").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<FileStorageOptions>>().Value);
        builder.Services.AddSingleton<IStorage, FileStorage>();

        // Security
        builder.Services.AddSingleton(new DefaultPasswordProviderOptions());
        builder.Services.AddSingleton<IPasswordProvider, DefaultPasswordProvider>();

        // Service
        builder.Services.AddCoreServices();

        // Report
        builder.Services.AddSingleton<Infrastructure.Reports.InvoiceReportBuilder>();

        // Setting
        builder.Services.AddOptions<ProfilerSetting>().BindConfiguration("Profiler").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<ProfilerSetting>>().Value);
        builder.Services.AddOptions<LogSetting>().BindConfiguration("Log").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<LogSetting>>().Value);
        builder.Services.AddOptions<CompressionSetting>().BindConfiguration("Compression").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<CompressionSetting>>().Value);
        builder.Services.AddOptions<CspSetting>().BindConfiguration("Csp").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<CspSetting>>().Value);
        builder.Services.AddOptions<AuthSetting>().BindConfiguration("Auth").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<AuthSetting>>().Value);
        builder.Services.AddOptions<TelemetrySetting>().BindConfiguration("Telemetry").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<TelemetrySetting>>().Value);

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Information
    //--------------------------------------------------------------------------------

    public static void LogStartupInformation(this WebApplication app)
    {
        ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);

        var prometheusSection = app.Configuration.GetSection("Prometheus");
        var prometheusUri = prometheusSection.GetValue("Uri", string.Empty);

        app.Logger.InfoServiceStart();
        app.Logger.InfoServiceSettingsRuntime(RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription, RuntimeInformation.RuntimeIdentifier);
        app.Logger.InfoServiceSettingsEnvironment(typeof(Program).Assembly.GetName().Version, Environment.CurrentDirectory);
        app.Logger.InfoServiceSettingsGC(GCSettings.IsServerGC, GCSettings.LatencyMode, GCSettings.LargeObjectHeapCompactionMode);
        app.Logger.InfoServiceSettingsThreadPool(workerThreads, completionPortThreads);
        app.Logger.InfoServiceSettingsTelemetry(app.Configuration.GetOtelExporterEndpoint(), prometheusUri);
    }

    //--------------------------------------------------------------------------------
    // End point
    //--------------------------------------------------------------------------------

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        // Develop
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            // [MEMO] Add yaml support
            app.MapOpenApi("/openapi/{documentName}.yaml");

            // NSwag UI (SwaggerUI / ReDoc) using MapOpenApi generated specification
            app.UseSwaggerUi(static options =>
            {
                options.DocumentPath = "/openapi/v1.json";
            });
            app.UseReDoc(static options =>
            {
                options.Path = "/redoc";
                options.DocumentPath = "/openapi/v1.json";
            });
        }

        // Static assets
        app.MapStaticAssets();

        // MVC
        app.MapControllers();

        // パスキー(WebAuthnオプション発行)
        app.MapPasskeyEndpoints();

        // Health
        app.MapHealthChecks(HealthEndpointPath);
        app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
        {
            Predicate = static r => r.Tags.Contains("live")
        });

        return app;
    }

    //--------------------------------------------------------------------------------
    // Startup
    //--------------------------------------------------------------------------------

    public static ValueTask InitializeApplicationAsync(this WebApplication app)
    {
        // Prepare instrument
        app.Services.GetRequiredService<ApplicationInstrument>();

        // Prepare storage
        Directory.CreateDirectory(app.Services.GetRequiredService<FileStorageOptions>().Root);

        // Prepare database
        app.Services.GetRequiredService<DataService>().CreateTable();

        var setting = app.Services.GetRequiredService<AuthSetting>();
        return app.Services.GetRequiredService<AccountService>().InitializeAsync(setting.InitialId, setting.InitialPassword, Roles.Administrator);
    }

    //--------------------------------------------------------------------------------
    // Configuration
    //--------------------------------------------------------------------------------

    private static bool IsOtelExporterEnabled(this IConfiguration configuration) =>
        !String.IsNullOrWhiteSpace(configuration.GetOtelExporterEndpoint());

    //--------------------------------------------------------------------------------
    // Profiler
    //--------------------------------------------------------------------------------

    // SQLトレースをログ/テレメトリそれぞれの設定で有効化する
    private static IProfileListener? CreateProfileListener(IServiceProvider provider, ProfilerSetting setting)
    {
        var listeners = new List<IProfileListener>();
        if (setting.SqlLog.Enable)
        {
            var option = new LoggingListenerOption
            {
                OutputParameter = setting.SqlLog.OutputParameter,
                ElapsedThreshold = TimeSpan.FromMilliseconds(setting.SqlLog.ElapsedThresholdMilliseconds)
            };
            listeners.Add(new LoggingListener(provider.GetRequiredService<ILogger<LoggingListener>>(), option));
        }

        if (setting.SqlTelemetry.Enable)
        {
            listeners.Add(new OpenTelemetryListener(new OpenTelemetryListenerOption()));
        }

        return listeners.Count switch
        {
            0 => null,
            1 => listeners[0],
            _ => new ChainListener(listeners)
        };
    }

    private static string GetOtelExporterEndpoint(this IConfiguration configuration) =>
        configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? string.Empty;
}

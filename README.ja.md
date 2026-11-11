# Template project for ASP.NET Core MVC

[English](README.md) | 日本語

[Template](https://usausa.github.io/template/) project for ASP.NET Core MVC.

# ✅ 実装

| 分類 | 機能 |
| --- | --- |
| UI | MVC(Areas・Razor ビュー) / Bootstrap 5 / クライアント検証(jQuery Validation) / 並べ替え一覧とページャー / フラッシュメッセージ / 未保存変更の警告 / エラー画面 |
| API | Web API コントローラー(`/api`) / OpenAPI(Swagger UI・ReDoc) / 検証 / ProblemDetails |
| 認証 | EF Core なしの Identity(Smart.Data.Accessor) / Cookie / パスキー(WebAuthn) / 新規登録 / ロックアウト / ロールポリシー / 認証のオン・オフ切り替え |
| データ | SQLite(Smart.Data.Accessor) / ページング・並べ替え・検索 / CSV 出力(CsvHelper) / PDF 帳票(OysterReport) / ファイル保管 |
| HTTP | 圧縮(Brotli・Gzip) / セキュリティヘッダー(CSP nonce) / 転送ヘッダー / CSRF 対策(Antiforgery) |
| 監視 | Serilog / HTTP ログ / W3C ログ / SQL プロファイラー(MiniDataProfiler) / OpenTelemetry(OTLP・Prometheus) / ヘルスチェック |
| 運用 | 機能フラグ / Windows サービス / systemd / Aspire AppHost |
| テスト | 単体(xUnit v3) / 結合(WebApplicationFactory・AngleSharp) / E2E(Playwright) |

# 📚 ドキュメント

| ドキュメント | 内容 |
| --- | --- |
| [authentication.md](docs/authentication.md) | 認証(流れ・画面・構成・ロールとクレイム・メモ) |
| [e2e.md](docs/e2e.md) | E2E テスト(Playwright) |

# 🔗 関連プロジェクト

| プロジェクト | 説明 |
| --- | --- |
| [template-web-api](https://github.com/usausa/template-web-api) | Web API のテンプレート(Minimal API) |
| [template-web-fullstack](https://github.com/usausa/template-web-fullstack) | フルスタック構成のテンプレート |
| [template-blazor-server](https://github.com/usausa/template-blazor-server) | Blazor Server アプリのテンプレート |
| [template-blazor-wasm](https://github.com/usausa/template-blazor-wasm) | Blazor WebAssembly + ASP.NET Core API のテンプレート |
| [template-grpc-server](https://github.com/usausa/template-grpc-server) | gRPC サーバーのテンプレート |

# 📦 自作ライブラリ

| ライブラリ | 説明 |
| --- | --- |
| [Usa.Smart.AspNetCore](https://github.com/usausa/Smart-Net-AspNetCore) | ASP.NET Core ヘルパー |
| [Usa.Smart.Data.Accessor](https://github.com/usausa/Smart-Net-Data-Accessor) | データアクセス |
| [Usa.Smart.Mapper](https://github.com/usausa/Smart-Net-Mapper) | オブジェクトマッパー |
| [OysterReport](https://github.com/usausa/oyster-report) | PDF 作成 |
| [MiniDataProfiler](https://github.com/usausa/mini-data-profiler) | SQL プロファイラー |
| [BunnyTail.ServiceRegistration](https://github.com/usausa/bunnytail-service-registration) | サービス登録 |

namespace Template.WebApp.Host.Application;

using Smart.Mapper;

using Template.WebApp.Host.Areas.Api.Models;
using Template.WebApp.Host.Areas.Default.Models;

internal static partial class DataMapper
{
    [Mapper]
    public static partial DataEditForm ToForm(this DataEntity entity);

    [Mapper]
    public static partial DataResponse ToResponse(this DataEntity entity);
}

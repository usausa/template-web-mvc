namespace Template.WebApp.Infrastructure.Storage;

public sealed class FileStorageOption
{
    [Required]
    public string Root { get; set; } = default!;
}

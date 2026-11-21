namespace Template.WebApp.Host.Areas.Default.Models;

using Smart.AspNetCore.ModelBinding;
using Smart.ComponentModel.DataAnnotations;
using Smart.Text.Japanese;

public sealed class ConversionSampleForm
{
    [KanaConvert(KanaOption.Wide)]
    [Ms932MaxLength(20)]
    [FullWidth]
    public string? ServerName { get; set; }

    [KanaConvert(KanaOption.HankanaToKatakana | KanaOption.HiraganaToKatakana)]
    [Katakana]
    public string? ServerKana { get; set; }

    [KanaConvert(KanaOption.RomanToNarrow | KanaOption.NumericToNarrow)]
    [UpperAlphanumeric]
    public string? ServerCode { get; set; }

    [KanaConvert(KanaOption.Wide, ClientValidation = true)]
    [Ms932MaxLength(20)]
    [FullWidth]
    public string? BrowserName { get; set; }

    [KanaConvert(KanaOption.HankanaToKatakana | KanaOption.HiraganaToKatakana, ClientValidation = true)]
    [Katakana]
    public string? BrowserKana { get; set; }

    [KanaConvert(KanaOption.RomanToNarrow | KanaOption.NumericToNarrow, ClientValidation = true)]
    [UpperAlphanumeric]
    public string? BrowserCode { get; set; }
}

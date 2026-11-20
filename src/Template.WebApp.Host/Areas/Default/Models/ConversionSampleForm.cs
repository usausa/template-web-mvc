namespace Template.WebApp.Host.Areas.Default.Models;

using Smart.AspNetCore.ModelBinding;
using Smart.ComponentModel.DataAnnotations;
using Smart.Text.Japanese;

public sealed class ConversionSampleForm
{
    [KanaConvert(KanaOption.Wide)]
    [Ms932MaxLength(20)]
    [FullWidth]
    public string? Name { get; set; }

    [KanaConvert(KanaOption.HankanaToKatakana | KanaOption.HiraganaToKatakana)]
    [Katakana]
    public string? Kana { get; set; }

    [KanaConvert(KanaOption.RomanToNarrow | KanaOption.NumericToNarrow)]
    [UpperAlphanumeric]
    public string? Code { get; set; }
}

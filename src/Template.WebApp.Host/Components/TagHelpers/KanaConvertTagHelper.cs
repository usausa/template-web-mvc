namespace Template.WebApp.Host.Components.TagHelpers;

using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using Smart.AspNetCore.ModelBinding;

[HtmlTargetElement("input", Attributes = ForAttributeName)]
public sealed class KanaConvertTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if ((For.Metadata is DefaultModelMetadata metadata) &&
            (metadata.Attributes.PropertyAttributes?.OfType<KanaConvertAttribute>().FirstOrDefault() is { ClientValidation: true } attribute))
        {
            output.Attributes.SetAttribute("data-kana-convert", attribute.Option.ToString());
        }
    }
}

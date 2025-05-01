using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Backend.Filter;

/// <summary>
/// Swagger schema filter to modify description of enum types so they
/// show the XML docs attached to each member of the enum.
/// </summary>
public class EnumSchemaFilter : ISchemaFilter {
    private readonly XDocument xmlComments;

    /// <summary>
    /// Initialize schema filter.
    /// </summary>
    /// <param name="xmlComments">Document containing XML docs for enum members.</param>
    public EnumSchemaFilter(XDocument xmlComments) {
        this.xmlComments = xmlComments;
    }

    /// <summary>
    /// Apply this schema filter.
    /// </summary>
    /// <param name="schema">Target schema object.</param>
    /// <param name="context">Schema filter context.</param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context) {
        Type enumType = context.Type;

        if (!enumType.IsEnum) {
            return;
        }

        // 避免重複加入描述
        if (schema.Description?.Contains("<p>Possible values:</p>") == true) {
            return;
        }

        StringBuilder sb = new(schema.Description);

        sb.AppendLine("<p>Possible values:</p>");
        sb.AppendLine("<ul>");

        foreach (string enumMemberName in Enum.GetNames(enumType)) {
            string fullEnumMemberName = $"F:{enumType.FullName}.{enumMemberName}";

            string enumMemberDescription = xmlComments.XPathEvaluate(
              $"normalize-space(//member[@name = '{fullEnumMemberName}']/summary/text())"
            ) as string;

            if (string.IsNullOrEmpty(enumMemberDescription)) {
                continue;
            }

            long enumValue = Convert.ToInt64(Enum.Parse(enumType, enumMemberName));

            // 實際要使用 Enum 值還是 Enum 名稱，自行評估
            sb.AppendLine($"<li><b>{enumValue}[{enumMemberName}]</b>: {enumMemberDescription}</li>");
        }

        sb.AppendLine("</ul>");

        schema.Description = sb.ToString();
    }
}
using System;
using System.Linq;
using System.Text;
using Glow.TypeScript;
using Serilog;

namespace Glow.Core.Typescript
{


    public class RenderRequestDescription
    {
        private readonly RequestDescription requestDescription;

        public RenderRequestDescription(RequestDescription requestDescription)
        {
            this.requestDescription = requestDescription;
        }

        public string Render()
        {
            var a = requestDescription;
            var builder = new StringBuilder();
            try
            {
                foreach (ParameterDescription item in a.ParameterDescriptions.Where(v => v.Name != "api-version"))
                {
                    if (!item.Type.FullName.StartsWith("System"))
                    {
                        Extensions2.AddType(item.Type);
                    }
                }

                builder.AppendLine($"export module {a.ControllerName.Replace("`", "")} {{");
                if (a.HttpMethod?.ToLower() == "post")
                {
                    builder.Append(
$@"  export async function {a.ActionName}_{a.GroupName}({string.Join(",", a.ParameterDescriptions.Where(v => v.Name != "api-version").Select(v => $"{v.Name}: {v.Type.ToTsType()}"))}) {{
    const response = await fetch(`/{a.RelativePath}?api-version=1.0`, {{
      method: ""POST"",
      headers: {{ ""content-type"": ""application/json"" }},
      body: JSON.stringify({a.ParameterDescriptions.Where(v => v.Name != "api-version").First().Name}),
    }})
    const data = await response.json()
    return data
  }}
");
                }
                else
                {
                    builder.Append(
$@"  export async function {a.ActionName}_{a.GroupName}({string.Join(",", a.ParameterDescriptions.Where(v => v.Name != "api-version").Select(v => $"{v.Name}: {v.Type.ToTsType()}"))}) {{
    const response = await fetch(`/{a.RelativePath.Replace("{", "${")}?api-version=1.0`)
    const data = await response.json()
    return data
  }}
");
                }
                builder.AppendLine($"}}");

                return builder.ToString();
            }
            catch (Exception e)
            {
                Log.Logger.Warning($"Could not generate client '{a.Id}' ({e.Message})");
                return "";
            }
        }
    }
}

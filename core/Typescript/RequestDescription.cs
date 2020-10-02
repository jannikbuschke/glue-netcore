using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace Glow.TypeScript
{

    public class RequestDescription
    {
        public string Id { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public string RelativePath { get; set; }
        public string HttpMethod { get; set; }
        public IEnumerable<ParameterDescription> ParameterDescriptions { get; set; }
    }
}


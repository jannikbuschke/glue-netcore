using Microsoft.AspNetCore.Mvc;

namespace Glow.Validation
{
    public class SerializableValidationResult
    {
        public bool IsValid { get; set; }
        public SerializableError Errors { get; set; }
    }
}

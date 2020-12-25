using System;

namespace Glow.Glue.AspNetCore
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string msg) : base(msg) { }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string msg) : base(msg) { }
    }

    public class MissingConsentException : Exception
    {
        public MissingConsentException(string msg, string scope) : base(msg)
        { Scope = scope; }

        public string Scope { get; }
    }
}

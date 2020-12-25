using System;

namespace Glow.Glue.AspNetCore
{
    public static class Guids
    {
        public const string GuidOne = "00000000-0000-0000-0000-000000000001";
        public const string GuidTwo = "00000000-0000-0000-0000-000000000002";
        public static readonly Guid ONE = Guid.Parse(GuidOne);
        public static readonly Guid TWO = Guid.Parse(GuidTwo);
    }
}

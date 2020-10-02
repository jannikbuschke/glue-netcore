using System;
using System.Collections.Generic;

namespace Glow.TypeScript
{
    public abstract class TypeScriptProfile
    {
        public IList<Type> Types { get; } = new List<Type>();
        protected void Add<T>()
        {
            Types.Add(typeof(T));
        }

    }
}


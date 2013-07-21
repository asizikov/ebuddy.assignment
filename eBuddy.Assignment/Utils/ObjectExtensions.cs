using System;

namespace eBuddy.Assignment.Utils
{
    public static class ObjectExtensions
    {
        public static void ThrowIfNull(this object target, string text)
        {
            if (target == null)
            {
                throw new ArgumentNullException(text);
            }
        }
    }
}

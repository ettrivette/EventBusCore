using System;

namespace EventBus.Abstractions
{
    public static class TypeExtensions
    {
        public static string GetEventName(this Type type)
        {
            return type.Name;
        }
    }
}

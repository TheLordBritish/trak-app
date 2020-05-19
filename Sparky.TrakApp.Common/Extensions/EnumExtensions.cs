using System;
using System.Linq;
using System.Reflection;

namespace Sparky.TrakApp.Common.Extensions
{
    public static class EnumExtensions
    {
        public static TExpected GetAttributeValue<T, TExpected>(this Enum @enum, Func<T, TExpected> exp)
        {
            var memberInfo = @enum
                .GetType()
                .GetMember(@enum.ToString())
                .FirstOrDefault(member => member.MemberType == MemberTypes.Field);
            
            if (memberInfo != null)
            {
                var attribute = memberInfo
                    .GetCustomAttributes(typeof(T), false)
                    .Cast<T>()
                    .SingleOrDefault();
            
                return attribute == null ? default : exp(attribute);
            }

            return default;
        }
    }
}
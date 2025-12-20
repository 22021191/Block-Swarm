using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Connect.Core
{
    public static class AttributeExtensions
    {
        public static TAttribute GetCustomTypeAttribute<TAttribute>(this Enum parameter)
            where TAttribute : Attribute
        {
            return GetCustomTypeAttributes<TAttribute>(parameter)?.FirstOrDefault();
        }

        public static List<TAttribute> GetCustomTypeAttributes<TAttribute>(this Enum parameter)
            where TAttribute : Attribute
        {
            var memberInfo = parameter?.GetType().GetMember(parameter.ToString()).FirstOrDefault();
            if (memberInfo == null)
            {
                return null;
            }
            return Attribute.GetCustomAttributes(memberInfo, typeof(TAttribute))
                .Cast<TAttribute>()
                .ToList();
        }
        private static Dictionary<Type, List<(Type enumType, List<(string enumName, List<Attribute> attributes)> matches)>> _assemblyEnumMemberCache =
            new Dictionary<Type, List<(Type enumType, List<(string enumName, List<Attribute> attributes)> matches)>>();
    }
}
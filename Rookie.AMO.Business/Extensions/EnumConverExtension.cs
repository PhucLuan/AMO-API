using System;
using System.ComponentModel;
using System.Reflection;

namespace Rookie.AMO.Business
{
    public static class EnumConverExtension
    {
        public static string GetNameString<T>(this T enumType) where T : Enum
        {
            return Enum.GetName(typeof(T), enumType);
        }
        public static int GetValueInt<T>(string name) where T : Enum
        {
            return (int)Enum.Parse(typeof(T), name);
        }
        public static string GetDescription<T>(this Enum value) where T : Enum
        {

            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }
}
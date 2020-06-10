using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace TsNode.Preset.Models
{
    public enum SerializableType
    {
        [BindType(Type = typeof(string))] String,
        [BindType(Type = typeof(float))]  Float,
        [BindType(Type = typeof(int))]    Int,
        [BindType(Type = typeof(bool))]   Bool,
        [BindType(Type = typeof(Vector3))]Vector3,
    }
    
    internal class BindTypeAttribute: Attribute
    {
        public Type Type { get; set; }
    }
    
    internal static class TypeSerializer
    {
        private static readonly Dictionary<SerializableType, Type> AttributeTypeCache = new Dictionary<SerializableType, Type>();
        public static Type ToSystemType(this SerializableType type)
        {
            if (AttributeTypeCache.ContainsKey(type))
                return AttributeTypeCache[type];

            var fieldInfo = typeof(SerializableType).GetField(type.ToString());
            Debug.Assert(fieldInfo != null);

            var attribs = fieldInfo.GetCustomAttributes(typeof(BindTypeAttribute), false) as BindTypeAttribute[];
            Debug.Assert(attribs != null);
            
            return AttributeTypeCache[type] = attribs[0].Type;
        }

        public static SerializableType ToSerializableType(this Type type)
        {
            var enumName = char.ToUpper(type.Name[0]) + type.Name.Substring(1);
#if NETCOREAPP3_1
            return Enum.Parse<SerializableType>(enumName);
#else
            return (SerializableType)Enum.Parse(typeof(SerializableType), enumName);
#endif
        }

        public static T CreateInstance<T>(this SerializableType type , Type generic)
        {
            var propertyType = generic.MakeGenericType(type.ToSystemType());
            return (T)Activator.CreateInstance(propertyType);
        }
        
        public static string Serialize(this object obj)
        {
            if (obj is null)
                return string.Empty;
            
            if (obj is Vector3 v)
                return $"{v.X},{v.Y},{v.Z}";
            
            return obj.ToString();
        }

        public static object Deserialize(Type type , string obj)
        {
            if (type == typeof(Vector3))
            {
                try
                {
                    var values = obj.Split(',').Select(float.Parse).ToArray();
                    return new Vector3 {X = values[0], Y = values[1], Z = values[2],};
                }
                catch
                {
                    return new Vector3(0,0,0);
                }
            }

            if (type == typeof(int))
            {
                if (int.TryParse(obj, out var result))
                    return result;
                return default(int);
            }

            if (type == typeof(float))
            {
                if (float.TryParse(obj, out var result))
                    return result;
                return default(float);
            }

            if (type == typeof(bool))
            {
                if (bool.TryParse(obj, out var result))
                    return result;
                return default(bool);
            }

            if (type == typeof(double))
            {
                if (double.TryParse(obj, out var result))
                    return result;
                return default(double);
            }

            return obj ?? string.Empty;
        }
    }
}
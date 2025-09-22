using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Collections;
namespace Snippets.NativeExtensions
{
    public static class ObjectExtensions {
        public static bool IsGenericList(this object o)
        {
            return (typeof(IList).IsAssignableFrom(o.GetType()));
        }
        public static TValue GetPropertyValueByName<TValue>(this object o, string name, BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance) {
            return (TValue)o.GetType().GetProperty(name, bindingFlags).GetValue(o);
        }
        public static bool TryCast<T>(this object obj, out T result)
        {
            try
            {
                result = default(T);
                if (obj is T)
                {
                    result = (T)obj;
                    return true;
                }
                if (obj == null)
                    return !typeof(T).IsValueType;
               
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter.CanConvertFrom(obj.GetType()))
                    result = (T)converter.ConvertFrom(obj);
                else
                    return false;
                return true;
            }
            catch(Exception ex) {
                Console.WriteLine(ex);
            }
            result = default(T);
            return false;
        }
        public static T ToObject<T>(this IDictionary<string, object> source)
      where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (var item in source)
            {
                someObjectType
                         .GetProperty(item.Key)
                         .SetValue(someObject, item.Value, null);
            }

            return someObject;
        }

        public static IDictionary<string, object> AsStringObjectDictionary(this object source,
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance,
            bool allowedToFailSomeProperties = true)
        {
            Type type = source.GetType();
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                if (typeof(IDictionary<string, object>).IsAssignableFrom(type)) return (IDictionary<string, object>)source;
                IDictionary iDictionary = ((IDictionary)source);
                return iDictionary.Keys
                                .Cast<object>()
                                .ToDictionary(k => k.ToString(), k => iDictionary[k]);
            }
        
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (PropertyInfo propertyInfo in source.GetType().GetProperties(bindingAttr))
            {
                try
                {

                    dictionary[propertyInfo.Name] = propertyInfo.GetValue(source, null);
                }
                catch (Exception ex) {
                    if (!allowedToFailSomeProperties) throw ex;
                }
            }
            return dictionary;
        }
    }
}

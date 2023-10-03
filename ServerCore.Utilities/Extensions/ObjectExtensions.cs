using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ServerCore.Utilities.Extensions
{
    public static class ObjectExtensions
    {
        public static string ConvertToString(this object ob)
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                foreach (PropertyInfo property in ob.GetType().GetProperties())
                {
                    object obj = ob.GetType().GetProperty(property.Name).GetValue(ob, (object[])null);
                    stringBuilder.AppendFormat("{0}:{1}| ", (object)property.Name, obj);
                }
            }
            catch
            {
            }
            return stringBuilder.ToString();
        }

        public static string ToURL(this object obj)
        {
            List<string> stringList = new List<string>();
            foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                stringList.Add(property.Name + "=" + property.GetValue(obj));
            return string.Join("&", (IEnumerable<string>)stringList);
        }
    }
}

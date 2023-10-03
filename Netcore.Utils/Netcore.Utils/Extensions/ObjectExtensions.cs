using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetCore.Utils.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToURL(this object obj)
        {
            var result = new List<string>();
            foreach (var property in obj.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                result.Add(property.Name + "=" + property.GetValue(obj));
            }

            return string.Join("&", result);
        }

        public static string ConvertToString(this object ob)
        {
            var sb = new StringBuilder();
            try
            {
                foreach (System.Reflection.PropertyInfo piOrig in ob.GetType().GetProperties())
                {
                    object editedVal = ob.GetType().GetProperty(piOrig.Name).GetValue(ob, null);
                    sb.AppendFormat("{0}:{1}| ", piOrig.Name, editedVal);
                }
            }
            catch
            {
            }
            return sb.ToString();
        }
        public static string ConvertToJson(this object ob)
        {
           return JsonConvert.SerializeObject(ob);
        }
        public static IDictionary<string, string> ConvertToDictionary(this object source)
        {
            var res = source.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(prop => prop.Name, (prop) =>
            {
                var value = prop.GetValue(source, null).ToString();
                if (value == "System.Object[]") value = "";
                return value;
            });
            return res;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SSOService.Models;

namespace SSOService
{
    public class SSOJSONConverter : JsonConverter
    {
        public SSOJSONConverter(params Type[] types)
        {
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);

            if (t.Type != JTokenType.Object)
            {
                t.WriteTo(writer);
            }
            else
            {
                JObject o = (JObject)t;
                //if(value.GetType() == typeof(LDAPAttributes<string, string>))
                if (value.GetType() == typeof(User))
                {
                    Dictionary<string, string> attrs = ((User)value).GetAttributes();
                    foreach(KeyValuePair<string, string> kvp in attrs)
                        o.Add(kvp.Key, kvp.Value);
                }
                o.WriteTo(writer);


                //IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();

                //o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));

            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            // return _types.Any(t => t == objectType);
            return true;
        }
    }
}

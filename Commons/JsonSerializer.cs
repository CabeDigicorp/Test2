


using Newtonsoft.Json;
using System.IO;

namespace Commons
{

    public class JsonSerializer
    {
        static public bool JsonSerialize(object obj, out string json)
        {
            json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                //TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });

            return true;
        }

        static public bool JsonDeserialize<T>(string json, out T obj, System.Type objectType)
        {
            obj = (T)JsonConvert.DeserializeObject(json, objectType, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            if (obj == null)
                return false;

            return true;
        }


    }

}
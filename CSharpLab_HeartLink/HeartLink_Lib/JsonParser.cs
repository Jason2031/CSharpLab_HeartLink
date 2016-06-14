using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeartLink_Lib
{
    public class JsonParser
    {
        public static string SerializeObject(Object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public static Dictionary<string, string> DeserializeObject(string json)
        {
            return (Dictionary<string, string>)JsonConvert.DeserializeObject(json, typeof(Dictionary<string, string>));
        }

        public static List<Dictionary<string, string>> DeserializeListOfDictionaryObject(string json)
        {
            return (List<Dictionary<string, string>>)JsonConvert.DeserializeObject(json, typeof(List<Dictionary<string, string>>));
        }

    }
}

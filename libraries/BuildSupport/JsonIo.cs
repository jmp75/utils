using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Utils.Lib.BuildSupport
{
    public class JsonIo
    {
        public static T Load<T>(string filename)
        {
            using (var sr = new StreamReader(filename))
            {
                return JsonConvert.DeserializeObject<T>(sr.ReadToEnd(), getSerSettings());
            }
        }

        public static void Save<T>(T defn, string filename)
        {
            var output = Serialize(defn);
            using (var f = File.CreateText(filename))
            {
                f.Write(output);
                f.Flush();
            }
        }

        public static string Serialize<T>(T defn)
        {
            string output = JsonConvert.SerializeObject(defn, Formatting.Indented, getSerSettings());
            return output;
        }

        private static JsonSerializerSettings getSerSettings()
        {
            var s = new JsonSerializerSettings();
            s.DefaultValueHandling = DefaultValueHandling.Include;
            s.PreserveReferencesHandling = PreserveReferencesHandling.None;
            s.MaxDepth = 5;
            return s;
        }
    }
}

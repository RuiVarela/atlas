using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Atlas
{
    class ConfigReader
    {
        static readonly ConfigReader _instance = new ConfigReader();
        Dictionary<String, String> _config;

        public static ConfigReader Instance { get { return _instance; } }

        private ConfigReader()
        {
            _config = new Dictionary<string, string>();
            try
            {
                StreamReader reader = new StreamReader("Config/config.ini");
                while (!reader.EndOfStream)
                {
                    String line = reader.ReadLine();
                    String[] keyValue = line.Split("=".ToCharArray());
                    if (keyValue.Length < 2) continue;
                    _config.Add(keyValue[0], keyValue[1]);
                }
                reader.Close();
            }
            catch (FileNotFoundException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
        }

        public String GetValueAsString(String key)
        {
            String result;
            _config.TryGetValue(key, out result);
            return result;
        }

        public bool IsKeyValid(String key)
        {
            return _config.ContainsKey(key);
        }

        //all these assume that key is valid, it's up to the programmer to check...
        
        //if it doesn't exist, 0 is returned
        public int GetValueAsInt(String key)
        {
            /*int result;
            if (int.TryParse(GetValueAsString(key), out result)) return result;
            else return 0;*/
            String value = GetValueAsString(key);
            if (value == null) return 0;
            return int.Parse(value);
        }

        //if it doesn't exist, false is returned
        public bool GetValueAsBool(String key)
        {
            /*bool result;
            if (bool.TryParse(GetValueAsString(key), out result)) return result;
            else return false;*/
            String value = GetValueAsString(key);
            if (value == null) return false;
            return bool.Parse(value);
        }
    }
}

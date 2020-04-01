using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.RepresentationModel;

namespace ExcelConvert
{
    public struct ConfigData
    {
        public string SourcePath;
        public string ExportJsonPath;
        public string ExportCsPath;
        public string SourceExtentions;
    }

    public class ConfigLoader
    {
        public ConfigLoader(string path)
        {
            Load(path);
        }

        void Load(string path)
        {
            if (File.Exists(path))
            {
                using (var input = new StreamReader(path))
                {
                    var deserializer = new YamlDotNet.Serialization.Deserializer();
                    m_config = deserializer.Deserialize<ConfigData>(input.ReadToEnd());
                }
            }
        }

        public ConfigData Config => m_config;

        ConfigData m_config;
    }
}

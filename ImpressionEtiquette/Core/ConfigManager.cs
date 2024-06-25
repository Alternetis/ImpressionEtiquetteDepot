using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ImpressionEtiquetteDepot.Core
{
    public class ConfigManager
    {
        private static readonly string configFilePath = "ImpressionEtiquetteDepot.exe.config";

        public static string GetSetting(string settingName)
        {
            XDocument doc = LoadConfig();
            XElement settingElement = doc?.Element("<ImpressionEtiquetteDepot.Properties.Settings>")?.Element(settingName);
            return settingElement?.Value;
        }

        public static void SetSetting(string settingName, string value)
        {
            XDocument doc = LoadConfig();
            //XElement settingsElement = doc?.Element("<ImpressionEtiquetteDepot.Properties.Settings>");
            XElement settingsElement = doc?.Descendants("ImpressionEtiquetteDepot.Properties.Settings").FirstOrDefault();

            // Vérifier si l'élément a été trouvé
            if (settingsElement != null)
            {
                // Rechercher l'élément setting avec le nom spécifié
                XElement settingToUpdate = settingsElement.Elements("setting")
                    .FirstOrDefault(e => e.Attribute("name")?.Value == settingName);

                // Vérifier si l'élément setting a été trouvé
                if (settingToUpdate != null)
                {
                    // Mettre à jour la valeur de l'élément value
                    settingToUpdate.Element("value").Value = value;


                }
                else
                {
                  Core.Log.WriteLog($"Aucun élément setting trouvé avec le nom '{settingName}'.");
                }
            }

                SaveConfig(doc);
        }

        private static XDocument LoadConfig()
        {
            if (File.Exists(configFilePath))
            {
                return XDocument.Load(configFilePath);
            }
            return new XDocument();
        }

        private static void SaveConfig(XDocument doc)
        {
            doc.Save(configFilePath);
        }
    }
}

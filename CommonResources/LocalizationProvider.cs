
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using WPFLocalizeExtension.Extensions;

namespace CommonResources
{
    public static class LocalizationProvider
    {
        static LocalizationProvider()
        {
            //Da abilitare solo per la traduzione (aggiornamento delle chiavi di String.it.resx, String.en.resx, etc...
            //UpdateResourceFilesName();
        }

        private static T GetLocalizedValue<T>(string key)
        {
            return LocExtension.GetLocalizedValue<T>("DigiCorp.CommonResources:Strings:" + key);
        }

        public static string GetString(string key)
        {
            string res = LocExtension.GetLocalizedValue<string>("DigiCorp.CommonResources:Strings:" + key);
            if (res == null)
                return string.Format("key:[{0}]", key);
            else if (!res.Trim().Any())
                return string.Format("val?[{0}]", key);
            else
                return res;
        }

        /// <summary>
        /// Aggiorna i dizionari delle lingue preservando le stringhe già tradotte
        /// </summary>
        public static void UpdateResourceFilesName()
        {
            Hashtable resourceEntries = new Hashtable();

            //Get existing resources
            //string projDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string projDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string path = string.Format("{0}{1}", projDir, "\\CommonResources\\Properties\\");

            //string path = "C:\\Sviluppo\\Source\\Workspaces\\v1_0\\BiMosaico\\CommonResources\\Properties\\"; 
            //string path = "C:\\Sviluppo\\Source\\Workspaces\\BiMosaicoWorking\\CommonResources\\Properties\\";
            string fileName = "Strings";

            string filepath = string.Format("{0}{1}.resx", path, fileName);
            string filepathIt = string.Format("{0}{1}.it.resx", path, fileName);
            string filepathEn = string.Format("{0}{1}.en.resx", path, fileName);
            string filepathDe = string.Format("{0}{1}.de.resx", path, fileName);

            ResXResourceReader reader = new ResXResourceReader(filepath);
            if (reader == null)
                return;

            //it
            Dictionary<string, string> dictIt = Read(filepathIt);
            ResXResourceWriter writerIt = new ResXResourceWriter(filepathIt);
            if (writerIt == null)
                return;

            //en
            Dictionary<string, string> dictEn = Read(filepathEn);
            ResXResourceWriter writerEn = new ResXResourceWriter(filepathEn);
            if (writerEn == null)
                return;

            //de
            Dictionary<string, string> dictDe = Read(filepathDe);
            ResXResourceWriter writerDe = new ResXResourceWriter(filepathDe);
            if (writerDe == null)
                return;

            //scorro le chiavi di strings.resx
            IDictionaryEnumerator id1 = reader.GetEnumerator();
            foreach (DictionaryEntry d in reader)
            {
                string key = d.Key.ToString();
                string value = string.Empty;

                Write(dictIt, writerIt, key);
                Write(dictEn, writerEn, key);
                Write(dictDe, writerDe, key);
            }
            reader.Close();

            writerIt.Close();
            writerEn.Close();
            writerDe.Close();

        }

        private static void Write(Dictionary<string, string> dict, ResXResourceWriter writer, string key)
        {
            string value;
            if (dict.ContainsKey(key))
                value = dict[key];
            else
                value = string.Empty;
            writer.AddResource(key, value);
            
        }

        static Dictionary<string, string> Read(string filepath)
        {
            
            ResXResourceReader reader = new ResXResourceReader(filepath);
            if (reader == null)
                return null;

            Dictionary<string, string> dict = new Dictionary<string, string>();

            IDictionaryEnumerator id = reader.GetEnumerator();
            foreach (DictionaryEntry d in reader)
            {
                dict.Add(d.Key.ToString(), d.Value.ToString());
            }
            reader.Close();

            return dict;
        }

    }
}

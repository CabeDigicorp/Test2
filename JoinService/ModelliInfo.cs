

using JoinService.Models;
using log4net;
using log4net.Repository.Hierarchy;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JoinService
{

    public class ModelliInfo
    {
        ILog log = log4net.LogManager.GetLogger(typeof(ModelliInfo));
        List<ModelloInfo> Items { get; set; } = new List<ModelloInfo>();

        public ModelliInfo()
        {
            Load();
        }

        public void Load()
        {  
            try
            {
                ModelliManifest ModelliManifest = GetManifest();

                Items.Clear();
                foreach (ModelliManifestItem info in ModelliManifest.Items)
                {
                    ModelloInfo prezInfo = new ModelloInfo();

                    string modelliPath = GetModelliDir();
                    string modelliFileFullName = string.Format("{0}\\{1}.join", modelliPath, info.FileName);

                    FileInfo fileInfo = new FileInfo(modelliFileFullName);
                    if (!fileInfo.Exists)
                        continue;

                    prezInfo.FileName = info.FileName;
                    prezInfo.Note = info.Note;
                    prezInfo.MinAppVersion = info.MinAppVersion;
                    prezInfo.Tags = info.Tags;
                    prezInfo.LastWriteTime = fileInfo.LastWriteTime;
                    prezInfo.Dimension = fileInfo.Length;//.GetBytesReadable();
                    prezInfo.UserName = info.UserName;

                    Items.Add(prezInfo);
                }

            }
            catch (Exception exc)
            {
                log.Error(exc.Message);
            }


        }

        public List<ModelloInfo> GetItems()
        {
            Load();
            return Items;
        }

        public ModelliManifest GetManifest()
        {
            string modelliPath = GetModelliDir();

            string ModelliManifestPath = string.Format("{0}\\{1}", modelliPath, "ModelliManifest.json");

            //CreateManifestByDir(modelliPath, ModelliManifestPath);

            if (!File.Exists(ModelliManifestPath))
                return null;

            string json = File.ReadAllText(ModelliManifestPath);

            ModelliManifest ModelliManifest = null;
            JsonSerializer.JsonDeserialize<ModelliManifest>(json, out ModelliManifest, typeof(ModelliManifest));

            return ModelliManifest;
        }

        /// <summary>
        /// Funzione utile per creare il file di manifest in base ai modelli contenuti in una cartella
        /// </summary>
        /// <param name="modelliPath"></param>
        /// <param name="ModelliManifestPath"></param>
        private static void CreateManifestByDir(string modelliPath, string ModelliManifestPath)
        {
            File.Delete(ModelliManifestPath);

            ModelliManifest ModelliManifest = new ModelliManifest();
            List<string> files = new List<string>(Directory.GetFiles(modelliPath));

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);

                ModelliManifestItem info = new ModelliManifestItem();
                //info.TempId = NewModelloInfoId();
                info.FileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                info.MinAppVersion = "1.0";
                info.Note = "Nota";
                info.UserName = string.Format("_{0}", info.FileName);

                ModelliManifest.Items.Add(info);
            }

            string json = "";
            JsonSerializer.JsonSerialize(ModelliManifest, out json);

            try
            {
                File.WriteAllText(ModelliManifestPath, json);
            }
            catch (UnauthorizedAccessException exc)
            {
                ILog log = log4net.LogManager.GetLogger(typeof(ModelloInfo));
                log.Error(exc.Message);
                //MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
                //MessageBox.Show(exc.Message);
            }
        }

        public List<int> GetModelliId()
        {
            //return Items.Select(item => item.TempId).ToList();
            return Items.Select(item => Items.IndexOf(item)).ToList();
        }



        //public int NewModelloInfoId()
        //{
        //    HashSet<int> ids = new HashSet<int>(Items.Select(item => item.TempId));
        //    for (int i = 0; i < 1000; i++)
        //    {
        //        if (!ids.Contains(i))
        //        {
        //            return i;
        //        }
        //    }
        //    return -1;
        //}

        public ModelloInfo GetModelloInfo(int id)
        {
            //return Items.FirstOrDefault(item => item.TempId == id);
            if (0 <= id && id < Items.Count)
                return Items[id];

            return null;
        }

        public ModelloInfo GetModelloInfo(string fileName)
        {
            return Items.FirstOrDefault(item => string.Equals(item.FileName, fileName, StringComparison.CurrentCultureIgnoreCase));
        }

        public string GetModelliDir()
        {
            string dir = System.AppDomain.CurrentDomain.BaseDirectory;
            string modelliPath = string.Format("{0}{1}", dir, "App_Data\\Modelli");


            
            return modelliPath;
        }

        static public string AddFileExtension(string fileName)
        {
            return string.Format("{0}.{1}", fileName, "join");
        }


        public FileStream Open(string filename)
        {
            ModelloInfo ModelloInfo = GetModelloInfo(filename);

            string fileFullPath = string.Format("{0}\\{1}", GetModelliDir(), AddFileExtension(ModelloInfo.FileName));

            return File.Open(fileFullPath, FileMode.Open, FileAccess.Read);
        }

        public long GetLength(string filename)
        {
            ModelloInfo ModelloInfo = GetModelloInfo(filename);

            string fileFullPath = string.Format("{0}\\{1}", GetModelliDir(), AddFileExtension(ModelloInfo.FileName));

            return new FileInfo(fileFullPath).Length;
        }




    }





}

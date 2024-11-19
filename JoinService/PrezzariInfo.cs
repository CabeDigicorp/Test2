

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

    public class PrezzariInfo
    {
        ILog log = log4net.LogManager.GetLogger(typeof(PrezzariInfo));
        List<PrezzarioInfo> Items { get; set; } = new List<PrezzarioInfo>();

        public PrezzariInfo()
        {
            Load();
        }

        public void Load()
        {  
            try
            {
                PrezzariManifest prezzariManifest = GetManifest();

                Items.Clear();
                foreach (PrezzariManifestItem info in prezzariManifest.Items)
                {
                    PrezzarioInfo prezInfo = new PrezzarioInfo();

                    string prezzariPath = GetPrezzariDir();
                    string prezzariFileFullName = string.Format("{0}\\{1}.join", prezzariPath, info.FileName);

                    FileInfo fileInfo = new FileInfo(prezzariFileFullName);
                    if (!fileInfo.Exists)
                        continue;

                    prezInfo.FileName = info.FileName;
                    prezInfo.Note = info.Note;
                    prezInfo.MinAppVersion = info.MinAppVersion;
                    prezInfo.Group = info.Group;
                    prezInfo.LastWriteTime = fileInfo.LastWriteTime;
                    prezInfo.Dimension = fileInfo.Length;//.GetBytesReadable();
                    prezInfo.Year = info.Year;

                    Items.Add(prezInfo);
                }

            }
            catch (Exception exc)
            {
                log.Error(exc.Message);
            }


        }

        public List<PrezzarioInfo> GetItems()
        {
            Load();
            return Items;
        }

        public PrezzariManifest GetManifest()
        {
            string prezzariPath = GetPrezzariDir();

            string prezzariManifestPath = string.Format("{0}\\{1}", prezzariPath, "prezzariManifest.json");

            //CreateManifestByDir(prezzariPath, prezzariManifestPath);

            if (!File.Exists(prezzariManifestPath))
                return null;

            string json = File.ReadAllText(prezzariManifestPath);

            PrezzariManifest prezzariManifest = null;
            JsonSerializer.JsonDeserialize<PrezzariManifest>(json, out prezzariManifest, typeof(PrezzariManifest));

            return prezzariManifest;
        }

        /// <summary>
        /// Funzione utile per creare il file di manifest in base ai prezzari contenuti in una cartella
        /// </summary>
        /// <param name="prezzariPath"></param>
        /// <param name="prezzariManifestPath"></param>
        private static void CreateManifestByDir(string prezzariPath, string prezzariManifestPath)
        {
            File.Delete(prezzariManifestPath);

            PrezzariManifest prezzariManifest = new PrezzariManifest();
            List<string> files = new List<string>(Directory.GetFiles(prezzariPath));

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);

                PrezzariManifestItem info = new PrezzariManifestItem();
                //info.TempId = NewPrezzarioInfoId();
                info.FileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                info.MinAppVersion = "1.0";
                info.Note = "Nota";

                prezzariManifest.Items.Add(info);
            }

            string json = "";
            JsonSerializer.JsonSerialize(prezzariManifest, out json);

            //File.WriteAllText(prezzariManifestPath, json);
            try
            {
                File.WriteAllText(prezzariManifestPath, json);
            }
            catch (UnauthorizedAccessException exc)
            {
                ILog log = log4net.LogManager.GetLogger(typeof(PrezzariInfo));
                log.Error(exc.Message);
                //MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
                //MessageBox.Show(exc.Message);
            }
        }

        public List<int> GetPrezzariId()
        {
            //return Items.Select(item => item.TempId).ToList();
            return Items.Select(item => Items.IndexOf(item)).ToList();
        }



        //public int NewPrezzarioInfoId()
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

        public PrezzarioInfo GetPrezzarioInfo(int id)
        {
            //return Items.FirstOrDefault(item => item.TempId == id);
            if (0 <= id && id < Items.Count)
                return Items[id];

            return null;
        }

        public PrezzarioInfo GetPrezzarioInfo(string fileName)
        {
            return Items.FirstOrDefault(item => string.Equals(item.FileName, fileName, StringComparison.CurrentCultureIgnoreCase));
        }

        public string GetPrezzariDir()
        {
            string dir = System.AppDomain.CurrentDomain.BaseDirectory;
            string prezzariPath = string.Format("{0}{1}", dir, "App_Data\\Prezzari");


            
            return prezzariPath;
        }

        static public string AddFileExtension(string fileName)
        {
            return string.Format("{0}.{1}", fileName, "join");
        }


        public FileStream Open(string filename)
        {
            PrezzarioInfo prezzarioInfo = GetPrezzarioInfo(filename);

            string fileFullPath = string.Format("{0}\\{1}", GetPrezzariDir(), AddFileExtension(prezzarioInfo.FileName));

            return File.Open(fileFullPath, FileMode.Open, FileAccess.Read);
        }

        public long GetLength(string filename)
        {
            PrezzarioInfo prezzarioInfo = GetPrezzarioInfo(filename);

            string fileFullPath = string.Format("{0}\\{1}", GetPrezzariDir(), AddFileExtension(prezzarioInfo.FileName));

            return new FileInfo(fileFullPath).Length;
        }




    }





}

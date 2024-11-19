using Commons;
using Microsoft.VisualBasic;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ModelSerializer
    {
        static ModelSerializer()
        {
        }


        public static Project Deserialize(Stream stream, out int projectVersion)
        {

            BinaryReader reader = new BinaryReader(stream);
            projectVersion = reader.ReadInt32();

            Project project = null;

            if (projectVersion <= (int) FileVersion.v101)
            {
                try
                {
                    project = Serializer.Deserialize<Project>(stream);
                }
                catch (Exception exc)
                {
                    MainAppLog.Error(MethodBase.GetCurrentMethod(), "Errore nell'apertura del file: ", exc);
                }

            }
            else/* if (projectVersion <= (int)FileVersion.v102)*/
            {

                using (var brotli = new BrotliStream(stream, CompressionMode.Decompress, true))
                {
                    project = Serializer.Deserialize<Project>(brotli);
                }
            }

            return project;
        }

        public static void Serialize(Stream stream, Project project, int currentFileVersion = -1)
        {
            int nVersion = currentFileVersion;
            if (nVersion < 0)
                nVersion = (int) FileVersion.vLast;

            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(nVersion);

            if (nVersion == (int)FileVersion.v101)
            {
                Serializer.Serialize(stream, project);
            }
            else if (nVersion >= (int)FileVersion.v102)
            {
                using (var brotli = new BrotliStream(stream, CompressionMode.Compress, true))
                {
                    Serializer.Serialize(brotli, project);
                }
            }
        }
    }

    public enum FileVersion
    {
        v100 = 100,
        v101 = 101,//calcolo di ValoreTesto Result
        v102 = 102,//aggiunta compressione Brotli
        v103 = 103,//eliminati gli EntityAttributo di tipo riferimento da Entity.Attributi
        v104 = 104,//Aggiornato il valore Attributo.AllowMasterGrouping sui AttributoRiferimento

        vLast = 104,
    }
}

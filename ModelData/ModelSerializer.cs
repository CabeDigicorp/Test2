using ModelData.Model;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ModelData
{
    public class ModelSerializer
    {
        static ModelSerializer()
        {
            InitRuntimeTypeModel();
        }

       
        public static T Deserialize<T>(string serializedObj)
        {
            byte[] data = Convert.FromBase64String(serializedObj);
            using (MemoryStream stream = new MemoryStream(data))
            {

                using (var brotli = new BrotliStream(stream, CompressionMode.Decompress, true))
                {
                    T obj = ProtoBuf.Serializer.Deserialize<T>(brotli);
                    return obj;
                }

                //T obj = ProtoBuf.Serializer.Deserialize<T>(stream);
                //return obj;
            }

        }

        public static string Serialize(object obj)
        {

            string str = string.Empty;

            try
            {


                MemoryStream msString = new MemoryStream();
                using (var brotli = new BrotliStream(msString, CompressionMode.Compress, true))
                {
                    ProtoBuf.Serializer.Serialize(brotli, obj);
                }
                str = Convert.ToBase64String(msString.ToArray());

                //using (MemoryStream stream = new MemoryStream())
                //{

                //    //ProtoBuf.Serializer.Serialize(stream, obj);
                //    //return Convert.ToBase64String(stream.ToArray());

                //}

            }
            catch (Exception e)
            {

            }

            return str;
        }

        /// <summary>
        /// RuntimeTypeModel di protobuf serve per definire le classi derivate da salvare su file
        /// </summary>
        static public void InitRuntimeTypeModel()//protobuf
        {
            //oss: vedere anche MongoDbSetup.RegisterClassMap

            //Entity
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1001, typeof(TreeEntity));
            RuntimeTypeModel.Default[typeof(TreeEntity)].AddSubType(1003, typeof(PrezzarioItem));
            RuntimeTypeModel.Default[typeof(TreeEntity)].AddSubType(1004, typeof(DivisioneItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1002, typeof(ComputoItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1005, typeof(ElementiItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1006, typeof(ContattiItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1007, typeof(InfoProgettoItem));
            RuntimeTypeModel.Default[typeof(TreeEntity)].AddSubType(1008, typeof(CapitoliItem));
            RuntimeTypeModel.Default[typeof(TreeEntity)].AddSubType(1009, typeof(DocumentiItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1010, typeof(ReportItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1011, typeof(StiliItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1012, typeof(ElencoAttivitaItem));
            RuntimeTypeModel.Default[typeof(TreeEntity)].AddSubType(1013, typeof(WBSItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1014, typeof(CalendariItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1015, typeof(VariabiliItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1016, typeof(AllegatiItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1017, typeof(TagItem));


            //EntityType        
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1001, typeof(TreeEntityType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1002, typeof(PrezzarioItemType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1003, typeof(PrezzarioItemParentType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1004, typeof(ComputoItemType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1005, typeof(DivisioneItemType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1006, typeof(DivisioneItemParentType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1007, typeof(ElementiItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1008, typeof(ContattiItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1009, typeof(InfoProgettoItemType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1010, typeof(CapitoliItemType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1011, typeof(CapitoliItemParentType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1012, typeof(DocumentiItemType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1013, typeof(DocumentiItemParentType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1014, typeof(ReportItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1015, typeof(StiliItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1016, typeof(ElencoAttivitaItemType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1017, typeof(WBSItemType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1018, typeof(WBSItemParentType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1019, typeof(CalendariItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1020, typeof(VariabiliItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1021, typeof(AllegatiItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1022, typeof(TagItemType));



            //Attributo
            RuntimeTypeModel.Default[typeof(Attributo)].AddSubType(1001, typeof(AttributoRiferimento));

            //ValoreCollection
            RuntimeTypeModel.Default[typeof(ValoreCollection)].AddSubType(1001, typeof(ValoreTestoCollection));
            RuntimeTypeModel.Default[typeof(ValoreCollection)].AddSubType(1002, typeof(ValoreGuidCollection));

            //ValoreCollectionItem
            RuntimeTypeModel.Default[typeof(ValoreCollectionItem)].AddSubType(1001, typeof(ValoreTestoCollectionItem));
            RuntimeTypeModel.Default[typeof(ValoreCollectionItem)].AddSubType(1002, typeof(ValoreGuidCollectionItem));

            //ValoreCondition
            RuntimeTypeModel.Default[typeof(ValoreCondition)].AddSubType(1001, typeof(ValoreConditionsGroup));
            RuntimeTypeModel.Default[typeof(ValoreCondition)].AddSubType(1002, typeof(AttributoValoreConditionSingle));

        }

        public static string FileJoinToProjectAsString(string fullFileName)
        {
            //string fileJoin = "D:\\Test\\Join projects\\Main\\incMateriali.join";
            string fileJoin = fullFileName;

            FileInfo fileInfo = new FileInfo(fileJoin);

            if (fileInfo == null)
                return null;

            if (!File.Exists(fileInfo.FullName))
                return null;

            Project project = null;
            Int32 projectVersion = 100;

            using (var file = File.OpenRead(fileInfo.FullName))
            {

                BinaryReader reader = new BinaryReader(file);
                projectVersion = reader.ReadInt32();

                try
                {
                    project = Serializer.Deserialize<Project>(file);

                }
                catch (Exception exc)
                {

                }
            }

            return Serialize(project);
        }


    }


    //static void Main(string[] args)
    //{
    //    var test = new ModelSerializer();
    //    test.FileJoinToProject();
    //}



}



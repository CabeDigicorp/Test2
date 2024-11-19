using Commons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceClient
{

    public static class ModelHelper
    {
        static ModelHelper()
        {
        }

        public static void ValidateModelData()
        {
            ////////////////////////////////////////////////
            ///Rimuovere "return" per validare e leggere gli errori nel Log
            ///Controllare gli ProtoInclude delle l'interfacce Valore e ValoreAttributo
            //Controllare InitRuntimeTypeModel()

            return;
            /////////////////////////////////////////////////



            IEnumerable<Type> types;

            string location = Assembly.GetExecutingAssembly().Location;

            string basePath = Path.GetDirectoryName(location);


            string path = Path.Combine(basePath, "DigiCorp.ModelData.dll");
            Assembly modelDataAss = Assembly.LoadFrom(path);
            IEnumerable<Type> typesModelData = modelDataAss.GetTypes();

            path = Path.Combine(basePath, "DigiCorp.Model.dll");
            Assembly ass = Assembly.LoadFrom(path);
            types = ass.GetTypes();

            path = Path.Combine(basePath, "DigiCorp.MasterDetailModel.dll");
            ass = Assembly.LoadFrom(path);
            types = types.Union(ass.GetTypes());

            types = types.Where(item => item.CustomAttributes.Any(att => att.AttributeType.Name == typeof(ProtoBuf.ProtoContractAttribute).Name));


            foreach (var type in types)
            {
                var targetType = modelDataAss.GetTypes().FirstOrDefault(item => item.Name == type.Name);
                if (targetType == null)
                {
                    //error
                    MainAppLog.Error(MethodBase.GetCurrentMethod(), String.Format("{0} {1}", "Missing class", type.Name));
                    continue;
                }


                foreach (var property in type.GetProperties().Where(m => m.CustomAttributes.Any(att => att.AttributeType.Name == typeof(ProtoBuf.ProtoMemberAttribute).Name)))
                {
                    PropertyInfo[] psInfo = targetType.GetProperties();
                    PropertyInfo pInfo = psInfo.FirstOrDefault(item => item.Name == property.Name);
                    
                    if (pInfo == null)
                    {
                        //error
                        MainAppLog.Error(MethodBase.GetCurrentMethod(), String.Format("{0} {1} {2} {3}", "Missing property", property.Name, "in class", type.Name));
                        continue; 
                    }

                    string pInfoTypeName = pInfo.PropertyType.Name;
                    string propertyTypeName = property.PropertyType.Name;

                    if (pInfoTypeName != propertyTypeName)
                    {
                        MainAppLog.Error(MethodBase.GetCurrentMethod(), String.Format("{0} {1} {2} {3}", "Type property error", property.Name, "in class", type.Name));
                        continue;
                    }

                    CustomAttributeData custAttData = property.CustomAttributes.FirstOrDefault(item => item.AttributeType.Name == typeof(ProtoBuf.ProtoMemberAttribute).Name);
                    int propId = (int) custAttData.ConstructorArguments.FirstOrDefault().Value ;

                    CustomAttributeData custAttData2 = pInfo.CustomAttributes.FirstOrDefault(item => item.AttributeType.Name == typeof(ProtoBuf.ProtoMemberAttribute).Name);
                    int propId2 = (int)custAttData.ConstructorArguments.FirstOrDefault().Value;

                    if (propId != propId2)
                    {
                        MainAppLog.Error(MethodBase.GetCurrentMethod(), String.Format("{0} {1} {2} {3}", "Id property error", property.Name, "in class", type.Name));
                        continue;
                    }

                }

            }


        }

    }
}

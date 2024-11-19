using _3DModelExchange;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [ProtoContract]
    public class Model3dPreferencesData
    {
        [ProtoMember(1)]
        public List<string> lstHiddenEntities { get; set; }  = new List<string>();

        //[ProtoMember(2)]
        //public bool ExcludeFromTabella_A { get; set; }

        //[ProtoMember(3)]
        //public bool MultithreadIsActivated { get; set; } = true;

        //[ProtoMember(4)]
        //public int NumberOfThreads { get; set; }

        //[ProtoMember(5)]
        //public bool TransparencyIsActivated { get; set; } = true;

        //[ProtoMember(6)]
        //public bool PrecViewIsActivated { get; set; } = false;

        //nuovo ProtoMember: 7
    }

    public class Model3dPreferencesDataConverter
    {

        public static Model3dPreferencesData ConvertFromPreferencesData(PreferencesData source)
        {
            Model3dPreferencesData target = new Model3dPreferencesData();
            target.lstHiddenEntities = new List<string>();
            if (source.lstHiddenEntities != null)
                source.lstHiddenEntities.ForEach(item => target.lstHiddenEntities.Add(item));
            //target.ExcludeFromTabella_A = source.ExcludeFromTabella_A;
            //target.MultithreadIsActivated = source.MultithreadIsActivated;
            //target.NumberOfThreads = source.NumberOfThreads;
            //target.TransparencyIsActivated = source.TransparencyIsActivated;
            //target.PrecViewIsActivated = source.PrecViewIsActivated;

            return target;
        }


        public static PreferencesData ConvertToPreferencesData(Model3dPreferencesData source, Int32 projectFileVersion)
        {
            PreferencesData target = new PreferencesData();


            //if (projectFileVersion >= x)
            //{

            //}
            //else
            if (projectFileVersion >= 0)
            {
                target.lstHiddenEntities = new List<string>();
                source.lstHiddenEntities.ForEach(item => target.lstHiddenEntities.Add(item));
                //target.ExcludeFromTabella_A = source.ExcludeFromTabella_A;
                //target.MultithreadIsActivated = source.MultithreadIsActivated;
                //target.NumberOfThreads = source.NumberOfThreads;
                //target.TransparencyIsActivated = source.TransparencyIsActivated;
                //target.PrecViewIsActivated = source.PrecViewIsActivated;
            }
            else
            {
                //versione non supportata
            }
            return target;


        }


    }
}

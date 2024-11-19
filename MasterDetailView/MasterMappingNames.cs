using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailView
{
    public class MasterMappingNames
    {

        int _nMappingNames = 100;//numero massimo di colonne nella griglia
        /// <summary>
        /// key:mappingName, value:Codice attributo
        /// </summary>
        Dictionary<string, string> MappingNames = new Dictionary<string, string>();

        /// <summary>
        /// key:valueMappingName, value:Codice attributo
        /// </summary>
        Dictionary<string, string> ValueMappingNames = new Dictionary<string, string>();

        public MasterMappingNames()
        {
            CreateMappingNames();
            CreateValueMappingNames();
        }

        void CreateMappingNames()
        {
            MappingNames.Clear();

            for (int i = 0; i < _nMappingNames; i++)
            {
                string mappingName = EntityView.GetMasterMappingNameByIndex(i);
                MappingNames.Add(mappingName, string.Empty);
            }
        }

        public string CreateNewMappingName()
        {
            foreach (string key in MappingNames.Keys)
            {
                if (MappingNames[key] == string.Empty)
                    return key;
            }
            return null;
        }

        void CreateValueMappingNames()
        {

            ValueMappingNames.Clear();

            for (int i = 0; i < _nMappingNames; i++)
            {
                string mappingName = EntityView.GetMasterValueMappingNameByIndex(i);
                ValueMappingNames.Add(mappingName, string.Empty);
            }
        }

        public string CreateNewValueMappingName()
        {
            foreach (string key in ValueMappingNames.Keys)
            {
                if (ValueMappingNames[key] == string.Empty)
                    return key;
            }
            return null;
        }

        public void SetValueMappingName(string mappingName, string codiceAttributo)
        {
            ValueMappingNames[mappingName] = codiceAttributo;
        }

        public void SetMappingName(string mappingName, string codiceAttributo)
        {
            MappingNames[mappingName] = codiceAttributo;
        }

        public string GetCodiceByValueMappingName(string valueMappingName)
        {
            if (ValueMappingNames.ContainsKey(valueMappingName))
                return ValueMappingNames[valueMappingName];

            return string.Empty;
        }

        public string GetCodiceByMappingName(string mappingName)
        {
            if (MappingNames.ContainsKey(mappingName))
                return MappingNames[mappingName];

            return string.Empty;
        }

        internal void Clear()
        {
            throw new NotImplementedException();
        }
    }
}

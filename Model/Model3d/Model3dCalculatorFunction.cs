using _3DModelExchange;
using Commons;
using Microsoft.Isam.Esent.Interop.Vista;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ValoreM3dCalculatorFunction : Model3dCalculatorFunction
    {
        IDataService DataService { get; set; }
        public I3DModelService Model3dService { get; set; } = null;

        /// <summary>
        /// valori Mancanti
        /// </summary>
        Dictionary<string, Model3dValue> MissingValues { get; set; } = new Dictionary<string, Model3dValue>();


        /// <summary>
        /// key: "projectGlobalId | globalId | className | valuePath" 
        /// </summary>
        Dictionary<string, Model3dValue> Values { get; set; } = new Dictionary<string, Model3dValue>();

        public ValoreM3dCalculatorFunction(IDataService dataService, string name) : base(name)
        {
            DataService = dataService;
        }

        private string CreateKey(string projectGlobalId, string globalId, string className, string itemPath, string valuePath)
        {
            return string.Format("{0} | {1} | {2} | {3} | {4}", projectGlobalId, globalId, className, itemPath, valuePath);
        }

        private void SplitKey(string key, Model3dValueData valueData)
        {
            string[] strs = key.Split('|');

            valueData.ProjectGlobalId = strs[0];
            valueData.GlobalId = strs[1];
            valueData.ClassName = strs[2];
            valueData.ValuePath = strs[3];
            valueData.ItemPath = strs[4];
        }

        public void Clear()
        {
            Values.Clear();
            MissingValues.Clear();
        }

        public void SetValues(IEnumerable<Model3dValueData> model3dValuesData)
        {

            //Values = model3dValuesData.ToDictionary(item => CreateKey(item.ProjectGlobalId, item.GlobalId, item.ClassName, item.ItemPath, item.ValuePath), item =>
            //{

            //    Model3dClassEnum classEnum = Model3dClassEnum.Nothing;
            //    Enum.TryParse(item.ClassName, out classEnum);

            //    return new Model3dValue()
            //    {
            //        ClassName = classEnum,
            //        GlobalId = item.GlobalId,
            //        ProjectGlobalId = item.ProjectGlobalId,
            //        ItemPath = item.ItemPath,
            //        ValuePath = item.ValuePath,
            //        Value = item.Value,
            //    };
            //});


            Values = new Dictionary<string, Model3dValue>();
            foreach (var item in model3dValuesData)
            {
                string key = CreateKey(item.ProjectGlobalId, item.GlobalId, item.ClassName, item.ItemPath, item.ValuePath);

                if (!Values.ContainsKey(key))
                {

                    Model3dClassEnum classEnum = Model3dClassEnum.Nothing;
                    Enum.TryParse(item.ClassName, out classEnum);

                    var model3dVal = new Model3dValue()
                    {
                        ClassName = classEnum,
                        GlobalId = item.GlobalId,
                        ProjectGlobalId = item.ProjectGlobalId,
                        ItemPath = item.ItemPath,
                        ValuePath = item.ValuePath,
                        Value = item.Value,
                    };

                    AddValue(key, model3dVal);
                    //Values.Add(key, model3dVal);
                }
                else
                {
                    MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), string.Format("{0} già presente nel dizionario", key));
                }

            }

        }


        public IEnumerable<Model3dValueData> GetValues()
        {

            List<Model3dValue> temp = Values.Values.ToList();
            //Model3dValuesData model3dValuesData = new Model3dValuesData();
            //model3dValuesData.Values = temp.Select(item => new Model3dValueData()
            IEnumerable<Model3dValueData> values = temp.Select(item => new Model3dValueData()
            {
                ClassName = item.ClassName.ToString(),
                GlobalId = item.GlobalId,
                ProjectGlobalId = item.ProjectGlobalId,
                ItemPath = item.ItemPath,
                ValuePath = item.ValuePath,
                Value = item.Value,
                Model3dType = item.Model3DType,

            });

            //DataService.SetModel3dValuesData(model3dValuesData);
            return values;


        }


        public void UpdateValuesFromModel3d()
        {
            if (Model3dService == null)
                return;


            //ricavo i valori da aggiornare
            Model3dValues mod3dValues = new Model3dValues();
            foreach (Model3dValue m3dVal in Values.Values)
            {
                //controllo in modo che non cerchi di calcolare per niente valori con il servizio sbagliato
                if (Model3dHelper.GetModel3dType(m3dVal.ClassName) == GetModel3dType())
                {
                    mod3dValues.Values.Add(new Model3dValue()
                    {
                        ClassName = m3dVal.ClassName,
                        GlobalId = m3dVal.GlobalId,
                        ProjectGlobalId = m3dVal.ProjectGlobalId,
                        ValuePath = m3dVal.ValuePath,
                        Model3DType = m3dVal.Model3DType,
                        ItemPath = m3dVal.ItemPath,
                    });
                }
            }


#if !NO_UPDATE_MODEL3D_VALUES
            //Model3dService aggiorna i valori richiesti
            mod3dValues = Model3dService.UpdateModel3dValues(mod3dValues);
#endif

            //update Values (solo quelli che ha trovato nel file ifc)
            foreach (Model3dValue m3dVal in mod3dValues.Values)
            {
                if (m3dVal.Value != null)
                {
                    string key = CreateKey(m3dVal.ProjectGlobalId, m3dVal.GlobalId, m3dVal.ClassName.ToString(),m3dVal.ItemPath, m3dVal.ValuePath);
                    if (Values.ContainsKey(key))
                        Values[key].Value = m3dVal.Value;
                }
                else
                {
                    bool nontrovato = true;
                }

            }



        }

        public override bool Calculate(string projectGuid, string guid, Model3dClassEnum classEnum, string itemPath, string valuePath, out double value)
        {
            bool res = false;
            value = 0.0;

            //controllo in modo che non cerchi di calcolare per niente valori con il servizio sbagliato
            if (classEnum != Model3dClassEnum.Nothing && Model3dHelper.GetModel3dType(classEnum) != GetModel3dType())
                return false;

            NumberFormatInfo formatProvider = new NumberFormatInfo();
            formatProvider.NumberDecimalSeparator = ".";
            formatProvider.NumberGroupSeparator = "";

            string key = CreateKey(projectGuid, guid, classEnum.ToString(), itemPath, valuePath);
            if (Values.ContainsKey(key))
            {
                Model3dValue val = Values[key];
                //res = Double.TryParse(val.Value, out value);

                string valTemp = val.Value;//.Replace(",", ".");
                res = Double.TryParse(valTemp, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, formatProvider, out value);
                
                
                return res;
            }



            Model3dValue missingValue = new Model3dValue()
            {
                ProjectGlobalId = projectGuid,
                GlobalId = guid,
                ClassName = classEnum,
                ItemPath = itemPath,
                ValuePath = valuePath,
                Model3DType = GetModel3dType(),
            };

            if (Model3dService != null)
            {
                //se è presente la sorgente dati model 3d vado a caricare il valore
                Model3dValues updatedvalues = new Model3dValues();
                updatedvalues.Values.Add(missingValue);

#if !NO_UPDATE_MODEL3D_VALUES
                updatedvalues = Model3dService.UpdateModel3dValues(updatedvalues);
#endif

                //res = Double.TryParse(updatedvalues.Values[0].Value, out value);
                string valTemp = updatedvalues.Values[0].Value;//.Replace(",", ".");
                res = Double.TryParse(valTemp, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, formatProvider, out value);

                if (res)
                {
                    missingValue.Value = updatedvalues.Values[0].Value;
                    AddValue(key, missingValue);
                }
                
                return res;
            }
            else if (!MissingValues.ContainsKey(key))
            {
                //AddValue(key, missingValue);//AU 23/10/2020
                MissingValues.Add(key, missingValue);
            }

            return res;
        }

        public override bool Calculate(string projectGuid, string guid, Model3dClassEnum classEnum, string itemPath, string valuePath, out string value)
        {
            bool res = false;
            value = "";

            if (projectGuid == null || !projectGuid.Any())
                return false;

            if (guid == null || !guid.Any())
                return false;


            //controllo in modo che non cerchi di calcolare per niente valori con il servizio sbagliato
            if (classEnum != Model3dClassEnum.Nothing)
            {
                if (Model3dHelper.GetModel3dType(classEnum) != GetModel3dType())
                    return false;
            }


            string key = CreateKey(projectGuid, guid, classEnum.ToString(), itemPath, valuePath);
            if (Values.ContainsKey(key))
            {
                Model3dValue val = Values[key];
                value = val.Value;
                return true;
            }

            Model3dValue missingValue = new Model3dValue()
            {
                ProjectGlobalId = projectGuid,
                GlobalId = guid,
                ClassName = classEnum,
                ValuePath = valuePath,
                ItemPath = itemPath,
                Model3DType = GetModel3dType(),
            };

            if (Model3dService != null)
            {
                //se è presente la sorgente dati model 3d vado a caricare il valore
                Model3dValues updatedvalues = new Model3dValues();
                updatedvalues.Values.Add(missingValue);

#if !NO_UPDATE_MODEL3D_VALUES
                updatedvalues = Model3dService.UpdateModel3dValues(updatedvalues);
#endif

                missingValue.Value = updatedvalues.Values[0].Value;
                AddValue(key, missingValue);

                value = missingValue.Value;
                return true;
            }
            else if (!MissingValues.ContainsKey(key))
            {
                //AddValue(key, missingValue);//AU 23/10/2020
                MissingValues.Add(key, missingValue);
            }

            return res;

        }

        private void AddValue(string key, Model3dValue m3dValue)
        {
            if (!Values.ContainsKey(key))
                Values.Add(key, m3dValue);
        }


    }
}

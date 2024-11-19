
using CommonResources;
using Commons;
using DevExpress.Charts.Designer.Native;
using DevExpress.Mvvm.Native;
using DevExpress.Xpf.Controls;
using FastReport.Data;
using log4net.Core;
using MasterDetailModel;
using Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace MainApp
{
    public class XlsxImport
    {
        //input
        public string EntityTypeKey { get; set; } = null;
        public IDataService DataService { get; set; } = null;
        public MainOperation MainOperation { get; set; } = null;
        public string TreeMasterCodiceSeparator { get; set; } = ".";
        //

        public static string FileExtension { get => "xlsx"; }

        EntityType _entityType { get; set; } = null;
        EntityType _entityTypeParent { get; set; } = null;
        EntitiesHelper _entitiesHelper = null;
        string _etichettaAttributoCodice = string.Empty;


        public bool Run(string fullFilePath)
        {
            if (!Validate(fullFilePath))
                return false;


            MemoryStream jsonStream = new MemoryStream();

            try
            {

                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    //Initialize Application
                    IApplication application = excelEngine.Excel;

                    //Set default version for application
                    application.DefaultVersion = ExcelVersion.Xlsx;

                    //Open a workbook to be export as CSV
                    IWorkbook workbook = application.Workbooks.Open(fullFilePath);

                    //Accessing first worksheet in the workbook
                    IWorksheet worksheet = workbook.Worksheets[0];

                    workbook.SaveAsJson(jsonStream, worksheet);

                    excelEngine.Dispose();
                }

                byte[] json = new byte[jsonStream.Length];

                //Read the JSON stream and convert to a JSON object.
                jsonStream.Position = 0;
                jsonStream.Read(json, 0, (int)jsonStream.Length);

                string jsonString = Encoding.UTF8.GetString(json);
                //string jsonString = Encoding.ASCII.GetString(json);

                AddEntities(jsonString); 
            
            }
            catch (Exception ex)
            {
                MainOperation.ShowMessageBarView(ex.Message);
                return false;
            }

            return true;
        }

        bool Validate(string fullFilePath)
        {
            if (string.IsNullOrEmpty(EntityTypeKey))
                return false;

            if (DataService == null)
                return false;

            if (!Path.Exists(fullFilePath))
                return false;

            if (Path.GetExtension(fullFilePath) != string.Format(".{0}", FileExtension))
                return false;

            _entityType = DataService.GetEntityType(EntityTypeKey);

            if (_entityType == null)
                return false;

            _entitiesHelper = new EntitiesHelper(DataService);

            if (_entityType.IsTreeMaster)
            {
                _entityTypeParent = _entitiesHelper.GetEntityTypeParent(EntityTypeKey);
                if (_entityTypeParent == null)
                    return false;

                //ricavo l'etichetta dell'attributo codice
                Attributo attCodice = _entityTypeParent.Attributi.Values.FirstOrDefault();
                _etichettaAttributoCodice = attCodice?.Etichetta;
            }


            return true;
        }

        private void AddEntities(string jsonString)
        {
            JToken entireJson = JToken.Parse(jsonString);
            JProperty foglioProperty = entireJson.First.Value<JProperty>();
            var foglioPropertyName = foglioProperty.Name;
            JArray items = entireJson[foglioPropertyName].Value<JArray>();

            ModelAction action = new ModelAction()
            {
                EntityTypeKey = this.EntityTypeKey,
                ActionName = ActionName.MULTI,
            };

            int parentLevel = 0;
            List<ModelAction> actionAddByLevel = new List<ModelAction>();

            List<string> codiciRoot = new List<string>();

            foreach (var item in items)
            {

                var itemAtts = new Dictionary<string, string>();

                if (item.First != null)
                {

                    JProperty? itemProperty = item.First.Value<JProperty>();
                    while (itemProperty != null)
                    {
                        string et = itemProperty.Name;
                        string val = itemProperty.Value.ToString();

                        itemAtts.Add(et, val);

                        itemProperty = (JProperty)itemProperty.Next;
                    }

                }

                int level = GetLevel(itemAtts, codiciRoot);

                ModelAction actionAdd = null;
                if (level > 0)
                {
                    actionAdd = new ModelAction()
                    {
                        EntityTypeKey = this.EntityTypeKey,
                        ActionName = ActionName.TREEENTITY_ADD_CHILD,
                    };
                }
                else
                {
                    actionAdd = new ModelAction()
                    {
                        EntityTypeKey = this.EntityTypeKey,
                        ActionName = ActionName.TREEENTITY_ADD,
                    };
                }


                if (SetEntityAttributes(actionAdd, itemAtts))
                {
                    if (level > 0)
                        actionAddByLevel[level - 1].NestedActions.Add(actionAdd);
                    else
                        action.NestedActions.Add(actionAdd);

                    if (actionAddByLevel.Count <= level)
                        actionAddByLevel.Add(actionAdd);
                    else
                        actionAddByLevel[level] = actionAdd;
                }

                parentLevel = level;


            }

            if (!action.NestedActions.Any())
            {
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NessunAttributoCorrispondeAllaPrimaRigaDelFoglioExcel"));
                return;
            }


            var mar = DataService.CommitAction(action);
            if (mar.ActionResponse == ActionResponse.OK)
            {

            }
        }

        bool SetEntityAttributes(ModelAction actionAdd, Dictionary<string, string> itemAtts)
        {
            foreach (var et in itemAtts.Keys)
            {
                var val = itemAtts[et];

                Attributo att = _entityType.Attributi.Values.FirstOrDefault(item => item.Etichetta == et);
                if (att == null)
                    continue;

                if (att is AttributoRiferimento)
                    continue;


                if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = this.EntityTypeKey, AttributoCode = att.Codice, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    actionAttMod.NewValore = new ValoreTesto() { V = val };
                    actionAdd.NestedActions.Add(actionAttMod);
                }
                else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoRTF)
                {
                    string descRtf = string.Empty;
                    //string str = ReplaceSpecialChars(art.DesEstesa);
                    ValoreHelper.RtfFromPlainString(val, out descRtf);

                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = this.EntityTypeKey, AttributoCode = att.Codice, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    actionAttMod.NewValore = new ValoreTestoRtf() { V = descRtf };
                    actionAdd.NestedActions.Add(actionAttMod);
                }
                else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = this.EntityTypeKey, AttributoCode = att.Codice, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    actionAttMod.NewValore = new ValoreReale() { V = val };
                    actionAdd.NestedActions.Add(actionAttMod);
                }
                else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = this.EntityTypeKey, AttributoCode = att.Codice, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    actionAttMod.NewValore = new ValoreContabilita() { V = val };
                    actionAdd.NestedActions.Add(actionAttMod);
                }
                else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = this.EntityTypeKey, AttributoCode = att.Codice, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    actionAttMod.NewValore = new ValoreGuid() { V = new Guid(val) };
                    actionAdd.NestedActions.Add(actionAttMod);
                }
            }

            if (!actionAdd.NestedActions.Any())
                return false;

            return true;
        }


        int GetLevel(Dictionary<string, string> itemAtts, List<string> codiciRoot)
        {
            if (string.IsNullOrEmpty(_etichettaAttributoCodice))
                return 0;

            int level = 0;
            string codice = string.Empty;
            itemAtts.TryGetValue(_etichettaAttributoCodice, out codice);

            if (string.IsNullOrEmpty(codice))
                return 0;

            for (int i = codiciRoot.Count-1; i>=0;i--)
            {
                string codiceRoot = codiciRoot[i];
                if (codice.StartsWith(codiceRoot))
                {
                    level = i + 1;
                    break;
                }
            }

            for (int i = codiciRoot.Count - 1; i >= level; i--)
                codiciRoot.RemoveAt(i);
                        

            if (codiciRoot.Count <= level)
                codiciRoot.Add(codice);
            else
                codiciRoot[level] = codice;

            
            return level;
        }

    }
}

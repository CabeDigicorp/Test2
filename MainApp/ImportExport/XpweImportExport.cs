using _3DModelExchange;
using CommonResources;
using Commons;
using DevExpress.Charts.Designer.Native;
using DevExpress.CodeParser;
using DevExpress.Data.Svg;
using log4net;
using MasterDetailModel;
using MasterDetailView;
using Microsoft.Win32;
using Model;
using Newtonsoft.Json;
using NuGet.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;

using JsonSerializer = Commons.JsonSerializer;

namespace MainApp
{
    public class XpweImportExport
    {
        public IMainOperation MainOperation { get; set; } = null;
        public IEntityWindowService WindowService { get; set; } = null;
        
        IDataService DataService { get; set; } = null;
        PweDocumento Xpwe { get; set; } = null;
        public static string FileExtension { get => "xpwe"; }
        string _modelloFileName { get => "- Import xpwe.join"; }
        //string _modelloFileName { get => "- Import xpwe WBS.join"; }

        NumberFormatInfo _formatProvider = new NumberFormatInfo();
        EntitiesHelper _entitiesHelper = null;

        ///// <summary>
        ///// key: capitolo id mosaico
        ///// value: indice di filteredEntitiesId
        ///// </summary>
        //Dictionary<string, int> _capitoliIndexById = new Dictionary<string, int>();
        //List<Guid> _capitoliItemsId = null;
        //int _capitoliCount { get; set; } = 0;

        /// <summary>
        /// Key: ISO-8859-1 Value: character (esempio key: &#128; value:€)
        /// </summary>
        Dictionary<string, string> _specialChars = new Dictionary<string, string>();

        Dictionary<string, Guid> _superCategorieIdsMap = new Dictionary<string, Guid>();
        Dictionary<string, Guid> _categorieIdsMap = new Dictionary<string, Guid>();
        Dictionary<string, Guid> _subCategorieIdsMap = new Dictionary<string, Guid>();

        Dictionary<string, Guid> _superCapitoliIdsMap = new Dictionary<string, Guid>();
        Dictionary<string, Guid> _capitoliIdsMap = new Dictionary<string, Guid>();
        Dictionary<string, Guid> _subCapitoliIdsMap = new Dictionary<string, Guid>();

        Dictionary<string, Guid> _articoliIdsMap = new Dictionary<string, Guid>();

        Dictionary<VcRgID, Guid> _vociComputoIdsMap = new Dictionary<VcRgID, Guid>();

        Dictionary<string, Guid> _vociWBSIdsMap = new Dictionary<string, Guid>();

        //key: Codice, Value: ID (mappa codice -> id nella classe DGWBSItem
        Dictionary<string, string> _pweVociWbsCodice_ID = new Dictionary<string, string>();

        //registrazioni che fanno riferimento a 
        List<VcRgID> _rgVV = new List<VcRgID>();


        public XpweImportExport()
        {


        }

        void Clear()
        {
            DataService = null;
            Xpwe = null;
            //_capitoliCount = 0;
            //_capitoliIndexById.Clear();
            _specialChars.Clear();
            _articoliIdsMap.Clear();
            _superCategorieIdsMap.Clear();
            _categorieIdsMap.Clear();
            _subCategorieIdsMap.Clear();
            _vociWBSIdsMap.Clear();
            _vociComputoIdsMap.Clear();
            _pweVociWbsCodice_ID.Clear();

            _formatProvider.NumberDecimalSeparator = ".";
            _formatProvider.NumberGroupSeparator = "";
        }

        //internal async Task RunImport(ClientDataService dataService)
        //{
        //    Clear();
        //    LoadSpecialChars();

        //    DataService = dataService;
        //    _entitiesHelper = new EntitiesHelper(dataService);

        //    string fullFileName = string.Empty;

        //    if (!ValidateTargetProject())
        //        return;


        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.DefaultExt = FileExtension;
        //    openFileDialog.Filter = string.Format("{0} files (*.{1})|*.{2}|All files (*.*)|*.*", FileExtension, FileExtension, FileExtension);
        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        fullFileName = openFileDialog.FileName;
        //        DeserializeObject(fullFileName);
        //        bool res = await Run();

        //        if (res)
        //            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione terminata correttamente"), true, 100);
        //        else
        //            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Errore durante l'importazione"), true, 100);

        //    }


        //}

        internal async Task RunImport(ClientDataService dataService, string fullFileName)
        {
            Clear();
            LoadSpecialChars();

            DataService = dataService;
            _entitiesHelper = new EntitiesHelper(dataService);

            DeserializeObject(fullFileName);

            bool res = await Run();

            if (res)
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione terminata correttamente"), true, 100);
            else
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Errore durante l'importazione"), true, 100);
        }

        public string GetModelloFullFileName()
        {
            string fullFileName = string.Format("{0}\\{1}", MainOperation.GetModelliFolder(), _modelloFileName);
            return fullFileName;
        }


        private void DeserializeObject(string filename)
        {
            try
            {

                Console.WriteLine("Reading with Stream");
                // Create an instance of the XmlSerializer.
                XmlSerializer serializer = new XmlSerializer(typeof(PweDocumento));

                // Declare an object variable of the type to be deserialized.
                using (Stream reader = new FileStream(filename, FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    Xpwe = (PweDocumento)serializer.Deserialize(reader);
                }
            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }

        }

        private async Task<bool> Run()
        {
            bool res = await Task.Run(async () =>
            {

                //bool ok = await ImportModelloXpwe();
                //if (!ok)
                //    return false;

                if (DataService == null)
                    return false;

                if (Xpwe == null)
                    return false;


                if (!AddConfigurazione())
                    return false;

                if (!AddDivisioni())
                    return false;

                if (!AddCapitoli())
                    return false;

                if (!AddArticoli())
                    return false;

                if (!AddVociWBS())
                    return false;

                if (!AddVociComputo())
                    return false;

                //if (!AddVociWBSInWBS())
                //    return false;

                return true;
            });

            return res;
        }





        //private async Task<bool> ImportModelloXpwe()//0->8
        //{
        //    double barDelta = 8;
        //    MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione modello..."), false, 0);

        //    string fullFileName = string.Format("{0}\\{1}", MainOperation.GetModelliFolder(), _modelloFileName);

        //    string modelloFileName = Path.GetFileNameWithoutExtension(_modelloFileName);

        //    //scaricamento o eventuale aggiornamento del modello
        //    var projectModelView = new ProjectModelView();
        //    projectModelView.WindowService = WindowService;
        //    projectModelView.MainOperation = MainOperation;
        //    await projectModelView.LoadAsync();

        //    //scarico il modello se non esiste in locale o lo aggiorno se è da aggiornare
        //    if (!File.Exists(fullFileName) || projectModelView.IsUpdateAvailable(modelloFileName))
        //    {

        //        bool ok = await projectModelView.DownloadModelloAsync(Path.GetFileNameWithoutExtension(_modelloFileName));

        //        if (!ok)
        //        {
        //            MainOperation.ShowMessageBarView(string.Format("{0} {1}", LocalizationProvider.GetString("Errore nello scaricamento del modello"), modelloFileName));
        //            return false;
        //        }
        //    }

        //    IDataService ds = MainOperation.GetDataServiceByFile(fullFileName, out _);

        //    if (ds == null)
        //    {
        //        MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Errore nell' apertura del modello"));
        //        return false;
        //    }

        //    MainOperation.ImportModel(ds);


        //    return true;

        //}

        #region Configurazione
        private bool AddConfigurazione()//0->10
        {
            if (Xpwe.DatiGenerali == null)
                return false;

            if (Xpwe.DatiGenerali.DGConfigurazione == null)
                return false;

            double barDelta = 2;
            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione configurazione..."), false, 0);

            if (Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri == null)
                return false;

            EntityType prezType = DataService.GetEntityType(BuiltInCodes.EntityType.Prezzario);
            EntityType prezTypeNew = prezType.Clone();



            if (!string.IsNullOrEmpty(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.Prezzi))
            {
                int nDec = 0;
                bool useThousandSeparator = false;
                DecomposeConfigNumero(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.Prezzi, out nDec, out useThousandSeparator);

                Attributo att = null;

                //prezzo
                if (prezTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.Prezzo, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);

                //prezzo2
                if (prezTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.Prezzo2, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);

                //prezzo3
                if (prezTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.Prezzo3, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);

                //prezzo4
                if (prezTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.Prezzo4, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);

                //prezzo5
                if (prezTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.Prezzo5, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);

                //PrezzoManodopera
                if (prezTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.PrezzoManodopera, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);

                //PrezzoMateriali
                if (prezTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.PrezzoMateriali, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);

                //PrezzoNoli
                if (prezTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.PrezzoNoli, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);

                //PrezzoSicurezza
                if (prezTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.PrezzoSicurezza, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);

                //PrezzoTrasporti
                if (prezTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.PrezzoTrasporti, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);


            }
            DataService.SetEntityType(prezTypeNew, false);


            EntityType computoType = DataService.GetEntityType(BuiltInCodes.EntityType.Computo);
            EntityType computoTypeNew = computoType.Clone();

            if (!string.IsNullOrEmpty(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.Lunghezza))
            {
                int nDec = 0;
                bool useThousandSeparator = false;
                DecomposeConfigNumero(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.Lunghezza, out nDec, out useThousandSeparator);

                Attributo att = null;
                
                if (computoTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.Lunghezza, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);
            }

            if (!string.IsNullOrEmpty(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.Larghezza))
            {
                int nDec = 0;
                bool useThousandSeparator = false;
                DecomposeConfigNumero(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.Larghezza, out nDec, out useThousandSeparator);

                Attributo att = null;

                if (computoTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.Larghezza, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);
            }

            if (!string.IsNullOrEmpty(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.HPeso))
            {
                int nDec = 0;
                bool useThousandSeparator = false;
                DecomposeConfigNumero(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.HPeso, out nDec, out useThousandSeparator);

                Attributo att = null;

                if (computoTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.Altezza, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);
            }

            if (!string.IsNullOrEmpty(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.PartiUguali))
            {
                int nDec = 0;
                bool useThousandSeparator = false;
                DecomposeConfigNumero(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.PartiUguali, out nDec, out useThousandSeparator);

                Attributo att = null;

                if (computoTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.PU, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);
            }

            if (!string.IsNullOrEmpty(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.Quantita))
            {
                int nDec = 0;
                bool useThousandSeparator = false;
                DecomposeConfigNumero(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.Quantita, out nDec, out useThousandSeparator);

                Attributo att = null;

                if (computoTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.QuantitaTotale, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);
            }


            if (!string.IsNullOrEmpty(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.PrezziTotale))//importi
            {
                int nDec = 0;
                bool useThousandSeparator = false;
                DecomposeConfigNumero(Xpwe.DatiGenerali.DGConfigurazione.DGConfigNumeri.PrezziTotale, out nDec, out useThousandSeparator);


                Attributo att = null;

                //importo
                if (computoTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.Importo, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);

                //importo manodopera
                if (computoTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.ImportoManodopera, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);

                //importo sicurezza
                if (computoTypeNew.Attributi.TryGetValue(BuiltInCodes.Attributo.ImportoSicurezza, out att))
                    SetAttributoFormat(att, nDec, useThousandSeparator);
            }


            DataService.SetEntityType(computoTypeNew, false);

            return true;
        }

        void DecomposeConfigNumero(string confNumero, out int nDec, out bool useThousandSeparator)
        {
            string[] splitted = confNumero.Split("|");

            string str = splitted[0].Last().ToString();
            int.TryParse(str, out nDec);

            str = splitted[1].Last().ToString();
            int thSep = 0;
            int.TryParse(str, out thSep);
            useThousandSeparator = (thSep == 1);
        }

        void SetAttributoFormat(Attributo att, int nDec, bool useThousandSeparator)
        {
            if (att == null)
                return;


            var numberFormat = NumericFormatHelper.DecomposeFormat(att.ValoreFormat);
            numberFormat.UseThousandSeparator = useThousandSeparator;
            numberFormat.DecimalDigitCount = nDec;
            att.ValoreFormat = NumericFormatHelper.ComposeFormat(numberFormat);

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
            {
                if (att.ValoreAttributo == null)
                    att.ValoreAttributo = new ValoreAttributoReale() { UseSignificantDigitsByFormat = true };
                else
                    (att.ValoreAttributo as ValoreAttributoReale).UseSignificantDigitsByFormat = true;
            }

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
            {
                if (att.ValoreAttributo == null)
                    att.ValoreAttributo = new ValoreAttributoContabilita() { UseSignificantDigitsByFormat = true };
                else
                    (att.ValoreAttributo as ValoreAttributoContabilita).UseSignificantDigitsByFormat = true;
            }

        }



        #endregion


        #region Divisioni
        private bool AddDivisioni()//10->20
        {
            if (Xpwe.DatiGenerali == null)
                return false;

            double barDelta = 10;
            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione categorie..."), false, 10);

            if (Xpwe.DatiGenerali.DGCapitoliCategorie == null)
                return false;

            var entTypes = DataService.GetEntityTypes();

            if (Xpwe.DatiGenerali.DGCapitoliCategorie.DGSuperCategorie != null)
            {
                DivisioneItemType divType = entTypes.Values.Where(item => item is DivisioneItemType).FirstOrDefault(item => item.Codice == BuiltInCodes.Attributo.Categorie1) as DivisioneItemType;

                if (divType != null)
                {
                    ModelAction action = new ModelAction()
                    {
                        EntityTypeKey = divType.GetKey(),
                        ActionName = ActionName.MULTI,
                    };

                    int i = 0;
                    List<string> supCatIds = new List<string>();

                    foreach (var item in Xpwe.DatiGenerali.DGCapitoliCategorie.DGSuperCategorie.DGSuperCategorieItem)
                    {
                        ModelAction actionAdd = new ModelAction()
                        {
                            EntityTypeKey = divType.GetKey(),
                            ActionName = ActionName.TREEENTITY_ADD,
                        };

                        if (AddDivisioneItem(new CapitoliCategorieItemHelper(item), actionAdd))
                        {
                            action.NestedActions.Add(actionAdd);
                            supCatIds.Add(item.ID);
                            i++;
                        }
                    }

                    ModelActionResponse mar = DataService.CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        //creo la mappa degli id (ID -> guid)
                        var newIds = mar.NewIds.ToList();
                        for (i = 0; i < supCatIds.Count; i++)
                        {
                            _superCategorieIdsMap.Add(supCatIds[i], newIds[i]);
                        }

                    }
                }

            }

            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione categorie..."), false, 20);

            if (Xpwe.DatiGenerali.DGCapitoliCategorie.DGCategorie != null)
            {
                DivisioneItemType divType = entTypes.Values.Where(item => item is DivisioneItemType).FirstOrDefault(item => item.Codice == BuiltInCodes.Attributo.Categorie2) as DivisioneItemType;

                if (divType != null)
                {
                    ModelAction action = new ModelAction()
                    {
                        EntityTypeKey = divType.GetKey(),
                        ActionName = ActionName.MULTI,
                    };

                    List<string> catIds = new List<string>();
                    int i = 0;

                    foreach (var item in Xpwe.DatiGenerali.DGCapitoliCategorie.DGCategorie.DGCategorieItem)
                    {
                        ModelAction actionAdd = new ModelAction()
                        {
                            EntityTypeKey = divType.GetKey(),
                            ActionName = ActionName.TREEENTITY_ADD,
                        };

                        if (AddDivisioneItem(new CapitoliCategorieItemHelper(item), actionAdd))
                        {
                            action.NestedActions.Add(actionAdd);
                            catIds.Add(item.ID);
                            i++;
                        }

                    }

                    ModelActionResponse mar = DataService.CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        //creo la mappa degli id (ID -> guid)
                        var newIds = mar.NewIds.ToList();
                        for (i = 0; i < catIds.Count; i++)
                        {
                            _categorieIdsMap.Add(catIds[i], newIds[i]);
                        }
                    }
                }

            }

            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione categorie..."), false, 30);

            if (Xpwe.DatiGenerali.DGCapitoliCategorie.DGSubCategorie != null)
            {
                DivisioneItemType divType = entTypes.Values.Where(item => item is DivisioneItemType).FirstOrDefault(item => item.Codice == BuiltInCodes.Attributo.Categorie3) as DivisioneItemType;

                if (divType != null)
                {
                    ModelAction action = new ModelAction()
                    {
                        EntityTypeKey = divType.GetKey(),
                        ActionName = ActionName.MULTI,
                    };
                    List<string> subCatIds = new List<string>();
                    int i = 0;

                    foreach (var item in Xpwe.DatiGenerali.DGCapitoliCategorie.DGSubCategorie.DGSubCategorieItem)
                    {
                        ModelAction actionAdd = new ModelAction()
                        {
                            EntityTypeKey = divType.GetKey(),
                            ActionName = ActionName.TREEENTITY_ADD,
                        };

                        if (AddDivisioneItem(new CapitoliCategorieItemHelper(item), actionAdd))
                        {
                            action.NestedActions.Add(actionAdd);
                            subCatIds.Add(item.ID);
                            i++;
                        }

                    }

                    ModelActionResponse mar = DataService.CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        //creo la mappa degli id (ID -> guid)
                        var newIds = mar.NewIds.ToList();
                        for (i = 0; i < subCatIds.Count; i++)
                        {
                            _subCategorieIdsMap.Add(subCatIds[i], newIds[i]);
                        }
                    }
                }

            }

            return true;
        }

        private bool AddDivisioneItem(CapitoliCategorieItemHelper item, ModelAction actionAdd)
        {
            try
            {
                EntityType divType = DataService.GetEntityType(actionAdd.EntityTypeKey);

                //Codice
                if (!string.IsNullOrEmpty(item.Codice))
                {
                    ModelAction actionCodice = new ModelAction() { EntityTypeKey = divType.GetKey(), AttributoCode = BuiltInCodes.Attributo.Nome, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    string codice = ReplaceSpecialChars(item.Codice);
                    actionCodice.NewValore = new ValoreTesto() { V = codice };
                    actionAdd.NestedActions.Add(actionCodice);
                }

                ////Descrizione
                //ModelAction actionDesc = new ModelAction() { EntityTypeKey = divTypeKey, AttributoCode = BuiltInCodes.Attributo.DescrizioneRTF, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                //string rtf = string.Empty;


                //string descSint = ReplaceSpecialChars(item.DesSintetica);
                //string descEstesa = ReplaceSpecialChars(item.DesEstesa);
                //string desc = string.Format("{0}\n{1}", descSint, descEstesa);

                //ValoreHelper.RtfFromPlainString(desc, out rtf);
                //actionDesc.NewValore = new ValoreTestoRtf() { V = rtf };
                //actionAdd.NestedActions.Add(actionDesc);


                //DescrizioneRTF
                if (!string.IsNullOrEmpty(item.DesEstesa))
                {
                    //Descrizione
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = divType.GetKey(), AttributoCode = BuiltInCodes.Attributo.DescrizioneRTF, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    string rtf = string.Empty;
                    string str = ReplaceSpecialChars(item.DesEstesa);

                    ValoreHelper.RtfFromPlainString(str, out rtf);
                    actionAttMod.NewValore = new ValoreTestoRtf() { V = rtf };
                    actionAdd.NestedActions.Add(actionAttMod);
                }
                //else if (!string.IsNullOrEmpty(item.DesSintetica)) //se desEstesa è vuota metto la desSintetica in DescrizioneRTF
                //{
                //    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = divType.GetKey(), AttributoCode = BuiltInCodes.Attributo.DescrizioneRTF, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                //    string rtf = string.Empty;
                //    string str = ReplaceSpecialChars(item.DesSintetica);

                //    ValoreHelper.RtfFromPlainString(str, out rtf);
                //    actionAttMod.NewValore = new ValoreTestoRtf() { V = rtf };
                //    actionAdd.NestedActions.Add(actionAttMod);
                //}

                //DesSintetica
                if (!string.IsNullOrEmpty(item.DesSintetica) && divType.Attributi.ContainsKey(BuiltInCodes.Attributo.Descrizione))
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = divType.GetKey(), AttributoCode = BuiltInCodes.Attributo.Descrizione, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    string str = ReplaceSpecialChars(item.DesSintetica);
                    actionAttMod.NewValore = new ValoreTesto() { V = str };
                    actionAdd.NestedActions.Add(actionAttMod);
                }

                //CodFase
                if (!string.IsNullOrEmpty(item.CodFase) && divType.Attributi.ContainsKey(BuiltInCodes.Attributo.CodFase))
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = divType.GetKey(), AttributoCode = BuiltInCodes.Attributo.CodFase, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    string str = ReplaceSpecialChars(item.CodFase);
                    actionAttMod.NewValore = new ValoreTesto() { V = str };
                    actionAdd.NestedActions.Add(actionAttMod);
                }


            }
            catch (Exception exc)
            {
                return false;
            }
            return true;

        }

        private bool AddDivisioneItem(WBSItemHelper item, ModelAction actionAdd, bool isParent)
        {
            try
            {
                EntityType divType = DataService.GetEntityType(actionAdd.EntityTypeKey);


                if (string.IsNullOrEmpty(item.Codice))
                    return false;


                //Codice2 - concateno al Codice
                string codiceExt = string.Empty;
                if (!isParent && !string.IsNullOrEmpty(item.CodiceExt) && divType.Attributi.ContainsKey(BuiltInCodes.Attributo.Codice2))
                {
                    codiceExt = ReplaceSpecialChars(item.CodiceExt);
                }


                //Codice
                ModelAction actionCodice = new ModelAction() { EntityTypeKey = divType.GetKey(), AttributoCode = BuiltInCodes.Attributo.Nome, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                string codice = ReplaceSpecialChars(item.Codice);
                actionCodice.NewValore = new ValoreTesto() { V = string.Format("{0} {1}", codice, codiceExt).Trim() };
                actionAdd.NestedActions.Add(actionCodice);

                //Codice2
                //if (!isParent && !string.IsNullOrEmpty(item.CodiceExt) && divType.Attributi.ContainsKey(BuiltInCodes.Attributo.Codice2))
                //{
                //    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = divType.GetKey(), AttributoCode = BuiltInCodes.Attributo.Codice2, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                //    string str = ReplaceSpecialChars(item.CodiceExt);
                //    actionAttMod.NewValore = new ValoreTesto() { V = str };
                //    actionAdd.NestedActions.Add(actionAttMod);
                //}


                ////DesSintetica
                //if (!string.IsNullOrEmpty(item.Titolo) && divType.Attributi.ContainsKey(BuiltInCodes.Attributo.DesSintetica))
                //{
                //    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = divType.GetKey(), AttributoCode = BuiltInCodes.Attributo.DesSintetica, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                //    string str = ReplaceSpecialChars(item.Titolo);
                //    actionAttMod.NewValore = new ValoreTesto() { V = str };
                //    actionAdd.NestedActions.Add(actionAttMod);
                //}

                //Descrizione
                if (!string.IsNullOrEmpty(item.Titolo) && divType.Attributi.ContainsKey(BuiltInCodes.Attributo.Descrizione))
                {
                    //Descrizione
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = divType.GetKey(), AttributoCode = BuiltInCodes.Attributo.Descrizione, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    string str = ReplaceSpecialChars(item.Titolo);

                    actionAttMod.NewValore = new ValoreTesto() { V = str };
                    actionAdd.NestedActions.Add(actionAttMod);
                }


            }
            catch (Exception exc)
            {
                return false;
            }
            return true;

        }
        #endregion

        #region Capitoli
        private bool AddCapitoli()//20->30
        {
            if (Xpwe.DatiGenerali == null)
                return false;

            if (Xpwe.DatiGenerali.DGCapitoliCategorie == null)
                return true;

            double barDelta = 10;
            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione capitoli..."), false, 20);

            var entTypes = DataService.GetEntityTypes();

            if (Xpwe.DatiGenerali.DGCapitoliCategorie.DGSuperCapitoli != null)
            {
                DivisioneItemType divType = entTypes.Values.Where(item => item is DivisioneItemType).FirstOrDefault(item => item.Codice == BuiltInCodes.Attributo.Capitoli1) as DivisioneItemType;

                if (divType != null)
                {
                    ModelAction action = new ModelAction()
                    {
                        EntityTypeKey = divType.GetKey(),
                        ActionName = ActionName.MULTI,
                    };

                    int i = 0;
                    List<string> supCapIds = new List<string>();

                    foreach (var item in Xpwe.DatiGenerali.DGCapitoliCategorie.DGSuperCapitoli.DGSuperCapitoliItem)
                    {
                        ModelAction actionAdd = new ModelAction()
                        {
                            EntityTypeKey = divType.GetKey(),
                            ActionName = ActionName.TREEENTITY_ADD,
                        };

                        if (AddDivisioneItem(new CapitoliCategorieItemHelper(item), actionAdd))
                        {
                            action.NestedActions.Add(actionAdd);
                            supCapIds.Add(item.ID);
                            i++;
                        }
                    }

                    ModelActionResponse mar = DataService.CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        //creo la mappa degli id (ID -> guid)
                        var newIds = mar.NewIds.ToList();
                        for (i = 0; i < supCapIds.Count; i++)
                        {
                            _superCapitoliIdsMap.Add(supCapIds[i], newIds[i]);
                        }

                    }
                }

            }

            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione capitoli..."), false, 20);

            if (Xpwe.DatiGenerali.DGCapitoliCategorie.DGCapitoli != null)
            {
                DivisioneItemType divType = entTypes.Values.Where(item => item is DivisioneItemType).FirstOrDefault(item => item.Codice == BuiltInCodes.Attributo.Capitoli2) as DivisioneItemType;

                if (divType != null)
                {
                    ModelAction action = new ModelAction()
                    {
                        EntityTypeKey = divType.GetKey(),
                        ActionName = ActionName.MULTI,
                    };

                    List<string> capIds = new List<string>();
                    int i = 0;

                    foreach (var item in Xpwe.DatiGenerali.DGCapitoliCategorie.DGCapitoli.DGCapitoliItem)
                    {
                        ModelAction actionAdd = new ModelAction()
                        {
                            EntityTypeKey = divType.GetKey(),
                            ActionName = ActionName.TREEENTITY_ADD,
                        };

                        if (AddDivisioneItem(new CapitoliCategorieItemHelper(item), actionAdd))
                        {
                            action.NestedActions.Add(actionAdd);
                            capIds.Add(item.ID);
                            i++;
                        }

                    }

                    ModelActionResponse mar = DataService.CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        //creo la mappa degli id (ID -> guid)
                        var newIds = mar.NewIds.ToList();
                        for (i = 0; i < capIds.Count; i++)
                        {
                            _capitoliIdsMap.Add(capIds[i], newIds[i]);
                        }
                    }
                }

            }

            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione capitoli..."), false, 30);

            if (Xpwe.DatiGenerali.DGCapitoliCategorie.DGSubCapitoli != null)
            {
                DivisioneItemType divType = entTypes.Values.Where(item => item is DivisioneItemType).FirstOrDefault(item => item.Codice == BuiltInCodes.Attributo.Capitoli3) as DivisioneItemType;

                if (divType != null)
                {
                    ModelAction action = new ModelAction()
                    {
                        EntityTypeKey = divType.GetKey(),
                        ActionName = ActionName.MULTI,
                    };
                    List<string> subCapIds = new List<string>();
                    int i = 0;

                    foreach (var item in Xpwe.DatiGenerali.DGCapitoliCategorie.DGSubCapitoli.DGSubCapitoliItem)
                    {
                        ModelAction actionAdd = new ModelAction()
                        {
                            EntityTypeKey = divType.GetKey(),
                            ActionName = ActionName.TREEENTITY_ADD,
                        };

                        if (AddDivisioneItem(new CapitoliCategorieItemHelper(item), actionAdd))
                        {
                            action.NestedActions.Add(actionAdd);
                            subCapIds.Add(item.ID);
                            i++;
                        }

                    }

                    ModelActionResponse mar = DataService.CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        //creo la mappa degli id (ID -> guid)
                        var newIds = mar.NewIds.ToList();
                        for (i = 0; i < subCapIds.Count; i++)
                        {
                            _subCapitoliIdsMap.Add(subCapIds[i], newIds[i]);
                        }
                    }
                }

            }
            return true;
        }

        private bool AddCapitoliItem(CapitoliCategorieItemHelper item, ModelAction actionAdd)
        {
            return AddDivisioneItem(item, actionAdd);
        }
        #endregion

        #region Articoli
        private bool AddArticoli()//30->60
        {

            if (Xpwe.Misurazioni.ElencoPrezzi == null)
                return true;

            double barDelta = 30;
            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione articoli..."), false, 30);

            int count = Xpwe.Misurazioni.ElencoPrezzi.EPItem.Count;

            ModelAction action = new ModelAction()
            {
                EntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                ActionName = ActionName.MULTI,
            };

            List<string> pweArtIDs = new List<string>();
            int i = 0;
            foreach (EPItem art in Xpwe.Misurazioni.ElencoPrezzi.EPItem)
            {
                ModelAction actionAdd = new ModelAction()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                    ActionName = ActionName.TREEENTITY_ADD,
                };

                if (AddArticolo(art, actionAdd))
                {
                    pweArtIDs.Add(art.ID);

                    action.NestedActions.Add(actionAdd);
                    i++;
                }
            }

            var mar = DataService.CommitAction(action);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                //creo la mappa degli id (ID -> guid)  degli articoli
                _articoliIdsMap.Clear();
                var newIds = mar.NewIds.ToList();
                for (i = 0; i < pweArtIDs.Count; i++)
                {
                    _articoliIdsMap.Add(pweArtIDs[i], newIds[i]);
                }
            }

            return true;
        }

        private bool AddArticolo(EPItem art, ModelAction actionAdd)
        {
            try
            {
                EntityType prezzarioType = DataService.GetEntityType(BuiltInCodes.EntityType.Prezzario);

                //Codice
                if (!string.IsNullOrEmpty(art.Tariffa))
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.Codice, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    actionAttMod.NewValore = new ValoreTesto() { V = art.Tariffa };
                    actionAdd.NestedActions.Add(actionAttMod);
                }

                //DescrizioneRTF
                if (!string.IsNullOrEmpty(art.DesEstesa))
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.DescrizioneRTF, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    string descRtf = string.Empty;
                    string str = ReplaceSpecialChars(art.DesEstesa);

                    ValoreHelper.RtfFromPlainString(str, out descRtf);
                    actionAttMod.NewValore = new ValoreTestoRtf() { V = descRtf };
                    actionAdd.NestedActions.Add(actionAttMod);
                }
                else if (!string.IsNullOrEmpty(art.DesRidotta)) //se desEstesa è vuota metto la DesRidotta in DescrizioneRTF
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = prezzarioType.GetKey(), AttributoCode = BuiltInCodes.Attributo.DescrizioneRTF, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    string rtf = string.Empty;
                    string str = ReplaceSpecialChars(art.DesRidotta);

                    ValoreHelper.RtfFromPlainString(str, out rtf);
                    actionAttMod.NewValore = new ValoreTestoRtf() { V = rtf };
                    actionAdd.NestedActions.Add(actionAttMod);
                }

                //DesRidotta
                if (!string.IsNullOrEmpty(art.DesRidotta) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.DesRidotta))
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.DesRidotta, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    string str = ReplaceSpecialChars(art.DesRidotta);
                    actionAttMod.NewValore = new ValoreTesto() { V = str };
                    actionAdd.NestedActions.Add(actionAttMod);
                }

                //DesBreve
                if (!string.IsNullOrEmpty(art.DesBreve) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.DesBreve))
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.DesBreve, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    string str = ReplaceSpecialChars(art.DesBreve);
                    actionAttMod.NewValore = new ValoreTesto() { V = str };
                    actionAdd.NestedActions.Add(actionAttMod);
                }


                //Unità misura
                if (!string.IsNullOrEmpty(art.UnMisura))
                {
                    ModelAction actionUM = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.UnitaMisura, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    string str = ReplaceSpecialChars(art.UnMisura);
                    actionUM.NewValore = new ValoreTesto() { V = str };
                    actionAdd.NestedActions.Add(actionUM);
                }

                //Prezzo (1)
                if (!string.IsNullOrEmpty(art.Prezzo1))
                {
                    string prezzo1 = Regex.Replace(art.Prezzo1, @"\s", "");

                    ModelAction actionPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.Prezzo, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    actionPrezzo.NewValore = new ValoreContabilita() { V = prezzo1 };
                    actionAdd.NestedActions.Add(actionPrezzo);
                }

                //Prezzo2
                if (!string.IsNullOrEmpty(art.Prezzo2) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.Prezzo2))
                {
                    string prezzo2 = Regex.Replace(art.Prezzo2, @"\s", "");

                    ModelAction actionPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.Prezzo2, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    actionPrezzo.NewValore = new ValoreContabilita() { V = prezzo2 };
                    actionAdd.NestedActions.Add(actionPrezzo);
                }

                //Prezzo3
                if (!string.IsNullOrEmpty(art.Prezzo3) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.Prezzo3))
                {
                    string prezzo3 = Regex.Replace(art.Prezzo3, @"\s", "");

                    ModelAction actionPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.Prezzo3, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    actionPrezzo.NewValore = new ValoreContabilita() { V = prezzo3 };
                    actionAdd.NestedActions.Add(actionPrezzo);
                }

                //Prezzo4
                if (!string.IsNullOrEmpty(art.Prezzo4) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.Prezzo4))
                {
                    string prezzo4 = Regex.Replace(art.Prezzo4, @"\s", "");

                    ModelAction actionPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.Prezzo4, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    actionPrezzo.NewValore = new ValoreContabilita() { V = prezzo4 };
                    actionAdd.NestedActions.Add(actionPrezzo);
                }

                //Prezzo5
                if (!string.IsNullOrEmpty(art.Prezzo5) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.Prezzo5))
                {
                    string prezzo5 = Regex.Replace(art.Prezzo5, @"\s", "");

                    ModelAction actionPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.Prezzo5, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    actionPrezzo.NewValore = new ValoreContabilita() { V = prezzo5 };
                    actionAdd.NestedActions.Add(actionPrezzo);
                }

                //SupCap
                if (!string.IsNullOrEmpty(art.IDSpCap) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.Capitolo1))
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.Capitolo1, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    Guid idSupCap = Guid.Empty;
                    if (_superCapitoliIdsMap.TryGetValue(art.IDSpCap, out idSupCap))
                    {
                        actionAttMod.NewValore = new ValoreGuid() { V = idSupCap };
                        actionAdd.NestedActions.Add(actionAttMod);
                    }
                }

                //Cap
                if (!string.IsNullOrEmpty(art.IDCap) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.Capitolo2))
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.Capitolo2, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    Guid idCap = Guid.Empty;
                    if (_capitoliIdsMap.TryGetValue(art.IDCap, out idCap))
                    {
                        actionAttMod.NewValore = new ValoreGuid() { V = idCap };
                        actionAdd.NestedActions.Add(actionAttMod);
                    }
                }

                //SubCap
                if (!string.IsNullOrEmpty(art.IDSbCap) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.Capitolo3))
                {
                    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.Capitolo3, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    Guid idSubCap = Guid.Empty;
                    if (_subCapitoliIdsMap.TryGetValue(art.IDSbCap, out idSubCap))
                    {
                        actionAttMod.NewValore = new ValoreGuid() { V = idSubCap };
                        actionAdd.NestedActions.Add(actionAttMod);
                    }
                }



                //Data
                if (!string.IsNullOrEmpty(art.Data) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.Data))
                {
                    DateTime data;
                    if (DateTime.TryParse(art.Data, out data))
                    {
                        ModelAction actionAttMod = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.Data, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };

                        actionAttMod.NewValore = new ValoreData() { V = data };
                        actionAdd.NestedActions.Add(actionAttMod);
                    }
                }

                //Sicurezza
                if (!string.IsNullOrEmpty(art.IncSIC) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.IncSicurezza))
                {
                    string incSIC = Regex.Replace(art.IncSIC, @"\s", "");

                    double dSic = 0;
                    if (double.TryParse(incSIC, NumberStyles.Any, CultureInfo.InvariantCulture, out dSic))
                    {
                        dSic = dSic / 100.0;

                        ModelAction actionAttMod = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.PrezzoSicurezza, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                        actionAttMod.NewValore = new ValoreReale() { V = dSic.ToString() };
                        actionAdd.NestedActions.Add(actionAttMod);

                    }
                }


                //Materiali
                if (!string.IsNullOrEmpty(art.IncMAT) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.IncMateriali))
                {
                    string incMAT = Regex.Replace(art.IncMAT, @"\s", "");

                    double dMat = 0;
                    if (double.TryParse(incMAT, NumberStyles.Any, CultureInfo.InvariantCulture, out dMat))
                    {
                        dMat = dMat / 100.0;

                        ModelAction actionAttMod = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.IncMateriali, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                        actionAttMod.NewValore = new ValoreReale() { V = dMat.ToString() };
                        actionAdd.NestedActions.Add(actionAttMod);

                    }
                }

                //Attrezzature
                if (!string.IsNullOrEmpty(art.IncATTR) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.IncNoli))
                {
                    string incATTR = Regex.Replace(art.IncATTR, @"\s", "");

                    double dAttr = 0;
                    if (double.TryParse(incATTR, NumberStyles.Any, CultureInfo.InvariantCulture, out dAttr))
                    {
                        dAttr = dAttr / 100.0;

                        ModelAction actionAttMod = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.IncNoli, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                        actionAttMod.NewValore = new ValoreReale() { V = dAttr.ToString() };
                        actionAdd.NestedActions.Add(actionAttMod);

                    }
                }

                //Manodopera
                if (!string.IsNullOrEmpty(art.IncMDO) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.IncManodopera))
                {
                    string incMdo = Regex.Replace(art.IncMDO, @"\s", "");

                    double dMdo = 0;
                    if (double.TryParse(incMdo, NumberStyles.Any, CultureInfo.InvariantCulture, out dMdo))
                    {

                        dMdo = dMdo / 100.0;

                        ModelAction actionAttMod = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.IncManodopera, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                        actionAttMod.NewValore = new ValoreReale() { V = dMdo.ToString() };
                        actionAdd.NestedActions.Add(actionAttMod);

                    }
                }

                //if (!string.IsNullOrEmpty(art.TagBIM) && prezzarioType.Attributi.ContainsKey(BuiltInCodes.Attributo.TagBIM))
                //{
                //    string str = ReplaceSpecialChars(art.TagBIM);
                //    var tagBIM = new TagBIM(str);

                //    ModelAction actionAttMod = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.TagBIM, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                //    actionAttMod.NewValore = new ValoreTestoCollection() { V = tagBIM.Items.Select(item => (ValoreCollectionItem) new ValoreTestoCollectionItem() { Testo1 = item.Etichetta, Testo2 = item.Valore }).ToList()};
                //    actionAdd.NestedActions.Add(actionAttMod);


                //}
            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), string.Format("{0}{1}-{2}", "Errore articolo ", art.Tariffa, exc.Message));
                return false;
            }

            return true;

        }
        #endregion

        #region Voci di computo
        bool AddVociComputo()//(60->80) //80->100
        {    
            
            if (Xpwe.Misurazioni.VociComputo == null)
                return true;
            
            
            double barDelta = 20;
            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione voci di computo..."), false, 80);


            int vcCount = Xpwe.Misurazioni.VociComputo.VCItem.Count;

            EntityType computoType = DataService.GetEntityType(BuiltInCodes.EntityType.Computo);

            //riferiemti (key riferisce a value)
            Dictionary<VcRgID, VcRgID> vvRgIDs = new Dictionary<VcRgID, VcRgID>();


            ModelAction action = new ModelAction()
            {
                EntityTypeKey = BuiltInCodes.EntityType.Computo,
                ActionName = ActionName.MULTI,
            };

            List<VcRgID> pweVociComputoIDs = new List<VcRgID>();
            int i = 0;
            foreach (VCItem vcItem in Xpwe.Misurazioni.VociComputo.VCItem)
            {

                if (vcItem.VCMisure == null)
                {
                    ModelAction actionAdd = new ModelAction()
                    {
                        EntityTypeKey = BuiltInCodes.EntityType.Computo,
                        ActionName = ActionName.ENTITY_ADD,
                    };

                    if (AddMisuraComputo(vcItem, null, actionAdd))
                    {
                        pweVociComputoIDs.Add(new VcRgID(vcItem.ID, string.Empty));
                        action.NestedActions.Add(actionAdd);
                    }

                }
                else
                {

                    int rgCount = vcItem.VCMisure.RGItem.Count;

                    foreach (RGItem rgItem in vcItem.VCMisure.RGItem)
                    {

                        List<VcRgItem> sourceRGItems = new List<VcRgItem>();

                        int depth = -1;
                        FillSourceRGItems(vcItem, rgItem, sourceRGItems, depth, vvRgIDs);

                        if (vvRgIDs.Any())
                        {     
                            //la registrazione fa riferimento ad altre registrazioni

                            foreach (VcRgItem sourceRGItem in sourceRGItems)
                            {
                                ModelAction actionAdd = new ModelAction()
                                {
                                    EntityTypeKey = BuiltInCodes.EntityType.Computo,
                                    ActionName = ActionName.ENTITY_ADD,
                                };


                                if (AddMisuraComputo(vcItem, rgItem, actionAdd, sourceRGItem.RgItem))
                                {
                                    pweVociComputoIDs.Add(new VcRgID(vcItem.ID, rgItem.ID, sourceRGItem.VcItem.ID, sourceRGItem.RgItem.ID));
                                    action.NestedActions.Add(actionAdd);
                                }
                            }
                        }
                        else
                        {
                            //la registrazione non fa riferimento a nessun altra

                            ModelAction actionAdd = new ModelAction()
                            {
                                EntityTypeKey = BuiltInCodes.EntityType.Computo,
                                ActionName = ActionName.ENTITY_ADD,
                            };

                            if (AddMisuraComputo(vcItem, rgItem, actionAdd))
                            {
                                pweVociComputoIDs.Add(new VcRgID(vcItem.ID, rgItem.ID, vcItem.ID, rgItem.ID));
                                action.NestedActions.Add(actionAdd);
                            } 
                        }
                    }
                    
                }
                i++;
            }

            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione voci di computo..."), false, 70);

            var mar = DataService.CommitAction(action);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                //creo la mappa degli id (ID -> guid)  delle voci di computo
                _vociComputoIdsMap.Clear();
                var newIds = mar.NewIds.ToList();

                if (newIds.Count != pweVociComputoIDs.Count)
                {
                    return false;
                }

                for (i = 0; i < pweVociComputoIDs.Count; i++)
                {
                    _vociComputoIdsMap.Add(pweVociComputoIDs[i], newIds[i]);
                }
            }


            //////////////////////////////////////////////////////////////////////////////////
            //Collegamento voci riferite (vediVoce)

            ModelAction actionCollegaVoci = new ModelAction()
            {
                EntityTypeKey = BuiltInCodes.EntityType.Computo,
                ActionName = ActionName.MULTI,
            };


            if (computoType.Attributi.ContainsKey(BuiltInCodes.Attributo.VediVoce))
            {
                //scorro le registrazioni con Vedi voce IDVV
                foreach (VcRgID vcrgID in vvRgIDs.Keys)
                {
                    var riferimento = vcrgID;
                    var riferito = vvRgIDs[vcrgID];

                    Guid computoItemid = Guid.Empty;
                    _vociComputoIdsMap.TryGetValue(riferimento, out computoItemid);

                    Guid sourceComputoItemId = Guid.Empty;
                    _vociComputoIdsMap.TryGetValue(riferito, out sourceComputoItemId);

                    if (computoItemid == Guid.Empty || sourceComputoItemId == Guid.Empty)
                        continue;


                    ModelAction actionCollegaVoce = new ModelAction()
                    {
                        EntityTypeKey = BuiltInCodes.EntityType.Computo,
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        AttributoCode = BuiltInCodes.Attributo.VediVoce,
                        EntitiesId = new HashSet<Guid>() { computoItemid },
                        NewValore = new ValoreGuid() { V = sourceComputoItemId },
                    };


                    actionCollegaVoci.NestedActions.Add(actionCollegaVoce);

                }

                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione voci di computo..."), false, 80);

                mar = DataService.CommitAction(actionCollegaVoci);
                if (mar.ActionResponse == ActionResponse.OK)
                {

                }
            }



            return true;

        }

        //ricorsiva
        void FillSourceRGItems(VCItem vcItem, RGItem rgItem, List<VcRgItem> sourceRGItems, int depth, Dictionary<VcRgID, VcRgID> vvRgIDs)
        {
            depth++;
 
            bool toAdd = true;
            if (!string.IsNullOrEmpty(rgItem.IDVV))
            {
                //cerco la voce di computo riferita
                VCItem vvVcItem = Xpwe.Misurazioni.VociComputo.VCItem.FirstOrDefault(item => item.ID == rgItem.IDVV);

                if (vvVcItem != null && vvVcItem.VCMisure != null)
                {
                    toAdd = false;
                    foreach (RGItem vvRgItem in vvVcItem.VCMisure.RGItem)
                    {
                        FillSourceRGItems(vvVcItem, vvRgItem, sourceRGItems, depth, vvRgIDs);

                        if (depth == 0)
                        {
                            foreach (VcRgItem sourceItem in sourceRGItems)
                            {

                                vvRgIDs.TryAdd(new VcRgID() { VcID = vcItem.ID, RgID = rgItem.ID, SourceVcID = sourceItem.VcItem.ID, SourceRgID = sourceItem.RgItem.ID, },
                                               new VcRgID() { VcID = vvVcItem.ID, RgID = vvRgItem.ID, SourceVcID = sourceItem.VcItem.ID, SourceRgID = sourceItem.RgItem.ID, });
                            }
                        }
                    }  
                }
            }

            if (toAdd)
                sourceRGItems.Add(new VcRgItem() { VcItem = vcItem, RgItem = rgItem });
        }

        bool AddMisuraComputo(VCItem vcItem, RGItem rgItem, ModelAction actionAdd, RGItem sourceRgItem = null)
        {
            if (vcItem == null)
                return false;

            bool isValidSourceRgItem = false;

            if (sourceRgItem != null && sourceRgItem != rgItem)
            {
                int flags = 0;
                int.TryParse(sourceRgItem.Flags, out flags);
                if (((FlagsEnum) flags & FlagsEnum.PARZIALE_QTA) != FlagsEnum.PARZIALE_QTA)//non è una qta parziale
                    isValidSourceRgItem = true;
            }



            EntityType computoType = DataService.GetEntityType(BuiltInCodes.EntityType.Computo);

            //Id Articolo
            ModelAction actionArtId = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, AttributoCode = BuiltInCodes.Attributo.PrezzarioItem_Guid, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
            Guid idArt = Guid.Empty;
            if (!_articoliIdsMap.TryGetValue(vcItem.IDEP, out idArt))
                return false;

            actionArtId.NewValore = new ValoreGuid() { V = idArt };
            actionAdd.NestedActions.Add(actionArtId);


            //DataMis
            if (!string.IsNullOrEmpty(vcItem.DataMis) && computoType.Attributi.ContainsKey(BuiltInCodes.Attributo.Data))
            {
                DateTime dataMis;
                if (DateTime.TryParse(vcItem.DataMis, out dataMis))
                {
                    ModelAction actionDataMis = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, AttributoCode = BuiltInCodes.Attributo.Data, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    actionDataMis.NewValore = new ValoreData() { V = dataMis };
                    actionAdd.NestedActions.Add(actionDataMis);
                }
            }


            //SupCat
            if (!string.IsNullOrEmpty(vcItem.IDSpCat) && computoType.Attributi.ContainsKey(BuiltInCodes.Attributo.Categoria1))
            {
                ModelAction actionSupCatId = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, AttributoCode = BuiltInCodes.Attributo.Categoria1, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                Guid idSupCat = Guid.Empty;
                if (_superCategorieIdsMap.TryGetValue(vcItem.IDSpCat, out idSupCat))
                {
                    actionSupCatId.NewValore = new ValoreGuid() { V = idSupCat };
                    actionAdd.NestedActions.Add(actionSupCatId);
                }
            }

            //Cat
            if (!string.IsNullOrEmpty(vcItem.IDCat) && computoType.Attributi.ContainsKey(BuiltInCodes.Attributo.Categoria2))
            {
                ModelAction actionCatId = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, AttributoCode = BuiltInCodes.Attributo.Categoria2, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                Guid idCat = Guid.Empty;
                if (_categorieIdsMap.TryGetValue(vcItem.IDCat, out idCat))
                {
                    actionCatId.NewValore = new ValoreGuid() { V = idCat };
                    actionAdd.NestedActions.Add(actionCatId);
                }
            }

            //SubCat
            if (!string.IsNullOrEmpty(vcItem.IDSbCat) && computoType.Attributi.ContainsKey(BuiltInCodes.Attributo.Categoria3))
            {
                ModelAction actionSubCatId = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, AttributoCode = BuiltInCodes.Attributo.Categoria3, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                Guid idSubCat = Guid.Empty;
                if (_subCategorieIdsMap.TryGetValue(vcItem.IDSbCat, out idSubCat))
                {
                    actionSubCatId.NewValore = new ValoreGuid() { V = idSubCat };
                    actionAdd.NestedActions.Add(actionSubCatId);
                }
            }

            //CodiceWBS
            if (!string.IsNullOrEmpty(vcItem.CodiceWBS) && computoType.Attributi.ContainsKey(BuiltInCodes.Attributo.WBS1))
            {
                ModelAction actionWBSId = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, AttributoCode = BuiltInCodes.Attributo.WBS1, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                string pweWBSId = string.Empty;
                if (_pweVociWbsCodice_ID.TryGetValue(vcItem.CodiceWBS, out pweWBSId))
                {
                    Guid idWBSItem = Guid.Empty;
                    if (_vociWBSIdsMap.TryGetValue(pweWBSId, out idWBSItem))
                    {
                        actionWBSId.NewValore = new ValoreGuid() { V = idWBSItem };
                        actionAdd.NestedActions.Add(actionWBSId);
                    }
                }
            }

            ////CodiceWBS
            //if (!string.IsNullOrEmpty(vcItem.CodiceWBS) && computoType.Attributi.ContainsKey(BuiltInCodes.Attributo.CodiceWBS))
            //{
            //    ModelAction actionCodice = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, AttributoCode = BuiltInCodes.Attributo.CodiceWBS, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
            //    //string desc = ReplaceSpecialChars(rgItem.CodiceWBS);
            //    actionCodice.NewValore = new ValoreTesto() { V = vcItem.CodiceWBS };
            //    actionAdd.NestedActions.Add(actionCodice);
            //}


            ////////////////////////////
            ///rgItem

            string formulaQta = string.Empty;
            bool anyFattoreQtaTot = false; //esiste almeno un fattore della quantità totale (L,L,H,Pu)
            bool anyFattoreQta = false; //esiste almeno un fattore della quantità (L,L,H)

            if (rgItem != null)
            {

                //Descrizione Qta
                if (!string.IsNullOrEmpty(rgItem.Descrizione) && computoType.Attributi.ContainsKey(BuiltInCodes.Attributo.DescrizioneQta))
                {
                    ModelAction actionDesc = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, AttributoCode = BuiltInCodes.Attributo.DescrizioneQta, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    string desc = ReplaceSpecialChars(rgItem.Descrizione);
                    actionDesc.NewValore = new ValoreTesto() { V = desc };
                    actionAdd.NestedActions.Add(actionDesc);

                }

                //PU
                if (!string.IsNullOrEmpty(rgItem.PartiUguali) && computoType.Attributi.ContainsKey(BuiltInCodes.Attributo.PU))
                {
                    string pu = Regex.Replace(rgItem.PartiUguali, @"\s", "");


                    double dAttr = 1;
                    if (!double.TryParse(pu, NumberStyles.Any, CultureInfo.InvariantCulture, out dAttr))
                        dAttr = 1;

                    if (dAttr != 0.0)
                    {
                        ModelAction actionPU = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, AttributoCode = BuiltInCodes.Attributo.PU, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                        actionPU.NewValore = new ValoreReale() { V = pu };
                        actionAdd.NestedActions.Add(actionPU);

                        anyFattoreQtaTot = true;
                    }
                }

                //Lunghezza
                if (!string.IsNullOrEmpty(rgItem.Lunghezza) && computoType.Attributi.ContainsKey(BuiltInCodes.Attributo.Lunghezza))
                {
                    string l = Regex.Replace(rgItem.Lunghezza, @"\s", "");

                    double dAttr = 1;
                    if (!double.TryParse(l, NumberStyles.Any, CultureInfo.InvariantCulture, out dAttr))
                        dAttr = 1;

                    if (dAttr != 0.0)
                    {

                        ModelAction actionLun = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, AttributoCode = BuiltInCodes.Attributo.Lunghezza, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                        actionLun.NewValore = new ValoreReale() { V = l };
                        actionAdd.NestedActions.Add(actionLun);

                        anyFattoreQtaTot = true;
                        anyFattoreQta = true;
                    }
                }

                //Larghezza
                if (!string.IsNullOrEmpty(rgItem.Larghezza) && computoType.Attributi.ContainsKey(BuiltInCodes.Attributo.Larghezza))
                {
                    string l = Regex.Replace(rgItem.Larghezza, @"\s", "");

                    double dAttr = 1;
                    if (!double.TryParse(l, NumberStyles.Any, CultureInfo.InvariantCulture, out dAttr))
                        dAttr = 1;

                    if (dAttr != 0.0)
                    {
                        ModelAction actionLar = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, AttributoCode = BuiltInCodes.Attributo.Larghezza, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                        actionLar.NewValore = new ValoreReale() { V = l };
                        actionAdd.NestedActions.Add(actionLar);

                        anyFattoreQtaTot = true;
                        anyFattoreQta = true;
                    }


                }

                //HPeso
                if (!string.IsNullOrEmpty(rgItem.HPeso) && computoType.Attributi.ContainsKey(BuiltInCodes.Attributo.Altezza))
                {
                    string h = Regex.Replace(rgItem.HPeso, @"\s", "");

                    double dAttr = 1;
                    if (!double.TryParse(h, NumberStyles.Any, CultureInfo.InvariantCulture, out dAttr))
                        dAttr = 1;

                    if (dAttr != 0.0)
                    {
                        ModelAction actionHPeso = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, AttributoCode = BuiltInCodes.Attributo.Altezza, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                        actionHPeso.NewValore = new ValoreReale() { V = h };
                        actionAdd.NestedActions.Add(actionHPeso);

                        anyFattoreQtaTot = true;
                        anyFattoreQta = true;
                    }
                }

                //Flags 
                bool detrazione = false;//qta in detrazione
                bool parzialeQta = false;//parziale di qta
                if (!string.IsNullOrEmpty(rgItem.Flags))
                {
                    int flags = 0;
                    int.TryParse(rgItem.Flags, out flags);
                    if (((FlagsEnum)flags & FlagsEnum.DETRAZIONE) == FlagsEnum.DETRAZIONE)
                        detrazione = true;
                    if (((FlagsEnum)flags & FlagsEnum.PARZIALE_QTA) == FlagsEnum.PARZIALE_QTA)
                        parzialeQta = true;
                }

                if (!anyFattoreQtaTot && !isValidSourceRgItem)
                    return false;

                if (parzialeQta)
                    return false;

                if (anyFattoreQta)
                {
                    string attLunghezza = string.Format("att{{{0}}}", computoType.Attributi[BuiltInCodes.Attributo.Lunghezza].Etichetta);
                    string attLarghezza = string.Format("att{{{0}}}", computoType.Attributi[BuiltInCodes.Attributo.Larghezza].Etichetta);
                    string attAltezza = string.Format("att{{{0}}}", computoType.Attributi[BuiltInCodes.Attributo.Altezza].Etichetta);

                    formulaQta = string.Format("{0}({1};{2};{3})", PrdFunction.FunctionName, attLunghezza, attLarghezza, attAltezza);
                }

                //applico detrazione
                if (anyFattoreQtaTot)
                {
                    if (detrazione)
                    {
                        if (formulaQta.Any())
                            formulaQta = formulaQta.Insert(0, "-");
                        else
                            formulaQta = "-1";
                    }
                }

                if (isValidSourceRgItem && !string.IsNullOrEmpty(sourceRgItem.Quantita))//registrazione riferite ad un'altra registrazione (vedi voce)
                {
                    string qta = Regex.Replace(sourceRgItem.Quantita, @"\s", "");

                    double dAttr = 1;
                    if (!double.TryParse(qta, NumberStyles.Any, CultureInfo.InvariantCulture, out dAttr))
                        dAttr = 1;

                    if (dAttr != 0.0)
                    {

                        string attVVQtaTot = string.Format("att{{{0}}}", computoType.Attributi[BuiltInCodes.Attributo.VediVoceQtaTot].Etichetta);

                        if (anyFattoreQta)
                            formulaQta = String.Format("{0}*{1}", attVVQtaTot, formulaQta);
                        else
                            formulaQta = attVVQtaTot;
                    }
                    else
                        return false;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(vcItem.Quantita))
                {
                    string qta = Regex.Replace(vcItem.Quantita, @"\s", "");

                    double dAttr = 1;
                    if (!double.TryParse(qta, NumberStyles.Any, CultureInfo.InvariantCulture, out dAttr))
                        dAttr = 1;

                    if (dAttr != 0.0)
                        formulaQta = qta;
                    else
                        return false;

                }
            }


            //Quantità
            ModelAction actionQta = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, AttributoCode = BuiltInCodes.Attributo.Quantita, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
            actionQta.NewValore = new ValoreReale() { V = formulaQta };
            actionAdd.NestedActions.Add(actionQta);

            return true;

        }
        #endregion


        #region Voci di WBS

        bool AddVociWBS()//60->80
        {


            if (Xpwe.DatiGenerali.DGWBS == null)
                return true;

            double barDelta = 20;
            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione voci di WBS..."), false, 60);

            var entTypes = DataService.GetEntityTypes();

            DivisioneItemType divType = entTypes.Values.Where(item => item is DivisioneItemType).FirstOrDefault(item => item.Codice == BuiltInCodes.Attributo.WBS1) as DivisioneItemType;

            if (divType == null)
            {
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Aggiornare il modello Import xpwe"));
                return false;
            }

            ModelAction action = new ModelAction()
            {
                EntityTypeKey = divType.GetKey(),
                ActionName = ActionName.MULTI,
            };



            List<string> pweVociWbsIDs = new List<string>();

            //key: CU, Value: CUParent
            Dictionary<string, string> pweVociWbsCUs = new Dictionary<string, string>();

            //key: CU, Value: ID
            Dictionary<string, string> pweVociWbsCU_ID = new Dictionary<string, string>();

            _pweVociWbsCodice_ID.Clear();

            //ricavo le voci di wbs parent
            HashSet<string> pweVociWBS_parents = Xpwe.DatiGenerali.DGWBS.DGWBSItem.Select(item => item.CUParent).ToHashSet();

            foreach (DGWBSItem wbsItem in Xpwe.DatiGenerali.DGWBS.DGWBSItem)
            {
                bool isParent = pweVociWBS_parents.Contains(wbsItem.ID);

                ModelAction actionAdd = new ModelAction()
                {
                    EntityTypeKey = divType.GetKey(),
                    ActionName = ActionName.ENTITY_ADD,

                };

                if (AddDivisioneItem(new WBSItemHelper(wbsItem), actionAdd, isParent))
                {
                    
                    pweVociWbsIDs.Add(wbsItem.ID);
                    pweVociWbsCUs.Add(wbsItem.CU, wbsItem.CUParent);
                    pweVociWbsCU_ID.Add(wbsItem.CU, wbsItem.ID);
                    _pweVociWbsCodice_ID.TryAdd(wbsItem.CODICE, wbsItem.ID);
                    action.NestedActions.Add(actionAdd);
                }
            }

            var mar = DataService.CommitAction(action);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                //creo la mappa degli id (ID -> guid) 
                _vociWBSIdsMap.Clear();
                var newIds = mar.NewIds.ToList();
                for (int i = 0; i < pweVociWbsCUs.Count; i++)
                {
                    _vociWBSIdsMap.Add(pweVociWbsIDs[i], newIds[i]);
                }
            }
            else
                return false;

            //Collegamenti per la struttura ad albero

            ModelAction actionTree = new ModelAction()
            {
                EntityTypeKey = divType.GetKey(),
                ActionName = ActionName.MULTI,
            };

            foreach (string cu in pweVociWbsCUs.Keys)
            {
                string pweID = pweVociWbsCU_ID[cu];
                Guid id = _vociWBSIdsMap[pweID];


                string cuParent = pweVociWbsCUs[cu];
                if (string.IsNullOrEmpty(cuParent))
                    continue;

                Guid idParent = Guid.Empty;
                string pweIDParent = String.Empty;
                if (pweVociWbsCU_ID.TryGetValue(cuParent, out pweIDParent))
                {
                    idParent = _vociWBSIdsMap[pweIDParent];
                }

                if (idParent != Guid.Empty)
                {
                    ModelAction actionMoveChild = new ModelAction() { EntityTypeKey = divType.GetKey(), ActionName = ActionName.TREEENTITY_MOVE_CHILD_OF };
                    actionMoveChild.EntitiesId = new HashSet<Guid>() { id };
                    actionMoveChild.NewTargetEntitiesId.Add(new TargetReference() { Id = idParent, TargetReferenceName = TargetReferenceName.CHILD_OF });

                    actionTree.NestedActions.Add(actionMoveChild);
                }
            }

            mar = DataService.CommitAction(actionTree);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Non viene più usata perchè si inseriscono le voci di wbs in una suddivisione
        /// </summary>
        /// <returns></returns>
        bool AddVociWBSInWBS()//80->100
        {


            if (Xpwe.DatiGenerali.DGWBS == null)
                return true;

            double barDelta = 10;
            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione voci di WBS..."), false, 80);


            ModelAction action = new ModelAction()
            {
                EntityTypeKey = BuiltInCodes.EntityType.WBS,
                ActionName = ActionName.MULTI,
            };



            List<string> pweVociWbsIDs = new List<string>();

            //key: CU, Value: CUParent
            Dictionary<string, string> pweVociWbsCUs = new Dictionary<string, string>();

            //key: CU, Value: ID
            Dictionary<string, string> pweVociWbsCU_ID = new Dictionary<string, string>();

            foreach (DGWBSItem wbsItem in Xpwe.DatiGenerali.DGWBS.DGWBSItem)
            {

                ModelAction actionAdd = new ModelAction()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.WBS,
                    ActionName = ActionName.ENTITY_ADD,

                };

                if (AddVoceWBS(wbsItem, actionAdd))
                {
                    pweVociWbsIDs.Add(wbsItem.ID);
                    pweVociWbsCUs.Add(wbsItem.CU, wbsItem.CUParent);
                    pweVociWbsCU_ID.Add(wbsItem.CU, wbsItem.ID);
                    action.NestedActions.Add(actionAdd);
                }
            }

            var mar = DataService.CommitAction(action);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                //creo la mappa degli id (ID -> guid) 
                _vociWBSIdsMap.Clear();
                var newIds = mar.NewIds.ToList();
                for (int i = 0; i < pweVociWbsCUs.Count; i++)
                {
                    _vociWBSIdsMap.Add(pweVociWbsIDs[i], newIds[i]);
                }
            }
            else
                return false;

            //Collegamenti per la struttura ad albero

            ModelAction actionTree = new ModelAction()
            {
                EntityTypeKey = BuiltInCodes.EntityType.WBS,
                ActionName = ActionName.MULTI,
            };

            foreach (string cu in pweVociWbsCUs.Keys)
            {
                string pweID = pweVociWbsCU_ID[cu];
                Guid id = _vociWBSIdsMap[pweID];


                string cuParent = pweVociWbsCUs[cu];
                if (string.IsNullOrEmpty(cuParent))
                    continue;

                Guid idParent = Guid.Empty;
                string pweIDParent = String.Empty;
                if (pweVociWbsCU_ID.TryGetValue(cuParent, out pweIDParent))
                {
                    idParent = _vociWBSIdsMap[pweIDParent];
                }

                if (idParent != Guid.Empty)
                {
                    ModelAction actionMoveChild = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.WBS, ActionName = ActionName.TREEENTITY_MOVE_CHILD_OF };
                    actionMoveChild.EntitiesId = new HashSet<Guid>() { id };
                    actionMoveChild.NewTargetEntitiesId.Add(new TargetReference() { Id = idParent, TargetReferenceName = TargetReferenceName.CHILD_OF });

                    actionTree.NestedActions.Add(actionMoveChild);
                }
            }

            mar = DataService.CommitAction(actionTree);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                return true;
            }

            return false;
        }


        bool AddVoceWBS(DGWBSItem wbsItem, ModelAction actionAdd)
        {

            if (wbsItem == null)
                return false;


            EntityType wbsType = DataService.GetEntityType(BuiltInCodes.EntityType.WBS);


            //CodiceExt
            if (!string.IsNullOrEmpty(wbsItem.CodiceExt) && wbsType.Attributi.ContainsKey(BuiltInCodes.Attributo.Codice))
            {
                ModelAction actionAtt = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.WBS, AttributoCode = BuiltInCodes.Attributo.Codice, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                actionAtt.NewValore = new ValoreTesto() { V = wbsItem.CodiceExt };
                actionAdd.NestedActions.Add(actionAtt);

            }

            //TITOLO
            if (!string.IsNullOrEmpty(wbsItem.TITOLO) && wbsType.Attributi.ContainsKey(BuiltInCodes.Attributo.Nome))
            {
                ModelAction actionAtt = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.WBS, AttributoCode = BuiltInCodes.Attributo.Nome, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                string str = ReplaceSpecialChars(wbsItem.TITOLO);
                actionAtt.NewValore = new ValoreTesto() { V = str };
                actionAdd.NestedActions.Add(actionAtt);

            }

            //CODICE
            if (!string.IsNullOrEmpty(wbsItem.CODICE))
            {
                //costruzione del filtro
                //
                var singleCondition = new ValoreConditionSingle();
                singleCondition.Condition = ValoreConditionEnum.StartsWith;
                singleCondition.Valore = new ValoreTesto() { V = wbsItem.CODICE };


                var attSingleCondition = new AttributoValoreConditionSingle();
                attSingleCondition.CodiceAttributo = BuiltInCodes.Attributo.CodiceWBS;
                attSingleCondition.ValoreConditionSingle = singleCondition;

                AttributoFilterData attFilterData = new AttributoFilterData();
                attFilterData.IsFiltroAttivato = true;
                attFilterData.EntityTypeKey = BuiltInCodes.EntityType.Computo;
                attFilterData.CodiceAttributo = BuiltInCodes.Attributo.CodiceWBS;
                attFilterData.CheckedValori = new HashSet<string>();
                attFilterData.FilterType = FilterTypeEnum.Conditions;
                attFilterData.ValoreConditions.MainGroup.Operator = ValoreConditionsGroupOperator.And;
                attFilterData.ValoreConditions.MainGroup.Conditions.Add(attSingleCondition);


                string json = string.Empty;

                if (JsonSerializer.JsonSerialize(attFilterData, out json))
                {

                    //prima action
                    ModelAction attributoFilterAction = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                        AttributoCode = BuiltInCodes.Attributo.AttributoFilter,
                    };
                    attributoFilterAction.NewValore = new ValoreTesto() { V = json };
                    actionAdd.NestedActions.Add(attributoFilterAction);

                    //seconda action
                    ModelAction attributoFilterTextAction = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                        AttributoCode = BuiltInCodes.Attributo.AttributoFilterText,
                    };
                    string desc = GetAttributoFilterTextDescription(attFilterData);
                    attributoFilterTextAction.NewValore = new ValoreTesto() { V = desc, Result = desc };
                    actionAdd.NestedActions.Add(attributoFilterTextAction);
                }

            }
            else
                return false;

            return true;
        }


        string GetAttributoFilterTextDescription(AttributoFilterData attFilterData)
        {

            string str = string.Empty;

            if (attFilterData == null)
                return string.Empty;

            EntityType entType = DataService.GetEntityTypes().Values.FirstOrDefault(item => item.Codice == attFilterData.EntityTypeKey);
            if (entType == null)
                return string.Empty;

            Attributo att = null;
            if (!entType.Attributi.TryGetValue(attFilterData.CodiceAttributo, out att))
                return string.Empty;

            if (attFilterData.FilterType == FilterTypeEnum.Conditions)
            {
                str = LocalizationProvider.GetString("_Condizione");
            }
            else
            {
                Attributo sourceAtt = _entitiesHelper.GetSourceAttributo(att);
                if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                {
                    IEnumerable<Guid> ids = attFilterData.CheckedValori.Select(item => new Guid(item));
                    List<Entity> ents = DataService.GetEntitiesById(sourceAtt.GuidReferenceEntityTypeKey, ids);

                    str = string.Join("\n", ents.Select(item => item.ToUserIdentity(UserIdentityMode.SingleLine)));
                }
                else
                {
                    str = string.Join("\n", attFilterData.CheckedValori);

                }
            }

            return str;
        }

        #endregion

        void LoadSpecialChars()
        {
            _specialChars.Clear();

            _specialChars.Add("\n", "\r");// \r nell'rtf verrà interpretato come paragrafo \par


            //
            _specialChars.Add("&gt;", ">");
            _specialChars.Add("&lt;", "<");
            _specialChars.Add("&amp;", "&");
            _specialChars.Add("&quot;", "\"");
            _specialChars.Add("&apos;", "'"); 
            //
            _specialChars.Add("&#128;", "€");
            _specialChars.Add("&#129;", " ");
            _specialChars.Add("&#130;", "‚");
            _specialChars.Add("&#131;", "ƒ");
            _specialChars.Add("&#132;", "„");
            _specialChars.Add("&#133;", "…");
            _specialChars.Add("&#134;", "†");
            _specialChars.Add("&#135;", "‡");
            _specialChars.Add("&#136;", "ˆ");
            _specialChars.Add("&#137;", "‰");
            _specialChars.Add("&#138;", "Š");
            _specialChars.Add("&#139;", "‹");
            _specialChars.Add("&#140;", "Œ");
            _specialChars.Add("&#141;", " ");
            _specialChars.Add("&#142;", "Ž");
            _specialChars.Add("&#143;", " ");
            _specialChars.Add("&#144;", " ");
            _specialChars.Add("&#145;", "‘");
            _specialChars.Add("&#146;", "’");
            _specialChars.Add("&#147;", "“");
            _specialChars.Add("&#148;", "”");
            _specialChars.Add("&#149;", "•");
            _specialChars.Add("&#150;", "–");
            _specialChars.Add("&#151;", "—");
            _specialChars.Add("&#152;", "˜");
            _specialChars.Add("&#153;", "™");
            _specialChars.Add("&#154;", "š");
            _specialChars.Add("&#155;", "›");
            _specialChars.Add("&#156;", "œ");
            _specialChars.Add("&#157;", " ");
            _specialChars.Add("&#158;", "ž");
            _specialChars.Add("&#159;", "Ÿ");
            _specialChars.Add("&#160;", " ");
            _specialChars.Add("&#161;", "¡");
            _specialChars.Add("&#162;", "¢");
            _specialChars.Add("&#163;", "£");
            _specialChars.Add("&#164;", "¤");
            _specialChars.Add("&#165;", "¥");
            _specialChars.Add("&#166;", "¦");
            _specialChars.Add("&#167;", "§");
            _specialChars.Add("&#168;", "¨");
            _specialChars.Add("&#169;", "©");
            _specialChars.Add("&#170;", "ª");
            _specialChars.Add("&#171;", "«");
            _specialChars.Add("&#172;", "¬");
            _specialChars.Add("&#173;", " ");
            _specialChars.Add("&#174;", "®");
            _specialChars.Add("&#175;", "¯");
            _specialChars.Add("&#176;", "°");
            _specialChars.Add("&#177;", "±");
            _specialChars.Add("&#178;", "²");
            _specialChars.Add("&#179;", "³");
            _specialChars.Add("&#180;", "´");
            _specialChars.Add("&#181;", "µ");
            _specialChars.Add("&#182;", "¶");
            _specialChars.Add("&#183;", "·");
            _specialChars.Add("&#184;", "¸");
            _specialChars.Add("&#185;", "¹");
            _specialChars.Add("&#186;", "º");
            _specialChars.Add("&#187;", "»");
            _specialChars.Add("&#188;", "¼");
            _specialChars.Add("&#189;", "½");
            _specialChars.Add("&#190;", "¾");
            _specialChars.Add("&#191;", "¿");
            _specialChars.Add("&#192;", "À");
            _specialChars.Add("&#193;", "Á");
            _specialChars.Add("&#194;", "Â");
            _specialChars.Add("&#195;", "Ã");
            _specialChars.Add("&#196;", "Ä");
            _specialChars.Add("&#197;", "Å");
            _specialChars.Add("&#198;", "Æ");
            _specialChars.Add("&#199;", "Ç");
            _specialChars.Add("&#200;", "È");
            _specialChars.Add("&#201;", "É");
            _specialChars.Add("&#202;", "Ê");
            _specialChars.Add("&#203;", "Ë");
            _specialChars.Add("&#204;", "Ì");
            _specialChars.Add("&#205;", "Í");
            _specialChars.Add("&#206;", "Î");
            _specialChars.Add("&#207;", "Ï");
            _specialChars.Add("&#208;", "Ð");
            _specialChars.Add("&#209;", "Ñ");
            _specialChars.Add("&#210;", "Ò");
            _specialChars.Add("&#211;", "Ó");
            _specialChars.Add("&#212;", "Ô");
            _specialChars.Add("&#213;", "Õ");
            _specialChars.Add("&#214;", "Ö");
            _specialChars.Add("&#215;", "×");
            _specialChars.Add("&#216;", "Ø");
            _specialChars.Add("&#217;", "Ù");
            _specialChars.Add("&#218;", "Ú");
            _specialChars.Add("&#219;", "Û");
            _specialChars.Add("&#220;", "Ü");
            _specialChars.Add("&#221;", "Ý");
            _specialChars.Add("&#222;", "Þ");
            _specialChars.Add("&#223;", "ß");
            _specialChars.Add("&#224;", "à");
            //_specialChars.Add("&#224;", "0x00E0");
            _specialChars.Add("&#225;", "á");
            _specialChars.Add("&#226;", "â");
            _specialChars.Add("&#227;", "ã");
            _specialChars.Add("&#228;", "ä");
            _specialChars.Add("&#229;", "å");
            _specialChars.Add("&#230;", "æ");
            _specialChars.Add("&#231;", "ç");
            _specialChars.Add("&#232;", "è");
            _specialChars.Add("&#233;", "é");
            _specialChars.Add("&#234;", "ê");
            _specialChars.Add("&#235;", "ë");
            _specialChars.Add("&#236;", "ì");
            _specialChars.Add("&#237;", "í");
            _specialChars.Add("&#238;", "î");
            _specialChars.Add("&#239;", "ï");
            _specialChars.Add("&#240;", "ð");
            _specialChars.Add("&#241;", "ñ");
            _specialChars.Add("&#242;", "ò");
            _specialChars.Add("&#243;", "ó");
            _specialChars.Add("&#244;", "ô");
            _specialChars.Add("&#245;", "õ");
            _specialChars.Add("&#246;", "ö");
            _specialChars.Add("&#247;", "÷");
            _specialChars.Add("&#248;", "ø");
            _specialChars.Add("&#249;", "ù");
            _specialChars.Add("&#250;", "ú");
            _specialChars.Add("&#251;", "û");
            _specialChars.Add("&#252;", "ü");
            _specialChars.Add("&#253;", "ý");
            _specialChars.Add("&#254;", "þ");
            _specialChars.Add("&#255;", "ÿ");


        }

        string ReplaceSpecialChars(string source)
        {

            //&#187;
            HashSet<string> specialChars = new HashSet<string>();


            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == '&')
                {
                    string str = null;
                    if (i <= source.Length - 6)
                    {
                        str = source.Substring(i, 6);
                        if (str[1] == '#' && str.Last() == ';')
                            specialChars.Add(str);
                        else if (str == "&quot;")
                            specialChars.Add(str);
                    }
                    else if (i <= source.Length - 5)
                        str = source.Substring(i, 5);
                    else if (i <= source.Length - 4)
                        str = source.Substring(i, 4);

                    if (str.StartsWith("&amp;"))
                        specialChars.Add("&amp;");

                    if (str.StartsWith("&gt;"))
                        specialChars.Add("&gt;");

                    if (str.StartsWith("&lt;"))
                        specialChars.Add("&lt;");
                }
                else if (source[i] == '\n')
                    specialChars.Add("\n");
            }

            //sostituisco
            string dest = source;
            foreach (string specialChar in specialChars)
            {
                if (_specialChars.ContainsKey(specialChar))
                {
                    string newChar = _specialChars[specialChar];
                    dest = dest.Replace(specialChar, newChar);
                }
            }


            return dest;
        }

        internal async Task UpdateXpweModel()
        {

            string fullFileName = string.Format("{0}\\{1}", MainOperation.GetModelliFolder(), _modelloFileName);

            string modelloFileName = Path.GetFileNameWithoutExtension(_modelloFileName);

            //scaricamento o eventuale aggiornamento del modello
            var projectModelView = new ProjectModelView();
            projectModelView.WindowService = WindowService;
            projectModelView.MainOperation = MainOperation;
            await projectModelView.LoadAsync();

            //scarico il modello se non esiste in locale o lo aggiorno se è da aggiornare
            if (!File.Exists(fullFileName) || projectModelView.IsUpdateAvailable(modelloFileName))
            {

                bool ok = await projectModelView.DownloadModelloAsync(Path.GetFileNameWithoutExtension(_modelloFileName));

                if (!ok)
                {
                    MainOperation.ShowMessageBarView(string.Format("{0} {1}", LocalizationProvider.GetString("Errore nello scaricamento del modello"), modelloFileName));
                }
            }
        }
    }

    class CapitoliCategorieItemHelper
    {
        public string DesSintetica { get; set; }
        public string DesEstesa { get; set; }
        public string Codice { get; set; }
        public string CodFase { get; set; }

        public CapitoliCategorieItemHelper(DGSuperCategorieItem item)
        {
            DesSintetica = item.DesSintetica;
            DesEstesa = item.DesEstesa;
            Codice = item.Codice;
            CodFase = item.CodFase;
        }

        public CapitoliCategorieItemHelper(DGCategorieItem item)
        {
            DesSintetica = item.DesSintetica;
            DesEstesa = item.DesEstesa;
            Codice = item.Codice;
            CodFase = item.CodFase;
        }

        public CapitoliCategorieItemHelper(DGSubCategorieItem item)
        {
            DesSintetica = item.DesSintetica;
            DesEstesa = item.DesEstesa;
            Codice = item.Codice;
            CodFase = item.CodFase;
        }

        public CapitoliCategorieItemHelper(DGSuperCapitoliItem item)
        {
            DesSintetica = item.DesSintetica;
            DesEstesa = item.DesEstesa;
            Codice = item.Codice;
            CodFase = item.CodFase;
        }

        public CapitoliCategorieItemHelper(DGCapitoliItem item)
        {
            DesSintetica = item.DesSintetica;
            DesEstesa = item.DesEstesa;
            Codice = item.Codice;
            CodFase = item.CodFase;
        }

        public CapitoliCategorieItemHelper(DGSubCapitoliItem item)
        {
            DesSintetica = item.DesSintetica;
            DesEstesa = item.DesEstesa;
            Codice = item.Codice;
            CodFase = item.CodFase;
        }


    }

    class WBSItemHelper
    {  
        public string Codice { get; set; }
        public string CodiceExt { get; set; }
        public string Titolo { get; set; }


        public WBSItemHelper(DGWBSItem item)
        {
            Codice = item.CODICE;
            CodiceExt = item.CodiceExt;
            Titolo = item.TITOLO;
        }
    }

    class TagBIM
    {
        //input
        //<TagBIM>#Categoria = "CALCESTRUZZO NON ARMATO";#TipoStruttura = "Sottofondazione";#ClasseResistenza = "C12/15";#ClasseEsposizione = "X0";#ClasseConsistenza = "S3";#ClasseCemento = "32.5R";</TagBIM>
        
        public List<TagBIMItem> Items { get; } = new List<TagBIMItem>();

        public TagBIM(string str)
        {
            string[] splitted = str.Split("#");
            foreach (var strItem in splitted)
                AddItem(strItem);
        }

        void AddItem(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                Items.Add(new TagBIMItem(str));
            }
        }
    }

    class TagBIMItem
    {
        public string Etichetta { get; set; }
        public string Valore { get; set; }

        public TagBIMItem(string str)
        {

            string[] vals = str.Split(new char[] { '=', '\"', ';' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            Etichetta = vals[0];
            Valore = vals[1];

        }

        public string Serialize()
        {
            string str = string.Format("#{0} = \"{1}\";", Etichetta, Valore);
            return str;
        }


    }

    /// <summary>
    /// Rappresenta l'id di una registrazione di una voce di compute per xpwe
    /// </summary>
    struct VcRgID
    {
        public string VcID { get; set; }
        public string RgID { get; set; }
        
        public string SourceVcID { get; set; }//voce di computo riferita con VediVoce
        public string SourceRgID { get; set; }//registrazione riferita con VediVoce
        

        public VcRgID(string vcID, string rgID, string sourceVcID = "", string sourceRgID = "")
        {
            VcID = vcID;
            RgID = rgID;
            SourceVcID = sourceVcID;
            SourceRgID = sourceRgID;
        }
    }

    struct VcRgItem
    {
        public VCItem VcItem { get; set; }
        public RGItem RgItem { get; set; }
    }

    enum FlagsEnum
    {
        NESSUNO = 0,
        DETRAZIONE = 1,
        PARZIALE_QTA = 2,
        //...?
        VEDIVOCE = 32768,
    }
}

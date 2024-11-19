using CommonResources;
using Commons;
using MasterDetailModel;
using Microsoft.Win32;
using Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;

namespace PrezzariWpf
{
    public class ImportPrezzarioXML
    {
        IDataService DataService { get; set; } = null;
        XMLMosaico XmlMosaico { get; set; } = null;
        string _fileExtension { get => "xml"; }

        NumberFormatInfo _formatProvider = new NumberFormatInfo();

        /// <summary>
        /// key: capitolo id mosaico
        /// value: indice di filteredEntitiesId
        /// </summary>
        Dictionary<string, int> _capitoliIndexById = new Dictionary<string, int>();
        List<Guid> _capitoliItemsId = null;
        int _capitoliCount { get; set; } = 0;

        /// <summary>
        /// Key: ISO-8859-1 Value: character (esempio key: &#128; value:€)
        /// </summary>
        Dictionary<string, string> _specialChars = new Dictionary<string, string>();


        void Clear()
        {
            DataService = null;
            XmlMosaico = null;
            _capitoliCount = 0;
            _capitoliIndexById.Clear();
            _specialChars.Clear();

            _formatProvider.NumberDecimalSeparator = ".";
            _formatProvider.NumberGroupSeparator = "";
        }

        internal void Run(ClientDataService dataService)
        {
            Clear();
            LoadSpecialChars();

            DataService = dataService;

            string fullFileName = string.Empty;

            if (!ValidateTargetProject())
                return;


            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = "xml";
            openFileDialog.Filter = string.Format("{0} files (*.{1})|*.{2}|All files (*.*)|*.*", _fileExtension, _fileExtension, _fileExtension);
            if (openFileDialog.ShowDialog() == true)
            {
                fullFileName = openFileDialog.FileName;

                DeserializeObject(fullFileName);
                if (XmlMosaico == null)
                    return;

                Run();
                
                MessageBox.Show("Importazione terminata correttamente", LocalizationProvider.GetString("AppName"));
            }

            
        }

        private bool ValidateTargetProject()
        {
            string msg = string.Empty;

            EntityType entType = DataService.GetEntityType(BuiltInCodes.EntityType.Prezzario);
            if (!entType.Attributi.ContainsKey(BuiltInCodes.Attributo.PrezzoSicurezza))
            {
                msg = string.Format("{0} mancante", BuiltInCodes.Attributo.PrezzoSicurezza);
            }

            if (!entType.Attributi.ContainsKey(BuiltInCodes.Attributo.IncSicurezza))
            {
                msg = string.Format("{0} mancante", BuiltInCodes.Attributo.IncSicurezza);
            }

            if (!entType.Attributi.ContainsKey(BuiltInCodes.Attributo.PrezzoManodopera))
            {
                msg = string.Format("{0} mancante", BuiltInCodes.Attributo.PrezzoManodopera);
            }

            if (!entType.Attributi.ContainsKey(BuiltInCodes.Attributo.IncManodopera))
            {
                msg = string.Format("{0} mancante", BuiltInCodes.Attributo.IncManodopera);
            }

            if (!entType.Attributi.ContainsKey(BuiltInCodes.Attributo.PrezzoMateriali))
            {
                msg = string.Format("{0} mancante", BuiltInCodes.Attributo.PrezzoMateriali);
            }

            if (!entType.Attributi.ContainsKey(BuiltInCodes.Attributo.IncMateriali))
            {
                msg = string.Format("{0} mancante", BuiltInCodes.Attributo.IncMateriali);
            }

            if (!entType.Attributi.ContainsKey(BuiltInCodes.Attributo.PrezzoNoli))
            {
                msg = string.Format("{0} mancante", BuiltInCodes.Attributo.PrezzoNoli);
            }

            if (!entType.Attributi.ContainsKey(BuiltInCodes.Attributo.IncNoli))
            {
                msg = string.Format("{0} mancante", BuiltInCodes.Attributo.IncNoli);
            }

            if (!entType.Attributi.ContainsKey(BuiltInCodes.Attributo.PrezzoTrasporti))
            {
                msg = string.Format("{0} mancante", BuiltInCodes.Attributo.PrezzoTrasporti);
            }

            if (!entType.Attributi.ContainsKey(BuiltInCodes.Attributo.IncTrasporti))
            {
                msg = string.Format("{0} mancante", BuiltInCodes.Attributo.IncTrasporti);
            }

            //if (!string.IsNullOrEmpty(msg))
            //{
            //    MessageBox.Show(msg, LocalizationProvider.GetString("AppName"));
            //    return false;
            //}



            return true;
        }

        private void DeserializeObject(string filename)
        {
            try
            {

                Console.WriteLine("Reading with Stream");
                // Create an instance of the XmlSerializer.
                XmlSerializer serializer = new XmlSerializer(typeof(XMLMosaico));

                // Declare an object variable of the type to be deserialized.
                using (Stream reader = new FileStream(filename, FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    XmlMosaico = (XMLMosaico)serializer.Deserialize(reader);
                }
            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);

            }

        }


        private void Run()
        {
            AddCapitoli();
            AddArticoli();
        }

        private void AddCapitoli()
        {
            //ModelAction action = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Capitoli, ActionName = ActionName.MULTI };

            if (XmlMosaico.Prezzario.Capitoli == null)
                return;

            int count = XmlMosaico.Prezzario.Capitoli.Capitolo.Count;
            int i = 0;
            foreach (Capitolo cap in XmlMosaico.Prezzario.Capitoli.Capitolo)
            {
                ModelAction actionAdd = new ModelAction()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.Capitoli,
                    ActionName = ActionName.TREEENTITY_ADD,
                };

                AddCapitolo(cap, actionAdd);

                //action.NestedActions.Add(actionAdd);
                ModelActionResponse mar = DataService.CommitAction(actionAdd);
                i++;
            }

            DataService.GetFilteredTreeEntities(BuiltInCodes.EntityType.Capitoli, null, null, out _capitoliItemsId);


            //DataService.CommitAction(action);
        }

        /// <summary>
        /// Recursive
        /// </summary>
        /// <param name="cap"></param>
        /// <param name="action"></param>
        private void AddCapitolo(Capitolo cap, ModelAction actionAdd)
        {
            

            //Codice
            ModelAction actionCodice = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Capitoli, AttributoCode = BuiltInCodes.Attributo.Codice, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
            string codice = ReplaceSpecialChars(cap.Codice);
            actionCodice.NewValore = new ValoreTesto() { V = codice };
            actionAdd.NestedActions.Add(actionCodice);

            //Descrizione
            ModelAction actionDesc = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Capitoli, AttributoCode = BuiltInCodes.Attributo.DescrizioneRTF, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
            string rtf = string.Empty;
            string desc = ReplaceSpecialChars(cap.Descrizione.Lingua.Descrizione);
            ValoreHelper.RtfFromPlainString(desc, out rtf);
            actionDesc.NewValore = new ValoreTestoRtf() { V = rtf };
            actionAdd.NestedActions.Add(actionDesc);

            _capitoliIndexById.Add(cap.IdCapitolo, _capitoliCount);
            _capitoliCount++;

            foreach (Capitolo capChild in cap.Capitoli)
            {
                ModelAction actionAddChild = new ModelAction()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.Capitoli,
                    ActionName = ActionName.TREEENTITY_ADD_CHILD
                };

                AddCapitolo(capChild, actionAddChild);

                actionAdd.NestedActions.Add(actionAddChild);
            }

        }

        private void AddArticoli()
        {
            //ModelAction action = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, ActionName = ActionName.MULTI };


            int count = XmlMosaico.Prezzario.Articoli.Articolo.Count;
            int i = 0;
            foreach (Articolo art in XmlMosaico.Prezzario.Articoli.Articolo)
            {
                ModelAction actionAdd = new ModelAction()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                    ActionName = ActionName.TREEENTITY_ADD,
                };

                AddArticolo(art, actionAdd);

                //action.NestedActions.Add(actionAdd);
                DataService.CommitAction(actionAdd);
                i++;
            }



            //DataService.CommitAction(action);
        }

        /// <summary>
        /// Recursive
        /// </summary>
        /// <param name="art"></param>
        /// <param name="action"></param>
        private void AddArticolo(Articolo art, ModelAction actionAdd, string parentDesc = null)
        {
            try
            {

                //Codice
                ModelAction actionCodice = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.Codice, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                string codice = ReplaceSpecialChars(art.Codice);
                actionCodice.NewValore = new ValoreTesto() { V = codice };
                actionAdd.NestedActions.Add(actionCodice);

                //Descrizione 
                if (art.Descrizione.Lingua.Descrizione.Contains("&#"))
                {
                }
                ModelAction actionDesc = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.DescrizioneRTF, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                string descRtf = string.Empty;
                string desc = ReplaceSpecialChars(art.Descrizione.Lingua.Descrizione);

                if (parentDesc != null) //caso particolare per aggiungere la descrizione del padre ai figli
                {
                    desc = string.Format("{0}\n{1}", parentDesc, desc);
                }

                ValoreHelper.RtfFromPlainString(desc, out descRtf);
                actionDesc.NewValore = new ValoreTestoRtf() { V = descRtf };
                actionAdd.NestedActions.Add(actionDesc);


                //Unità misura
                if (art.UnitaMisura.Contains("&#"))
                {
                }
                ModelAction actionUM = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.UnitaMisura, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                string um = ReplaceSpecialChars(art.UnitaMisura);
                actionUM.NewValore = new ValoreTesto() { V = um };
                actionAdd.NestedActions.Add(actionUM);

                //Prezzo
                if (art.Prezzi != null && art.Prezzi.Prezzo != null)
                {
                    ModelAction actionPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.Prezzo, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                    actionPrezzo.NewValore = new ValoreContabilita() { V = art.Prezzi.Prezzo.PrezzoApplicazione };
                    actionAdd.NestedActions.Add(actionPrezzo);
                }

                //Capitolo
                if (art.IdCapitolo != null && XmlMosaico.Prezzario.Capitoli != null)
                {
                    if (_capitoliIndexById.ContainsKey(art.IdCapitolo))
                    {
                        int capIndex = _capitoliIndexById[art.IdCapitolo];
                        if (capIndex < _capitoliItemsId.Count)
                        {
                            Guid capitoloItemId = _capitoliItemsId[capIndex];
                            ModelAction actionCapId = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.CapitoliItem_Guid, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                            actionCapId.NewValore = new ValoreGuid() { V = capitoloItemId };
                            actionAdd.NestedActions.Add(actionCapId);
                        }
                    }
                }

                //Sicurezza
                if (art.Prezzi != null && art.Prezzi.Prezzo != null)
                {
                    if (art.Prezzi.Prezzo.IsSicurezzaPercentuale == null || art.Prezzi.Prezzo.OneriSicurezza == null)
                    {
                        ModelAction actionIncPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.IncSicurezza, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                        actionIncPrezzo.NewValore = new ValoreReale() { V = "0" };
                        actionAdd.NestedActions.Add(actionIncPrezzo);
                    }
                    else
                    {
                        int isSicurezzaPerc = -1;
                        if (int.TryParse(art.Prezzi.Prezzo.IsSicurezzaPercentuale, out isSicurezzaPerc))
                        {
                            string sicurezza = art.Prezzi.Prezzo.OneriSicurezza;
                            if (isSicurezzaPerc == 1)
                            {
                                double dSic = 0;
                                double dPrezzoApp = 0;
                                if (double.TryParse(sicurezza, NumberStyles.AllowDecimalPoint, _formatProvider, out dSic)
                                    && double.TryParse(art.Prezzi.Prezzo.PrezzoApplicazione, NumberStyles.AllowDecimalPoint, _formatProvider, out dPrezzoApp))
                                {
                                    dSic = (dSic * dPrezzoApp) / 100.0;
                                    sicurezza = dSic.ToString();
                                }
                                else
                                {
                                    sicurezza = string.Empty;
                                }
                            }

                            ModelAction actionPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.PrezzoSicurezza, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                            actionPrezzo.NewValore = new ValoreContabilita() { V = sicurezza };
                            actionAdd.NestedActions.Add(actionPrezzo);
                        }
                    }
                }


                //Manodopera
                if (art.Prezzi != null && art.Prezzi.Prezzo != null)
                {
                    if (art.Prezzi.Prezzo.IsManodoperaPercentuale == null || art.Prezzi.Prezzo.Manodopera == null)
                    {
                        ModelAction actionIncPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.IncManodopera, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                        actionIncPrezzo.NewValore = new ValoreReale() { V = "0" };
                        actionAdd.NestedActions.Add(actionIncPrezzo);
                    }
                    else
                    { 
                        int isManodoperaPerc = -1;
                        if (int.TryParse(art.Prezzi.Prezzo.IsManodoperaPercentuale, out isManodoperaPerc))
                        {
                            string manodopera = art.Prezzi.Prezzo.Manodopera;
                            if (isManodoperaPerc == 1)
                            {
                                double dMan = 0;
                                double dPrezzoApp = 0;


                                if (double.TryParse(manodopera, NumberStyles.AllowDecimalPoint, _formatProvider, out dMan) &&
                                    double.TryParse(art.Prezzi.Prezzo.PrezzoApplicazione, NumberStyles.AllowDecimalPoint, _formatProvider, out dPrezzoApp))
                                {
                                    dMan = (dMan * dPrezzoApp) / 100.0;
                                    manodopera = dMan.ToString();
                                }
                                else
                                {
                                    manodopera = string.Empty;
                                }
                            }

                            ModelAction actionPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.PrezzoManodopera, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                            actionPrezzo.NewValore = new ValoreContabilita() { V = manodopera };
                            actionAdd.NestedActions.Add(actionPrezzo);
                        }
                    }
                }


                //Materiali
                if (art.Prezzi != null && art.Prezzi.Prezzo != null)
                {
                    if (art.Prezzi.Prezzo.IsMaterialiPercentuale == null || art.Prezzi.Prezzo.Materiali == null)
                    {
                        ModelAction actionIncPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.IncMateriali, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                        actionIncPrezzo.NewValore = new ValoreReale() { V = "0" };
                        actionAdd.NestedActions.Add(actionIncPrezzo);
                    }
                    else
                    {

                        int isMaterialiPerc = -1;
                        if (int.TryParse(art.Prezzi.Prezzo.IsMaterialiPercentuale, out isMaterialiPerc))
                        {
                            string materiali = art.Prezzi.Prezzo.Materiali;
                            if (isMaterialiPerc == 1)
                            {
                                double dMan = 0;
                                double dPrezzoApp = 0;


                                if (double.TryParse(materiali, NumberStyles.AllowDecimalPoint, _formatProvider, out dMan) &&
                                    double.TryParse(art.Prezzi.Prezzo.PrezzoApplicazione, NumberStyles.AllowDecimalPoint, _formatProvider, out dPrezzoApp))
                                {
                                    dMan = (dMan * dPrezzoApp) / 100.0;
                                    materiali = dMan.ToString();
                                }
                                else
                                {
                                    materiali = string.Empty;
                                }
                            }

                            ModelAction actionPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.PrezzoMateriali, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                            actionPrezzo.NewValore = new ValoreContabilita() { V = materiali };
                            actionAdd.NestedActions.Add(actionPrezzo);
                        }
                    }
                }

                //Noli/Attrezzature
                if (art.Prezzi != null && art.Prezzi.Prezzo != null)
                {
                    if (art.Prezzi.Prezzo.IsAttrezzaturePercentuale == null || art.Prezzi.Prezzo.Attrezzature == null)
                    {
                        ModelAction actionIncPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.IncNoli, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                        actionIncPrezzo.NewValore = new ValoreReale() { V = "0" };
                        actionAdd.NestedActions.Add(actionIncPrezzo);
                    }
                    else
                    {

                        int isAttrezzaturePerc = -1;
                        if (int.TryParse(art.Prezzi.Prezzo.IsAttrezzaturePercentuale, out isAttrezzaturePerc))
                        {
                            string attrezzature = art.Prezzi.Prezzo.Attrezzature;
                            if (isAttrezzaturePerc == 1)
                            {
                                double dMan = 0;
                                double dPrezzoApp = 0;


                                if (double.TryParse(attrezzature, NumberStyles.AllowDecimalPoint, _formatProvider, out dMan) &&
                                    double.TryParse(art.Prezzi.Prezzo.PrezzoApplicazione, NumberStyles.AllowDecimalPoint, _formatProvider, out dPrezzoApp))
                                {
                                    dMan = (dMan * dPrezzoApp) / 100.0;
                                    attrezzature = dMan.ToString();
                                }
                                else
                                {
                                    attrezzature = string.Empty;
                                }
                            }

                            ModelAction actionPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.PrezzoNoli, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                            actionPrezzo.NewValore = new ValoreContabilita() { V = attrezzature };
                            actionAdd.NestedActions.Add(actionPrezzo);
                        }
                    }
                }

                //Trasporti
                if (art.Prezzi != null && art.Prezzi.Prezzo != null)
                {
                    if (art.Prezzi.Prezzo.IsTrasportiPercentuale == null || art.Prezzi.Prezzo.Trasporti == null)
                    {
                        ModelAction actionIncPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.IncTrasporti, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                        actionIncPrezzo.NewValore = new ValoreReale() { V = "0" };
                        actionAdd.NestedActions.Add(actionIncPrezzo);
                    }
                    else
                    {
                        int isTrasportiPerc = -1;
                        if (int.TryParse(art.Prezzi.Prezzo.IsTrasportiPercentuale, out isTrasportiPerc))
                        {
                            string trasporti = art.Prezzi.Prezzo.Trasporti;
                            if (isTrasportiPerc == 1)
                            {
                                double dMan = 0;
                                double dPrezzoApp = 0;


                                if (double.TryParse(trasporti, NumberStyles.AllowDecimalPoint, _formatProvider, out dMan) &&
                                    double.TryParse(art.Prezzi.Prezzo.PrezzoApplicazione, NumberStyles.AllowDecimalPoint, _formatProvider, out dPrezzoApp))
                                {
                                    dMan = (dMan * dPrezzoApp) / 100.0;
                                    trasporti = dMan.ToString();
                                }
                                else
                                {
                                    trasporti = string.Empty;
                                }
                            }

                            ModelAction actionPrezzo = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Prezzario, AttributoCode = BuiltInCodes.Attributo.PrezzoTrasporti, ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY };
                            actionPrezzo.NewValore = new ValoreContabilita() { V = trasporti };
                            actionAdd.NestedActions.Add(actionPrezzo);
                        }
                    }
                }


                foreach (Articolo artChild in art.Articoli)
                {

                    ModelAction actionAddChild = new ModelAction()
                    {
                        EntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                    };

                    parentDesc = null;
                    //Gestione caso particolare di padre con prezzo.
                    //Concateno la descrizione dei padri a quella dei figli
                    if (art.Prezzi != null && art.Prezzi.Prezzo != null)
                    {
                        actionAddChild.ActionName = ActionName.TREEENTITY_ADD;
                        parentDesc = desc;
                    }
                    else
                    {
                        actionAddChild.ActionName = ActionName.TREEENTITY_ADD_CHILD;
                    }

                    AddArticolo(artChild, actionAddChild, parentDesc);

                    actionAdd.NestedActions.Add(actionAddChild);
                }

            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), string.Format("{0}{1}-{2}","Errore articolo ",art.Codice, exc.Message));
            }

        }


        void LoadSpecialChars()
        {
            _specialChars.Clear();

            _specialChars.Add("\n", "\r");// \r nell'rtf verrà interpretato come paragrafo \par
            

            //
            _specialChars.Add("&gt;", ">");
            _specialChars.Add("&lt;", "<");
            _specialChars.Add("&amp;", "&");
            _specialChars.Add("&quot;", "\"");
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


            for (int i=0; i<source.Length; i++)
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


    }
}

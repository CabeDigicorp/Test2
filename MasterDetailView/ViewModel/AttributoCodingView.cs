using CommonResources;
using Commons;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailView
{
    public class AttributoCodingView : NotificationBase
    {
        public List<Attributo> LisTaAttributiTesto { get; set; }
        public List<AttributoCoding> ListaAttributoCoding { get; set; }
        public ObservableCollection<string> ListaAttributi { get; set; }
        public ObservableCollection<ComportamentoCodice> ListaComportamentiCodice { get; set; }

        private string _SelectedAttribute;
        public string SelectedAttribute
        {
            get
            {
                return _SelectedAttribute;
            }
            set
            {
                if (SetProperty(ref _SelectedAttribute, value))
                {
                    _SelectedAttribute = value;
                }
            }
        }
        public static string SelectedAttributeCodice { get; set; }

        private ComportamentoCodice _SelectedComportamento;
        public ComportamentoCodice SelectedComportamento
        {
            get
            {
                return _SelectedComportamento;
            }
            set
            {
                if (SetProperty(ref _SelectedComportamento, value))
                {
                    _SelectedComportamento = value;
                }
            }
        }

        private ObservableCollection<AttributoCodingSetting> _ListaAttributoCodingSetting;
        public ObservableCollection<AttributoCodingSetting> ListaAttributoCodingSetting
        {
            get
            {
                return _ListaAttributoCodingSetting;
            }
            set
            {
                if (SetProperty(ref _ListaAttributoCodingSetting, value))
                {
                    _ListaAttributoCodingSetting = value;
                }
            }
        }
        public List<AttributoCodingHeader> ListaAttributoCodingHeader { get; set; }


        public Dictionary<string, List<AttributoCodingSetting>> AttributiSetting { get; set; }
        public int MaxDepth { get; set; }
        public string EntityTypeKey { get; set; }
        public EntitiesCodingHelper EntsCodingHelper { get; set; }
        public static List<int> SelectedLevels { get; set; }
        public ClientDataService DataService { get; set; }
        public IMainOperation MainOperation { get; set; }
        

        //private bool _IsColumnHidden;
        //public bool IsColumnHidden
        //{
        //    get
        //    {
        //        return _IsColumnHidden;
        //    }
        //    set
        //    {
        //        if (SetProperty(ref _IsColumnHidden, value))
        //        {
        //            _IsColumnHidden = value;
        //        }
        //    }
        //}

        public string AlertText
        {
            get
            {
                if (ListaAttributoCodingSetting.All(r => r.Codifica) == false)
                {
                    return LocalizationProvider.GetString("Nessun livello selezionato nella colonna Codifica");
                }
                else
                {
                    return LocalizationProvider.GetString("Procedi per effettuare la Codifica");
                }
            }
        }

        private System.Windows.Visibility _CalloutVisibility;
        public System.Windows.Visibility CalloutVisibility
        {
            get
            {
                return _CalloutVisibility;
            }
            set
            {
                if (SetProperty(ref _CalloutVisibility, value))
                {
                    _CalloutVisibility = value;
                    if (value == System.Windows.Visibility.Visible)
                        IsAcceptButtonEnable = false;
                    else
                        IsAcceptButtonEnable = true;
                }
            }
        }

        private bool _IsAcceptButtonEnable;
        public bool IsAcceptButtonEnable
        {
            get
            {
                return _IsAcceptButtonEnable;
            }
            set
            {
                if (SetProperty(ref _IsAcceptButtonEnable, value))
                    _IsAcceptButtonEnable = value;
            }
        }


        public AttributoCodingView()
        {
            CalloutVisibility = System.Windows.Visibility.Collapsed;
            ListaAttributi = new ObservableCollection<string>();
            AttributiSetting = new Dictionary<string, List<AttributoCodingSetting>>();
            ListaAttributoCodingSetting = new ObservableCollection<AttributoCodingSetting>();
            ListaAttributoCodingHeader = new List<AttributoCodingHeader>();
            ListaComportamentiCodice = new ObservableCollection<ComportamentoCodice>();
            ListaComportamentiCodice.Add(new ComportamentoCodice() { Comportamento = LocalizationProvider.GetString("Sostituisci codice esistente"), Codice = 0 });
            ListaComportamentiCodice.Add(new ComportamentoCodice() { Comportamento = LocalizationProvider.GetString("Anteponi al codice esistente"), Codice = 1 });
            ListaComportamentiCodice.Add(new ComportamentoCodice() { Comportamento = LocalizationProvider.GetString("Postponi al codice esistente"), Codice = 2 });
            SelectedComportamento = ListaComportamentiCodice.FirstOrDefault();
        }

        public void Init()
        {
            ListaAttributi = new ObservableCollection<string>(LisTaAttributiTesto.Select(a => a.Etichetta));
            if (ListaAttributoCoding.Count() > 0)
            {
                foreach (var Attributocoding in ListaAttributoCoding)
                {
                    List<AttributoCodingSetting> LivelliSetting = new List<AttributoCodingSetting>();
                    foreach (var Livello in Attributocoding.LevelsCoding)
                    {
                        AttributoCodingSetting LivelloSetting = new AttributoCodingSetting();
                        LivelloSetting.AggiungiCodiceSuperiore = Livello.AddHigherLevel;
                        LivelloSetting.Codifica = Livello.IsCoding;
                        LivelloSetting.Livello = Livello.Level;
                        LivelloSetting.Passo = Livello.Step;
                        LivelloSetting.Prefisso = Livello.Prefix;
                        LivelloSetting.Suffisso = Livello.Suffix;
                        LivelloSetting.ValoreIncrementale = Livello.IncrementalValue;
                        if (LivelliSetting.Count() > 1)
                            LivelloSetting.AttributoCodingSettingPrecedente = LivelliSetting.ElementAt(LivelloSetting.Livello - 2);
                        LivelloSetting.AttributoCodingSettingUpdaterHanlder += LivelloSetting_AttributoCodingSettingUpdaterHanlder;
                        LivelloSetting.CanCodifyHanlder += LivelloSetting_CanCodifyHanlder;
                        LivelliSetting.Add(LivelloSetting);
                    }
                    string Etichetta = LisTaAttributiTesto.Where(d => d.Codice == Attributocoding.AttributoCodice).FirstOrDefault().Etichetta;
                    AttributiSetting.Add(Etichetta, LivelliSetting);
                    AttributoCodingHeader AttributoCodingHeader = new AttributoCodingHeader() { Etichetta = Etichetta, PosizionamentoRispettoCodiceEsistente = Attributocoding.PosizionamentoRispettoCodiceEsistente };
                    ListaAttributoCodingHeader.Add(AttributoCodingHeader);
                }
                SelectedAttribute = AttributiSetting.LastOrDefault().Key;
                SelectedAttributeCodice = LisTaAttributiTesto.Where(d => d.Etichetta == SelectedAttribute).FirstOrDefault().Codice;
            }
        }

        public void SelectionAttributeChange(System.Collections.IList OldAttribute, System.Collections.IList NewAttribute)
        {
            SelectedAttributeCodice = LisTaAttributiTesto.Where(d => d.Etichetta == SelectedAttribute).FirstOrDefault().Codice;
            ListaAttributoCodingSetting = new ObservableCollection<AttributoCodingSetting>();
            int Valore = 0;

            //if (SelectedAttributeCodice != BuiltInCodes.Attributo.Codice)
            //    IsColumnHidden = true;
            //else
            //    IsColumnHidden = false;

            int Contatore = 0;
            if (!AttributiSetting.ContainsKey(NewAttribute[0].ToString()))
            {
                //if (SelectedAttributeCodice != BuiltInCodes.Attributo.Codice)
                //{
                //    ListaAttributoCodingSetting.Clear();
                //    AttributoCodingSetting AttributoCodingSetting = new AttributoCodingSetting() { Codifica = true, Livello = 1, Passo = 1 };
                //    AttributoCodingSetting.AttributoCodingSettingUpdaterHanlder += LivelloSetting_AttributoCodingSettingUpdaterHanlder;
                //    ListaAttributoCodingSetting.Add(AttributoCodingSetting);
                //}
                //else
                GeneraListaGridVuota();
            }
            else
            {
                ListaAttributoCodingSetting.Clear();
                foreach (var AttributoSettingSalvato in AttributiSetting[SelectedAttribute])
                {
                    AttributoCodingSetting LivelloSetting = null;
                    //|| SelectedAttributeCodice != BuiltInCodes.Attributo.Codice
                    if (SelectedLevels.Contains(Contatore))
                    {
                        LivelloSetting = new AttributoCodingSetting();
                        LivelloSetting.Livello = AttributoSettingSalvato.Livello;
                        if (AttributoSettingSalvato.Passo == 0)
                        {
                            LivelloSetting.Passo = 1;
                        }
                        else
                        {
                            LivelloSetting.Passo = AttributoSettingSalvato.Passo;
                        }
                        LivelloSetting.Prefisso = AttributoSettingSalvato.Prefisso;
                        LivelloSetting.Suffisso = AttributoSettingSalvato.Suffisso;
                        LivelloSetting.ValoreIncrementale = AttributoSettingSalvato.ValoreIncrementale;
                        if (ListaAttributoCodingSetting.Count() != 0)
                            LivelloSetting.AttributoCodingSettingPrecedente = ListaAttributoCodingSetting.ElementAt(LivelloSetting.Livello - 2);
                        LivelloSetting.AggiungiCodiceSuperiore = AttributoSettingSalvato.AggiungiCodiceSuperiore;
                    }
                    else
                    {
                        LivelloSetting = new AttributoCodingSetting();
                        LivelloSetting.Livello = AttributoSettingSalvato.Livello;
                    }
                    LivelloSetting.AttributoCodingSettingUpdaterHanlder += LivelloSetting_AttributoCodingSettingUpdaterHanlder;
                    LivelloSetting.CanCodifyHanlder += LivelloSetting_CanCodifyHanlder;
                    ListaAttributoCodingSetting.Add(LivelloSetting);

                    //if (SelectedAttributeCodice != BuiltInCodes.Attributo.Codice)
                    //    break;

                    Contatore++;
                }

                //if (SelectedAttributeCodice == BuiltInCodes.Attributo.Codice)
                //{
                while (ListaAttributoCodingSetting.Count() < MaxDepth)
                {
                    AttributoCodingSetting AttributoCodingSetting = new AttributoCodingSetting() { Codifica = false, Livello = ListaAttributoCodingSetting.Count() + 1, AttributoCodingSettingPrecedente = ListaAttributoCodingSetting.ElementAt(ListaAttributoCodingSetting.Count() - 1) };
                    AttributoCodingSetting.AttributoCodingSettingUpdaterHanlder += LivelloSetting_AttributoCodingSettingUpdaterHanlder;
                    AttributoCodingSetting.CanCodifyHanlder += LivelloSetting_CanCodifyHanlder;
                    ListaAttributoCodingSetting.Add(AttributoCodingSetting);
                }
                //}

                //if (SelectedAttributeCodice == BuiltInCodes.Attributo.Codice)
                //{
                while (ListaAttributoCodingSetting.Count() > MaxDepth)
                {
                    ListaAttributoCodingSetting.RemoveAt(ListaAttributoCodingSetting.Count() - 1);
                }
                //}

                int PosizionamentoRispettoCodiceEsistente = ListaAttributoCodingHeader.Where(f => f.Etichetta == SelectedAttribute).FirstOrDefault().PosizionamentoRispettoCodiceEsistente;
                SelectedComportamento = ListaComportamentiCodice.Where(f => f.Codice == PosizionamentoRispettoCodiceEsistente).FirstOrDefault();
            }

            //if (int.TryParse(SelectedAttributeCodice, out Valore))
            if (SelectedAttributeCodice != BuiltInCodes.Attributo.Codice && SelectedAttributeCodice != BuiltInCodes.Attributo.Nome)
            {
                foreach (AttributoCodingSetting attributoCodingSetting in ListaAttributoCodingSetting)
                {
                    attributoCodingSetting.Codifica = true;
                }
            }


            AggiornaEsempi();
        }

        private void LivelloSetting_AttributoCodingSettingUpdaterHanlder(object sender, EventArgs e)
        {
            AggiornaEsempi();
        }

        private void LivelloSetting_CanCodifyHanlder(object sender, ExecuteOperationEventArgs e)
        {

            int Valore;
            //if (int.TryParse(SelectedAttributeCodice, out Valore))
            if (SelectedAttributeCodice != BuiltInCodes.Attributo.Codice && SelectedAttributeCodice != BuiltInCodes.Attributo.Nome)
            {
                //    if (ListaAttributoCodingSetting.Count() == ((AttributoCodingSetting)sender).Livello)
                //        e.Cancel = false;
                //    else
                //        e.Cancel = true;
                //}
                //else
                //{
                e.Force = true;
                e.Cancel = false;
                e.Value = true;
            }
        }
        private void AggiornaEsempi()
        {
            CalloutVisibility = System.Windows.Visibility.Visible;
            foreach (var AttributoCodingSetting in ListaAttributoCodingSetting)
            {
                if (AttributoCodingSetting.Codifica)
                    CalloutVisibility = System.Windows.Visibility.Collapsed;
                EntsCodingHelper.GeneraCodice(AttributoCodingSetting);
            }
        }

        public bool AcceptLocal()
        {
            if (String.IsNullOrEmpty(SelectedAttribute))
                return false;

            AttributoCoding AttributoCoding = null;

            if (ListaAttributoCoding.Where(a => a.AttributoCodice == SelectedAttributeCodice).FirstOrDefault() == null)
            {
                AttributoCoding = new AttributoCoding();
                AttributoCoding.LevelsCoding = new List<AttributoLevelCoding>();
                AttributoCoding.EntityTypeKey = EntityTypeKey;
                AttributoCoding.AttributoCodice = SelectedAttributeCodice;
                AttributoCoding.PosizionamentoRispettoCodiceEsistente = SelectedComportamento.Codice;
                ListaAttributoCoding.Add(AttributoCoding);
            }
            else
            {
                AttributoCoding = ListaAttributoCoding.Where(a => a.AttributoCodice == SelectedAttributeCodice).FirstOrDefault();
                AttributoCoding.PosizionamentoRispettoCodiceEsistente = SelectedComportamento.Codice;
            }

            AttributoLevelCoding AttributoLevelCoding = new AttributoLevelCoding();

            foreach (var AttributoCodingSettingLivello in ListaAttributoCodingSetting)
            {
                AttributoLevelCoding = new AttributoLevelCoding();
                AttributoLevelCoding.AddHigherLevel = AttributoCodingSettingLivello.AggiungiCodiceSuperiore;
                AttributoLevelCoding.IsCoding = AttributoCodingSettingLivello.Codifica;
                AttributoLevelCoding.Level = AttributoCodingSettingLivello.Livello;
                AttributoLevelCoding.Step = AttributoCodingSettingLivello.Passo;
                AttributoLevelCoding.Prefix = AttributoCodingSettingLivello.Prefisso;
                AttributoLevelCoding.Suffix = AttributoCodingSettingLivello.Suffisso;
                AttributoLevelCoding.IncrementalValue = AttributoCodingSettingLivello.ValoreIncrementale;
                if (ListaAttributoCoding.Where(r => r.AttributoCodice == SelectedAttributeCodice).FirstOrDefault().LevelsCoding.Where(t => t.Level == AttributoLevelCoding.Level).FirstOrDefault() == null)
                {
                    AttributoCoding.LevelsCoding.Add(AttributoLevelCoding);
                }
                else
                {
                    ListaAttributoCoding.Where(r => r.AttributoCodice == SelectedAttributeCodice).FirstOrDefault().LevelsCoding.Remove(ListaAttributoCoding.Where(r => r.AttributoCodice == SelectedAttributeCodice).FirstOrDefault().LevelsCoding.Where(t => t.Level == AttributoLevelCoding.Level).FirstOrDefault());
                    ListaAttributoCoding.Where(r => r.AttributoCodice == SelectedAttributeCodice).FirstOrDefault().LevelsCoding.Add(AttributoLevelCoding);
                }
            }

            ListaAttributoCoding.Where(r => r.AttributoCodice == SelectedAttributeCodice).FirstOrDefault().LevelsCoding = ListaAttributoCoding.Where(r => r.AttributoCodice == SelectedAttributeCodice).FirstOrDefault().LevelsCoding.OrderBy(o => o.Level).ToList();
            return true;
        }

        private void GeneraListaGridVuota()
        {
            ListaAttributoCodingSetting.Clear();
            for (int i = 0; i < MaxDepth; i++)
            {
                if (i == 0)
                {
                    AttributoCodingSetting AttributoCodingSetting = new AttributoCodingSetting() { Codifica = false, Livello = i + 1, Passo = 1, ValoreIncrementale = "1" };
                    AttributoCodingSetting.AttributoCodingSettingUpdaterHanlder += LivelloSetting_AttributoCodingSettingUpdaterHanlder;
                    AttributoCodingSetting.CanCodifyHanlder += LivelloSetting_CanCodifyHanlder;
                    ListaAttributoCodingSetting.Add(AttributoCodingSetting);
                }
                else
                {
                    AttributoCodingSetting AttributoCodingSetting = new AttributoCodingSetting() { Codifica = false, Livello = i + 1, AttributoCodingSettingPrecedente = ListaAttributoCodingSetting.ElementAt(i - 1), Passo = 1, Prefisso = ".", ValoreIncrementale = "1", AggiungiCodiceSuperiore = true };
                    AttributoCodingSetting.AttributoCodingSettingUpdaterHanlder += LivelloSetting_AttributoCodingSettingUpdaterHanlder;
                    AttributoCodingSetting.CanCodifyHanlder += LivelloSetting_CanCodifyHanlder;
                    ListaAttributoCodingSetting.Add(AttributoCodingSetting);
                }
            }
        }
    }

}

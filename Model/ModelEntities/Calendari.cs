using CommonResources;
using Commons;
using MasterDetailModel;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [ProtoContract]
    [DataContract]
    [KnownType(typeof(CalendariItemType))]
    public class CalendariItem : Entity
    {
        public CalendariItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.Calendari;
        }

        public WeekHours GetWeekHours()
        {
            WeekHours weekHours = null;
            ValoreTesto valJson = GetValoreAttributo(BuiltInCodes.Attributo.WeekHours, false, false) as ValoreTesto;
            string json = valJson.PlainText;
            if (JsonSerializer.JsonDeserialize(json, out weekHours, typeof(WeekHours)))
                return weekHours;

            return WeekHours.Default;
        }

        public CustomDays GetCustomDays()
        {
            CustomDays customDays = null;
            ValoreTesto valJson = GetValoreAttributo(BuiltInCodes.Attributo.CustomDays, false, false) as ValoreTesto;
            string json = valJson.PlainText;
            if (string.IsNullOrEmpty(json))
                return new CustomDays();

            if (JsonSerializer.JsonDeserialize(json, out customDays, typeof(CustomDays)))
                return customDays;

            return new CustomDays();
        }




    }
    [ProtoContract]
    [DataContract]
    public class CalendariItemType : EntityType
    {
        static string Separator = "_";

        public CalendariItemType() { }

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.Calendari;
            Name = LocalizationProvider.GetString("Calendari");
            FunctionName = EAtCalculatorFunction.FunctionName;
            string codiceAttributo;
            int viewOrder = 0;
            //IsTreeMaster = false;
            Attributo att = null;

            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);

            //Attributi.Clear();
            AttributiMasterCodes.Clear();

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.IsDigicorpOwner);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Booleano].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("IsDigicorpOwner"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Dettagli"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    ValoreDefault = new ValoreBooleano(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;
            att.IsValoreReadOnly = false;
            att.DetailViewOrder = viewOrder++;
            att.IsInternal = true;
            
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Codice);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    GroupName = LocalizationProvider.GetString("Dettagli"),
                    IsVisible = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            att.DetailViewOrder = viewOrder++;
            att.GroupOperation = ValoreOperationType.Equivalent;
            att.Etichetta = LocalizationProvider.GetString("Nome");
            AttributiMasterCodes.Add(codiceAttributo);
            
            //

            //codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.GiorniLavorativi);
            //List<ValoreAttributoElencoItem> ggSettimana = new List<ValoreAttributoElencoItem>();
            //ggSettimana.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Lunedi"), Id = 1 });
            //ggSettimana.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Martedi"), Id = 2 });
            //ggSettimana.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Mercoledi"), Id = 4 });
            //ggSettimana.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Giovedi"), Id = 8 });
            //ggSettimana.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Venerdi"), Id = 16 });
            //ggSettimana.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Sabato"), Id = 32 });
            //ggSettimana.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Domenica"), Id = 64 });
            //if (!Attributi.ContainsKey(codiceAttributo))
            //{
            //    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
            //    {
            //        IsBuiltIn = true,
            //    });

            //}
            //att = Attributi[codiceAttributo];
            //att.ValoreAttributo = new ValoreAttributoElenco() { Items = ggSettimana, IsMultiSelection = true };
            //att.Etichetta = LocalizationProvider.GetString("GiorniLavorativi");
            //att.AllowSort = true;
            //att.AllowMasterGrouping = true;
            //att.AllowValoriUnivoci = true;
            //att.GroupName = LocalizationProvider.GetString("Dettagli");
            //att.IsVisible = true;
            //att.IsValoreReadOnly = false;
            //att.DetailViewOrder = viewOrder++;
            //att.ValoreDefault = new ValoreElenco() { ValoreAttributoElencoId = 31 };

            ////
            //codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.OrarioLavoro);
            //if (!Attributi.ContainsKey(codiceAttributo))
            //{
            //    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
            //    {

            //        IsBuiltIn = true,
            //        AllowValoriUnivoci = true,
            //        AllowSort = true,
            //        DetailViewOrder = viewOrder++,
            //        GroupName = LocalizationProvider.GetString("Dettagli"),
            //        IsVisible = true,
            //        IsValoreReadOnly = false,
            //        AllowMasterGrouping = true,
            //        GroupOperation = GroupOperationType.Equivalent,
            //    });
            //}
            //att = Attributi[codiceAttributo];
            //att.AllowMasterGrouping = false;
            //att.AllowSort = false;
            //att.DetailViewOrder = viewOrder++;
            //att.Etichetta = LocalizationProvider.GetString("OrarioLavoro");
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.WeekHoursText);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {

                    IsBuiltIn = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            att.IsValoreReadOnly = true;
            att.AllowReplaceInText = false;
            att.DetailViewOrder = viewOrder++;
            att.GroupName = LocalizationProvider.GetString("Dettagli");
            att.Height = 20;
            att.IsVisible = true;
            att.Etichetta = LocalizationProvider.GetString("OrarioLavoro");
            att.IsExpanded = true;
            //
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.WeekHours);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    IsBuiltIn = true,
                    IsInternal = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = false;
            att.AllowReplaceInText = false;
            att.DetailViewOrder = viewOrder++;
            att.GroupName = LocalizationProvider.GetString("Dettagli");
            att.Height = 20;
            att.IsVisible = false;
            att.IsInternal = true;
            att.Etichetta = "OrarioLavoroJson";
            
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.CustomDaysText);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {

                    IsBuiltIn = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            att.IsValoreReadOnly = true;
            att.AllowReplaceInText = false;
            att.DetailViewOrder = viewOrder++;
            att.GroupName = LocalizationProvider.GetString("Dettagli");
            att.Height = 20;
            att.IsVisible = true;
            att.Etichetta = LocalizationProvider.GetString("Eccezioni");
            
            //
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.CustomDays);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    //Etichetta = "CustomDaysJson",
                    IsBuiltIn = true,
                    IsInternal = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = false;
            att.AllowReplaceInText = false;
            att.DetailViewOrder = viewOrder++;
            att.GroupName = LocalizationProvider.GetString("Dettagli");
            att.Height = 20;
            att.IsVisible = false;
            att.IsInternal = true;
            att.Etichetta = "CustomDaysJson";


        }
        public override EntityType Clone()
        {

            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            EntityType clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        //internal static string DivisioneAttributoCodice(string divTypeKey, string divAttCodice)
        //{
        //    string str = string.Join(Separator, divTypeKey, divAttCodice);
        //    return str;
        //}

        static public string CreateKey()
        {
            return BuiltInCodes.EntityType.Calendari;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Calendari;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Calendari;

        public override MasterType MasterType => MasterType.List;

        public override EntityComparer EntityComparer { get; set; } = new CalendariItemKeyComparer();

        public override bool IsCustomizable() { return false; }
    }

    [ProtoContract]
    [DataContract]
    [KnownType(typeof(CalendariItem))]
    public class CalendariItemCollection : EntityCollection
    {
    }

    public class CalendariItemKeyComparer : EntityComparer
    {
        public override List<string> AttributiCode { get; set; } = new List<string>()
        { BuiltInCodes.Attributo.Codice };

        public override bool Equals(string key1, string key2)
        {
            if (key1 == key2)
                return true;

            return false;
        }
    }
}

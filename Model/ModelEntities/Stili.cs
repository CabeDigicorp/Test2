using _3DModelExchange;
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
using System.Windows.Media;

namespace Model
{
    [ProtoContract]
    [DataContract]
    [KnownType(typeof(StiliItemType))]
    public class StiliItem : Entity
    {

        public StiliItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.Stili;
        }

        public Guid GetDivisioneItemId(string codiceAttributoGuid)
        {
            return GetAttributoGuidId(codiceAttributoGuid);
            //ValoreGuid valGuid = Attributi[codiceAttributoGuid].Valore as ValoreGuid;
            //return valGuid == null ? Guid.Empty : valGuid.V;
        }
    }

    [ProtoContract]
    [DataContract]
    public class StiliItemType : EntityType
    {
        private static string Separator = "_";

        public StiliItemType() { }//protobuf

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.Stili;
            Name = LocalizationProvider.GetString("Stili");
            string codiceAttributo;
            int viewOrder = 0;
            FunctionName = ElmCalculatorFunction.FunctionName;
            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);
            Attributo att = null;

            //Attributi.Clear();
            AttributiMasterCodes.Clear();


            IEnumerable<DivisioneItemType> divisioniType = entityTypes.Values.Where(item => item is DivisioneItemType).Cast<DivisioneItemType>();
            ////////////////////////////////////////
            //Codice
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Codice);
            ValoreAttributoElenco valAttEl = new ValoreAttributoElenco()
            {
                Type = ValoreAttributoElencoType.Default,
                Items = new List<ValoreAttributoElencoItem>() {
                    new ValoreAttributoElencoItem (){Id = 1, Text = "Normal"},
                    new ValoreAttributoElencoItem (){Id = 2, Text = "Heading 1"},
                    new ValoreAttributoElencoItem (){Id = 3, Text = "Heading 2"},
                    new ValoreAttributoElencoItem (){Id = 4, Text = "Heading 3"},
                    new ValoreAttributoElencoItem (){Id = 5, Text = "Heading 4"},
                    new ValoreAttributoElencoItem (){Id = 6, Text = "Heading 5"},
                    new ValoreAttributoElencoItem (){Id = 7, Text = "Heading 6"},
                    new ValoreAttributoElencoItem (){Id = 8, Text = "Heading 7"},
                    new ValoreAttributoElencoItem (){Id = 9, Text = "Heading 8"},
                    new ValoreAttributoElencoItem (){Id = 10, Text = "Heading 9"},
                    new ValoreAttributoElencoItem (){Id = 11, Text = "Title"},
                    new ValoreAttributoElencoItem (){Id = 12, Text = "Subtitle"},
                    new ValoreAttributoElencoItem (){Id = 13, Text = "Subtle Emphasis"},
                    new ValoreAttributoElencoItem (){Id = 14, Text = "Emphasis"},
                    new ValoreAttributoElencoItem (){Id = 15, Text = "Strong"},
                    new ValoreAttributoElencoItem (){Id = 16, Text = "Quote"},
                    new ValoreAttributoElencoItem (){Id = 17, Text = "Intense Quote"},
                    new ValoreAttributoElencoItem (){Id = 18, Text = "Subtle Reference"},
                    new ValoreAttributoElencoItem (){Id = 19, Text = "Intense Reference"},
                    new ValoreAttributoElencoItem (){Id = 20, Text = "Caption"},
                    new ValoreAttributoElencoItem (){Id = 21, Text = "Hyperlink"},
                    }
            };

            if (Attributi.ContainsKey(codiceAttributo)) //elimino il vecchio attributo codice di tipo Testo
            {
                if (Attributi[codiceAttributo].DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.Elenco)
                    Attributi.Remove(codiceAttributo);
            }
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("Codice"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("ImpostazioniStile"),
                    
                });
            }
            att = Attributi[codiceAttributo];
            att.IsVisible = true;
            att.IsAdvanced = false;
            att.DetailViewOrder = viewOrder++;
            att.IsValoreReadOnly = false;
            att.ValoreAttributo = valAttEl;
            att.Etichetta = LocalizationProvider.GetString("Codice");
            att.GroupName = LocalizationProvider.GetString("ImpostazioniStile");
            AttributiMasterCodes.Add(codiceAttributo);


            //codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Codice);
            //if (!Attributi.ContainsKey(codiceAttributo))
            //{
            //    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
            //    {
            //        Etichetta = LocalizationProvider.GetString("Codice"),
            //        IsBuiltIn = true,
            //        AllowValoriUnivoci = true,
            //        AllowSort = true,
            //        AllowReplaceInText = true,
            //        DetailViewOrder = viewOrder++,
            //        GroupName = LocalizationProvider.GetString("ImpostazioniStile"),
            //    });
            //}
            //att = Attributi[codiceAttributo];
            //att.IsVisible = MainViewStatus.IsAdvancedMode;
            //att.IsAdvanced = MainViewStatus.IsAdvancedMode;
            //att.DetailViewOrder = viewOrder++;
            //att.IsValoreReadOnly = false;


            /////////////////////////////
            //Nome
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Nome);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("Nome"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("ImpostazioniStile"),
                    IsVisible = true,
                    //IsValoreReadOnly = true,
                });
                
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            att.Etichetta = LocalizationProvider.GetString("Nome");
            att.GroupName = LocalizationProvider.GetString("ImpostazioniStile");
            AttributiMasterCodes.Add(codiceAttributo);
            //

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Carattere);
            ValoreAttributoElenco elencoFont = new ValoreAttributoElenco();
            elencoFont.Type = ValoreAttributoElencoType.Font;
            foreach (var item in Fonts.SystemFontFamilies.OrderBy(x => x.Source))
            {
                elencoFont.Items.Add(new ValoreAttributoElencoItem()
                {
                    Text = item.Source,
                    Id = -1,//elencoFont.NewId(), //NB: non è possibile identificare il font con un intero (l'Id è il nome stesso del font)
                }) ;
            }
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("TipoCarattere"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Carattere"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    //ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                    //ValoreAttributo = elencoFont,
                    ValoreDefault = new ValoreElenco(),

                });
                
                viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("TipoCarattere");
            att.GroupName = LocalizationProvider.GetString("Carattere");
            att.DetailViewOrder = viewOrder++;
            //(att.ValoreAttributo as ValoreAttributoElenco).Type = ValoreAttributoElencoType.Font;
            att.ValoreAttributo = elencoFont;
            AttributiMasterCodes.Add(codiceAttributo);
            //
            //
            //codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Stile);
            //if (!Attributi.ContainsKey(codiceAttributo))
            //{
            //    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
            //    {
            //        Etichetta = LocalizationProvider.GetString("Stile"),
            //        IsBuiltIn = true,
            //        AllowSort = true,
            //        AllowValoriUnivoci = true,
            //        DetailViewOrder = viewOrder++,
            //        GroupName = LocalizationProvider.GetString("Carattere"),
            //        IsVisible = true,
            //        IsValoreReadOnly = false,
            //        ValoreAttributo = new ValoreAttributoElenco(),
            //        ValoreDefault = new ValoreElenco(),
            //    });
            //    viewOrder++;
            //}
            //att = Attributi[codiceAttributo];
            //att.DetailViewOrder = viewOrder++;
            //att.IsVisible = false;
            //
            ///////////////////////////////////////////////////////////
            //DimensioneCarattere
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.DimensioneCarattere);
            List<ValoreAttributoElencoItem>  Lista = new List<ValoreAttributoElencoItem>();
            foreach (var item in new List<string>() { "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "36", "48", "72" }) { Lista.Add(new ValoreAttributoElencoItem() { Text = item, Id = int.Parse(item) }); };
            
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("DimensioneCarattere"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Carattere"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    //ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                    ValoreDefault = new ValoreElenco(),
                });
                viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("DimensioneCarattere");
            att.GroupName = LocalizationProvider.GetString("Carattere");
            att.DetailViewOrder = viewOrder++;
            att.ValoreAttributo = new ValoreAttributoElenco() { Items = Lista };
            AttributiMasterCodes.Add(codiceAttributo);

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Grassetto);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Booleano].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("Grassetto"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Carattere"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    ValoreDefault = new ValoreBooleano(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Grassetto");
            att.GroupName = LocalizationProvider.GetString("Carattere");
            att.DetailViewOrder = viewOrder++;

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Italic);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Booleano].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("Corsivo"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Carattere"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    ValoreDefault = new ValoreBooleano(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Corsivo");
            att.GroupName = LocalizationProvider.GetString("Carattere");
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Barrato);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Booleano].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("Barrato"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Carattere"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    ValoreDefault = new ValoreBooleano(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Barrato");
            att.GroupName = LocalizationProvider.GetString("Carattere");
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Sottolineato);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Booleano].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("Sottolineato"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Carattere"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    ValoreDefault = new ValoreBooleano(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Sottolineato");
            att.GroupName = LocalizationProvider.GetString("Carattere");
            att.DetailViewOrder = viewOrder++;

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.ColoreCarattere);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Colore].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("ColoreCarattere"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Carattere"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    ValoreAttributo = new ValoreAttributoColore(),
                    ValoreDefault = new ValoreColore(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("ColoreCarattere");
            att.GroupName = LocalizationProvider.GetString("Carattere");
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Allineamento);
            
            Lista = new List<ValoreAttributoElencoItem>();
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Sinistra"), Id = (int) TextAlignmentEnum.Left});
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Centro"), Id = (int)TextAlignmentEnum.Center });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Destra"), Id = (int)TextAlignmentEnum.Right });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Giustifica"), Id = (int)TextAlignmentEnum.Justify });
            //foreach (var item in new List<string>() { "Sinistra", "Centro", "Destra", "Giustifica"}) { Lista.Add(new ValoreAttributoElencoItem() { Text = item, Id = Guid.NewGuid() }); };
            
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("Allineamento"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Paragrafo"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    //ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                    ValoreDefault = new ValoreElenco(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Allineamento");
            att.GroupName = LocalizationProvider.GetString("Paragrafo");
            att.DetailViewOrder = viewOrder++;
            att.ValoreAttributo = new ValoreAttributoElenco() { Items = Lista };

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.ColoreSfondo);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Colore].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("ColoreSfondo"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Paragrafo"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    ValoreAttributo = new ValoreAttributoColore(),
                    ValoreDefault = new ValoreColore(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("ColoreSfondo");
            att.GroupName = LocalizationProvider.GetString("Paragrafo");
            att.DetailViewOrder = viewOrder++;

            UpdateEtichetteMap();
        }

        private void AddAttributo(DefinizioneAttributo definizioneAttributo, string codice, string etichetta, bool allowReplaceInText)
        {
            Attributi.Add(codice, new Attributo(definizioneAttributo.Clone(), this, codice) { Etichetta = etichetta, AllowReplaceInText = allowReplaceInText });
        }

        public override EntityType Clone()
        {

            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            EntityType clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public static string DivisioneAttributoCodice(string divTypeKey, string divAttCodice)
        {
            string str = string.Join(Separator, divTypeKey, divAttCodice);
            return str;
        }

        static public string CreateKey()
        {
            return BuiltInCodes.EntityType.Stili;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Stili;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Stili;

        public override MasterType MasterType => MasterType.List;
        public override EntityComparer EntityComparer { get; set; } = new StiliItemKeyComparer();

        public override bool IsCustomizable() { return false; }

    }

    [ProtoContract]
    [DataContract]
    public class StiliItemCollection : EntityCollection
    {
    }

    public class StiliItemKeyComparer : EntityComparer
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

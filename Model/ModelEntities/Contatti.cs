using CommonResources;
using Commons;
using MasterDetailModel;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Model
{
    [ProtoContract]
    [DataContract]
    [KnownType(typeof(ContattiItemType))]
    public class ContattiItem : Entity
    {
        
        public ContattiItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.Contatti;
        }

        //public Guid GetDivisioneItemId(string codiceAttributoGuid)
        //{
        //    return GetAttributoGuidId(codiceAttributoGuid);

        //    //ValoreGuid valGuid = Attributi[codiceAttributoGuid].Valore as ValoreGuid;
        //    //return valGuid == null ? Guid.Empty : valGuid.V;
        //}

        public override string ToUserIdentity(UserIdentityMode mode)
        {
            string nome = GetValoreAttributo(BuiltInCodes.Attributo.Nome, false, true).PlainText;
            string cognome = GetValoreAttributo(BuiltInCodes.Attributo.Cognome, false, true).PlainText;

            if (mode == UserIdentityMode.SingleLine1)
                return nome;

            if (mode == UserIdentityMode.SingleLine2)
                return cognome;

            return string.Format("{0} {1}", nome, cognome);
        }

        //public override string AsString()
        //{
        //    string result = "";
        //    Entity entIter = this;
        //    while (entIter != null)
        //    {
        //        string str = "";
        //        Valore valCodice = entIter.GetValoreAttributo("Codice", false, true);//codice
        //        Valore valDesc = entIter.GetValoreAttributo("DescrizioneRTF", false, true);

        //        if (valCodice != null)
        //            str = valCodice.ToPlainText();

        //        if (valDesc != null)
        //            str += " " + valDesc.ToPlainText();

        //        if (result.Any())
        //            result = string.Join(HierarchySeparator, str, result);
        //        else
        //            result = str;

        //        entIter = entIter.Parent;
        //    }
        //    return result;
        //}

    }

    [ProtoContract]
    [DataContract]
    public class ContattiItemType : EntityType
    {
        static string Separator = "_";

        public ContattiItemType() { }//protobuf

        //public ContattiItemParentType ContattiItemParentType { get; set; }

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.Contatti;
            Name = LocalizationProvider.GetString("Contatti");
            FunctionName = CntCalculatorFunction.FunctionName;
            string codiceAttributo;
            int viewOrder = 0;
            //IsTreeMaster = false;
            Attributo att = null;
            
            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);

            //Attributi.Clear();
            //AttributiMasterCodes.Clear();

            codiceAttributo = string.Join(Separator, "Nome");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Nome Ragione sociale"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Anagrafica"),
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;

            //
            codiceAttributo = string.Join(Separator, "Cognome");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Cognome"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Anagrafica"),
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            //  
            
            codiceAttributo = string.Join(Separator, "CF");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Codice fiscale"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Anagrafica"),
                    IsVisible = true,
                });
                
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;

            bool isEmptyModel = true;
            if (!isEmptyModel)
            {
                //
                codiceAttributo = string.Join(Separator, "Indirizzo");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Indirizzo"),
                        IsBuiltIn = false,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Anagrafica"),
                        IsVisible = true,
                    });

                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;
                //
                codiceAttributo = string.Join(Separator, "CAP");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("CAP"),
                        IsBuiltIn = false,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Anagrafica"),
                        IsVisible = true,
                    });

                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;

                //
                codiceAttributo = string.Join(Separator, "Comune");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Comune"),
                        IsBuiltIn = false,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Anagrafica"),
                        IsVisible = true,
                    });
                    AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;
                //
                codiceAttributo = string.Join(Separator, "Provincia");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Provincia"),
                        IsBuiltIn = false,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Anagrafica"),
                        IsVisible = true,
                    });

                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;


                //
                codiceAttributo = string.Join(Separator, "PI");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Partita IVA"),
                        IsBuiltIn = false,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Anagrafica"),
                        IsVisible = true,
                    });
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;
                //
                codiceAttributo = string.Join(Separator, "Telefono");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoCollection].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Telefono"),
                        IsBuiltIn = false,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Anagrafica"),
                        IsVisible = true,
                    });
                }
                att = Attributi[codiceAttributo];
                att.AllowValoriUnivoci = true;
                att.AllowSort = false;
                att.AllowMasterGrouping = false;
                att.AllowReplaceInText = false;
                att.Height = 60;



                //
                codiceAttributo = string.Join(Separator, "Email");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoCollection].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Email"),
                        IsBuiltIn = false,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Anagrafica"),
                        IsVisible = true,
                    });
                }
                att = Attributi[codiceAttributo];
                att.AllowValoriUnivoci = true;
                att.AllowSort = false;
                att.AllowMasterGrouping = false;
                att.AllowReplaceInText = false;
                att.Height = 60;

                //
                codiceAttributo = string.Join(Separator, "CCIAA");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Iscrizione CCIAA"),
                        IsBuiltIn = false,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Impresa"),
                        IsVisible = false,
                    });

                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;

                //
                codiceAttributo = string.Join(Separator, "INPS");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Matricola INPS"),
                        IsBuiltIn = false,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Impresa"),
                        IsVisible = false,
                    });

                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;

                //
                codiceAttributo = string.Join(Separator, "INAIL");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Posizione INAIL"),
                        IsBuiltIn = false,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Impresa"),
                        IsVisible = false,
                    });

                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;
            }

            UpdateEtichetteMap();
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
            return BuiltInCodes.EntityType.Contatti;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Contatti;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Contatti;

        public override MasterType MasterType => MasterType.List;

        public override EntityComparer EntityComparer { get; set; } = new ContattiItemKeyComparer();

        public override bool IsCustomizable() { return true; }
    }


    [ProtoContract]
    [DataContract]
    [KnownType(typeof(ContattiItem))]
    public class ContattiItemCollection : EntityCollection
    {
    }

    public class ContattiItemKeyComparer : EntityComparer
    {
        public override List<string> AttributiCode { get; set; } = new List<string>()
        { BuiltInCodes.Attributo.CF, BuiltInCodes.Attributo.Nome, BuiltInCodes.Attributo.Cognome };

        public override bool Equals(string key1, string key2)
        {
            string[] key1s = key1.Split(KeySeparator.ToCharArray());
            string[] key2s = key2.Split(KeySeparator.ToCharArray());

            //Se è uguale il codice fiscale
            if (key1s[0] == key2s[0])
                return true;

            //Se è uguale il nome e il cognome
            if (key1s.Length > 1 && key2s.Length > 1 && key1s[1] == key2s[1])
                if (key1s.Length > 2 && key2s.Length > 2 && key1s[2] == key2s[2])
                    return true;

            return false;
        }
    }


}
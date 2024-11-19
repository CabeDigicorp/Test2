using _3DModelExchange;
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
    [KnownType(typeof(DivisioneItemType))]
    [KnownType(typeof(DivisioneItemParentType))]
    public class DivisioneItem : TreeEntity
    {

        public DivisioneItem()//per protobuf
        {
        }

        public DivisioneItem(Guid divTypeId)
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = DivisioneItemType.CreateKey(divTypeId);
            ParentEntityTypeCodice = DivisioneItemParentType.CreateKey(divTypeId);
        }

        public override void CopyValoriAttributiFrom(Entity ent)
        {
            TreeEntity treeEnt = ent as TreeEntity;

            //bool isParent = true;
            //if (Attributi.Count > ParentEntityType.Attributi.Count)
            //    isParent = false;

            //bool isEntParent = true;
            //if (treeEnt.Attributi.Count > treeEnt.ParentEntityType.Attributi.Count)
            //    isEntParent = false;

            bool isParent = IsParent;
            bool isEntParent = treeEnt.IsParent;


            if (isParent == isEntParent)
                base.CopyValoriAttributiFrom(ent);
            else if (isEntParent)
            {
                //EntityAttributo entAtt = Attributi.First(item => item.Attributo.Codice == "PrezzarioItem_Codice");
                //entAtt.Valore = ent.Attributi.First(item => item.Attributo.Codice == "PrezzarioItem_Codice").Valore.Clone();

                EntityAttributo entAtt = Attributi["Nome"];/*PrezzarioItem_*/
                entAtt.Valore = ent.Attributi["Nome"].Valore.Clone();/*PrezzarioItem_*/
            }

        }

        public override string ToUserIdentity(UserIdentityMode mode)
        {
            string result = string.Empty;

            if (mode == UserIdentityMode.Deep)
            {
                
                TreeEntity entIter = this;
                while (entIter != null)
                {
                    string str = new string(' ', entIter.Depth * 2);
                    Valore valNome = entIter.GetValoreAttributo(BuiltInCodes.Attributo.Nome, false, false);//codice
                    Valore valDesc = entIter.GetValoreAttributo(BuiltInCodes.Attributo.Descrizione, false, false);

                    if (valNome != null)
                        str += valNome.ToPlainText();

                    string desc = valDesc.ToPlainText();
                    if (valDesc != null && desc.Any())
                    {
                        str = string.Format("{0} - {1}", str, desc);
                    }

                    if (result.Any())
                        result = string.Join("\n", str, result);
                    else
                        result = str;

                    entIter = entIter.Parent;
                }
            }
            else if (mode == UserIdentityMode.SingleLine)
            {
                //{codice foglia} - {descrizione nonno} \ {descrizione padre} \ {descrizione foglia}

                TreeEntity entIter = this;
                Valore valCodice = null;
                string leafCod = null;

                while (entIter != null)
                {

                    valCodice = entIter.GetValoreAttributo(BuiltInCodes.Attributo.Nome, false, true);//codice
                    if (valCodice != null && leafCod == null)
                        leafCod = valCodice.ToPlainText();

                    string desc = string.Empty;
                    ValoreTestoRtf valDesc = entIter.GetValoreAttributo(BuiltInCodes.Attributo.Descrizione, false, true) as ValoreTestoRtf;
                    if (valDesc != null)
                        desc = valDesc.BriefPlainText;


                    if (result.Any())
                        result = string.Format("{0} \\ {1}", desc, result);
                    else
                    {
                        result = desc;
                    }

                    entIter = entIter.Parent;
                }

                result = string.Format("{0} - {1}", leafCod, result);


            }
            else if (mode == UserIdentityMode.SingleLine1)
            {
                Valore valNome = GetValoreAttributo(BuiltInCodes.Attributo.Nome, false, true);
                if (valNome != null)
                    result = valNome.ToPlainText();
            }
            else if (mode == UserIdentityMode.SingleLine2)
            {
                Valore valDesc = GetValoreAttributo(BuiltInCodes.Attributo.Descrizione, false, true);
                if (valDesc != null)
                    result = valDesc.ToPlainText();
            }
            else
            {
                Valore valNome = GetValoreAttributo(BuiltInCodes.Attributo.Nome, false, true);
                Valore valDesc = GetValoreAttributo(BuiltInCodes.Attributo.Descrizione, false, true);

                if (valDesc.PlainText.Any())
                    result = string.Format("{0} - {1}", valNome.PlainText, valDesc.PlainText);
                else
                    result = valNome.PlainText;
            }
            return result;
        }



    }

    [ProtoContract]
    [DataContract]
    public class DivisioneItemType : TreeEntityType
    {
        [ProtoMember(1)]
        [DataMember]
        public Guid DivisioneId { get; set; }

        [ProtoMember(2)]
        [DataMember]
        public Model3dClassEnum Model3dClassName {get;set;}

        [ProtoMember(3)]
        [DataMember]
        public bool IsBuiltIn { get; set; } = false;

        [ProtoMember(4)]
        [DataMember]
        public int Position { get; set; } = -1;

        public DivisioneItemType() { }//protobuf

        public DivisioneItemType(Guid id, string codice, string name, Model3dClassEnum model3dClassName)
        {
            DivisioneId = id;
            Name = name;
            Codice = codice;
            Model3dClassName = model3dClassName;
        }

        //public DivisioneItemParentType DivisioneItemParentType { get; set; }

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)/*, Guid id, string nomeDivisione*/
        {
            FunctionName = "att";
            string codiceAttributo;
            int viewOrder = 0;
            
            string sep = "_";
            Attributo att = null;

            AttributiMasterCodes.Clear();

            if (Model3dHelper.GetModel3dType(Model3dClassName) == Model3dType.Unknown)
            {
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.Nome);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Nome"),
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        IsVisible = true,
                        IsBuiltIn = true,
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;
                att.AllowValoriUnivoci = true;
                att.AllowSort = true;
                AttributiMasterCodes.Add(codiceAttributo);


                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.Descrizione);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Descrizione"),
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        IsVisible = false,
                        IsBuiltIn = true,
                    });

                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;
                att.AllowValoriUnivoci = true;
                att.AllowSort = true;
                AttributiMasterCodes.Add(codiceAttributo);

                //
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.DescrizioneRTF);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                    {
                        Etichetta = string.Format("{0} RTF", LocalizationProvider.GetString("Descrizione")),
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        Height = 20,
                        IsVisible = false,
                        IsBuiltIn = true,
                        ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault },
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = false;//true

            }
            else if (Model3dHelper.GetModel3dType(Model3dClassName) == Model3dType.Ifc)
            {

                //aggiunta di attributi di competenza del modello (potrà essere rimosso)
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.Nome);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Nome"),
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        IsVisible = true,
                        IsBuiltIn = true,
                        ValoreDefault = new ValoreTesto() { V = "ifc{Name}" },
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;
                att.AllowValoriUnivoci = true;
                att.AllowSort = true;
                AttributiMasterCodes.Add(codiceAttributo);


                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.Descrizione);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Descrizione"),
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        IsVisible = false,
                        IsBuiltIn = true,
                    });

                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;
                att.AllowValoriUnivoci = true;
                att.AllowSort = true;
                AttributiMasterCodes.Add(codiceAttributo);

                //
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.DescrizioneRTF);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                    {
                        Etichetta = string.Format("{0} RTF", LocalizationProvider.GetString("Descrizione")),
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        Height = 20,
                        IsVisible = false,
                        IsBuiltIn = true,
                        ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault },
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = false;//true


                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.Nome);
                att = Attributi[codiceAttributo];

                //
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.IfcLabel);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Etichetta Ifc"),
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        IsVisible = true,
                        ValoreDefault = new ValoreTesto() { V = "ifc{_Label}" },
                        IsValoreLockedByDefault = true,
                        IsBuiltIn = false,
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;

                //
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.IfcClass);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Classe Ifc"),
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        IsVisible = true,
                        ValoreDefault = new ValoreTesto() { V = "ifc{_Class}" },
                        IsValoreLockedByDefault = true,
                        IsBuiltIn = false,
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;

                //
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.IfcFileName);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("NomeFileIfc"),
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        AllowMasterGrouping = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Nothing,
                        IsBuiltIn = false,
                        ValoreDefault = new ValoreTesto() { V = "ifc{_FileName}" },
                        IsValoreLockedByDefault = true,
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;

                //
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.ProjectGlobalId);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("ProjectGlobalId"),
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        AllowMasterGrouping = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Nothing,
                        IsBuiltIn = true,
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;

                //
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.GlobalId);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("GlobalId"),
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        AllowMasterGrouping = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Nothing,
                        ValoreDefault = new ValoreTesto() { V = "ifc{GlobalId}" },
                        IsValoreLockedByDefault = true,
                        IsBuiltIn = true,
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;


            }
            else if (Model3dHelper.GetModel3dType(Model3dClassName) == Model3dType.Revit)
            {

                //aggiunta di attributi di competenza del modello (potrà essere rimosso)
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.Nome);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Nome"),
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        IsVisible = true,
                        IsBuiltIn = true,
                        ValoreDefault = new ValoreTesto() { V = "rvt{_Name}" },
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;
                att.AllowValoriUnivoci = true;
                att.AllowSort = true;
                AttributiMasterCodes.Add(codiceAttributo);


                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.Descrizione);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Descrizione"),
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        IsVisible = false,
                        IsBuiltIn = true,
                    });

                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;
                att.AllowValoriUnivoci = true;
                att.AllowSort = true;
                AttributiMasterCodes.Add(codiceAttributo);

                //
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.DescrizioneRTF);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                    {
                        Etichetta = string.Format("{0} RTF", LocalizationProvider.GetString("Descrizione")),
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        Height = 20,
                        IsVisible = false,
                        IsBuiltIn = true,
                        ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault },
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = false;//true

                ////
                //codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.Mark);
                //if (!Attributi.ContainsKey(codiceAttributo))
                //{
                //    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                //    {
                //        Etichetta = LocalizationProvider.GetString("Mark"),
                //        AllowValoriUnivoci = true,
                //        AllowSort = true,
                //        AllowReplaceInText = true,
                //        DetailViewOrder = viewOrder++,
                //        GroupName = LocalizationProvider.GetString("Dettagli"),
                //        IsVisible = true,
                //        ValoreDefault = new ValoreTesto() { V = string.Format("rvt{{_Mark}}") },
                //        IsValoreLockedByDefault = true,
                //        IsBuiltIn = false,
                //    });
                //    //AttributiMasterCodes.Add(codiceAttributo);
                //}
                //att = Attributi[codiceAttributo];
                //att.AllowMasterGrouping = true;

                //
                codiceAttributo = string.Join(sep, "RvtCategory");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Categoria"),
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        IsVisible = true,
                        ValoreDefault = new ValoreTesto() { V = "rvt{_Category}" },
                        IsValoreLockedByDefault = true,
                        IsBuiltIn = false,
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;

                //
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.RvtFileName);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("NomeFileRvt"),
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        AllowMasterGrouping = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Nothing,
                        IsBuiltIn = false,
                        ValoreDefault = new ValoreTesto() { V = "rvt{_FileName}" },
                        IsValoreLockedByDefault = true,
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;

                //
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.ProjectGlobalId);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("ProjectGlobalId"),
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        AllowMasterGrouping = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Nothing,
                        IsBuiltIn = true,
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;

                //
                codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.GlobalId);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("GlobalId"),
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Dettagli"),
                        AllowMasterGrouping = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Nothing,
                        ValoreDefault = new ValoreTesto() { V = "ifc{GlobalId}" },
                        IsValoreLockedByDefault = true,
                        IsBuiltIn = true,
                    });
                    //AttributiMasterCodes.Add(codiceAttributo);
                }
                att = Attributi[codiceAttributo];
                att.AllowMasterGrouping = true;


            }
            else
            {
                foreach (Attributo attr in Attributi.Values.Where(item => item.Codice != BuiltInCodes.Attributo.Codice && item.Codice != BuiltInCodes.Attributo.Descrizione && item.Codice != BuiltInCodes.Attributo.DescrizioneRTF))
                    attr.IsBuiltIn = false;
            }

            

            UpdateEtichetteMap();

        }

        public override string GetKey()
        {
            return CreateKey(DivisioneId);
        }

        static public string CreateKey(Guid id)
        {
            return string.Format("{0}-{1}", BuiltInCodes.EntityType.Divisione, id.ToString());
        }

        public override bool AttributoIsPreviewable(string referenceCodiceAttributo)
        {
            return AssociedType.Attributi.ContainsKey(referenceCodiceAttributo);
        }

        public override void ResolveReferences(Dictionary<string, EntityType> entityTypes, Dictionary<string, DefinizioneAttributo> definizioniAttributo)
        {
            string divItemParentTypeKey = DivisioneItemParentType.CreateKey(DivisioneId);
            AssociedType = entityTypes[divItemParentTypeKey] as DivisioneItemParentType;
            base.ResolveReferences(entityTypes, definizioniAttributo);
        }

        public override EntityType Clone()
        {

            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            EntityType clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Divisione;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Divisione;

        public override MasterType MasterType => MasterType.Tree;

        public override EntityComparer EntityComparer { get; set; } = new DivisioneItemKeyComparer();

        public override bool IsCustomizable() { return true; }


    }


    [ProtoContract]
    [DataContract]
    [KnownType(typeof(DivisioneItem))]
    public class DivisioneItemCollection : TreeEntityCollection
    {
    }

    [ProtoContract]
    [DataContract]
    public class DivisioneItemParentType : TreeEntityType
    {
        [ProtoMember(1)]
        [DataMember]
        public Guid DivisioneId { get; set; }

        //[ProtoMember(2)]
        //[DataMember]
        //public string Model3dClassName { get; set; }

        [ProtoMember(2)]
        [DataMember]
        public Model3dClassEnum Model3dClassName { get; set; }

        [ProtoMember(3)]
        [DataMember]
        public bool IsBuiltIn { get; set; } = false;

        public DivisioneItemParentType() { }//protobuf

        public DivisioneItemParentType(Guid id, string codice, string name, Model3dClassEnum model3dClassName)
        {
            DivisioneId = id;
            Name = name;
            Codice = codice;
            Model3dClassName = model3dClassName;
        }

        //public DivisioneItemType DivisioneItemType { get; set; }

        public DivisioneItemType GetAssociedType() { return AssociedType as DivisioneItemType; }

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)/*, Guid id*/
        {
            string codiceAttributo; //CodiceEntity + "_" + code
            int viewOrder = 0;
            FunctionName = "";
            string sep = "_";
            Attributo att = null;

            AttributiMasterCodes.Clear();

            codiceAttributo = string.Join(sep, "Nome");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Nome"),
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Dettagli"),
                    IsVisible = true,
                    IsBuiltIn = true,
                });
                
            }
            AttributiMasterCodes.Add(codiceAttributo);

            //
            codiceAttributo = string.Join(sep, "Descrizione");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Descrizione"),
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Dettagli"),
                    IsVisible = false,
                    IsBuiltIn = true,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            AttributiMasterCodes.Add(codiceAttributo);


            codiceAttributo = string.Join(sep, "DescrizioneRTF");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Descrizione"),
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Dettagli"),
                    Height = 20,
                    IsVisible = false,
                    IsBuiltIn = true,
                    ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];


            UpdateEtichetteMap();

        }


        public override string GetKey()
        {
            return CreateKey(DivisioneId);
        }

        static public string CreateKey(Guid id)
        {
            return string.Format("{0}-{1}", "DivisioneItemParent", id.ToString());
        }

        public override bool AttributoIsPreviewable(string referenceCodiceAttributo)
        {
            return AssociedType.Attributi.ContainsKey(referenceCodiceAttributo);
        }

        public override EntityType Clone()
        {

            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            EntityType clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Nothing;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Nothing;

        public override MasterType MasterType => MasterType.Tree;

        public override bool IsParentType() { return true; }

        public override bool IsCustomizable() { return true; }
    }

    public class DivisioneItemKeyComparer : EntityComparer
    {
        public override List<string> AttributiCode { get; set; } = new List<string>()
        { BuiltInCodes.Attributo.Nome };

        public override bool Equals(string key1, string key2)
        {
            if (key1 == key2)
                return true;

            return false;
        }
    }
}
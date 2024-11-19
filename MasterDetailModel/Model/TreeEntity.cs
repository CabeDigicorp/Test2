

using Commons;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;


namespace MasterDetailModel
{
    //[ProtoContract]
    [DataContract]
    public class TreeEntityMasterInfo
    {
        //[ProtoMember(1)]
        //[DataMember]
        public Guid Id = Guid.Empty;

        //[ProtoMember(2)]
        //[DataMember]
        public Guid ParentId = Guid.Empty;

        /// <summary>
        /// Chiave utente per l'entità
        /// </summary>
        public string ComparerKey = string.Empty;

    }

    [ProtoContract]
    [DataContract]
    public abstract class TreeEntityType : EntityType
    {
        /// <summary>
        /// Parent Or Children type
        /// </summary>
        public TreeEntityType AssociedType { get; set; }

        public List<string> GetParentAttributi()
        {
            if (IsParentType())
                return null;

            if (AssociedType == null)
                return null;

            return AssociedType.Attributi.Select(item => item.Value.Codice).ToList();
        }

        public override bool AttributoIsPreviewable(string codiceAttributo)
        {
            if (AssociedType == null)
                return false;

            if (AssociedType.Attributi.ContainsKey(codiceAttributo))
                return true;

            return false;

        }

    }

    [ProtoContract]
    [DataContract]
    public class TreeEntity : Entity
    {
        public string HierarchySeparator { get; } = "\\";

        /// <summary>
        /// Profondità del nodo (-1 = rootNode non visibile)
        /// </summary>
        [ProtoMember(1)]
        [DataMember]
        public int Depth { get; set; } = 0;

        [ProtoMember(2)]
        [DataMember]
        public string ParentEntityTypeCodice { get; set; }
        public EntityType ParentEntityType { get; protected set; }//ref

        ///// <summary>
        ///// Genitore (possibilmente da eliminare)
        ///// </summary>
        public TreeEntity Parent { get; set; } = null;//ref 
        ///// <summary>
        /// 
        /// Figli (possibilmente da eliminare)
        /// </summary>
        public List<TreeEntity> Children = new List<TreeEntity>(); //ref

        /// <summary>
        /// Indica se l'entità ha gli attributi del parent o della foglia
        /// </summary>
        [DataMember]
        public bool IsParent { get; set; } = false;


        public override void ResolveReferences(Dictionary<string, EntityType> entityTypes)
        {

            if (IsParent)
            {
                EntityType = entityTypes[EntityTypeCodice];
                ParentEntityType = entityTypes[ParentEntityTypeCodice];

                foreach (EntityAttributo entAtt in Attributi.Values)
                {
                    entAtt.ResolveReferences(this, ParentEntityType.Attributi);
                }
            }
            else
            {
                ParentEntityType = entityTypes[ParentEntityTypeCodice];
                base.ResolveReferences(entityTypes);
            }

            //N.B. parent e children vengono risolti da TreeEntityCollection
        }


        public TreeEntityType TreeEntityType() { return EntityType as TreeEntityType; }



        public override void CopyFrom(Entity ent) //non copia i riferimenti ad altre entità (parent, children)
        {
            TreeEntity treeEnt = ent as TreeEntity;
            if (ent.EntityType.Codice == EntityType.Codice)
            {
                base.CopyFrom(ent);

                Depth = treeEnt.Depth;

                //HasChildren = treeEnt.HasChildren;
                
            }
        }


        ///// <summary>
        ///// Restituisce il numero dei discendenti
        ///// non viene usata
        ///// </summary>
        ///// <returns></returns>
        //public int DescendantsCount()
        //{
        //    int dc = Children.Count;
        //    foreach (TreeEntity child in Children)
        //        dc += child.DescendantsCount();
        //    return dc;
        //}

        ///// <summary>
        ///// Rimuove i riferimenti ad altre entità dell'albero
        ///// update children, parent, depth
        ///// </summary>
        //public void MakeAlone()
        //{
        //    if (Parent != null)
        //    {
        //        Parent.Children.Remove(this);
        //        Parent.Children.AddRange(Children);
        //    }

        //    foreach (TreeEntity ent in Children)
        //        ent.Parent = Parent;


        //    foreach (TreeEntity child in Children)
        //        child.UpdateDescendantsDepth(Parent == null ? 0 : Parent.Depth + 1);


        //    Children.Clear();
        //    Parent = null;
        //    Depth = 0;
        //}

        ///// <summary>
        ///// Aggiunge un fratello subito sotto sibling 
        ///// update children, parent, depth
        ///// </summary>
        ///// <param name="sibling"></param>
        //public void MakeSiblingOf(TreeEntity sibling, bool before = false)
        //{
        //    if (sibling == null)
        //        return;

        //    if (sibling.Parent != null && !sibling.Parent.Children.Contains(this))
        //    {
        //        int siblingIndex = sibling.Parent.Children.IndexOf(sibling);

        //        int newIndex = siblingIndex;
        //        if (!before)
        //            newIndex++;

        //        if (newIndex < sibling.Parent.Children.Count)
        //        {
        //            sibling.Parent.Children.Insert(newIndex, this);
        //        }
        //        else //se sibling è l'ultimo fratello
        //        {
        //            sibling.Parent.Children.Add(this);
        //        }

        //    }

        //    Depth = sibling.Depth;
        //    Parent = sibling.Parent;
        //    UpdateDescendantsDepth(Depth);

        //}

        ///// <summary>
        ///// Aggiunge in coda ai figli di parent
        ///// update children, parent, depth
        ///// </summary>
        ///// <param name="parent"></param>
        //public void MakeChildOf(TreeEntity parent)
        //{
        //    if (parent == null)
        //    {
        //        Parent = null;
        //        Depth = 0;
        //    }
        //    else if (!parent.Children.Contains(this))
        //    {
        //        parent.Children.Add(this);
        //        Parent = parent;
        //        Depth = parent.Depth + 1;
        //        UpdateDescendantsDepth(Depth);


        //    }
        //}



        public void CreaAttributiParent()
        {
            foreach (Attributo att in ParentEntityType.Attributi.Values)
            {
                Attributi.Add(att.Codice, new EntityAttributo(this, att) {Valore = att.ValoreDefault.Clone() });
            }
            IsParent = true;
        }

        public void RimuoviAttributiNonParent()
        {
            foreach (KeyValuePair<string, Attributo> att in EntityType.Attributi)
            {
                if (!ParentEntityType.Attributi.ContainsKey(att.Key))
                //if (ParentEntityType.Attributi.FirstOrDefault(item => item.Codice == att.Codice) == null)
                {
                    //Attributi.Remove(Attributi.First(item => item.Attributo.Codice == att.Key));
                    Attributi.Remove(att.Key);
                }
            }

        }

        /// <summary>
        /// Rende foglia un'entità 
        /// </summary>
        /// <returns>Restituisce true se è necessario un ricalcolo</returns>
        public virtual bool MakeLeaf()
        {
            CreaAttributi();
            IsParent = false;

            return false;
        }

        public virtual void MakeParent()
        {
            RimuoviAttributiNonParent();
            IsParent = true;
        }

        public virtual bool CanBeParent()
        {
            foreach (EntityAttributo entAtt in Attributi.Values)
            {
                if (!ParentEntityType.Attributi.ContainsKey(entAtt.Attributo.Codice))
                {
                    if (entAtt.Valore.HasValue())
                    {
                        //if (!entAtt.Valore.ResultEquals(entAtt.Attributo.ValoreDefault))
                        //    return false;
                        if (!entAtt.Valore.Equals(entAtt.Attributo.ValoreDefault))
                            return false;
                    }
                }
            }
            return true;

        }

        //public bool IsParent()
        //{
        //    if (Attributi.Count > ParentEntityType.Attributi.Count)
        //        return false;

        //    if (!HasChildren)
        //        return false;

        //    return true;
        //}

        public override Valore GetValoreAttributo(string codiceAttributo, bool deep, bool brief)
        {
            //string result = string.Empty;

            if (!Attributi.ContainsKey(codiceAttributo))
                return null;
            
            if (EntityType.AttributoIsPreviewable(codiceAttributo))
            {
                if (Attributi[codiceAttributo].Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoRTF)
                {
                    string result = string.Empty;

                    TreeEntity entIter = this;
                    while (entIter != null)
                    {
                        if (entIter.Attributi[codiceAttributo].Valore is ValoreTestoRtf)
                        {
                            ValoreTestoRtf valRtf = entIter.Attributi[codiceAttributo].Valore as ValoreTestoRtf;
                            string rtf;

                            //brief rtf
                            //oss: non faccio calcolare la breve se poi non serve, se vuole uno la chiede runtime con BriefRtf
                            //Invece se chiedo la concatenazione dei padri mi serve calcolare la brief
                            if (brief && deep)
                            {
                                rtf = valRtf.BriefRtf;
                            }
                            else
                            {
                                rtf = valRtf.V;
                            }

                            //rtf
                            if (result.Any())
                                ValoreHelper.RtfConcat(ref result, rtf);
                            else
                                result = rtf;

                        }

                        if (deep)
                            entIter = entIter.Parent;
                        else
                            break;
                    }

                    return new ValoreTestoRtf() { V = result };
                }
                if (Attributi[codiceAttributo].Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
                {
                    string concatV = string.Empty;
                    string concatR = string.Empty;

                    TreeEntity entIter = this;
                    while (entIter != null)
                    {

                        if (entIter.Attributi[codiceAttributo].Valore is ValoreTesto)
                        {
                            ValoreTesto valTesto = entIter.Attributi[codiceAttributo].Valore as ValoreTesto;
                            string monofontV = valTesto.V;
                            if (concatV != null && concatV.Any())
                                concatV = string.Join(HierarchySeparator, monofontV, concatV);
                            else
                                concatV = monofontV;

                            string monofontR = valTesto.Result;
                            if (concatR != null && concatR.Any())
                                concatR = string.Join(HierarchySeparator, monofontR, concatR);
                            else
                                concatR = monofontR;
                        }

                        if (deep)
                            entIter = entIter.Parent;
                        else
                            break;
                    }

                    Valore val = new ValoreTesto() { V = concatV, Result = concatR };

                    if (Attributi[codiceAttributo].Attributo.ValoreAttributo != null)
                        Attributi[codiceAttributo].Attributo.ValoreAttributo.UpdatePlainText(val);

                    return val;
                }
            }

            return base.GetValoreAttributo(codiceAttributo, deep, brief);
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
                    Valore valCodice = entIter.GetValoreAttributo(BuiltInCodes.Attributo.Codice, false, true);//codice
                    ValoreTestoRtf valDesc = entIter.GetValoreAttributo(BuiltInCodes.Attributo.DescrizioneRTF, false, true) as ValoreTestoRtf;

                    if (valCodice != null)
                        str += valCodice.ToPlainText();

                    if (valDesc != null)
                    {
                        string desc = valDesc.BriefPlainText;
                        if (desc != null && desc.Any())
                        {
                            str = string.Format("{0} - {1}", str, desc);
                        }
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
                    
                    valCodice = entIter.GetValoreAttributo(BuiltInCodes.Attributo.Codice, false, true);//codice
                    if (valCodice != null && leafCod == null)
                        leafCod = valCodice.ToPlainText();

                    string desc = string.Empty;
                    ValoreTestoRtf valDesc = entIter.GetValoreAttributo(BuiltInCodes.Attributo.DescrizioneRTF, false, true) as ValoreTestoRtf;
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
            else
            {
                Valore valCodice = GetValoreAttributo(BuiltInCodes.Attributo.Codice, false, true);
                ValoreTestoRtf valDesc = GetValoreAttributo(BuiltInCodes.Attributo.DescrizioneRTF, false, true) as ValoreTestoRtf;

                if (!string.IsNullOrEmpty(valDesc.BriefPlainText))
                    result = string.Format("{0} - {1}", valCodice.PlainText, valDesc.BriefPlainText);
                else
                    result = valCodice.PlainText;
            }
            return result;
        }

        public virtual bool IsParentDependsOnChild() { return false; }


    }

    //[ProtoContract]
    [DataContract]
    public class TreeEntityCollection
    {
        //[ProtoMember(1)]
        [DataMember]
        public List<TreeEntity> TreeEntities { get; set; }

        public void ResolveAllReferences(Dictionary<string, EntityType> entityTypes)
        {
            TreeEntities.ForEach(item => item.ResolveReferences(entityTypes));

            if (!TreeEntities.Any())
                return;

            TreeEntity treeEnt0 = TreeEntities[0];
            List<TreeEntity> parents = new List<TreeEntity>();
            for (int i = 0; i < treeEnt0.Depth; i++)
                parents.Add(null);
            
            for (int index = 0; index < TreeEntities.Count; index++)
            {
                TreeEntity treeEnt = TreeEntities[index];

                int depth = treeEnt.Depth;

                treeEnt.Children = new List<TreeEntity>();
                if (parents.Count <= depth)
                    parents.Add(treeEnt);
                else
                    parents[depth] = treeEnt;

                if (treeEnt.Depth > 0)
                {

                    TreeEntity parent = parents[treeEnt.Depth - 1];
                    if (parent != null)
                    {
                        treeEnt.Parent = parent;
                        parent.Children.Add(treeEnt);
                    }
                }
            }
        }

        /// <summary>
        /// depth -> children, parent
        /// </summary>
        /// <param name="entityTypes"></param>
        /// <param name="treeEnt"></param>
        public void ResolveReferences(Dictionary<string, EntityType> entityTypes, TreeEntity treeEnt)
        {
            treeEnt.ResolveReferences(entityTypes);

            int index = TreeEntities.IndexOf(treeEnt);
            
            //Resolve Parent References
            int i = index - 1;
            int depth = treeEnt.Depth;
            while (i >= 0 && TreeEntities[i].Depth >= treeEnt.Depth)
                i--;

            if (i >= 0)
                treeEnt.Parent = TreeEntities[i];
            else
                treeEnt.Parent = null;


            //Resolve Children References
            //treeEnt.Children = TreeEntities.Where(item => item.Parent == treeEnt).ToList();

            treeEnt.Children.Clear();
            if (index >= 0)
            {
                for (i = index; i < TreeEntities.Count; i++)
                {
                    if (TreeEntities[i].Depth <= depth)
                        break;

                    if (TreeEntities[i].Depth == depth + 1)
                        treeEnt.Children.Add(TreeEntities[i]);
                }
            }

        }


        public TreeEntityCollection Clone()
        {
            List<TreeEntity> source = TreeEntities;
            TreeEntityCollection entityCollection = new TreeEntityCollection() { TreeEntities = source };
            string json = "";
            JsonSerializer.JsonSerialize(entityCollection, out json);

            TreeEntityCollection clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, entityCollection.GetType());

            return clone;

        }


    }


}
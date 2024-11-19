

using CommonResources;
using Commons;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace MasterDetailModel
{
    //[ProtoContract]
    //[DataContract]
    public class EntityMasterInfo
    {
        //[ProtoMember(1)]
        //[DataMember]
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Chiave di ragruppamento per ogni livello
        /// </summary>
        //[ProtoMember(2)]
        //[DataMember]
        public List<string> GroupKeys { get; set; } = new List<string>();

        /// <summary>
        /// Chiave utente per l'entità
        /// </summary>
        public string ComparerKey { get; set; } = string.Empty;

    }

    [ProtoContract]
    [DataContract]
    public class Entity
    {

        [ProtoMember(1)]
        [DataMember]
        public Guid EntityId = Guid.Empty;

        [ProtoMember(2)]
        [DataMember]
        //public List<EntityAttributo> Attributi = new List<EntityAttributo>();
        public Dictionary<string, EntityAttributo> Attributi { get; set; } = new Dictionary<string, EntityAttributo>();

        //[ProtoMember(3)]
        //[DataMember]
        public bool Deleted { get; set; } = false;

        [ProtoMember(4)]
        [DataMember]
        public string EntityTypeCodice { get; set; }
        public EntityType EntityType { get; protected set; }//ref


        //[DataMember]
        //public Guid ProjectId { get; set; } = Guid.Empty;

        /// <summary>
        /// Colore per evidenziare l'entità (N.B. non viene salvato ma ricalcolato)
        /// </summary>
        [ProtoMember(5)]
        [DataMember]
        public string HighlighterColorName { get; set; } = MyColorsEnum.Transparent.ToString();

        public virtual void ResolveReferences(Dictionary<string, EntityType> entityTypes)
        {
            EntityType = entityTypes[EntityTypeCodice];

            foreach (EntityAttributo entAtt in Attributi.Values)
            {
                entAtt.ResolveReferences(this, EntityType.Attributi);
            }
        }        
        public virtual void CopyFrom(Entity ent)
        {
            Attributi.Clear();
            foreach (EntityAttributo entAtt in ent.Attributi.Values)
            {
                Attributi.Add(entAtt.AttributoCodice, entAtt.Clone());
            }

            Deleted = ent.Deleted;
            EntityType = ent.EntityType;
            EntityTypeCodice = ent.EntityTypeCodice;
        }

        public virtual Entity Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            Entity clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }


        public virtual bool AddValoriAttributiByTabbedString(string tabbedString)
        {
            string[] tabs = Regex.Split(tabbedString, "\t");

            CreaAttributi();
            List<EntityAttributo> attributiOrdered = Attributi.Values.OrderBy(item => item.Attributo.DetailViewOrder).ToList();

            for (int i= 0; i< tabs.Length; i++)
            {

                if (i < attributiOrdered.Count)
                {
                    Attributo att = attributiOrdered[i].Attributo;

                    //bool allowPast = ! (att.IsValoreReadOnly ||  att.IsValoreLockedByDefault || 
                    //                att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento ||
                    //                att.IsPreviewMode);

                    //if (allowPast)
                        attributiOrdered[i].Valore.SetByString(tabs[i]);
                }

            }
            
            return true;

        }

        public virtual void CopyValoriAttributiFrom(Entity ent)
        {
            foreach (EntityAttributo entAtt in Attributi.Values)
            {
                //EntityAttributo found = ent.Attributi.FirstOrDefault(item => item.Value.Attributo.Definizione.Codice == entAtt.Attributo.Definizione.Codice).Value;
                EntityAttributo found = ent.Attributi.FirstOrDefault(item => item.Value.Attributo.Codice == entAtt.Attributo.Codice).Value;
                if (found != null)
                    entAtt.Valore = found.Valore.Clone();
            }
        }

        public void CreaAttributi()
        {

            //aggiungo gli attributi se non ci sono già
            foreach (Attributo att in EntityType.Attributi.Values)
            {

                ////by Ale 18/04/2023
                if (att.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.Riferimento)
                {

                    if (!Attributi.ContainsKey(att.Codice))
                            Attributi.Add(att.Codice, new EntityAttributo(this, att) { Valore = att.ValoreDefault.Clone() });

                }

            }

        }

        /// <summary>
        /// Internal use
        /// </summary>
        /// <param name="codiceAttributo"></param>
        /// <param name="deep"> Valore concatenazione dei padri (vale solo per entità strutturate ad albero)</param>
        /// <param name="brief">Valore abbreviato (descrizione breve)</param>
        /// <returns></returns>
        public virtual Valore GetValoreAttributo(string codiceAttributo, bool deep, bool brief)
        {
            if (Attributi.ContainsKey(codiceAttributo))
            {
                Valore val = Attributi[codiceAttributo].Valore;
                if (Attributi[codiceAttributo].Attributo.ValoreAttributo != null)
                    Attributi[codiceAttributo].Attributo.ValoreAttributo.UpdatePlainText(val);


                return val;
            }



            return null;
        }


        public virtual string ToUserIdentity(UserIdentityMode mode)
        {
            return string.Empty;
        }

        public Guid GetAttributoGuidId(string codiceAttributoGuid)
        {
            if (Attributi.ContainsKey(codiceAttributoGuid))
            {
                ValoreGuid valGuid = Attributi[codiceAttributoGuid].Valore as ValoreGuid;
                return valGuid == null ? Guid.Empty : valGuid.V;
            }
            return Guid.Empty;
        }

        public string GetComparerKey(EntityComparer entComparer = null)
        {
            string key = string.Empty;

            if (EntityType == null)
                return string.Empty;

            if (entComparer == null)
                entComparer = EntityType.EntityComparer;


            if (entComparer == null)
                return string.Empty;

            foreach (string str in entComparer.AttributiCode)
            {
                if (key.Any())
                    key += entComparer.KeySeparator;

                if (str == BuiltInCodes.Attributo.Id)
                    key += EntityId.ToString();
                else
                {
                    Valore val = GetValoreAttributo(str, false, false);
                    if (val != null)
                        key += val.PlainText;

                }
            }

            return key;
        }

        public virtual bool IsDigicorpOwner()
        {
            ValoreBooleano val = GetValoreAttributo(BuiltInCodes.Attributo.IsDigicorpOwner, false, false) as ValoreBooleano;
            if (val == null)
                return false;

            if (val.V == true)
                return true;

            return false;
        }

        public virtual void Validate() { }

    }

    [ProtoContract]
    [DataContract]
    public class EntityCollection
    {
        [ProtoMember(1)]
        [DataMember]
        public List<Entity> Entities { get; set; }

        public virtual void ResolveAllReferences(Dictionary<string, EntityType> entityTypes)
        {
            Entities.ForEach(item => item.ResolveReferences(entityTypes));
        }

        public EntityCollection Clone()
        {
            List<Entity> source = Entities;
            EntityCollection entityCollection = new EntityCollection() { Entities = source};
            string json = "";
            JsonSerializer.JsonSerialize(entityCollection, out json);

            EntityCollection clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, entityCollection.GetType());

            return clone;

        }

    }

    public class EntityComparer : IEqualityComparer<string>
    {
        public string KeySeparator { get; set; } = "|";
        public virtual List<string> AttributiCode { get; set; } = new List<string>();

        public virtual bool Equals(string x, string y)
        {
            return false;
        }

        public int GetHashCode(string obj)
        {
            return base.GetHashCode();
        }
    }


    //public class SoggettoFake
    //{
    //    public string Nome { get; set; }
    //    public string Cognome { get; set; }

    //}

    public enum UserIdentityMode
    {
        Nothing = 0,
        Deep,
        SingleLine,
        SingleLine1,
        SingleLine2,
    }



}
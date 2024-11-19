


using Commons;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MasterDetailModel
{

    [DataContract]
    [ProtoContract]
    [KnownType(typeof(ValoreTesto))]
    [KnownType(typeof(ValoreTestoRtf))]
    [KnownType(typeof(ValoreContabilita))]
    [KnownType(typeof(ValoreReale))]
    [KnownType(typeof(ValoreTestoCollection))]
    [KnownType(typeof(ValoreGuidCollection))]
    [KnownType(typeof(ValoreData))]
    [KnownType(typeof(ValoreBooleano))]
    [KnownType(typeof(ValoreElenco))]
    [KnownType(typeof(ValoreColore))]
    [KnownType(typeof(ValoreFormatoNumero))]
    public class EntityAttributo
    {
        [ProtoMember(2)]
        [DataMember]
        public string AttributoCodice { get; set; }
        public Attributo Attributo { get; private set; }//ref

        [ProtoMember(3)]
        [DataMember]
        public Valore Valore
        {
            get;
            set;
        }

        public Valore GetValore()    { return Valore; }

        //public Valore Valore
        //{
        //    get
        //    {
        //        if (Entity is null)
        //        {
        //            int p = 0;
        //        }
        //        return Entity.GetValoreAttributo(AttributoCodice, false, false);
        //    }
        //    set
        //    {
        //        FullValore = value;
        //    }
        //}

        public Entity Entity { get; private set; }//ref

        public EntityAttributo() { }
        public EntityAttributo(Entity entity, Attributo att)
        {
            Attributo = att;
            Valore = Attributo.ValoreDefault.Clone();
            AttributoCodice = att.Codice;
            Entity = entity;
        }



        public EntityAttributo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            EntityAttributo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            //Resolve references
            clone.Attributo = Attributo;
            clone.Entity = Entity;

            if (Valore != null)
                clone.Valore = Valore.Clone();

            return clone;

            //return new EntityAttributo()  { Id = this.Id, Attributo = this.Attributo, AttributoCodice = this.AttributoCodice, Valore = this.Valore.Clone() };
        }

        public void ResolveReferences(Entity entity, Dictionary<string, Attributo> attributi)
        {
            if (attributi.ContainsKey(AttributoCodice))
                Attributo = attributi[AttributoCodice];

            Entity = entity;
        }



    }

}
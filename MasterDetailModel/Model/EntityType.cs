using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailModel
{
    [ProtoContract]
    [DataContract]
    public abstract class EntityType
    {

        [ProtoMember(1)]
        [DataMember]
        public string Codice { get; set; }

        [ProtoMember(2)]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Obsoleto
        /// </summary>
        //[ProtoMember(3)]
        //[DataMember]
        //public bool IsTreeMaster { get; set; }
        public bool IsTreeMaster { get => MasterType == MasterType.Tree; }

        [ProtoMember(4)]
        [DataMember]
        public Dictionary<string, Attributo> Attributi { get; set; } = new Dictionary<string, Attributo>();

        /// <summary>
        /// Attributi da portare nel master (per la griglia verranno mappati nelle proprietà di EntityView in base all'indice cioè l'attributo all'indice 3 in  EntityAttributoView03)
        /// </summary>
        [ProtoMember(5)]
        [DataMember]
        public List<string> AttributiMasterCodes { get; set; } = new List<string>();

        [ProtoMember(6)]
        [DataMember]
        public string FunctionName { get; set; }

        [ProtoMember(7)]
        [DataMember]
        public double DetailAttributoEtichettaWidth { get; set; } = 150;

        [ProtoMember(8)]
        [DataMember]
        public int UserNewAttributoCodice { get; set; } = 0;

        /// <summary>
        /// Mappa etichetta -> codice attributo
        /// </summary>
        public Dictionary<string, string> EtichetteMap { get; } = new Dictionary<string, string>();

        public virtual EntityComparer EntityComparer { get; set; } = null;


        public virtual void ResolveReferences(Dictionary<string, EntityType> entityTypes, Dictionary<string, DefinizioneAttributo> definizioniAttributo)
        {
            foreach (Attributo att in Attributi.Values)
                att.ResolveReferences(entityTypes, definizioniAttributo);

            UpdateEtichetteMap();
        }
        protected void UpdateEtichetteMap()
        {
            EtichetteMap.Clear();
            foreach (Attributo att in Attributi.Values)
            {
                if (!EtichetteMap.ContainsKey(att.Etichetta))//non so perchè sia necesssario...
                    EtichetteMap.Add(att.Etichetta, att.Codice);
            }
        }

        public virtual string GetKey()
        {
            return Codice;
        }

        public virtual bool AttributoIsPreviewable(string referenceCodiceAttributo) { return false; }

        public abstract void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes);
        public abstract EntityType Clone();

        public abstract EntityTypeDependencyEnum DependencyEnum { get; }
        public abstract DependentEntityTypesEnum DependentTypesEnum { get; }

        public abstract MasterType MasterType { get; }

        public virtual bool IsParentType() { return false; }

        public virtual bool IsCustomizable() { return false; }

        public virtual HashSet<string> LimitedEntityTypesOnImport() { return null;}//nessuna limitazione

        public virtual bool UpdateAttributi(Dictionary<string, EntityType> entityTypes, EntityType entTypeOld, bool removed = false) { return false; }

    }

    public enum EntityTypeDependencyEnum
    {
        Nothing = 0,
        Divisione = 1,
        Capitoli = 2,
        Contatti = 4,
        InfoProgetto = 8,
        Prezzario = 16,
        Elementi = 32,
        Computo = 64,
        Documenti = 128,
        Report = 256,
        Stili = 512,
        ElencoAttivita = 1024,
        WBS = 2048,
        Calendari = 4096,
        Variabili = 8192,
        Allegati = 16384,
        Tag = 32768,
    }


    public enum DependentEntityTypesEnum
    {
        Nothing = 0,
        Variabili = 0,
        Allegati = 0,
        Tag = 0,
        Divisione = EntityTypeDependencyEnum.Variabili + EntityTypeDependencyEnum.Allegati + EntityTypeDependencyEnum.Tag,
        Capitoli = EntityTypeDependencyEnum.Divisione + EntityTypeDependencyEnum.Variabili + EntityTypeDependencyEnum.Allegati + EntityTypeDependencyEnum.Tag,
        Contatti = EntityTypeDependencyEnum.Divisione + EntityTypeDependencyEnum.Variabili + EntityTypeDependencyEnum.Allegati + EntityTypeDependencyEnum.Tag, 
        InfoProgetto = EntityTypeDependencyEnum.Divisione + EntityTypeDependencyEnum.Contatti + EntityTypeDependencyEnum.WBS + EntityTypeDependencyEnum.Variabili + EntityTypeDependencyEnum.Computo + EntityTypeDependencyEnum.Allegati,
        Prezzario = EntityTypeDependencyEnum.Divisione + EntityTypeDependencyEnum.Contatti + EntityTypeDependencyEnum.Capitoli + EntityTypeDependencyEnum.ElencoAttivita + EntityTypeDependencyEnum.Variabili + EntityTypeDependencyEnum.Allegati + EntityTypeDependencyEnum.Tag,
        Elementi = EntityTypeDependencyEnum.Divisione + EntityTypeDependencyEnum.Contatti + EntityTypeDependencyEnum.Variabili + EntityTypeDependencyEnum.Allegati + EntityTypeDependencyEnum.Tag,
        Computo = EntityTypeDependencyEnum.Divisione + EntityTypeDependencyEnum.Contatti + 
            EntityTypeDependencyEnum.Prezzario + EntityTypeDependencyEnum.Elementi + EntityTypeDependencyEnum.Variabili + EntityTypeDependencyEnum.Allegati + EntityTypeDependencyEnum.Tag +
            EntityTypeDependencyEnum.Computo,
        Documenti = EntityTypeDependencyEnum.Report + EntityTypeDependencyEnum.Stili + EntityTypeDependencyEnum.Tag,
        Report = 0,
        Stili = 0,
        ElencoAttivita = EntityTypeDependencyEnum.Divisione + EntityTypeDependencyEnum.Variabili + EntityTypeDependencyEnum.Allegati + EntityTypeDependencyEnum.Tag,
        WBS = EntityTypeDependencyEnum.Divisione + EntityTypeDependencyEnum.Computo + EntityTypeDependencyEnum.ElencoAttivita + EntityTypeDependencyEnum.Contatti + 
            EntityTypeDependencyEnum.Elementi + EntityTypeDependencyEnum.Calendari + EntityTypeDependencyEnum.Variabili + EntityTypeDependencyEnum.Allegati + EntityTypeDependencyEnum.Tag,
        Calendari = EntityTypeDependencyEnum.Divisione + EntityTypeDependencyEnum.Variabili,
    }

    public class DependentEntityTypesComparer : IComparer<DependentEntityTypesEnum>
    {
        public int Compare(DependentEntityTypesEnum x, DependentEntityTypesEnum y)
        {
            if ((x & y) == x)
                return 1;
            else if ((x & y) == y)
                return -1;

            return 0;
        }
    }


    public enum MasterType
    {
        Nothing = 0,
        NoMaster, //ex info
        List, //ex contatti
        Tree, //ex elenco prezzi
        Grid, //ex computo
    }


}

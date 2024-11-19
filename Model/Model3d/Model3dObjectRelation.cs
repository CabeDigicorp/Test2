using _3DModelExchange;
using DevExpress.SpreadsheetSource.Xlsx.Import.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public struct DivisioneItemId
    {
        /// <summary>
        /// Entity type a cui fa riferiemento l'Id
        /// </summary>
        public string EntityTypeKey { get; set; }

        /// <summary>
        /// Item Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Percorso per l'item (es: _FromRoom oppure _FromRoom("Arredi") )
        /// </summary>
        public string ItemPath { get; set; }

        public DivisioneItemId()
        {
            EntityTypeKey = string.Empty;
            Id = Guid.Empty;
            ItemPath = string.Empty;
        }
    }

    public class Model3dElementRelation
    {
        /// <summary>
        /// Key di un elemento del modello 3d
        /// </summary>
        public Model3dObjectKey Model3dObject { get; set; }

        /// <summary>
        /// key: DivisioneTypeKey
        /// </summary>
        public Dictionary<string, HashSet<DivisioneItemId>> Divisioni = new Dictionary<string, HashSet<DivisioneItemId>>();

    }


    public class ComputoItemByRule
    {
        public Guid PrezzarioItemId { get; set; } = Guid.Empty;

        public Guid ElementiItemId { get; set; } = Guid.Empty;

        public Model3dFilterData Filter { get; set; } = null;

        public Model3dRuleComputo Rule { get; set; } = null;

        public Guid ExistingComputoItemId { get; set; } = Guid.Empty;

        public bool ToRemove { get; set; } = false;

    }




}

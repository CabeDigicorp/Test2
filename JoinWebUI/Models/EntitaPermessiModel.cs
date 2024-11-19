using JoinWebUI.Models;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using System.ComponentModel.DataAnnotations;

namespace JoinWebUI.Models
{
    public class EntitaPermessiModel
    {
        [Required]
        public Guid Id { get; set; }

        public string IdString { get { return Id.ToString(); } }

        public string Info { get; set; } = string.Empty;
        public string Type { get; set; } = nameof(EntitaPermessiModel);
        public string TypeDescription { get; set; } = "EntitaPermessi";

        public List<EntitaPermessiModel> SoggettiChildren { get; set; } = new List<EntitaPermessiModel>();
        public List<EntitaPermessiModel> SoggettiMembers { get; set; } = new List<EntitaPermessiModel>();

        public bool IsContainerOnly { get; protected set; } = false;
        
        public bool IsMemberOnly { get; set; } = false;

        public List<EntitaPermessiModel> OggettiChildren { get; set; } = new List<EntitaPermessiModel>();

        public Guid? ParentId { get; protected set; } = null;

        //public EntitaPermessiModel? DynamicParent { get; set; } = null;


        public EntitaPermessiModel()
        {
            Id = Guid.NewGuid();
        }
    }

    //public class SoggettoPermessiModel : EntitaPermessiModel
    //{
    //    public override string Type { get; set; } = nameof(SoggettoPermessiModel);
    //    public override string TypeDescription { get; set; } = "SoggettoPermessiModel";
    //    public List<SoggettoPermessiModel> SoggettiChildren { get; set; } = new List<SoggettoPermessiModel>();

    //    public bool IsContainerOnly = false;
    //    public bool IsMemberOnly = false;

    //    public SoggettoPermessiModel() : base() { }

    //    public SoggettoPermessiModel(bool isContainer, bool isMember) : base()
    //    {
    //        IsContainerOnly = isContainer;
    //        IsMemberOnly = isMember;
    //    }

    //}

    //public class OggettoPermessiModel : EntitaPermessiModel
    //{
    //    public override string Type { get; set; } = nameof(OggettoPermessiModel);
    //    public override string TypeDescription { get; set; } = "OggettoPermessiModel";
    //    public List<OggettoPermessiModel> OggettiChildren { get; set; } = new List<OggettoPermessiModel>();

    //    public OggettoPermessiModel() : base() { }

    //}


}
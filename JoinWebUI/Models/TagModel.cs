using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JoinWebUI.Models
{
    public class TagModel : IComparable<TagModel>, IListBoxCardBaseModel
    {


        [Required]
        public Guid Id { get; set; }

        public string IdString { get => Id.ToString(); }

        public string Nome { get; set; } = string.Empty;

        public Guid ClienteId { get; set; }

        public string TextLine1 { get => Nome; }

        public string TextLine2 { get => string.Empty; }

        public bool MultiLine { get => false; }


        public int CompareTo(TagModel? other)
        {
            if(other == null)
            {
                return 1;
            }
            return string.Compare(Nome, other.Nome, StringComparison.CurrentCultureIgnoreCase);
        }

    }
}

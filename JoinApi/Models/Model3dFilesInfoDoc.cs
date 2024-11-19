using ModelData.Model;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace JoinApi.Models
{
    public class Model3dFilesInfoDoc : ModelData.Model.Model3dFilesInfo
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
        public string? FileName { get; set; } = null;

        //public List<Model3dFileInfoDoc> Items { get; set; } = new List<Model3dFileInfoDoc>();
    }

       //public class Model3dFileInfoDoc
       // {
       //     public string FilePath { get; set; }
       //     public string Note { get; set; }
       //     public string FileName { get; set; }
       //     public bool IsChecked { get; set; }
       // }

    }

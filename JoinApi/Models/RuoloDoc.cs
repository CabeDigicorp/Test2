using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System.Security.Policy;
using ModelData.Utilities;

namespace JoinApi.Models
{
    [CollectionName("ruoli")]
    public class RuoloDoc : MongoIdentityRole<Guid>
    {
        public RuoloDoc() : base()
        {
        }
        public RuoloDoc(string roleName) : base(roleName)
        {
        }

        public bool Inheritable
        {
            get
            {
                bool result = true;
                var strings = Claims.Where(c => c.Type == ModelData.Utilities.ApplicationClaimTypes.Ereditabile).Select(c => c.Value).ToList();
                if (strings.Count == 1)
                { 
                    bool.TryParse(strings[0], out result);
                }
                
                return result;
            }
        }


        public List<Azioni> Azioni
        {
            get
            {
                var strings = Claims.Where(c => c.Type == ModelData.Utilities.ApplicationClaimTypes.Azione).Select(c => c.Value).ToList();
                var result = new List<Azioni>();
                foreach(var azione in strings)
                {
                    Azioni enumerated;
                    if (Enum.TryParse<Azioni>(azione, true, out enumerated))
                    {
                        result.Add(enumerated);
                    }
                    else
                    {
                        //TODO Loggare
                    }

                }
                return result;
            }
        }

    }


}

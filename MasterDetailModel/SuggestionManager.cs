


using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterDetailModel
{
    //public class SuggestionPackage
    //{
    //    public string Codice { get; set; }

    //    /// <summary>
    //    /// key = codice attributo
    //    /// </summary>
    //    public Dictionary<string, Valore> Valori;
    //    public SuggestionPackage Clone()
    //    {
    //        SuggestionPackage sp = new SuggestionPackage() { Codice = this.Codice };

    //        sp.Valori = new Dictionary<string, Valore>(this.Valori.Count, this.Valori.Comparer);
    //        foreach (KeyValuePair<string, Valore> entry in this.Valori)
    //        {
    //            sp.Valori.Add(entry.Key, (Valore)entry.Value.Clone());
    //        }
    //        return sp;
    //    }

    //}

    //public class SuggestionPackageEqualityComparer : IEqualityComparer<SuggestionPackage>
    //{
    //    public bool Equals(SuggestionPackage x, SuggestionPackage y)
    //    {
    //        foreach (KeyValuePair<string, Valore> val in x.Valori)
    //        {
    //            if (!val.Value.ResultEquals(y.Valori[val.Key]))
    //                return false;
    //        }
    //        return true;
    //    }

    //    public int GetHashCode(SuggestionPackage obj)
    //    {
    //        return base.GetHashCode();
    //    }
    //}

    //public class SuggestionManager
    //{
    //    Dictionary<string, HashSet<SuggestionPackage>> OriginalSuggestionsDictionary = new Dictionary<string, HashSet<SuggestionPackage>>();
    //    Dictionary<string, HashSet<SuggestionPackage>> RecentSuggestionsDictionary = new Dictionary<string, HashSet<SuggestionPackage>>();

    //    public void Init()
    //    {
    //        //OriginalSuggestionsDictionary = ClientService.GetSuggestions();
    //    }

    //    public HashSet<SuggestionPackage> GetSuggestions(string suggestionCode)
    //    {
    //        HashSet<SuggestionPackage> suggestions = new HashSet<SuggestionPackage>();

    //        if (OriginalSuggestionsDictionary.ContainsKey(suggestionCode))
    //        {
    //            foreach (SuggestionPackage sp in OriginalSuggestionsDictionary[suggestionCode])
    //                suggestions.Add(sp.Clone());
    //        }

    //        if (RecentSuggestionsDictionary.ContainsKey(suggestionCode))
    //        {
    //            foreach (SuggestionPackage sp in RecentSuggestionsDictionary[suggestionCode])
    //                suggestions.Add(sp.Clone());
    //        }

    //        return suggestions;
    //    }

    //    public async void UpdateRecentSuggestions(string entityTypeCode, string suggestionCode, HashSet<Guid> recentEntitiesModifiedId)
    //    {
    //        //List<Entity> entities = await ClientService.GetEntitiesById(entityTypeCode, recentEntitiesModifiedId.ToList());

    //        //RecentSuggestionsDictionary.Clear();
    //        //RecentSuggestionsDictionary.Add(suggestionCode, new HashSet<SuggestionPackage>(entities.Select(ent => new SuggestionPackage() { Codice = suggestionCode, Valori = ent.Attributi.Where(att => att.Attributo.SuggestionCode == suggestionCode).ToDictionary(item => item.Attributo.Codice, item => item.Valore.Clone()) }).ToList(), new SuggestionPackageEqualityComparer()));
    //    }
    //}
}

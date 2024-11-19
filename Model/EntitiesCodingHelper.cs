using Commons;
using MasterDetailModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class EntitiesCodingHelper
    {
        Dictionary<int, Char> UpperCharachters = new Dictionary<int, Char>();
        Dictionary<int, Char> LowerCharachters = new Dictionary<int, Char>();
        private EntitiesHelper entitiesHelper;
        public EntitiesCodingHelper()
        {
            UpperCharachters = new Dictionary<int, Char>();
            UpperCharachters.Add(1, 'A'); UpperCharachters.Add(2, 'B'); UpperCharachters.Add(3, 'C');
            UpperCharachters.Add(4, 'D'); UpperCharachters.Add(5, 'E'); UpperCharachters.Add(6, 'F');
            UpperCharachters.Add(7, 'G'); UpperCharachters.Add(8, 'H'); UpperCharachters.Add(9, 'I');
            UpperCharachters.Add(10, 'J'); UpperCharachters.Add(11, 'K'); UpperCharachters.Add(12, 'L');
            UpperCharachters.Add(13, 'M'); UpperCharachters.Add(14, 'N'); UpperCharachters.Add(15, 'O');
            UpperCharachters.Add(16, 'P'); UpperCharachters.Add(17, 'Q'); UpperCharachters.Add(18, 'R');
            UpperCharachters.Add(19, 'S'); UpperCharachters.Add(20, 'T'); UpperCharachters.Add(21, 'U');
            UpperCharachters.Add(22, 'V'); UpperCharachters.Add(23, 'W'); UpperCharachters.Add(24, 'X');
            UpperCharachters.Add(25, 'Y'); UpperCharachters.Add(26, 'Z');

            LowerCharachters = new Dictionary<int, Char>();
            LowerCharachters.Add(1, 'a'); LowerCharachters.Add(2, 'b'); LowerCharachters.Add(3, 'c');
            LowerCharachters.Add(4, 'd'); LowerCharachters.Add(5, 'e'); LowerCharachters.Add(6, 'f');
            LowerCharachters.Add(7, 'g'); LowerCharachters.Add(8, 'h'); LowerCharachters.Add(9, 'i');
            LowerCharachters.Add(10, 'j'); LowerCharachters.Add(11, 'k'); LowerCharachters.Add(12, 'l');
            LowerCharachters.Add(13, 'm'); LowerCharachters.Add(14, 'n'); LowerCharachters.Add(15, 'o');
            LowerCharachters.Add(16, 'p'); LowerCharachters.Add(17, 'q'); LowerCharachters.Add(18, 'r');
            LowerCharachters.Add(19, 's'); LowerCharachters.Add(20, 't'); LowerCharachters.Add(21, 'u');
            LowerCharachters.Add(22, 'v'); LowerCharachters.Add(23, 'w'); LowerCharachters.Add(24, 'x');
            LowerCharachters.Add(25, 'y'); LowerCharachters.Add(26, 'z');
        }
        public IDataService DataService { get; set; }
        public IMainOperation MainOperation { get; set; }
        public string EntityTypeKey { get; set; }

        public void GeneraCodice(AttributoCodingSetting AttributoLevelCoding)
        {
            if (AttributoLevelCoding.AttributoCodingSettingPrecedente != null)
                if (AttributoLevelCoding.AggiungiCodiceSuperiore)
                {
                    AttributoLevelCoding.Esempio = AttributoLevelCoding.AttributoCodingSettingPrecedente.Esempio + GeneraCodice(ConversioneInAttributoLevelCoding(AttributoLevelCoding), null, false);
                }
                else
                {
                    AttributoLevelCoding.Esempio = GeneraCodice(ConversioneInAttributoLevelCoding(AttributoLevelCoding), null, false);
                }
            else
                AttributoLevelCoding.Esempio = GeneraCodice(ConversioneInAttributoLevelCoding(AttributoLevelCoding), null, false);

        }
        public string GeneraCodice(AttributoLevelCoding AttributoLevelCoding, string CodicePrecedenteCodificato = null, bool GeneraValIncrementale = true)
        {
            string CodificaGenerata = null;

            if (!string.IsNullOrEmpty(CodicePrecedenteCodificato) && AttributoLevelCoding.AddHigherLevel)
                CodificaGenerata = CodicePrecedenteCodificato;

            CodificaGenerata = CodificaGenerata + AttributoLevelCoding.Prefix;
            if (GeneraValIncrementale)
                CodificaGenerata = CodificaGenerata + GeneraValoreIncrementale(AttributoLevelCoding.IncrementalValue, AttributoLevelCoding.Step);
            else
                CodificaGenerata = CodificaGenerata + AttributoLevelCoding.IncrementalValue;

            CodificaGenerata = CodificaGenerata + AttributoLevelCoding.Suffix;

            return CodificaGenerata;
        }

        private string GeneraValoreIncrementale(string IncrementalValue, int Step)
        {
            int numero;
            if (int.TryParse(IncrementalValue, out numero))
                return GeneraValoreIncrementaleNumerico(IncrementalValue, Step);
            else
                return GeneraValoreIncrementaleAlfabetico(IncrementalValue, Step);
        }

        private string GeneraValoreIncrementaleNumerico(string incrementalValue, int step)
        {
            string IncrementalValueLocal = incrementalValue;
            string Zero = null;

            foreach (char ivl in IncrementalValueLocal)
            {
                if (ivl == '0')
                {
                    Zero = Zero + ivl.ToString();
                }
            }

            int NumericIncrementalValueLocal = Int32.Parse(IncrementalValueLocal);
            NumericIncrementalValueLocal = NumericIncrementalValueLocal + step;
            string NewncrementalValueLocal = NumericIncrementalValueLocal.ToString();

            while (IncrementalValueLocal.Length > NewncrementalValueLocal.Length)
            {
                NewncrementalValueLocal = "0" + NewncrementalValueLocal;
            }

            return NewncrementalValueLocal;
        }
        private string GeneraValoreIncrementaleAlfabetico(string incrementalValue, int step)
        {
            string IncrementalValueLocal = incrementalValue;
            string NewncrementalValueLocal = null;
            bool Aumenta = true;

            if (!string.IsNullOrEmpty(IncrementalValueLocal))
            {
                for (int i = IncrementalValueLocal.Length - 1; i >= 0; i--)
                {
                    Char CharValue = IncrementalValueLocal[i];

                    if (Aumenta)
                    {
                        //int index = UpperCharachters.Where(r => r.Value == IncrementalValueLocal[i]).FirstOrDefault().Key;
                        int index = EstraiIndice(CharValue);
                        if (UpperCharachters.ContainsKey(index + step))
                            //NewncrementalValueLocal = NewncrementalValueLocal + UpperCharachters[index + step];
                            NewncrementalValueLocal = NewncrementalValueLocal + EstraiLettera(CharValue, index + step);
                        else
                        {
                            int scarto = (UpperCharachters.Count() - (index + step)) * (-1);
                            NewncrementalValueLocal = null;
                            if (i == 0)
                            {
                                for (int r = 0; r < IncrementalValueLocal.Length; r++)
                                {
                                    //NewncrementalValueLocal = NewncrementalValueLocal + UpperCharachters[1];
                                    NewncrementalValueLocal = NewncrementalValueLocal + EstraiLettera(CharValue, 1);
                                }
                                //NewncrementalValueLocal = NewncrementalValueLocal + UpperCharachters[1];
                                NewncrementalValueLocal = NewncrementalValueLocal + EstraiLettera(CharValue, 1);
                                scarto = scarto - 1;
                                return GeneraValoreIncrementaleAlfabetico(NewncrementalValueLocal, scarto);
                            }
                            else
                            {
                                int IndexCiclo = RetrocediRicorsivamenteIndice(i, UpperCharachters, IncrementalValueLocal);
                                Char CharValueLocal = IncrementalValueLocal[IndexCiclo];
                                int indexPrecedente = EstraiIndice(CharValueLocal);
                                //int indexPrecedente = UpperCharachters.Where(r => r.Value == IncrementalValueLocal[IndexCiclo]).FirstOrDefault().Key;
                                bool passato = false;
                                for (int t = 0; t < IncrementalValueLocal.Length; t++)
                                {
                                    if (t == IndexCiclo)
                                    {
                                        if (t == IndexCiclo)
                                        {
                                            if (!UpperCharachters.ContainsKey(indexPrecedente + 1))
                                            {
                                                for (int r = 0; r < IncrementalValueLocal.Length; r++)
                                                {
                                                    //NewncrementalValueLocal = NewncrementalValueLocal + UpperCharachters[1];
                                                    NewncrementalValueLocal = NewncrementalValueLocal + EstraiLettera(CharValue, 1);
                                                }
                                                //NewncrementalValueLocal = NewncrementalValueLocal + UpperCharachters[1];
                                                NewncrementalValueLocal = NewncrementalValueLocal + EstraiLettera(CharValue, 1);
                                                scarto = scarto - 1;
                                                return GeneraValoreIncrementaleAlfabetico(NewncrementalValueLocal, scarto);
                                            }
                                        }

                                        //NewncrementalValueLocal = NewncrementalValueLocal + UpperCharachters[indexPrecedente + 1];
                                        NewncrementalValueLocal = NewncrementalValueLocal + EstraiLettera(CharValue, indexPrecedente + 1);
                                        passato = true;
                                    }
                                    else
                                    {
                                        if (passato)
                                        {
                                            //NewncrementalValueLocal = NewncrementalValueLocal + UpperCharachters[1];
                                            NewncrementalValueLocal = NewncrementalValueLocal + EstraiLettera(CharValue, 1);
                                        }
                                        else
                                        {
                                            NewncrementalValueLocal = NewncrementalValueLocal + IncrementalValueLocal[t];
                                        }
                                    }
                                }
                                scarto = scarto - 1;
                                return GeneraValoreIncrementaleAlfabetico(NewncrementalValueLocal, scarto);
                            }
                        }
                        Aumenta = false;
                        continue;
                    }
                    else
                    {
                        //NewncrementalValueLocal = IncrementalValueLocal[i].ToString() + NewncrementalValueLocal;
                        NewncrementalValueLocal = CharValue.ToString() + NewncrementalValueLocal;
                    }
                }
            }

            return NewncrementalValueLocal;
        }

        private int RetrocediRicorsivamenteIndice(int IndexCiclo, Dictionary<int, char> Charachters, string IncrementalValueLocal)
        {
            if (IndexCiclo == 0)
            {
                return IndexCiclo;
            }

            Char CharValue = IncrementalValueLocal[IndexCiclo - 1];
            int IndexPrecedente = EstraiIndice(CharValue);

            //int IndexPrecedente = Charachters.Where(r => r.Value == IncrementalValueLocal[IndexCiclo - 1]).FirstOrDefault().Key;
            int IndexCicloLocal = IndexCiclo - 1;
            if (IndexPrecedente == 26)
            {
                return RetrocediRicorsivamenteIndice(IndexCicloLocal, Charachters, IncrementalValueLocal);
            }
            else
                return IndexCicloLocal;
        }
        private int EstraiIndice(Char CharValue)
        {
            if (Char.IsLower(CharValue))
            {
                return LowerCharachters.Where(r => r.Value == CharValue).FirstOrDefault().Key;
            }
            else
            {
                return UpperCharachters.Where(r => r.Value == CharValue).FirstOrDefault().Key;
            }
        }
        private Char EstraiLettera(Char CharValue, int Index)
        {
            if (Char.IsLower(CharValue))
            {
                return LowerCharachters[Index];
            }
            else
            {
                return UpperCharachters[Index];
            }
        }

        public AttributoLevelCoding ConversioneInAttributoLevelCoding(AttributoCodingSetting AttributoCodingSetting)
        {
            AttributoLevelCoding AttributoLevelCoding = new AttributoLevelCoding();
            AttributoLevelCoding = new AttributoLevelCoding();
            AttributoLevelCoding.AddHigherLevel = AttributoCodingSetting.AggiungiCodiceSuperiore;
            AttributoLevelCoding.IsCoding = AttributoCodingSetting.Codifica;
            AttributoLevelCoding.Level = AttributoCodingSetting.Livello;
            AttributoLevelCoding.Step = AttributoCodingSetting.Passo;
            AttributoLevelCoding.Prefix = AttributoCodingSetting.Prefisso;
            AttributoLevelCoding.Suffix = AttributoCodingSetting.Suffisso;
            AttributoLevelCoding.IncrementalValue = AttributoCodingSetting.ValoreIncrementale;
            return AttributoLevelCoding;
        }
        public bool Run(HashSet<Guid> CheckedEntitiesId, AttributoCoding AttributoCoding, string AttributeCodice)
        {
            if (AttributeCodice == BuiltInCodes.Attributo.Codice || AttributeCodice == BuiltInCodes.Attributo.Nome)
                return EseguiAggiornamentoCodiceSuAlbero(CheckedEntitiesId, AttributoCoding);
            else
                return EseguiAggiornamentoCodiceSuAlbero(CheckedEntitiesId, AttributoCoding, false);
            //return EseguiAggiornamentoPiattoSuFoglie(CheckedEntitiesId, AttributoCoding);
        }

        public bool EseguiAggiornamentoCodiceSuAlbero(HashSet<Guid> CheckedEntitiesId, AttributoCoding AttributoCoding, bool UpdatePadri = true)
        {
            entitiesHelper = new EntitiesHelper(DataService);
            List<Guid> entitiesFound = null;
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            List<Entity> Entities = new List<Entity>();

            List<Entity> EntitiesLivello = null;
            EntityType entType = DataService.GetEntityType(EntityTypeKey);
            if (entType.IsTreeMaster)
            {

                List<TreeEntityMasterInfo> TreeInfo = DataService.GetFilteredTreeEntities(EntityTypeKey, null, null, out entitiesFound);
                TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, CheckedEntitiesId);
                TreeEntities = DataService.GetTreeEntitiesById(EntityTypeKey, TreeInfo.Select(item => item.Id));

                EntitiesLivello = TreeEntities.Where(l => l.Depth == 0).Cast<Entity>().ToList();
            }
            else
            {
                List<EntityMasterInfo> entsInfo = DataService.GetFilteredEntities(EntityTypeKey, null, null, null, out entitiesFound);
                EntitiesLivello = DataService.GetEntitiesById(EntityTypeKey, entitiesFound);

            }

            ModelActionResponse mar = null;
            ModelAction action = new ModelAction() { EntityTypeKey = EntityTypeKey };
            action.ActionName = ActionName.MULTI;
            List<ModelAction> NestedAction = new List<ModelAction>();

            Dictionary<int, string> DictionaryIncrementalValue = new Dictionary<int, string>();
            Dictionary<int, int> DictionaryStep = new Dictionary<int, int>();

            string CodicePrecedenteCodificato = null;
            int Livello = 0;

            string CodiceCodificato = null;
            int ContatoreFartelli = 1;


            //foreach (TreeEntity FiglioTreeEntity in TreeEntities.Where(l => l.Depth == Livello))
            foreach (Entity FiglioTreeEntity in EntitiesLivello)
            {
                AttributoLevelCoding AttributoLevelCoding = new AttributoLevelCoding();
                AttributoLevelCoding = AttributoCoding.LevelsCoding.Where(r => r.Level == Livello + 1).FirstOrDefault();
                int AttributoLevelCodingIniaziale = AttributoLevelCoding.Step;
                if (AttributoLevelCoding.IsCoding && CheckedEntitiesId.Contains(FiglioTreeEntity.EntityId))
                {
                    //if (FiglioTreeEntity.Attributi.Values.Where(a => a.AttributoCodice == AttributoCoding.AttributoCodice).FirstOrDefault() != null)
                    if (AttributoCoding.AttributoCodice == BuiltInCodes.Attributo.Codice || AttributoCoding.AttributoCodice == BuiltInCodes.Attributo.Nome)
                    {
                        //foreach (EntityAttributo Attributo in FiglioTreeEntity.Attributi.Values.Where(a => a.AttributoCodice == AttributoCoding.AttributoCodice))
                        foreach (Attributo att in entitiesHelper.GetAttributi(FiglioTreeEntity).Values.Where(a => a.Codice == AttributoCoding.AttributoCodice))
                        {
                            //Attributo att = Attributo.Attributo;
                            string codiceAttributo = att.Codice;
                            HashSet<Guid> Id = new HashSet<Guid>();
                            Id.Add(FiglioTreeEntity.EntityId);
                            Valore val = entitiesHelper.GetValoreAttributo(FiglioTreeEntity, att.Codice, false, true);
                            string ValoreEsistente = null;

                            if (val is ValoreTesto)
                            {
                                ValoreTesto valTesto = val as ValoreTesto;
                                ValoreEsistente = valTesto.V;
                            }

                            string ValoreCodificato = null;
                            if (ContatoreFartelli == 1)
                                ValoreCodificato = GeneraCodice(AttributoLevelCoding, CodicePrecedenteCodificato, false);
                            else
                            {
                                AttributoLevelCoding.Step = AttributoLevelCoding.Step * (ContatoreFartelli - 1);
                                ValoreCodificato = GeneraCodice(AttributoLevelCoding, CodicePrecedenteCodificato);
                                AttributoLevelCoding.Step = AttributoLevelCodingIniaziale;
                            }


                            if (AttributoCoding.PosizionamentoRispettoCodiceEsistente == 1)
                                ValoreCodificato = ValoreCodificato + ValoreEsistente;
                            if (AttributoCoding.PosizionamentoRispettoCodiceEsistente == 2)
                                ValoreCodificato = ValoreEsistente + ValoreCodificato;

                            CodiceCodificato = ValoreCodificato;

                            if ((FiglioTreeEntity is TreeEntity) && (FiglioTreeEntity as TreeEntity).IsParent)
                            {
                                if (UpdatePadri)

                                    NestedAction.AddRange(CreateNestedAction(ValoreCodificato, codiceAttributo, Id));
                            }
                            else
                            {
                                NestedAction.AddRange(CreateNestedAction(ValoreCodificato, codiceAttributo, Id));
                            }
                            //NestedAction.AddRange(CreateNestedAction(ValoreCodificato, codiceAttributo, Id));
                            ContatoreFartelli++;
                        }
                    }
                    else
                    {
                        //foreach (EntityAttributo Attributo in FiglioTreeEntity.Attributi.Values.Where(a => a.AttributoCodice == BuiltInCodes.Attributo.Codice))
                        foreach (Attributo att in entitiesHelper.GetAttributi(FiglioTreeEntity).Values.Where(a => a.Codice == AttributoCoding.AttributoCodice))
                        {
                            //Attributo att = Attributo.Attributo;
                            string codiceAttributo = att.Codice;
                            HashSet<Guid> Id = new HashSet<Guid>();
                            Id.Add(FiglioTreeEntity.EntityId);
                            Valore val = entitiesHelper.GetValoreAttributo(FiglioTreeEntity, att.Codice, false, true);
                            string ValoreEsistente = null;

                            if (val is ValoreTesto)
                            {
                                ValoreTesto valTesto = val as ValoreTesto;
                                ValoreEsistente = valTesto.V;
                            }

                            string ValoreCodificato = null;
                            if (ContatoreFartelli == 1)
                                ValoreCodificato = GeneraCodice(AttributoLevelCoding, CodicePrecedenteCodificato, false);
                            else
                            {
                                AttributoLevelCoding.Step = AttributoLevelCoding.Step * (ContatoreFartelli - 1);
                                ValoreCodificato = GeneraCodice(AttributoLevelCoding, CodicePrecedenteCodificato);
                                AttributoLevelCoding.Step = AttributoLevelCodingIniaziale;
                            }


                            if (AttributoCoding.PosizionamentoRispettoCodiceEsistente == 1)
                                ValoreCodificato = ValoreCodificato + ValoreEsistente;
                            if (AttributoCoding.PosizionamentoRispettoCodiceEsistente == 2)
                                ValoreCodificato = ValoreEsistente + ValoreCodificato;

                            CodiceCodificato = ValoreCodificato;
                            //NestedAction.AddRange(CreateNestedAction(ValoreCodificato, codiceAttributo, Id));
                            ContatoreFartelli++;
                        }
                    }
                }
                else
                {
                    //foreach (EntityAttributo Attributo in FiglioTreeEntity.Attributi.Values.Where(a => a.AttributoCodice == AttributoCoding.AttributoCodice))
                    foreach (Attributo att in entitiesHelper.GetAttributi(FiglioTreeEntity).Values.Where(a => a.Codice == AttributoCoding.AttributoCodice))
                    {
                        //Attributo att = Attributo.Attributo;
                        string codiceAttributo = att.Codice;
                        HashSet<Guid> Id = new HashSet<Guid>();
                        Id.Add(FiglioTreeEntity.EntityId);
                        Valore val = entitiesHelper.GetValoreAttributo(FiglioTreeEntity, att.Codice, false, true);
                        string ValoreEsistente = null;

                        if (val is ValoreTesto)
                        {
                            ValoreTesto valTesto = val as ValoreTesto;
                            ValoreEsistente = valTesto.V;
                        }

                        CodiceCodificato = ValoreEsistente;
                        //ContatoreFartelli++;


                    }
                }

                if (entType.IsTreeMaster)
                    AddAttributeToNestedAction(FiglioTreeEntity as TreeEntity, NestedAction, AttributoCoding, CodiceCodificato, Livello + 1, CheckedEntitiesId, UpdatePadri);
            }

            action.NestedActions = NestedAction;
            mar = DataService.CommitAction(action);
            if (mar.ActionResponse == ActionResponse.OK)
                MainOperation.UpdateEntityTypesView(new List<string>() { EntityTypeKey });
            else
                return false;

            return true;
        }

        private void AddAttributeToNestedAction(TreeEntity TreeEntity, List<ModelAction> NestedAction, AttributoCoding AttributoCoding, string CodicePrecedenteCodificato, int Livello, HashSet<Guid> CheckedEntitiesId, bool UpdatePadri)
        {
            string CodiceCodificato = null;
            int ContatoreFartelli = 1;

            foreach (TreeEntity FiglioTreeEntity in TreeEntity.Children.Where(l => l.Depth == Livello))
            {
                AttributoLevelCoding attributoLevelCoding = new AttributoLevelCoding();
                attributoLevelCoding = AttributoCoding.LevelsCoding.Where(r => r.Level == Livello + 1).FirstOrDefault();
                int AttributoLevelCodingIniaziale = attributoLevelCoding.Step;
                if (attributoLevelCoding.IsCoding && CheckedEntitiesId.Contains(FiglioTreeEntity.EntityId))
                {
                    //if (FiglioTreeEntity.Attributi.Values.Where(a => a.AttributoCodice == AttributoCoding.AttributoCodice).FirstOrDefault() != null)
                    if (AttributoCoding.AttributoCodice == BuiltInCodes.Attributo.Codice || AttributoCoding.AttributoCodice == BuiltInCodes.Attributo.Nome)
                    {
                        //foreach (EntityAttributo Attributo in FiglioTreeEntity.Attributi.Values.Where(a => a.AttributoCodice == AttributoCoding.AttributoCodice))
                        foreach (Attributo att in entitiesHelper.GetAttributi(FiglioTreeEntity).Values.Where(a => a.Codice == AttributoCoding.AttributoCodice))
                        {
                            //Attributo att = Attributo.Attributo;
                            string codiceAttributo = att.Codice;
                            HashSet<Guid> Id = new HashSet<Guid>();
                            Id.Add(FiglioTreeEntity.EntityId);
                            Valore val = entitiesHelper.GetValoreAttributo(FiglioTreeEntity, att.Codice, false, true);
                            string ValoreEsistente = null;

                            if (val is ValoreTesto)
                            {
                                ValoreTesto valTesto = val as ValoreTesto;
                                ValoreEsistente = valTesto.V;
                            }

                            string ValoreCodificato = null;
                            if (ContatoreFartelli == 1)
                                ValoreCodificato = GeneraCodice(attributoLevelCoding, CodicePrecedenteCodificato, false);
                            else
                            {
                                attributoLevelCoding.Step = attributoLevelCoding.Step * (ContatoreFartelli - 1);
                                ValoreCodificato = GeneraCodice(attributoLevelCoding, CodicePrecedenteCodificato);
                                attributoLevelCoding.Step = AttributoLevelCodingIniaziale;
                            }

                            if (AttributoCoding.PosizionamentoRispettoCodiceEsistente == 1)
                                ValoreCodificato = ValoreCodificato + ValoreEsistente;
                            if (AttributoCoding.PosizionamentoRispettoCodiceEsistente == 2)
                                ValoreCodificato = ValoreEsistente + ValoreCodificato;

                            CodiceCodificato = ValoreCodificato;

                            if (FiglioTreeEntity.IsParent)
                            {
                                if (UpdatePadri)

                                    NestedAction.AddRange(CreateNestedAction(ValoreCodificato, codiceAttributo, Id));
                            }
                            else
                            {
                                NestedAction.AddRange(CreateNestedAction(ValoreCodificato, codiceAttributo, Id));
                            }
                            //NestedAction.AddRange(CreateNestedAction(ValoreCodificato, codiceAttributo, Id));
                            ContatoreFartelli++;
                        }
                    }
                    else
                    {
                        //foreach (EntityAttributo Attributo in FiglioTreeEntity.Attributi.Values.Where(a => a.AttributoCodice == BuiltInCodes.Attributo.Codice))
                        foreach (Attributo att in entitiesHelper.GetAttributi(FiglioTreeEntity).Values.Where(a => a.Codice == BuiltInCodes.Attributo.Codice))
                        {
                            //Attributo att = Attributo.Attributo;
                            string codiceAttributo = att.Codice;
                            HashSet<Guid> Id = new HashSet<Guid>();
                            Id.Add(FiglioTreeEntity.EntityId);
                            Valore val = entitiesHelper.GetValoreAttributo(FiglioTreeEntity, att.Codice, false, true);
                            string ValoreEsistente = null;

                            if (val is ValoreTesto)
                            {
                                ValoreTesto valTesto = val as ValoreTesto;
                                ValoreEsistente = valTesto.V;
                            }

                            string ValoreCodificato = null;
                            if (ContatoreFartelli == 1)
                                ValoreCodificato = GeneraCodice(attributoLevelCoding, CodicePrecedenteCodificato, false);
                            else
                            {
                                attributoLevelCoding.Step = attributoLevelCoding.Step * (ContatoreFartelli - 1);
                                ValoreCodificato = GeneraCodice(attributoLevelCoding, CodicePrecedenteCodificato);
                                attributoLevelCoding.Step = AttributoLevelCodingIniaziale;
                            }


                            if (AttributoCoding.PosizionamentoRispettoCodiceEsistente == 1)
                                ValoreCodificato = ValoreCodificato + ValoreEsistente;
                            if (AttributoCoding.PosizionamentoRispettoCodiceEsistente == 2)
                                ValoreCodificato = ValoreEsistente + ValoreCodificato;

                            CodiceCodificato = ValoreCodificato;
                            //NestedAction.AddRange(CreateNestedAction(ValoreCodificato, codiceAttributo, Id));
                            ContatoreFartelli++;
                        }
                    }

                    AddAttributeToNestedAction(FiglioTreeEntity, NestedAction, AttributoCoding, CodiceCodificato, Livello + 1, CheckedEntitiesId, UpdatePadri);
                }
                else
                {
                    //foreach (EntityAttributo Attributo in FiglioTreeEntity.Attributi.Values.Where(a => a.AttributoCodice == AttributoCoding.AttributoCodice))
                    foreach (Attributo att in entitiesHelper.GetAttributi(FiglioTreeEntity).Values.Where(a => a.Codice == AttributoCoding.AttributoCodice))
                    {
                        //Attributo att = Attributo.Attributo;
                        string codiceAttributo = att.Codice;
                        HashSet<Guid> Id = new HashSet<Guid>();
                        Id.Add(FiglioTreeEntity.EntityId);
                        Valore val = entitiesHelper.GetValoreAttributo(FiglioTreeEntity, att.Codice, false, true);
                        string ValoreEsistente = null;

                        if (val is ValoreTesto)
                        {
                            ValoreTesto valTesto = val as ValoreTesto;
                            ValoreEsistente = valTesto.V;
                        }

                        CodiceCodificato = ValoreEsistente;
                        //ContatoreFartelli++;


                    }
                }

                AddAttributeToNestedAction(FiglioTreeEntity, NestedAction, AttributoCoding, CodiceCodificato, Livello + 1, CheckedEntitiesId, UpdatePadri);
            }
        }

        private bool EseguiAggiornamentoPiattoSuFoglie(HashSet<Guid> CheckedEntitiesId, AttributoCoding AttributoCoding)
        {
            entitiesHelper = new EntitiesHelper(DataService);
            List<Guid> entitiesFound = null;
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            List<Entity> Entities = new List<Entity>();
            List<TreeEntityMasterInfo> TreeInfo = DataService.GetFilteredTreeEntities(EntityTypeKey, null, null, out entitiesFound);
            TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, CheckedEntitiesId);
            TreeEntities = DataService.GetTreeEntitiesById(EntityTypeKey, TreeInfo.Select(item => item.Id));

            ModelActionResponse mar = null;
            ModelAction action = new ModelAction() { EntityTypeKey = EntityTypeKey };
            action.ActionName = ActionName.MULTI;
            List<ModelAction> NestedAction = new List<ModelAction>();

            Dictionary<int, string> DictionaryIncrementalValue = new Dictionary<int, string>();
            Dictionary<int, int> DictionaryStep = new Dictionary<int, int>();

            string CodicePrecedenteCodificato = null;

            string CodiceCodificato = null;
            int ContatoreFartelli = 1;

            foreach (TreeEntity FiglioTreeEntity in TreeEntities.Where(l => !l.IsParent))
            {
                AttributoLevelCoding AttributoLevelCoding = new AttributoLevelCoding();
                AttributoLevelCoding = AttributoCoding.LevelsCoding.FirstOrDefault();
                int AttributoLevelCodingIniaziale = AttributoLevelCoding.Step;
                if (CheckedEntitiesId.Contains(FiglioTreeEntity.EntityId))
                {
                    //foreach (EntityAttributo Attributo in FiglioTreeEntity.Attributi.Values.Where(a => a.AttributoCodice == AttributoCoding.AttributoCodice))
                    foreach (Attributo att in entitiesHelper.GetAttributi(FiglioTreeEntity).Values.Where(a => a.Codice == AttributoCoding.AttributoCodice))
                    {
                        //Attributo att = Attributo.Attributo;
                        string codiceAttributo = att.Codice;
                        HashSet<Guid> Id = new HashSet<Guid>();
                        Id.Add(FiglioTreeEntity.EntityId);
                        Valore val = entitiesHelper.GetValoreAttributo(FiglioTreeEntity, att.Codice, false, true);
                        string ValoreEsistente = null;

                        if (val is ValoreTesto)
                        {
                            ValoreTesto valTesto = val as ValoreTesto;
                            ValoreEsistente = valTesto.V;
                        }

                        string ValoreCodificato = null;
                        if (ContatoreFartelli == 1)
                            ValoreCodificato = GeneraCodice(AttributoLevelCoding, CodicePrecedenteCodificato, false);
                        else
                        {
                            AttributoLevelCoding.Step = AttributoLevelCoding.Step * (ContatoreFartelli - 1);
                            ValoreCodificato = GeneraCodice(AttributoLevelCoding, CodicePrecedenteCodificato);
                            AttributoLevelCoding.Step = AttributoLevelCodingIniaziale;
                        }


                        if (AttributoCoding.PosizionamentoRispettoCodiceEsistente == 1)
                            ValoreCodificato = ValoreCodificato + ValoreEsistente;
                        if (AttributoCoding.PosizionamentoRispettoCodiceEsistente == 2)
                            ValoreCodificato = ValoreEsistente + ValoreCodificato;

                        CodiceCodificato = ValoreCodificato;
                        NestedAction.AddRange(CreateNestedAction(ValoreCodificato, codiceAttributo, Id));
                        ContatoreFartelli++;
                    }
                }
            }

            action.NestedActions = NestedAction;
            mar = DataService.CommitAction(action);
            if (mar.ActionResponse == ActionResponse.OK)
                MainOperation.UpdateEntityTypesView(new List<string>() { EntityTypeKey });
            else
                return false;

            return true;
        }
        public IEnumerable<ModelAction> CreateNestedAction(string ValoreCodice, string CodiceAttributo, HashSet<Guid> EntitiesId)
        {
            List<ModelAction> Actions = new List<ModelAction>();

            ModelAction actionCodice = new ModelAction()
            {
                EntityTypeKey = EntityTypeKey,
                ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                AttributoCode = CodiceAttributo
            };
            actionCodice.EntitiesId = EntitiesId;

            actionCodice.NewValore = new ValoreTesto() { V = ValoreCodice };
            Actions.Add(actionCodice);


            return Actions;

        }
    }
}

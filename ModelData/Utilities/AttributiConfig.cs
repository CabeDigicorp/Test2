using ModelData.Model;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using ModelData.Dto;
using System.Xml.Linq;

namespace ModelData.Utilities
{
    public class AttributiConfig
    {
        private static Dictionary<string, List<ValoreConditionEnum>> _typeConditions;
        private static Dictionary<string, List<ValoreConditionsGroupOperator>> _typeOperators;

        static AttributiConfig()
        {
            _showSettings = new TupleList<string, bool>
            {
                { BuiltInCodes.DefinizioneAttributo.Guid, true },
                { BuiltInCodes.DefinizioneAttributo.Testo, true },
                { BuiltInCodes.DefinizioneAttributo.TestoRTF, true },
                { BuiltInCodes.DefinizioneAttributo.Reale, true },
                { BuiltInCodes.DefinizioneAttributo.Contabilita, true },
                { BuiltInCodes.DefinizioneAttributo.Riferimento, true },
                { BuiltInCodes.DefinizioneAttributo.Data, true },
                { BuiltInCodes.DefinizioneAttributo.TestoCollection, true },
                { BuiltInCodes.DefinizioneAttributo.GuidCollection, false },
                { BuiltInCodes.DefinizioneAttributo.Elenco, true },
                { BuiltInCodes.DefinizioneAttributo.Booleano, true },
                { BuiltInCodes.DefinizioneAttributo.Colore, true },
                { BuiltInCodes.DefinizioneAttributo.FormatoNumero, true },
                { BuiltInCodes.DefinizioneAttributo.Variabile, true }
            };

            _typeConditions = new Dictionary<string, List<ValoreConditionEnum>>
            {
                { BuiltInCodes.DefinizioneAttributo.Reale, new List<ValoreConditionEnum> { ValoreConditionEnum.Equal, ValoreConditionEnum.Unequal, ValoreConditionEnum.GreaterThan, ValoreConditionEnum.GreaterOrEqualThan, ValoreConditionEnum.LessThan, ValoreConditionEnum.LessOrEqualThan } },
                { BuiltInCodes.DefinizioneAttributo.Contabilita, new List<ValoreConditionEnum> { ValoreConditionEnum.Equal, ValoreConditionEnum.Unequal, ValoreConditionEnum.GreaterThan, ValoreConditionEnum.GreaterOrEqualThan, ValoreConditionEnum.LessThan, ValoreConditionEnum.LessOrEqualThan } },
                { BuiltInCodes.DefinizioneAttributo.Testo, new List<ValoreConditionEnum> { ValoreConditionEnum.Equal, ValoreConditionEnum.Unequal, ValoreConditionEnum.StartsWith, ValoreConditionEnum.EndsWith, ValoreConditionEnum.Contains, ValoreConditionEnum.NotContains } },
                { BuiltInCodes.DefinizioneAttributo.Data, new List<ValoreConditionEnum> { ValoreConditionEnum.Equal, ValoreConditionEnum.Unequal, ValoreConditionEnum.GreaterThan, ValoreConditionEnum.GreaterOrEqualThan, ValoreConditionEnum.LessThan, ValoreConditionEnum.LessOrEqualThan }  },
                { BuiltInCodes.DefinizioneAttributo.Elenco, new List<ValoreConditionEnum> { ValoreConditionEnum.Equal, ValoreConditionEnum.Unequal, ValoreConditionEnum.StartsWith, ValoreConditionEnum.EndsWith, ValoreConditionEnum.Contains, ValoreConditionEnum.NotContains }  }
            }; 
            
            _typeOperators = new Dictionary<string, List<ValoreConditionsGroupOperator>>
            {
                { "Logic", new List<ValoreConditionsGroupOperator> { ValoreConditionsGroupOperator.And, ValoreConditionsGroupOperator.Or } }
            };
        }

        private static TupleList<string, bool> _showSettings;
        public static TupleList<string, bool> ShowSettings { get { return _showSettings; } }

        public class TupleList<T1, T2> : List<Tuple<T1, T2>>
        {
            public void Add(T1 item, T2 item2)
            {
                Add(new Tuple<T1, T2>(item, item2));
            }
        }

        public static bool GetConditionsForType(string typeName, out List<ValoreConditionEnum>? operations)
        {
            return _typeConditions.TryGetValue(typeName, out operations);
        }

        public static int? GetEnumPositionForType(string typeName, ValoreConditionEnum value)
        {
            if (_typeConditions.TryGetValue(typeName, out List<ValoreConditionEnum>? operations) && operations != null)
            {
                int index = operations.IndexOf(value);

                return index != -1 ? index : 0;
            }

            return null;
        }

        public static bool IsAttributoFilterable(string? definizioneAttributo)
        {
            if(definizioneAttributo != null)
            {
                return _typeConditions.ContainsKey(definizioneAttributo);

            }
            return false;
        }

        public static List<ValoreConditionObj>? GetConditionsForType(string typeName)
        {
            try
            {
                List<ValoreConditionObj>? operations = new();
                if (_typeConditions.TryGetValue(typeName, out var enumList))
                {
                    operations = enumList.Select(e => new ValoreConditionObj
                    {
                        Nome = GetEnumDescription(e),
                        ValoreCondition = e
                    }).ToList();
                    return operations;
                }
                else
                {
                    return new();
                }
            }
            catch (Exception)
            {
                return new();
            }
        }

        public static bool GetOperatorsForType(string typeName, ref List<OperatoreLogicoObj>? operations)
        {
            if (_typeOperators.TryGetValue(typeName, out var enumList))
            {
                operations = enumList.Select(e => new OperatoreLogicoObj
                {
                    Nome = GetEnumLogicOperator(e),
                    OperatoreLogico = e
                }).ToList();
                return true;
            }
            operations = null;
            return false;
        }

        private static string GetEnumDescription(ValoreConditionEnum value)
        {
            switch (value)
            {
                case ValoreConditionEnum.Equal:
                    return "Uguale a";
                case ValoreConditionEnum.Unequal:
                    return "Diverso da";
                case ValoreConditionEnum.GreaterThan:
                    return "Maggiore di";
                case ValoreConditionEnum.GreaterOrEqualThan:
                    return "Maggiore o uguale di";
                case ValoreConditionEnum.LessThan:
                    return "Minore di";
                case ValoreConditionEnum.LessOrEqualThan:
                    return "Minore o uguale di";
                case ValoreConditionEnum.StartsWith:
                    return "Inizia con";
                case ValoreConditionEnum.EndsWith:
                    return "Termina con";
                case ValoreConditionEnum.Contains:
                    return "Contiene";
                case ValoreConditionEnum.NotContains:
                    return "Non contiene";
                default:
                    return value.ToString();
            }
        }        
        
        private static string GetEnumLogicOperator(ValoreConditionsGroupOperator value)
        {
            switch (value)
            {
                case ValoreConditionsGroupOperator.And:
                    return "AND";
                case ValoreConditionsGroupOperator.Or:
                    return "OR";
                default:
                    return value.ToString();
            }
        }
    }
}

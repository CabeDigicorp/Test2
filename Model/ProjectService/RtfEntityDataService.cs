using CommonResources;
using Commons;
using DevExpress.XtraRichEdit;
using MasterDetailModel;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Model
{
    public class RtfEntityDataService : RtfFieldsDataService
    {
        IDataService _dataService = null;

        char _propertyPathSeparator = '.';
        char _propertyPathIndexOpen = '[';
        char _propertyPathIndexClose = ']';
        public static char PropertySpacePlaceholder { get => '·'; }


        Dictionary<string, string> _fieldsDisplayNames = new Dictionary<string, string>();

        public static string RtfFieldClipboardFormat { get => "RtfFieldFormat"; }
        

        EntitiesHelper _entitiesHelper = null;

        //Altri dati che non si ricavano da _dataService

        //numero pagina
        public static string PageNumberDisplayName { get => "PageNumber"; }
        public static string PageNumberResult { get => "«PageNumber»"; }

        //numero totale pagine
        public static string NumberPagesDisplayName { get => "NumberPages"; }
        public static string NumberPagesResult { get => "«NumberPages»"; }

        //Nome File
        public static string FileNameFieldDisplayName { get => "ProjectFileName"; }
        string FileNameFieldResult { get; set; } = string.Empty;


        public RtfEntityDataService(IMainOperation mainOperation)
        {
            if (mainOperation == null)
                return;

            _dataService = mainOperation.GetCurrentProjectDataService();
            _entitiesHelper = new EntitiesHelper(_dataService);

            _fieldsDisplayNames.Add(BuiltInCodes.EntityType.InfoProgetto, "ProjectInfo");
            _fieldsDisplayNames.Add(BuiltInCodes.EntityType.Variabili, "Variable");

            string fullFileName = mainOperation.GetProjectFullFileName();
            if (fullFileName != null)
                FileNameFieldResult = Path.GetFileName(fullFileName);
            else
                FileNameFieldResult = LocalizationProvider.GetString("SenzaNome");
        }

        //public RtfDataService(IDataService dataService)
        //{
        //    _dataService = dataService;
        //    _entitiesHelper = new EntitiesHelper(dataService);

        //    _fieldsDisplayNames.Add(BuiltInCodes.EntityType.InfoProgetto, "ProjectInfo");

        //}

        public override string GetResult(string propertyPath, out ResultType resType)
        {
            resType = ResultType.PlainText;

            if (propertyPath == FileNameFieldDisplayName)
            {
                return FileNameFieldResult;
            }
            else if (propertyPath == PageNumberDisplayName)
            {
                return PageNumberResult;
            }
            else if (propertyPath == NumberPagesDisplayName)
            {
                return NumberPagesResult;
            }
            else
            {

                Valore val = GetValoreAttributo(propertyPath);

                if (val is ValoreTesto)
                {
                    return (val as ValoreTesto).Result;
                }
                else if (val is ValoreTestoRtf)
                {
                    resType = ResultType.RtfText;
                    return (val as ValoreTestoRtf).V;
                }
            }

            return null;
        }

        string GetEntityTypeKeyByFieldDisplayName(string entityTypeKeydisplayName)
        {
            return _fieldsDisplayNames.FirstOrDefault(item => item.Value == entityTypeKeydisplayName).Key;
        }

        public string GetRtfFieldDisplayName(string entityTypeKey, string codiceAttributo = null)
        {
            string displayName = string.Empty;

            Dictionary<string, EntityType> entTypes = _dataService.GetEntityTypes();
            EntityType entType = null;
            if (!entTypes.TryGetValue(entityTypeKey, out entType))
                return displayName;

            Attributo att = null;
            if (!entType.Attributi.TryGetValue(codiceAttributo, out att))
                return displayName;

            if (entityTypeKey == BuiltInCodes.EntityType.InfoProgetto)
                displayName = string.Format("{0}.{1}", _fieldsDisplayNames[BuiltInCodes.EntityType.InfoProgetto], att.Etichetta);

            if (entityTypeKey == BuiltInCodes.EntityType.Variabili)
                displayName = string.Format("{0}.{1}", _fieldsDisplayNames[BuiltInCodes.EntityType.Variabili], att.Etichetta);

            return displayName;
        }

        public Valore GetValoreAttributo(string propertyPath)
        {
            if (_dataService == null)
                return null;

            if (propertyPath == null || !propertyPath.Any())
                return null;




            string[] _0 = propertyPath.Split(_propertyPathSeparator);
            string entTypeName = _0[0];
            string _1 = _0[1];

            string entityTypeKey = GetEntityTypeKeyByFieldDisplayName(entTypeName);
            EntityType entType = _dataService.GetEntityTypes().Values.FirstOrDefault(item => item.GetKey() == entityTypeKey);
            if (entType == null)
                return null;

            string entityKey = string.Empty;
            Regex rx = new Regex(@"\[(.*?)\]");
            MatchCollection matches = rx.Matches(_1);
            if (matches.Count > 0)
            {
                entityKey = matches[0].Value;
            }
            
            string[] _2 = _1.Split(_propertyPathIndexOpen);
            string attEtichetta = _2[0].Replace(PropertySpacePlaceholder, ' ');
           


            Attributo att = entType.Attributi.Values.FirstOrDefault(item => item.Etichetta == attEtichetta);
            if (att == null)
            {
                string displayName = RtfHelperDevExpress.CreateFieldDisplayName(propertyPath);
                return new ValoreTesto() { V = displayName };
            }


            Valore val = _entitiesHelper.GetValoreAttributo(att.EntityTypeKey, att.Codice, false, false);

            if (val == null)
                val = new ValoreTesto() { V = string.Empty };
            else if (val is ValoreReale)
            {
                AttributoFormatHelper attFormatHelper = new AttributoFormatHelper(_dataService);
                string format = attFormatHelper.GetValorePaddedFormat(att);
                string str = (val as ValoreReale).FormatRealResult(format);
                val = new ValoreTesto() { V = str};
            }
            else if (val is ValoreContabilita)
            {
                AttributoFormatHelper attFormatHelper = new AttributoFormatHelper(_dataService);
                string format = attFormatHelper.GetValorePaddedFormat(att);
                string str = (val as ValoreContabilita).FormatRealResult(format);
                val = new ValoreTesto() { V = str };
            }

            return val;

        }

        




    }




}

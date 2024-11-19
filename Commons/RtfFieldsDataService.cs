using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class RtfFieldsDataService : List<object>
    {
        

        public RtfFieldsDataService()
        {
            Add(new MailMergeDataItem(this)
            {
                Impresa = "Impresa Rossi",
                Committente = "Pippo",
            });
        }

        public virtual string GetResult(string propertyPath, out ResultType resType)
        {
            resType = ResultType.PlainText;
            return string.Empty;
        }

    }

    public enum ResultType
    {
        PlainText = 0,
        RtfText = 1,
    }

    public class MailMergeDataItem
    {
        public MailMergeDataItem(RtfFieldsDataService owner)
        {
            _owner = owner;
        }

        RtfFieldsDataService _owner = null;
        public RtfFieldsDataService Owner { get => _owner; }

        public string Committente { get; set; }
        public string Impresa { get; set; }

    }


}

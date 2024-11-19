using Commons;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Documents.FormatProviders.Rtf;
using Telerik.Windows.Documents.Model;

namespace MasterDetailWpf
{
    public class RtfField : MergeField
    {
        public const string CustomFieldName = "RTFFIELD";
        public override string FieldTypeName { get => RtfField.CustomFieldName; }

        static RtfField()
        {
            //CodeBasedFieldFactory.RegisterFieldType(RtfField.CustomFieldName, () => { return new RtfField(); });
        }

        public override Field CreateInstance()
        {
            return new RtfField();
        }

        protected override DocumentFragment GetResultFragment()
        {
            RtfEntityDataService rtfDataService = null;
            if (Document.MailMergeDataSource.ItemsSource is RtfEntityDataService)
            {
                rtfDataService = Document.MailMergeDataSource.ItemsSource as RtfEntityDataService;
            }
            else if (Document.MailMergeDataSource.ItemsSource is IEnumerable<object>)
            {
                MailMergeDataItem obj = (Document.MailMergeDataSource.ItemsSource as IEnumerable<object>).First() as MailMergeDataItem;
                rtfDataService = obj.Owner as RtfEntityDataService;
            }

            if (rtfDataService == null)
                return null;

            //return base.GetResultFragment();
            ResultType resType = ResultType.PlainText;
            string res = rtfDataService.GetResult(PropertyPath, out resType);

            if (res != null)
                return this.CreateFragmentFromText(res);

            Valore val = rtfDataService.GetValoreAttributo(PropertyPath);

            if (val is ValoreTestoRtf)
            {
                ValoreTestoRtf valRtf = val as ValoreTestoRtf;

                //RtfFormatProvider rtfProvider = new RtfFormatProvider();
                var rtfProvider = (ValoreHelper.FormattedTextHelper as TelerikRtfHelper).RtfProvider;
                RadDocument doc = rtfProvider.Import(valRtf.V);

                return new DocumentFragment(doc);
            }
            else if (val != null)
            {
                return this.CreateFragmentFromText(val.PlainText);
            }

            return base.GetResultFragment();
        }



    }

    //public class PageNumberField : MergeField
    //{
    //    public const string CustomFieldName = "PAGENUMBERFIELD";
    //    public override string FieldTypeName { get => PageNumberField.CustomFieldName; }


    //    public override Field CreateInstance()
    //    {
    //        return new PageNumberField();
    //    }

    //    protected override DocumentFragment GetDisplayNameFragment()
    //    {
    //        return this.CreateFragmentFromText(RtfHelper.PageNumberResult);
    //    }

    //    protected override DocumentFragment GetResultFragment()
    //    {
    //        return this.CreateFragmentFromText(RtfHelper.PageNumberResult);
    //    }

    //}

    //public class NumberPagesField : MergeField
    //{
    //    public const string CustomFieldName = "NUMBERPAGESFIELD";
    //    public override string FieldTypeName { get => NumberPagesField.CustomFieldName; }


    //    public override Field CreateInstance()
    //    {
    //        return new NumberPagesField();
    //    }

    //    protected override DocumentFragment GetDisplayNameFragment()
    //    {
    //        return this.CreateFragmentFromText(RtfHelper.NumberPagesResult);
    //    }

    //    protected override DocumentFragment GetResultFragment()
    //    {
    //        return this.CreateFragmentFromText(RtfHelper.NumberPagesResult);
    //    }

    //}


}

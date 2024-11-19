using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatiGeneraliWpf.View
{
    public class UnitaMisuraView : SectionItemTemplateView
    {
        public ClientDataService DataService { get; internal set; }
        public IEntityWindowService WindowService { get; internal set; }
        public ModelActionsStack ModelActionsStack { get; internal set; }
        public IMainOperation MainOperation { get; internal set; }
        public NumericFormatView NumericFormatView { get; internal set; }

        public int Code => (int)DatiGeneraliSectionItemsId.UnitaDiMisura;

        internal void Init()
        {
            NumericFormatView.MainOperation = MainOperation;
            NumericFormatView.DataService = DataService;
            NumericFormatView.ModelActionsStack = ModelActionsStack;
            NumericFormatView.Init();
        }


    }
}

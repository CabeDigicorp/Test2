using Commons;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StampeWpf.Wizard
{
    public class ReportWizardSettingData : NotificationBase
    {
        public StampeData ReportSetting { get; set; }
        public ReportSettingViewHelper ReportSettingViewHelper { get; set; }
        public ReportSettingDataViewHelper ReportSettingDataViewHelper { get; set; }
        public string CodiceReport { get; set; }

        public string PreviousSelectedItemInGroup;

        private bool _IsAllFieldRtfFormat;
        public bool IsAllFieldRtfFormat
        {
            get
            {
                return _IsAllFieldRtfFormat;
            }
            set
            {
                if (SetProperty(ref _IsAllFieldRtfFormat, value))
                {
                    _IsAllFieldRtfFormat = value;
                }
            }
        }

        private bool _IsTabellaBordata;
        public bool IsTabellaBordata
        {
            get
            {
                return _IsTabellaBordata;
            }
            set
            {
                if (SetProperty(ref _IsTabellaBordata, value))
                {
                    _IsTabellaBordata = value;
                }
            }
        }

        private string _ButtonImage;
        public string ButtonImage
        {
            get
            {
                return _ButtonImage;
            }
            set
            {
                if (SetProperty(ref _ButtonImage, value))
                {
                    _ButtonImage = value;
                }
            }
        }

        private string _ButtonContent;
        public string ButtonContent
        {
            get
            {
                return _ButtonContent;
            }
            set
            {
                if (SetProperty(ref _ButtonContent, value))
                {
                    _ButtonContent = value;
                }
            }
        }
        public bool IsInLoadReportSaved { get; set; }
        public int ContatoreColonnaInserita { get; set; }

        #region Property of ReportWizardSettingDataWnd

        private string _Title;
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                if (SetProperty(ref _Title, value))
                {
                    _Title = value;
                }
            }
        }

        #endregion

        #region Property of StampeWizardStep1Ctrl

        private string _DescrizionrReport;
        public string DescrizionrReport
        {
            get
            {
                return _DescrizionrReport;
            }
            set
            {
                if (SetProperty(ref _DescrizionrReport, value))
                {
                    _DescrizionrReport = value;
                }
            }
        }
        private string _NumeroColonne;
        public string NumeroColonne
        {
            get
            {
                return _NumeroColonne;
            }
            set
            {
                if (SetProperty(ref _NumeroColonne, value))
                {
                    _NumeroColonne = value;
                }
            }

        }
        public ObservableCollection<CommonResources.Controls.ComboBoxTreeLevel> Sezioni { get; set; }
        public int GuidSezione { get; set; }
        public CommonResources.Controls.ComboBoxTreeLevel Sezione { get; set; }

        #endregion

        #region Property of StampeWizardStep2Ctrl
        public DocumentoView DocumentoCorpoView { get; set; }
        private ObservableCollection<RaggruppamentiItemsView> _ItemsRaggruppamenti;
        public ObservableCollection<RaggruppamentiItemsView> ItemsRaggruppamenti
        {
            get
            {
                return _ItemsRaggruppamenti;
            }
            set
            {
                if (SetProperty(ref _ItemsRaggruppamenti, value))
                {
                    _ItemsRaggruppamenti = value;
                }
            }
        }
        #endregion

        #region Button properties
        private Visibility _IsVisibleButtonForOperation;
        public Visibility IsVisibleButtonForOperation
        {
            get
            {
                return _IsVisibleButtonForOperation;
            }
            set
            {
                if (SetProperty(ref _IsVisibleButtonForOperation, value))
                {
                    _IsVisibleButtonForOperation = value;

                }
            }
        }
        private Visibility _IsVisibleDeleteButton;
        public Visibility IsVisibleDeleteButton
        {
            get
            {
                return _IsVisibleDeleteButton;
            }
            set
            {
                if (SetProperty(ref _IsVisibleDeleteButton, value))
                {
                    _IsVisibleDeleteButton = value;
                }
            }
        }
        private Visibility _IsAcceptButtonVisible;
        public Visibility IsAcceptButtonVisible
        {
            get
            {
                return _IsAcceptButtonVisible;
            }
            set
            {
                if (SetProperty(ref _IsAcceptButtonVisible, value))
                {
                    _IsAcceptButtonVisible = value;
                }
            }
        }
        public Dictionary<string, DocumentoView> GroupSetting { get; set; }

        #endregion
        public OrdinamentoView OrdinamentoView { get; set; }
    }
}

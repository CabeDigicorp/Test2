using CommonResources;
using Commons;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for WebSaveProjectWnd.xaml
    /// </summary>
    public partial class WebSelectProjectWnd : Window
    {   
        public ClientDataService DataService { get; internal set; }
        public WindowService WindowService { get; internal set; }
        public ModelActionsStack ModelActionsStack { get; internal set; }
        public IMainOperation MainOperation { get; internal set; }

        //in/out
        public Guid OperaId { get; private set; }
        public Guid ProgettoId { get; set; }
        public string NomeProgetto { get; set; }
        public WebSelectProjectWndType Type { get; set; } = WebSelectProjectWndType.Save;

        public WebSelectProjectWnd()
        {
            
            InitializeComponent();
            Loaded += WebSelectProjectWnd_Loaded;
            MessageBar.Visibility = Visibility.Collapsed;
            OpereWebCtrl.SelectedOperaIdChanged += OpereWebCtrl_SelectedOperaIdChanged;
            OpereWebCtrl.View.ErrorMsg += OpereView_ErrorMsg;
        }

        private void WebSelectProjectWnd_Loaded(object sender, RoutedEventArgs e)
        {
            Activate();
        }

        private void OpereView_ErrorMsg(object sender, EventArgs e)
        {
            MessageBarText.Text = (e as OpereWebViewMessageEventArgs)?.Message;
            MessageBar.Visibility = Visibility.Visible;
        }

        private void OpereWebCtrl_SelectedOperaIdChanged(object sender, EventArgs e)
        {
            UpdateStepBtns();
        }

        private void UpdateStepBtns()
        {
            if (CurrentPanelIndex == 0 && OpereWebCtrl.SelectedOperaId != Guid.Empty)
                StepNextBtn.IsEnabled = true;
            else
                StepNextBtn.IsEnabled = false;

            if (CurrentPanelIndex == 1)
                StepPrevBtn.IsEnabled = true;
            else
                StepPrevBtn.IsEnabled = false;
        }

        int _currentPanelIndex = -1;
        int CurrentPanelIndex
        {
            get => _currentPanelIndex;
            set
            {
                _currentPanelIndex = value;
                if (_currentPanelIndex == 0)
                {
                    OpereWebCtrl.Visibility = Visibility.Visible;
                    ProgettiWebCtrl.Visibility = Visibility.Collapsed;
                    UpdateStepBtns();
                    TitleText.Text = LocalizationProvider.GetString("Seleziona l'opera");
                    AcceptButton.IsEnabled = false;
                    OpereWebCtrl.Load();


                }
                else if (_currentPanelIndex == 1)
                {
                    OpereWebCtrl.Visibility = Visibility.Collapsed;
                    ProgettiWebCtrl.Visibility = Visibility.Visible;
                    UpdateStepBtns();
                    TitleText.Text = string.Format("{0} {1}",LocalizationProvider.GetString("Progetti di"), OpereWebCtrl.SelectedOperaNome);
                    AcceptButton.IsEnabled = true;
                    ProgettiWebCtrl.Load(OpereWebCtrl.SelectedOperaId);
                }
            }
        }


        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            OperaId = OpereWebCtrl.SelectedOperaId;
            ProgettoId = ProgettiWebCtrl.ProgettoId;
            NomeProgetto = ProgettiWebCtrl.NomeProgetto;

            if (string.IsNullOrEmpty(NomeProgetto))
            {
                MessageBarText.Text = LocalizationProvider.GetString("Inserire il nome del progetto");
                MessageBar.Visibility = Visibility.Visible;
                //MessageBarView.Show(LocalizationProvider.GetString("Inserire il nome del progetto"));
                return;
            }

            DialogResult = true;
        }

        internal void Init()
        {
            CurrentPanelIndex = 0;
            ProgettiWebCtrl.NomeProgetto = NomeProgetto;

            if (Type == WebSelectProjectWndType.Save)
            {
                Title = LocalizationProvider.GetString("SalvaSuWeb");
                ProgettiWebCtrl.IsNomeProgettoReadOnly = false;
            }
            else if (Type == WebSelectProjectWndType.Open)
            {
                Title = LocalizationProvider.GetString("ApriDaWeb");
                ProgettiWebCtrl.IsNomeProgettoReadOnly = true;
            }
        }

        private void StepNextBtn_Click(object sender, RoutedEventArgs e)
        {
            CurrentPanelIndex++;
        }

        private void StepPrevBtn_Click(object sender, RoutedEventArgs e)
        {
            CurrentPanelIndex--;
        }

        private void MessageBarOk_Click(object sender, RoutedEventArgs e)
        {
            MessageBar.Visibility = Visibility.Collapsed;
        }


    }

    public enum WebSelectProjectWndType
    {
        Save = 1,
        Open = 2,
    }

}

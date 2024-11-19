using MasterDetailView;
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
using System.Windows.Threading;

namespace StampeWpf
{
    /// <summary>
    /// Interaction logic for PreviewWnd.xaml
    /// </summary>
    public partial class PreviewWnd : Window
    {
        FastReport.Preview.PreviewControl prew = new FastReport.Preview.PreviewControl();
        public JReport.Report report { get; set; }
        public PreviewWnd()
        {
            InitializeComponent();
            report = new JReport.Report();
            
            FastReport.Utils.Config.ReportSettings.ShowProgress = true;

        }



        public void Init(bool IsPreview)
        {
            if (report == null)
                return;

            if (IsPreview)
            {
                prew.Buttons = FastReport.PreviewButtons.Find | FastReport.PreviewButtons.Navigator | FastReport.PreviewButtons.Zoom | FastReport.PreviewButtons.PageSetup;
            }
            else
            {
                prew.Buttons = FastReport.PreviewButtons.Find | FastReport.PreviewButtons.Navigator | FastReport.PreviewButtons.Print | FastReport.PreviewButtons.Save | FastReport.PreviewButtons.Zoom;
            }

            

            //RICARICO REPORT PER FORZRE AGGIORNAMENTO LAYOUT DI PREVIEW
            //report.Save(@"C:\Users\alberto.cantele\Desktop\Prova.frx");
            //string pippo = report.SaveToString();

            ////JReport.Report ReportObjecttest = new JReport.Report();
            ////report.Load(@"C:\Users\alberto.cantele\Desktop\Prova.frx");
            //report.LoadFromString(pippo);
            //ReportObjecttest.Prepare();
            //ReportObjecttest.ShowPrepared();
            try
            {
                report.Preview = prew;

                //ShowAsync();

                if (report.Prepare())
                {
                    report.ShowPrepared();
                    WindowsFormsHostControl.Child = prew;
                }
                



            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message.ToString());
            }   
        }

        protected async void ShowAsync()
        {
            //await Task.Run(() =>
            //{
            //    report.Prepare();
            //    //report.ShowPrepared();
            //    //WindowsFormsHostControl.Child = prew;
            //});

            //report.ShowPrepared();
            //WindowsFormsHostControl.Child = prew;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
            {
                report.Prepare();
                report.ShowPrepared();
                WindowsFormsHostControl.Child = prew;
            });

        }
    }
}

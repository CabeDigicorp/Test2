using Syncfusion.Windows.Controls.Gantt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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

namespace AttivitaWpf
{
    /// <summary>
    /// Interaction logic for ScalaCronologicaWnd.xaml
    /// </summary>
    public partial class ScalaCronologicaWnd : Window
    {
        private ScalaCronologicaView View { get { return DataContext as ScalaCronologicaView; } }
        public ScalaCronologicaWnd()
        {
            InitializeComponent();
            //Gantt.StickCurrentDateLineTo = CurentDateLinePositions.None;
            //ObservableCollection<TaskDetails> CollectionFake = new ObservableCollection<TaskDetails>();
            //CollectionFake.Add(new TaskDetails() { StartDate = DateTime.Today, FinishDate = DateTime.Today.AddSeconds(1) });
            //Gantt.ItemsSource = CollectionFake;
            //Gantt.Loaded += Gantt_Loaded;
            //Gantt.WeekBeginsOn = DayOfWeek.Monday;
            this.Loaded += ScalaCronologicaWnd_Loaded;
            this.Unloaded += ScalaCronologicaWnd_Unloaded;
        }

        private void ScalaCronologicaWnd_Unloaded(object sender, RoutedEventArgs e)
        {
            IsLoaded = false;
        }

        private bool IsLoaded;
        private void ScalaCronologicaWnd_Loaded(object sender, RoutedEventArgs e)
        {
            IsLoaded = true;
            int level = View.TimescaleRulerCount;
            View.TimescaleRulerCount = 0;
            View.TimescaleRulerCount = level;
            Gantt.View.SplitterWidth = 0;
        }

        //private void Gantt_Loaded(object sender, RoutedEventArgs e)
        //{
        //    FieldInfo fieldInfo = Gantt.GetType().GetField("ScheduleViewScrollViewer", BindingFlags.NonPublic | BindingFlags.Instance);
        //    if (fieldInfo != null)
        //    {
        //        ScrollViewer ScrollerGantt = fieldInfo.GetValue(Gantt) as ScrollViewer;
        //        if (ScrollerGantt != null)
        //        {
        //            ScrollerGantt.Style = null;
        //        }
        //    }
        //}

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (View.Accept())
                DialogResult = true;
        }

        private void ComboBoxAdv_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!View.FirstInitialization)
            {
                View.SelectionChanged();
                View.UpdateUI();
            }
        }

        private void ComboBoxAdv_SelectionChangedDetail(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!View.FirstInitialization)
            {
                View.SetFormatoDefault();
                View.RigenerateCustomSchedule();

            }
        }

        //private void Gantt_ScheduleCellCreated(object sender, ScheduleCellCreatedEventArgs args)
        //{
        //    View.ScheduleCellCreated(args);
        //}

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (!View.FirstInitialization)
            //    View.RigenerateCustomSchedule();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!View.FirstInitialization)
                View.RigenerateCustomSchedule();
        }

        private void ComboBoxAdvStile_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            View.UpdateUI();
        }

        private void GanttView_RequestTimescaleRulers(object sender, DevExpress.Xpf.Gantt.RequestTimescaleRulersEventArgs e)
        {
            if (View != null && IsLoaded)
            {
                if (View.RulerType == 1)
                {
                    Gantt.View.FirstVisibleDate = new DateTime(2074,1,1);
                }
                else
                {
                    Gantt.View.FirstVisibleDate = DateTime.Today;
                }
                
                View.GenerateTimeScale(e);
                Gantt.View.Zoom = View.GetZoom();
            }
                
        }

        private void ButtonRipristinaFormato_Click(object sender, RoutedEventArgs e)
        {
            View.SetFormatoDefault();
        }


    }
}

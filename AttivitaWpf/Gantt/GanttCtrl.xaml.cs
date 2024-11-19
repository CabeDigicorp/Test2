using AttivitaWpf.View;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AttivitaWpf
{
    /// <summary>
    /// Interaction logic for GanttCtrl.xaml
    /// </summary>
    public partial class GanttCtrl : Window
    {
        public GanttView View { get { return DataContext as GanttView; } }
        public GanttCtrl()
        {
            InitializeComponent();
            this.Loaded += GanttCtrl_Loaded;
        }

        private void GanttCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            int Livelli = View.TimescaleRulerCount;
            View.TimescaleRulerCount = 0;
            View.TimescaleRulerCount = Livelli;
            Gantt.View.Zoom = GetZoom();
        }

        private void GanttView_RequestTimescaleRulers(object sender, DevExpress.Xpf.Gantt.RequestTimescaleRulersEventArgs e)
        {
            if (View != null)
            {
                e.TimescaleRulers.Clear();
                bool ContinueCreateTimeRuler = false;
                double TimeScaleHeight = 0;

                if (View.ScalaCronologicaView.GetNumeroLivelli() == 3)
                {
                    AttivitaWpf.TabItemView TabItemViewSup = View.ScalaCronologicaView.TabItemViews.ElementAt(0);
                    View.GenerateGanttTimeScaleRuler(TabItemViewSup, e);
                    ContinueCreateTimeRuler = true;
                    if (TimeScaleHeight == 0)
                        TimeScaleHeight = 21;
                }
                if (View.ScalaCronologicaView.GetNumeroLivelli() == 2 || ContinueCreateTimeRuler)
                {
                    AttivitaWpf.TabItemView TabItemViewInt = View.ScalaCronologicaView.TabItemViews.ElementAt(1);
                    View.GenerateGanttTimeScaleRuler(TabItemViewInt, e);
                    ContinueCreateTimeRuler = true;
                    if (TimeScaleHeight == 0)
                        TimeScaleHeight = 21;
                    else
                        TimeScaleHeight = TimeScaleHeight + 21;
                }
                if (View.ScalaCronologicaView.GetNumeroLivelli() == 1 || ContinueCreateTimeRuler)
                {
                    AttivitaWpf.TabItemView TabItemViewInf = View.ScalaCronologicaView.TabItemViews.ElementAt(2);
                    View.GenerateGanttTimeScaleRuler(TabItemViewInf, e);
                    if (TimeScaleHeight == 0)
                        TimeScaleHeight = 21;
                    else
                        TimeScaleHeight = TimeScaleHeight + 21;
                }

                if (View.IsActiveNascondiDate)
                {
                    e.NonworkingDayVisibility = System.Windows.Visibility.Collapsed;
                    e.NonworkingTimeVisibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    e.NonworkingDayVisibility = System.Windows.Visibility.Visible;
                    e.NonworkingTimeVisibility = System.Windows.Visibility.Visible;
                }

                if (View.IsActiveProgressiva)
                    View.GetNoWotkingdays(true);
                else
                    View.GetNoWotkingdays();
            }
        }

        public TimeSpan GetZoom()
        {
            switch (View.ScalaCronologicaView.TabItemViews.LastOrDefault().SelectedUnita.Key)
            {
                case 0:
                    return new TimeSpan(288, 1, 0);
                    break;
                case 1:
                    return new TimeSpan(0, 1, 0);
                    break;
                case 2:
                    return new TimeSpan(0, 1, 0);
                    break;
                case 3:
                    return new TimeSpan(26, 0, 0);
                    break;
                case 4:
                    return new TimeSpan(0, 1, 0);
                    //DECADI
                    break;
                case 5:
                    return new TimeSpan(3, 0, 0);
                    break;
                case 6:
                    return new TimeSpan(0, 35, 0);
                    break;
                case 7:
                    return new TimeSpan(0, 2, 0);
                    break;
                case 8:
                    break;
                default:
                    return new TimeSpan(0, 1, 0);
                    break;
            }
            return new TimeSpan(0, 1, 0);
        }
    }
}

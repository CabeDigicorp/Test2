using AttivitaWpf.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AttivitaWpf
{
    public class AttivitaTemplateSelector : DataTemplateSelector
    {

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            if (item != null)
            {
                string dataType = item.GetType().ToString();

                if (item.GetType() == typeof(AttivitaView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttivita") as DataTemplate;
                }
                else if (item.GetType() == typeof(ElencoAttivitaView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateElencoAttivita") as DataTemplate;
                }
                else if (item.GetType() == typeof(GanttView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateGantt") as DataTemplate;
                }
                else if (item.GetType() == typeof(WBSView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateWBS") as DataTemplate;
                }
                else if (item.GetType() == typeof(CalendariView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateCalendari") as DataTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}

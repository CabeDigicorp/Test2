
using PrezzariWpf.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PrezzariWpf
{
    public class ElencoPrezziTemplateSelector : DataTemplateSelector
    {

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            if (item != null)
            {
                string dataType = item.GetType().ToString();

                if (item.GetType() == typeof(ElencoPrezziView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateElencoPrezzi") as DataTemplate;
                }
                else if (item.GetType() == typeof(CapitoliView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateCapitoli") as DataTemplate;
                }
                else if (item.GetType() == typeof(PrezzarioView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templatePrezzario") as DataTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}

using StampeWpf;
using StampeWpf.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StampeWpf
{
    public class StampeTemplateSelector : DataTemplateSelector
    {

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            if (item != null)
            {
                string dataType = item.GetType().ToString();

                if (item.GetType() == typeof(DocumentiView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateDocumenti") as DataTemplate;
                }
                else if (item.GetType() == typeof(ReportView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateReport") as DataTemplate;
                }
                else if (item.GetType() == typeof(StampeView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateStampe") as DataTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}

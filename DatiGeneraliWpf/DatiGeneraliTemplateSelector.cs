using ContattiWpf;
using DatiGeneraliWpf.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DatiGeneraliWpf
{
    public class DatiGeneraliTemplateSelector : DataTemplateSelector
    {

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            if (item != null)
            {
                string dataType = item.GetType().ToString();

                if (item.GetType() == typeof(DatiGeneraliView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateDatiGenerali") as DataTemplate;
                }
                else if (item.GetType() == typeof(InfoProgettoView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateInfoProgetto") as DataTemplate;
                }
                else if (item.GetType() == typeof(ContattiProgettoView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateContattiProgetto") as DataTemplate;
                }
                else if (item.GetType() == typeof(StiliProgettoView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateStiliProgetto") as DataTemplate;
                }
                else if (item.GetType() == typeof(UnitaMisuraView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateUnitaMisuraProgetto") as DataTemplate;
                }
                else if (item.GetType() == typeof(VariabiliView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateVariabiliProgetto") as DataTemplate;
                }
                else if (item.GetType() == typeof(AllegatiView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAllegatiProgetto") as DataTemplate;
                }
                else if (item.GetType() == typeof(TagView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateTagProgetto") as DataTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}

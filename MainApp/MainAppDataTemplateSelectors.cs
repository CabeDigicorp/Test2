using MainApp;
using System.Windows;
using System.Windows.Controls;

namespace MainApp
{
    //public class MainCurrentViewTemplateSelector : DataTemplateSelector
    //{

    //    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    //    {

    //        if (item != null)
    //        {
    //            string dataType = item.GetType().ToString();

    //            if (item.GetType() == typeof(ProjectView))
    //            {
    //                ContentPresenter pres = container as ContentPresenter;
    //                return pres.FindResource("templateProject") as DataTemplate;
    //            }
    //            else if (item.GetType() == typeof(MainMenuView))
    //            {
    //                ContentPresenter pres = container as ContentPresenter;
    //                return pres.FindResource("templateMainMenu") as DataTemplate;
    //            }

    //        }

    //        return base.SelectTemplate(item, container);
    //    }
    //}
}
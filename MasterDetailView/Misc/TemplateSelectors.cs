using MasterDetailModel;
using System.Windows;
using System.Windows.Controls;


namespace MasterDetailView
{

    public class ValoreAttributoTemplateSelector : DataTemplateSelector
    {

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            if (item != null)
            {
                string dataType = item.GetType().ToString();

                if (item.GetType() == typeof(ValoreTestoView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateTestoNew") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreDataView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateData") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreTestoCollectionView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateTestoCollection") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreGuidCollectionView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateGuidCollection") as DataTemplate;
                }
                //else if (item.GetType() == typeof(ValoreTestoSuggestView))
                //{
                //    return (Application.Current.Resources["ResourceDictionaries"] as ResourceDictionary)["templateTestoSuggest"] as DataTemplate;
                //}
                else if (item.GetType() == typeof(ValoreTestoRtfView))
                {
                    ContentPresenter pres = container as ContentPresenter;

                    return pres.FindResource("templateTestoRtf") as DataTemplate;
//                  return pres.FindResource("templateTestoRtfOld") as DataTemplate;

                }
                else if (item.GetType() == typeof(ValoreContabilitaView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateContabilita") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreRealeView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateReale") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreGuidView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateGuid") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreElencoView))
                {
                    ContentPresenter pres = container as ContentPresenter;

                    ValoreElencoView valoreElencoView = item as ValoreElencoView;
                    if (valoreElencoView.ValoreAttributoElenco.Type == ValoreAttributoElencoType.Font)
                        return pres.FindResource("templateElencoFont") as DataTemplate;
                    else
                        return pres.FindResource("templateElenco") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreColoreView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateColore") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreBooleanoView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateCheckBox") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreFormatoNumeroView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateFormatoNumero") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreLinkView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateLink") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreLinkCollectionView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateLinkCollection") as DataTemplate;
                }
                //else if (item.GetType() == typeof(ValoreVariabileView))
                //{
                //    ContentPresenter pres = container as ContentPresenter;
                //    return pres.FindResource("templateVariabile") as DataTemplate;
                //}
                //else if (item.GetType() == typeof(ValorePercentualeView))
                //{
                //    ContentPresenter pres = container as ContentPresenter;
                //    return pres.FindResource("templatePercentuale") as DataTemplate;
                //}
            }

            return base.SelectTemplate(item, container);
        }
    }

    public class RightSplitPaneTemplateSelector : DataTemplateSelector
    {

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            if (item != null)
            {
                string dataType = item.GetType().ToString();

                if (item.GetType() == typeof(FilterView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateFilter") as DataTemplate;
                    
                }
                else if (item.GetType() == typeof(SortView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateSort") as DataTemplate;
                    
                }
                else if (item.GetType() == typeof(GroupView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateGroup") as DataTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }

    public class AttributoSettingsTemplateSelector : DataTemplateSelector
    {

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            if (item != null)
            {
                string dataType = item.GetType().ToString();

                if (item.GetType() == typeof(AttributoSettingsTestoView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsTesto") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsDataView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsData") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsTestoRtfView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsTestoRtf") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsRealeView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsReale") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsContabilitaView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsContabilita") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsGuidRiferimentoView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsGuidRiferimento") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsGuidView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsGuid") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsTestoCollectionView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsTestoCollection") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsGuidCollectionView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsGuidCollection") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsElencoView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsElenco") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsColoreView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsColore") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsBooleanoView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsBooleano") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsFormatoNumeroView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsFormatoNumero") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsGuidCollectionRiferimentoView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsGuidCollectionRiferimento") as DataTemplate;
                }
                else if (item.GetType() == typeof(AttributoSettingsVariabileView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoSettingsVariabile") as DataTemplate;
                }
                //else if (item.GetType() == typeof(AttributoSettingsPercentualeView))
                //{
                //    ContentPresenter pres = container as ContentPresenter;
                //    return pres.FindResource("templateAttributoSettingsPercentuale") as DataTemplate;
                //}
            }

            return base.SelectTemplate(item, container);
        }
    }

    public class ValoreConditionSingleTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            if (item != null)
            {
                string dataType = item.GetType().ToString();

                if (item.GetType() == typeof(ValoreContabilitaConditionSingleView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateValoreContabilitaConditionSingle") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreRealeConditionSingleView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateValoreRealeConditionSingle") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreDataConditionSingleView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateValoreDataConditionSingle") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreTestoConditionSingleView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateValoreTestoConditionSingle") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreElencoConditionSingleView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateValoreElencoConditionSingle") as DataTemplate;
                }
                else if (item.GetType() == typeof(ValoreBooleanoConditionSingleView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateValoreBooleanoConditionSingle") as DataTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }

    public class ValoreConditionTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            if (item != null)
            {
                string dataType = item.GetType().ToString();

                if (item.GetType() == typeof(AttributoValoreConditionSingleView))
                {
                    ContentPresenter pres = container as ContentPresenter;
                    return pres.FindResource("templateAttributoValoreConditionSingle") as DataTemplate;
                }
                if (item.GetType() == typeof(ValoreConditionsGroupView))
                {
                    ValoreConditionsGroupView.FakeGroupView = item as ValoreConditionsGroupView;
                    ContentPresenter pres = container as ContentPresenter;
                    DataTemplate dataTemplate = pres.FindResource("templateValoreConditionsGroup") as DataTemplate;
                    return dataTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
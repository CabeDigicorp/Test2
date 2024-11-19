using MasterDetailModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MasterDetailWpf
{
    /// <summary>
    /// Interaction logic for ValoreConditionsGroupCtrl.xaml
    /// </summary>
    public partial class ValoreConditionsGroupCtrl : UserControl
    {
        public ValoreConditionsGroupView View { get => DataContext as ValoreConditionsGroupView; }

        static List<ValoreConditionsGroupView> AddedGroups = new List<ValoreConditionsGroupView>();

        public ValoreConditionsGroupCtrl()
        {
            InitializeComponent();

            if (AddedGroups.Any())
            {
                DataContext = AddedGroups.FirstOrDefault();
                AddedGroups.RemoveAt(0);
            }
            else
            {
                DataContext = new ValoreConditionsGroupView();
                View.ConditionItems.CollectionChanged += ConditionItems_CollectionChanged;
            }
            
        }

        private void ConditionItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems[0] is ValoreConditionsGroupView)
                {
                    ValoreConditionsGroupView groupView = e.NewItems[0] as ValoreConditionsGroupView;
                    groupView.ConditionItems.CollectionChanged += ConditionItems_CollectionChanged;
                    AddedGroups.Add(groupView);

                }
            }
        }

    }
}

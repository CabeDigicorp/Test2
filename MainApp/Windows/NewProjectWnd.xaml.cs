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

namespace MainApp
{
    /// <summary>
    /// Interaction logic for NewProjectWnd.xaml
    /// </summary>
    public partial class NewProjectWnd : Window
    {
        public ProjectModelView ProjectModelView { get; protected set; }

        public NewProjectWnd()
        {
            InitializeComponent();
            ProjectModelView = ProjectModelCtrl.DataContext as ProjectModelView;
            ProjectModelView.CurrentModelloChanged += ProjectModelView_CurrentModelloChanged;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectModelView.LoadAndUpdateClientManifest(false);
            DialogResult = true;
        }

        private void ProjectModelView_CurrentModelloChanged(object sender, EventArgs e)
        {
            //string modellofullFileName = ProjectModelView.GetCurrentModelloFullFileName();
            //if (modellofullFileName.Any())
            //    AcceptButton.IsEnabled = true;
            //else
            //    AcceptButton.IsEnabled = false;
        }

        private void ProjectModelCtrl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement frEl = e.OriginalSource as FrameworkElement;
            if (frEl != null)
            {
                if (frEl.DataContext is ClientModelloInfoView)
                {
                    ProjectModelView.LoadAndUpdateClientManifest(false);
                    string modellofullFileName = ProjectModelView.GetCurrentModelloFullFileName();
                    DialogResult = true;
                }
            }
        }
    }
}

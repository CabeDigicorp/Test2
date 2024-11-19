using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CommonResources.Controls
{
    /// <summary>
    /// Interaction logic for ComboBoxCtrl.xaml
    /// </summary>
    public partial class ComboBoxCtrl : UserControl
    {
        public event EventHandler<SelectedItemChangedEventArgs> SelectedItemChangedEvent;
        public event EventHandler<ClosePopUpEventArgs> CloseAllPopUpWhenOneIsOpenHandler;

        public Guid GuidIdentificativo { get; set; }

        public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register("HeaderHeight", typeof(double), typeof(ComboBoxCtrl));

        public double HeaderHeight
        {
            get 
            {
                return (double)GetValue(HeaderHeightProperty); 
            }
            set 
            { 
                SetValue(HeaderHeightProperty, value);
            }
        }

        public static readonly DependencyProperty TreeSourceProperty = DependencyProperty.Register("TreeSource", typeof(ObservableCollection<ComboBoxTreeLevel>), typeof(ComboBoxCtrl));

        public ObservableCollection<ComboBoxTreeLevel> TreeSource
        {
            get
            {
                return (ObservableCollection<ComboBoxTreeLevel>)GetValue(TreeSourceProperty);
            }
            set
            {
                SetValue(TreeSourceProperty, value);
                Tree.Items.Clear();
                AddSources();
            }
        }

        public ComboBoxTreeLevel SelectedItem { get; set; }

        public ComboBoxCtrl()
        {
            InitializeComponent();
            this.PreviewKeyDown += ComboBoxCtrl_PreviewKeyDown;
            GuidIdentificativo = Guid.NewGuid();
        }

        private void ComboBoxCtrl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                CloseAllPopUp();
        }


        public void AddSources()
        {
            if (TreeSource != null)
            {
                foreach (var FirstLevel in TreeSource)
                {
                    var FirstLevelItem = new TreeViewItem();
                    FirstLevelItem.Header = FirstLevel.Content;
                    Tree.Items.Add(FirstLevelItem);

                    foreach (var SecondLevel in FirstLevel.TreeContent)
                    {
                        var SecondLevelItem = new TreeViewItem();
                        SecondLevelItem.Header = SecondLevel.Content;
                        FirstLevelItem.Items.Add(SecondLevelItem);

                        foreach (var ThirdLevel in SecondLevel.TreeContent)
                        {
                            var ThirdLevelItem = new TreeViewItem();
                            ThirdLevelItem.Header = ThirdLevel.Content;
                            SecondLevelItem.Items.Add(ThirdLevelItem);
                        }
                    }
                }
            }
        }

        private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var trv = sender as TreeView;
            var trvItem = trv.SelectedItem as TreeViewItem;
            if (trvItem.Items.Count != 0) return;
            Header.Text = trvItem.Header.ToString();
            SelectedItem = new ComboBoxTreeLevel();
            var eventArgs = new SelectedItemChangedEventArgs();
            eventArgs.Content = trvItem.Header.ToString();
            SelectedItem.Content = trvItem.Header.ToString();
            SelectedItemChangedEvent?.Invoke(this, eventArgs);

            foreach (var Source in TreeSource)
            {
                foreach (var Content in Source.TreeContent)
                {
                    if (Content.Content == SelectedItem.Content)
                    {
                        SelectedItem.Name = Content.Name;
                        SelectedItem.Key = Content.Key;
                    }
                }
            }
            CloseAllPopUp();
        }

        public void InsertSelectedItemFromExternal(string Item)
        {
            Header.Text = Item;
            SelectedItem = new ComboBoxTreeLevel();
            SelectedItem.Content = Item;

            foreach (var FirstLevel in TreeSource)
            {
                if (FirstLevel.Content == Item)
                {
                    SelectedItem.Key = FirstLevel.Key;
                    SelectedItem.Name = FirstLevel.Name;
                }

                foreach (var SecondLevel in FirstLevel.TreeContent)
                {
                    if (SecondLevel.Content == Item)
                    {
                        SelectedItem.Key = SecondLevel.Key;
                        SelectedItem.Name = SecondLevel.Name;
                    }

                    foreach (var ThirdLevel in SecondLevel.TreeContent)
                    {
                        if (ThirdLevel.Content == Item)
                        {
                            SelectedItem.Key = ThirdLevel.Key;
                            SelectedItem.Name = ThirdLevel.Name;
                        }
                    }
                }
            }
        }


        private void Header_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PopUp.IsOpen) { PopUp.IsOpen = false; return; }
            PopUp.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
            PopUp.VerticalOffset = Header.Height;
            PopUp.StaysOpen = true;
            PopUp.Height = Tree.Height;
            PopUp.Width =Header.ActualWidth;
            PopUp.IsOpen = true;
        }

        public void CloseAllPopUp()
        {
            PopUp.IsOpen = false;
        }

        public void ClosePopUp(bool FromInterface)
        {
            if (FromInterface)
            {
                CloseAllPopUpWhenOneIsOpenHandler?.Invoke(this, new ClosePopUpEventArgs(null));
            }
            else
            {
                CloseAllPopUpWhenOneIsOpenHandler?.Invoke(this, new ClosePopUpEventArgs(GuidIdentificativo.ToString()));
            }

        }

        private void PopUp_Opened(object sender, EventArgs e)
        {
            if (PopUp.IsOpen)
            {
                ClosePopUp(false);
            }
        }
    }

    public class ComboBoxTreeLevel
    {
        public string Content { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public bool IsTreeMaster { get; set; }
        public ObservableCollection<ComboBoxTreeLevel> TreeContent = new ObservableCollection<ComboBoxTreeLevel>();
    }

    public class SelectedItemChangedEventArgs : EventArgs
    {
        public string Content { get; set; }
    }
    public class ClosePopUpEventArgs : EventArgs
    {
        public string Identificativo { get; set; }
        public ClosePopUpEventArgs(string id)
        {
            Identificativo = id;
        }
    }

}

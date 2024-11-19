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

namespace MainApp.AI
{
    /// <summary>
    /// Interaction logic for AIWnd.xaml
    /// </summary>
    public partial class AIWnd : Window
    {
        MainView MainView { get; set; }

        public AIWnd(MainView mainView)
        {
            InitializeComponent();

            MainView = mainView;
        }


        private async void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            AnswareBox.Text = string.Empty;
            AnswareBox.Text = await AI.Run(QueryBox.Text);

        }

        private async void EmbeddingBtn_Click(object sender, RoutedEventArgs e)
        {
            AnswareBox.Text = string.Empty;
            AnswareBox.Text = await AI.CreateEmbedding(QueryBox.Text);
        }

        private async void RunChat_Click(object sender, RoutedEventArgs e)
        {
            AnswareBox.Text = string.Empty;
            AnswareBox.Text = await AI.GetChatbotAnsware(QueryBox.Text);
        }
    }
}

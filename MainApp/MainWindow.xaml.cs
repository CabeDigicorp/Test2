using Commons;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using WebServiceClient.Clients;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MainView MainView { get { return DataContext as MainView; } }


        public MainWindow()
        {
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr");
            InitializeComponent();

            MainView.MainMenuView = MainMenuCtrl.DataContext as MainMenuView;
            MainView.MainMenuView.MainView = MainView;

            //MainView.MainMenuView.SetMainVersionWindowBorder(this);

            //scopo: non far coprire la task bar di windows una volta maximized
            //MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            MainView.WindowActivated += MainView_WindowActivated;
            KeyDown += MainWindow_KeyDown;

        }



        private void Titlebar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void MainView_WindowActivated(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                SystemCommands.RestoreWindow(this);
            }

            Activate();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            if (RadSplashScreenManager.IsSplashScreenActive)
                RadSplashScreenManager.Close();

            MainView.Init();
            Activate();

        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //////////////////
            //Scopo: vedere la progress bar di salvataggio prima della chiusura del programma. Altrimenti il programma si chiude (crash) prima che abbia finito il salvataggio
            if (MainView.IsProjectSavable)
                e.Cancel = true;
            ///////////////////

            bool close = await MainView.OnClosing();
            if (!close)
                e.Cancel = true;
            else if (e.Cancel == true)
            {
                SystemCommands.CloseWindow(this);
            }

            
            
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }




        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                SystemCommands.MaximizeWindow(this);
                MainView.IsMaximized = true;
            }
            else
            {
                SystemCommands.RestoreWindow(this);
                MainView.IsMaximized = false;
            }

            
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        /// <summary>
        /// Scopo: cambiare l'icona del pulsante maximized anche su doppio click sulla barra del titolo
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStateChanged(EventArgs e)
        {
            

            if (WindowState == WindowState.Maximized)
            {
                MainView.IsMaximized = true;
            }
            else
                MainView.IsMaximized = false;

            

            base.OnStateChanged(e);
        }

        /// <summary>
        /// gestione dei comandi nascosti
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    if (Keyboard.IsKeyDown(Key.S))//salvataggio del file .join in formato json (.jj)
                    {
                        MainView.MainMenuView.SaveCurrentProjectAsJson();
                    }
                    else if (Keyboard.IsKeyDown(Key.L))//apertura del file di log
                    {
                        MainAppLog.Show();
                    }
                    else if (Keyboard.IsKeyDown(Key.A))//test AI
                    {
                        if (DeveloperVariables.IsAIEnabled)
                        {
                            var AIWnd = new AI.AIWnd(MainView);
                            AIWnd.Show();

                        }
                    }

                    else if (Keyboard.IsKeyDown(Key.P))//Visualizza buttons privati
                    {
                        if (!MainView.MainMenuView.IsProjectOpened)
                        {
                            MainViewStatus.IsAdvancedMode = !MainViewStatus.IsAdvancedMode;
                            if (MainViewStatus.IsAdvancedMode)
                                JoinIcon.Background = FindResource("AdvancedModeColor") as SolidColorBrush;
                            else
                                JoinIcon.Background = new SolidColorBrush(Colors.Transparent);
                        }

                        MainView.MainMenuView.UpdateUI();
                    }

                }
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (Keyboard.IsKeyDown(Key.Z)) //Ctrl Z
                {
                    MainView.MainMenuView.ProjectUndo();
                }

            }

        }


        private void JoinWindow_LocationChanged(object sender, EventArgs e)
        {
             UpdateMaxSize();
        }

        /// <summary>
        /// Metodo per la gestione del Maximize della finestra sia con pulsante che con doppio click sulla title bar.
        /// Se non lo si gestisce a parte la finestra va a coprire la task bar di windows.
        /// Questa funzione utilizza System.Windows.Forms per riuscire ad identificare il monitor sul quale risiede la maggior
        /// parte della finestra.
        /// </summary>
        void UpdateMaxSize()
        {
            if (!IsActive)
                return;

            Point p = new Point((Left + Width) / 2.0, (Top + Height) / 2.0);
            Point wndCenter = PointToScreen(p);

            System.Drawing.Point pt = new System.Drawing.Point((int)wndCenter.X, (int)wndCenter.Y);
            var scr = System.Windows.Forms.Screen.FromPoint(pt);
            System.Windows.Forms.Screen screen = scr.Bounds.Contains(pt) ? scr : null;

            double primaryScalingRatio = (int)(100 * System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth);//125%
            if (screen != null)
            {
                MaxHeight = ((screen.WorkingArea.Height * 100)/ primaryScalingRatio) + SystemParameters.WindowResizeBorderThickness.Top + SystemParameters.WindowResizeBorderThickness.Bottom;
                MaxWidth = screen.WorkingArea.Width + SystemParameters.WindowResizeBorderThickness.Left + SystemParameters.WindowResizeBorderThickness.Right;
            }

        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MainView.MainMenuView.Guide();
        }

        private void JoinWindow_Activated(object sender, EventArgs e)
        {
            UpdateMaxSize();
        }

    }
}

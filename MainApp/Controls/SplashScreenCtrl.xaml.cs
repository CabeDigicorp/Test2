﻿using CommonResources;
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

namespace MainApp
{
    /// <summary>
    /// Interaction logic for SplashScreenCtrl.xaml
    /// </summary>
    public partial class SplashScreenCtrl : UserControl
    {

        public SplashScreenCtrl()
        {
            InitializeComponent();
            LoadingText.Text = LocalizationProvider.GetString("CaricamentoInCorso");
        }


    }
}

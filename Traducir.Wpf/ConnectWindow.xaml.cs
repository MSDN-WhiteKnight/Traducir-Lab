/* Traducir Windows client
 * Copyright (c) 2020,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight) 
 * License: MIT */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Traducir.Wpf
{
    public partial class ConnectWindow : Window
    {
        public string ConnectionString
        {
            get { return tbConnectionString.Text; }
            set { tbConnectionString.Text = value; }
        }

        public ConnectWindow()
        {
            InitializeComponent();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}

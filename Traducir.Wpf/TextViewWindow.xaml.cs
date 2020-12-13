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
    /// <summary>
    /// TextViewWindow codebehind
    /// </summary>
    public partial class TextViewWindow : Window
    {
        public TextViewWindow()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return this.tbContent.Text; }
            set { this.tbContent.Text = value; }
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

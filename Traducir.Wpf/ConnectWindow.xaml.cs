/* Traducir Windows client
 * Copyright (c) 2020,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight) 
 * License: MIT */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace Traducir.Wpf
{
    public partial class ConnectWindow : Window
    {
        public string ConnectionString
        {
            get { return tbConnectionString.Text; }
            set { tbConnectionString.Text = value; }
        }

        public string BackupFilePath
        {
            get { return tbBackupPath.Text; }
            set { tbBackupPath.Text = value; }
        }

        public bool RestoreBackup
        {
            get { return chbRestoreBackup.IsChecked == true; }
            set { chbRestoreBackup.IsChecked = value; }
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

        private void bBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.RestoreDirectory = true;
            ofd.DefaultExt = "bak";
            ofd.Filter = "SQL Server backup or archive (*.bak;*.bacpac;*.tgz;*.gz)|*.bak;*.bacpac;*.tgz;*.gz|All files|*.*";
            
            if (ofd.ShowDialog(this) == true)
            {
                tbBackupPath.Text = ofd.FileName;
            }
        }
    }
}

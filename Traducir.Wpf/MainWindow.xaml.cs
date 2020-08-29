/* Traducir Windows client
 * Copyright (c) 2020,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight) 
 * License: MIT */
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Traducir.Core.Services;
using Traducir.Core.Models;

namespace Traducir.Wpf
{
    public partial class MainWindow : Window
    {
        SOStringService svc;

        public MainWindow()
        {
            InitializeComponent();
            ConfigurationImpl config = new ConfigurationImpl();
            DbService db = new DbService(config);
            UserService usr = new UserService(db, config);
            this.svc = new SOStringService(db, usr, config);
        }

        private async void bShow_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            Func<SOString, bool> predicate = null;

            try
            {
                if (!String.IsNullOrEmpty(tbSourceRegex.Text))
                {
                    Regex regex;
                    try
                    {
                        regex = new Regex(tbSourceRegex.Text, RegexOptions.Compiled);
                    }
                    catch (ArgumentException ex)
                    {
                        MessageBox.Show(this, "Invalid source regex: " + ex.ToString(), "Error");
                        return;
                    }

                    predicate = s => regex.IsMatch(s.OriginalString);

                    ImmutableArray<SOString> res = await svc.GetStringsAsync(predicate);
                    dgContent.ItemsSource = res;
                }
                else
                {
                    ImmutableArray<SOString> res = await svc.GetStringsAsync();
                    dgContent.ItemsSource = res;
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                int TotalStrings = 0;

                while (true)
                {
                    TotalStrings = await svc.CountStringsAsync(s => !s.IsIgnored);
                    if (TotalStrings > 0) break;
                    await Task.Delay(500);
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }
    }
}

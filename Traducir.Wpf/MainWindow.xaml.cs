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
using System.ComponentModel;
using System.Globalization;

namespace Traducir.Wpf
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        SOStringService svc;
        SOString curr_string;
        int results_count;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            ConfigurationImpl config = new ConfigurationImpl();
            DbService db = new DbService(config);
            UserService usr = new UserService(db, config);
            this.svc = new SOStringService(db, usr, config);
            this.DataContext = this;
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

        public SOString CurrentString
        {
            get { return this.curr_string; }
            set
            {
                this.curr_string = value;
                this.OnPropertyChanged("CurrentString");
                this.OnPropertyChanged("Key");
                this.OnPropertyChanged("OriginalString");
                this.OnPropertyChanged("Translation");
            }
        }

        public int ResultsCount
        {
            get { return this.results_count; }
            set
            {
                this.results_count = value;
                this.OnPropertyChanged("ResultsCount");
            }
        }

        public string Key
        {
            get
            {
                if (CurrentString == null) return "";

                return CurrentString.Key;
            }
        }

        public string OriginalString
        {
            get
            {
                if (CurrentString == null) return "";

                return CurrentString.OriginalString;
            }
        }

        public string Translation
        {
            get
            {
                if (CurrentString == null) return "";

                return CurrentString.Translation;
            }
        }

        void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }

        Func<SOString, bool> ComposePredicate(Func<SOString, bool> oldPredicate, Func<SOString, bool> newPredicate)
        {
            if (oldPredicate == null)
            {
                return newPredicate;
            }

            return s => oldPredicate(s) && newPredicate(s);
        }

        private async void bShow_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            Func<SOString, bool> predicate = null;

            try
            {
                if (!String.IsNullOrEmpty(tbKeyFilter.Text))
                {
                    predicate = ComposePredicate(
                        predicate,
                        s => s.Key.StartsWith(tbKeyFilter.Text, true, CultureInfo.InvariantCulture)
                        );
                }

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

                    predicate = ComposePredicate(predicate, s => regex.IsMatch(s.OriginalString));
                }

                if (!String.IsNullOrEmpty(tbTranlationRegex.Text))
                {
                    Regex regex;
                    try
                    {
                        regex = new Regex(tbTranlationRegex.Text, RegexOptions.Compiled);
                    }
                    catch (ArgumentException ex)
                    {
                        MessageBox.Show(this, "Invalid translation regex: " + ex.ToString(), "Error");
                        return;
                    }

                    predicate = ComposePredicate(predicate, s => s.HasTranslation && regex.IsMatch(s.Translation));
                }

                ImmutableArray<SOString> res;

                if (predicate != null)
                {
                    res = await svc.GetStringsAsync(predicate);
                }
                else
                {
                    res = await svc.GetStringsAsync();
                }

                lvContent.ItemsSource = res;
                this.ResultsCount = res.Length;
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            tbKeyFilter.Text = "";
            tbSourceRegex.Text = "";
            tbTranlationRegex.Text = "";
            this.ResultsCount = 0;
            lvContent.ItemsSource = null;
        }
    }
}

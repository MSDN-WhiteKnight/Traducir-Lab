/* Traducir Windows client
 * Copyright (c) 2021,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight) 
 * License: MIT */
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Globalization;
using Traducir.Core;
using Traducir.Core.Models;
using Traducir.Core.Services;
using Traducir.Core.TextAnalysis;

namespace Traducir.Wpf
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        SOStringService svc;
        SOString curr_string;
        int results_count;

        const string StringsDirectory = "../../../html/strings";
        const string HtmlDirectory = "../../../html";

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ConnectWindow cwnd = new ConnectWindow();
            cwnd.Owner = this;
            cwnd.ConnectionString = Properties.Settings.Default.CONNECTION_STRING;

            if (cwnd.ShowDialog() == true)
            {
                string con_str = cwnd.ConnectionString;
                Properties.Settings.Default.CONNECTION_STRING = con_str;
                Properties.Settings.Default.Save();
            }
            else
            {
                this.Close();
                return;
            }

            if (cwnd.RestoreBackup)
            {
                if (String.Equals(cwnd.BackupFilePath.Trim(), String.Empty, StringComparison.InvariantCulture))
                {
                    MessageBox.Show(this, "Backup file path is not specified", "Error");
                    return;
                }

                this.Cursor = Cursors.Wait;

                try
                {
                    await DB.RestoreBackup(cwnd.BackupFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.GetType().ToString() + ":" + ex.Message, "SQL error");
                }
                finally
                {
                    this.Cursor = Cursors.Arrow;
                }
            }

            try
            {
                this.Cursor = Cursors.Wait;

                ConfigurationImpl config = new ConfigurationImpl();
                DbService db = new DbService(config);
                UserService usr = new UserService(db, config);
                this.svc = new SOStringService(db, usr, config);
                int TotalStrings = 0;

                while (true)
                {
                    TotalStrings = await svc.CountStringsAsync(s => !s.IsIgnored);
                    if (TotalStrings > 0) break;
                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.GetType().ToString() + ":" + ex.Message, "SQL error");
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
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.GetType().ToString() + ":" + ex.Message, "Error");
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

        async Task ShowTranslationHistory()
        {
            if (this.CurrentString == null) return;

            StringBuilder sb = new StringBuilder(1000);

            try
            {
                TextWriter wr = new StringWriter(sb);
                await History.HistoryToText(this.svc, this.CurrentString, wr);
                await wr.FlushAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error");
                return;
            }

            TextViewWindow wnd = new TextViewWindow();
            wnd.Text = sb.ToString();
            wnd.Title = "String translation history";
            wnd.Owner = this;
            wnd.ShowDialog();
        }

        private async void lvContent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //show string translation history for current string
            await ShowTranslationHistory();
        }

        private async void bHistory_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentString == null)
            {
                MessageBox.Show(this, "Select string first to show translation history", "Error");
                return;
            }

            await ShowTranslationHistory();
        }

        async Task ExportStringHistory(SOString str, string dir)
        {
            string path = Path.Combine(dir, str.Key.Replace('|', '_') + ".htm");
            StreamWriter wr = new StreamWriter(path, false, Encoding.UTF8);
            using (wr)
            {
                await HtmlGeneration.HistoryToHTML(svc, str, wr);
            }
        }

        private async void bExportHistory_Click(object sender, RoutedEventArgs e)
        {
            //export ALL string histories to HTML files
            this.Cursor = Cursors.Wait;

            try
            {
                var strings = await svc.GetStringsAsync(null, true);

                Directory.CreateDirectory(StringsDirectory);

                for (int i = 0; i < strings.Length; i++)
                {
                    await ExportStringHistory(strings[i], StringsDirectory);
                }

                //recent strings
                string path = Path.Combine(HtmlDirectory, "recent.htm");

                StreamWriter wr = new StreamWriter(path, false, Encoding.UTF8);
                using (wr)
                {
                    SOString[] recent = await svc.GetRecentStringsAsync();
                    string body = await HtmlGeneration.StringsToHTML(recent);
                    string title = "Recent strings - Traducir Extensions";

                    await HtmlGeneration.WriteTemplatedPage(title, body, wr);
                }

                //recent translations
                path = Path.Combine(HtmlDirectory, "recent-translations.htm");

                wr = new StreamWriter(path, false, Encoding.UTF8);
                using (wr)
                {
                    SOString[] recent = await svc.GetRecentTranslationsAsync();
                    string body = await HtmlGeneration.RecentTranslationsToHTML(recent);
                    string title = "Recent translations - Traducir Extensions";

                    await HtmlGeneration.WriteTemplatedPage(title, body, wr);
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        async Task<SOString[]> FindSimilarStrings(SOString str)
        {
            int len = str.OriginalString.Length;
            if (len < 12) len = 12;
            SOString[] strings = await svc.GetStringsByLenAsync(len - 10, len);

            if (strings.Length <= 1) return new SOString[0];

            bool found = false;
            int index=-1;

            //find input string in list
            for (int i = 0; i < strings.Length; i++)
            {
                if (strings[i].OriginalString.Equals(str.OriginalString, StringComparison.Ordinal))
                {
                    found = true;
                    index = i;
                    break;
                }
            }

            List<SOString> list = new List<SOString>(strings);

            if (!found)
            {
                //if it's not found, insert string to the end of the list
                list.Add(str);
                index = strings.Length;
            }

            //build vectors list
            List<DataVector> vectors = new List<DataVector>(strings.Length);

            DataVector v;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].OriginalString.Length > len) continue;

                v = DataVector.FromSoString(list[i], len);
                vectors.Add(v);
            }//end for

            double r = len*2.0;
            Tax tax = new Tax(vectors, r);

            List<VectorGroup> groups = tax.Classify();//запуск процесса классификации
            DataVector[] similar = null;

            //find group with input string
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].Contains(str.OriginalString))
                {
                    similar = groups[i].Vectors.ToArray();
                    break;
                }
            }

            if(similar==null) return new SOString[0];

            //return strings from the group
            SOString[] ret = new SOString[similar.Length];

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (SOString)similar[i].Tag;
            }

            return ret;
        }

        private async void bClassify_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            string s;

            try
            {
                SOString[] arr=await FindSimilarStrings(
                    new SOString { OriginalString = "Help new users be successful on the site by reviewing their first questions." }
                    );

                s = string.Empty;

                for (int i = 0; i < arr.Length; i++)
                {
                    s += arr[i].OriginalString;
                    s += Environment.NewLine;
                }
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }

            TextViewWindow wnd = new TextViewWindow();
            wnd.Text = s;
            wnd.Title = "Classification";
            wnd.Owner = this;
            wnd.ShowDialog();
        }
    }
}

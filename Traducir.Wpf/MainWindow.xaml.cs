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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using Traducir.Core.Services;
using Traducir.Core.Models;

namespace Traducir.Wpf
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        SOStringService svc;
        SOString curr_string;
        int results_count;

        const string StringsDirectory = "../../../html/strings";

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
                MessageBox.Show(this, ex.GetType().ToString()+":"+ex.Message, "SQL error");
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

        async Task HistoryToText(SOString str,TextWriter target)
        {
            var suggestions = await svc.GetSuggestionsByString(str.Id);
            await target.WriteLineAsync("Key: " + str.Key);
            await target.WriteLineAsync("Created: " + str.CreationDate.ToString());

            await target.WriteLineAsync("Original string: ");
            await target.WriteLineAsync(str.OriginalString);

            if (str.HasTranslation)
            {
                await target.WriteLineAsync("Current translation: ");
                await target.WriteLineAsync(str.Translation);
            }
            await target.WriteLineAsync();

            if (suggestions.Length == 0) await target.WriteLineAsync("(No suggestions found)");

            for (int i = 0; i < suggestions.Length; i++)
            {
                await target.WriteLineAsync("Suggestion #" + (i + 1).ToString() + ": ");
                await target.WriteLineAsync(suggestions[i].Suggestion);
                await target.WriteLineAsync("Author: " + suggestions[i].CreatedByName + " (" + suggestions[i].CreatedById.ToString() + ")");
                await target.WriteLineAsync("State: " + suggestions[i].State.ToString());
                await target.WriteLineAsync();

                for (int j = 0; j < suggestions[i].Histories.Length; j++)
                {
                    var hist = suggestions[i].Histories[j];
                    await target.WriteAsync(hist.CreationDate.ToString().PadRight(20, ' '));
                    
                    await target.WriteAsync(hist.UserName);
                    await target.WriteAsync(" [ID:");
                    await target.WriteAsync(hist.UserId.ToString());
                    await target.WriteAsync("] ");
                    await target.WriteAsync(hist.HistoryType.ToString());

                    if (!String.IsNullOrEmpty(hist.Comment))
                    {
                        await target.WriteAsync(" (");
                        await target.WriteAsync(hist.Comment);
                        await target.WriteAsync(')');
                    }

                    await target.WriteLineAsync();
                }

                await target.WriteLineAsync();
            }
        }

        async Task HistoryToHTML(SOString str, TextWriter target)
        {
            await target.WriteLineAsync("<html><head><title>String translation history - Traducir Lab</title>");
            await target.WriteLineAsync("</head><body><h1>String translation history</h1>");            

            var suggestions = await svc.GetSuggestionsByString(str.Id);

            await target.WriteAsync("<p>");
            await target.WriteAsync("<b>Key:</b> ");
            await target.WriteAsync("<a href=\"https://ru.traducir.win/filters?key="+WebUtility.UrlEncode(str.Key));
            await target.WriteAsync("\">" + str.Key + "</a><br/>");
            await target.WriteAsync("<b>Created:</b> " + str.CreationDate.ToString() + "</p>");

            await target.WriteLineAsync("<p><b>Original string:</b> <br/>");
            await target.WriteLineAsync(WebUtility.HtmlEncode(str.OriginalString));
            await target.WriteLineAsync("</p>");

            if (str.HasTranslation)
            {
                await target.WriteLineAsync("<p><b>Current translation:</b> <br/>");
                await target.WriteLineAsync(WebUtility.HtmlEncode(str.Translation));
                await target.WriteLineAsync("</p>");
            }
            await target.WriteLineAsync();

            if (suggestions.Length == 0) await target.WriteLineAsync("<p><i>(No suggestions found)</i></p>");

            for (int i = 0; i < suggestions.Length; i++)
            {
                await target.WriteLineAsync("<h2>Suggestion #" + (i + 1).ToString() + "</h2><p> ");
                await target.WriteAsync(WebUtility.HtmlEncode(suggestions[i].Suggestion));

                await target.WriteAsync("</p><p><b>Author:</b> <a href=\"https://ru.stackoverflow.com/users/");
                await target.WriteAsync(suggestions[i].CreatedById.ToString());
                await target.WriteAsync("/\">");
                await target.WriteAsync(WebUtility.HtmlEncode(suggestions[i].CreatedByName));
                await target.WriteAsync("</a><br/>");

                await target.WriteAsync("<b>State:</b> " + suggestions[i].State.ToString());
                await target.WriteLineAsync("</p>");

                await target.WriteLineAsync("<table border=\"1\" cellpadding=\"4\"><tr>");
                await target.WriteLineAsync("<th>Date</th>");
                await target.WriteLineAsync("<th>User</th>");
                await target.WriteLineAsync("<th>Action</th>");
                await target.WriteLineAsync("<th>Comment</th></tr>");

                for (int j = 0; j < suggestions[i].Histories.Length; j++)
                {
                    await target.WriteLineAsync("<tr>");

                    var hist = suggestions[i].Histories[j];
                    await target.WriteAsync("<td>"+hist.CreationDate.ToString()+ "</td>");
                    await target.WriteAsync("<td>");

                    await target.WriteAsync("<a href=\"https://ru.stackoverflow.com/users/");
                    await target.WriteAsync(hist.UserId.ToString());
                    await target.WriteAsync("/\">");
                    await target.WriteAsync(WebUtility.HtmlEncode(hist.UserName));
                    await target.WriteAsync("</a>");
                    
                    await target.WriteAsync("</td><td>");
                    await target.WriteAsync(hist.HistoryType.ToString());
                    await target.WriteAsync("</td><td>");

                    if (!String.IsNullOrEmpty(hist.Comment))
                    {
                        await target.WriteAsync(WebUtility.HtmlEncode(hist.Comment));
                    }

                    await target.WriteLineAsync("</td></tr>");
                }

                await target.WriteLineAsync("</table>");
            }

            await target.WriteLineAsync("<hr/>");
            await target.WriteLineAsync("<i>This document was automatically generated by ");
            await target.WriteLineAsync("<a href=\"https://github.com/MSDN-WhiteKnight/Traducir-Lab\">Traducir.Wpf</a></i>");
            await target.WriteLineAsync("</body></html>");
        }

        async Task ShowTranslationHistory()
        {
            if (this.CurrentString == null) return;

            StringBuilder sb = new StringBuilder(1000);

            try
            {
                TextWriter wr = new StringWriter(sb);
                await HistoryToText(this.CurrentString, wr);
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
                MessageBox.Show(this,"Select string first to show translation history","Error");
                return;
            }

            await ShowTranslationHistory();
        }

        async Task ExportStringHistory(SOString str, string dir)
        {
            string path = Path.Combine(dir, str.Key.Replace('|','_')+".htm");
            StreamWriter wr = new StreamWriter(path, false, Encoding.UTF8);
            using (wr)
            {
                await HistoryToHTML(str, wr);
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
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }
    }
}

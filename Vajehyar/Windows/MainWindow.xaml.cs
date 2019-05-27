﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Vajehyar.Properties;
using Vajehyar.Utility;
using Application = System.Windows.Forms.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace Vajehyar.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private ICollectionView _lines;
        public ICollectionView Lines
        {
            get => _lines;
            set { _lines = value; NotifyPropertyChanged("Lines"); }
        }

        private string _hint;

        public string Hint
        {
            get => _hint;
            set { _hint = value; NotifyPropertyChanged("Hint"); }
        }

        public MainWindow(Database database)
        {
            InitializeComponent();
            Lines = CollectionViewSource.GetDefaultView(database.Lines);
            Lines.Filter = FilterResult;
            Hint = $"جستجوی فارسی بین {database.GetCount().Round().Format()} واژه";
#if (!DEBUG)
            CheckUpdate();
#endif
        }

        private async void CheckUpdate()
        {
            if (Settings.Default.CheckUpdate)
                await GithubHelper.CheckUpdate();
        }

        private string _filterString;
        public string FilterString
        {
            get => _filterString;
            set
            {
                _filterString = value;
                NotifyPropertyChanged("FilterString");
                FilterCollection();
            }
        }

        private void FilterCollection()
        {
            _lines?.Refresh();
        }

       
        public bool FilterResult(object obj)
        {
            if (string.IsNullOrEmpty(_filterString))
                return false;

            string str = obj as string;
            string pattern = _filterString;

            if (FullText.IsChecked==true)
            {
                txtSearch.Text.Replace("*", "");
                return str.Contains(_filterString);
            }

            if (WholeWord.IsChecked==true)
            {
                txtSearch.Text.Replace("*", "");
                pattern = @"\b" + _filterString.Replace("*", "") + @"\b";
            }

            if (UseWildcards.IsChecked==true)
            {
                pattern = _filterString.WildCardToRegular();
            }

            
            return Regex.IsMatch(str, pattern);
        }

        #region Events

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterString = txtSearch.Text;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                txtSearch.SelectAll();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private void TxtSearch_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            bool space = Keyboard.IsKeyDown(Key.Space);
            bool two = Keyboard.IsKeyDown(Key.D2);

            if (shift && space || ctrl && shift && two)
            {
                e.Handled = true;
                txtSearch.Text += "\u200c";
                txtSearch.SelectionStart = txtSearch.Text.Length;
                txtSearch.SelectionLength = 0;
            }
        }

        private async void TxtSearch_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (await txtSearch.GetIdle())
            {
                FilterString = txtSearch.Text;
            }
        }

        private void RadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            FilterString = txtSearch.Text;
        }
    }

    
}

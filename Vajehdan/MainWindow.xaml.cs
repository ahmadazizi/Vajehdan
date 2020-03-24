﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using BFF.DataVirtualizingCollection;
using Gma.System.MouseKeyHook;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using Vajehdan;
using Vajehdan.Properties;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Clipboard;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;


namespace Vajehdan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly int _triggerThreshold = 500;
        private int _lastCtrlTick;

        private AsyncVirtualizingCollection<string[]> _motaradefList;
        public AsyncVirtualizingCollection<string[]> MotaradefList
        {
            get { return _motaradefList; }
            set
            {
                _motaradefList = value;
                NotifyPropertyChanged("MotaradefList");
            }
        }


        private GridVirtualizingCollectionView _gridVirtualizingItemsSource;
        public GridVirtualizingCollectionView GridVirtualizingItemsSource
        {
            get { return _gridVirtualizingItemsSource; }
            set
            {
                _gridVirtualizingItemsSource = value;
                NotifyPropertyChanged("GridVirtualizingItemsSource");
            }
        }

        private ICollectionView _motaradefMotazadList;
        public ICollectionView MotaradefMotazadList
        {
            get => _motaradefMotazadList;
            set { _motaradefMotazadList = value; NotifyPropertyChanged("MotaradefMotazadList"); }
        }

        private ICollectionView _TeyfiList;
        public ICollectionView TeyfiList
        {
            get => _TeyfiList;
            set { _TeyfiList = value; NotifyPropertyChanged("TeyfiList"); }
        }

        private ICollectionView _EmlaeiList;
        public ICollectionView EmlaeiList
        {
            get => _EmlaeiList;
            set { _EmlaeiList = value; NotifyPropertyChanged("EmlaeiList"); }
        }

        private ICollectionView _didYouMeanList;
        public ICollectionView DidYouMeanList
        {
            get => _didYouMeanList;
            set { _didYouMeanList = value; NotifyPropertyChanged("DidYouMeanList"); }
        }

        public MainWindow()
        {
            

            InitializeComponent();

            //MotaradefMotazadList = CollectionViewSource.GetDefaultView(Database.Motaradef());
            //MotaradefMotazadList.Filter = FilterResult;
            //var motaradefCollectionView = MotaradefMotazadList as ListCollectionView;
            //motaradefCollectionView.CustomSort = new CustomSorter(this);
            /*
            TeyfiList = CollectionViewSource.GetDefaultView(database.words_teyfi);
            TeyfiList.Filter = FilterResult;
            var teyfiCollectionView = TeyfiList as ListCollectionView;
            teyfiCollectionView.CustomSort = new CustomSorter(this);

            EmlaeiList = CollectionViewSource.GetDefaultView(database.words_emlaei);
            EmlaeiList.Filter = EmlaeiFilterResult;*/

            //GridVirtualizingItemsSource = new GridVirtualizingCollectionView(Database.Motaradef());
            IItemsProvider<string[]> items=new MotaradefItemProvider(Database.Motaradef().Count,50);
            MotaradefList=new AsyncVirtualizingCollection<string[]>(items);
            


            var globalMouseHook = Hook.GlobalEvents();
            globalMouseHook.MouseDown += GlobalMouseHook_MouseDown;
            
            _keyboardHook = new KeyboardHook();
            _keyboardHook.SetHook();
            _keyboardHook.OnKeyDownEvent += (o, arg) =>
            {
                //if (Windows.OfType<SettingWindow>().Any()) return;

                if (arg.KeyData == Keys.Escape)
                {
                    HideMainWindow();
                    return;
                }

                if (!Settings.Default.OpenByDoubleAlt)
                    return;

                if (arg.Modifiers != Keys.Alt)
                    return;

                int thisCtrlTick = Environment.TickCount;
                int elapsed = thisCtrlTick - _lastCtrlTick;

                if (elapsed <= _triggerThreshold)
                {
                    ShowMainWindow();
                }

                _lastCtrlTick = thisCtrlTick;
            };

            HideMainWindow();




#if (!DEBUG)
            CheckUpdate();

#endif
        }

        private async void CheckUpdate()
        {
            await Helper.CheckUpdate();
        }

        private KeyboardHook _keyboardHook;


        public bool FilterResult(object obj)
        {
            string filterString = txtSearch.Text;
            if (string.IsNullOrEmpty(filterString))
                return false;

            var words = string.Join("،",obj as string[]);

            return words.Contains(filterString);

        }



        private bool EmlaeiFilterResult(object obj)
        {
            string filterString = txtSearch.Text;
            if (string.IsNullOrEmpty(filterString))
                return false;

            var words = obj as string;
            
            return words.RemoveDiacritics().Contains(filterString);
        }

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
        
        private async Task ShowMessage(string message)
        {
            AutoClosemessage.Text = message;
            AutoCloseMessageContainer.Visibility = Visibility.Visible;
            await Task.Delay(2000);
            AutoCloseMessageContainer.Visibility = Visibility.Collapsed;

        }

        private async void Word_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            string word = (sender as Button).Content.ToString();
            Clipboard.SetText(word);
            string message = $".واژۀ «{word}» کپی شد";
            await ShowMessage(message);
        }

        private void Word_OnClick(object sender, RoutedEventArgs e)
        {
            //txtSearch.Text = FilterString = (sender as Button).Content.ToString();
            txtSearch.Focus();
            txtSearch.SelectAll();
        }

        private void TxtSearch_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (txtSearch.SelectedText.Length == 0)
            {
                txtSearch.SelectAll();
            }
        }

        private void TxtSearch_OnLostFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher?.BeginInvoke((ThreadStart)(() =>
            {
                Keyboard.Focus(txtSearch);
            }));
        }

        private void TxtSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearch.Text.Length==1)
                return;
            
            //if (await txtSearch.IsIdle())
            {
                //DataGrid_Motaradef.View.Filter = FilterResult;
                //DataGrid_Motaradef.View.RefreshFilter();
            }
        }

        private void TxtSearch_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //FilterString = txtSearch.Text;
            }
        }

        private void Menu_RunCommand(object sender, RoutedEventArgs e)
        {
            switch (((FrameworkElement)sender).Name)
            {
                case "ItemSetting":
                    new SettingWindow().Show();
                    break;

                case "ItemAbout":
                    new AboutWindow().Show();
                    break;

                case "ItemExit":
                    Application.Current.Shutdown();
                    break;
            }
        }

        public void ShowMainWindow()
        {
            WindowState = WindowState.Normal;
            Show();
            txtSearch.Focus();
            txtSearch.SelectAll();
        }

        public void HideMainWindow()
        {
            Hide();
            WindowState = WindowState.Minimized;
        }

        private void GlobalMouseHook_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!Settings.Default.MinimizeWhenClickOutside)
                return;

            if (e.X < Left || e.X > Left + Width || e.Y < Top || e.Y > Top + Height)
            {
                HideMainWindow();
            }
        }


        private void NotifyIconClickCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ShowMainWindow();
        }


    }

}

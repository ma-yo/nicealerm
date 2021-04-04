﻿using System;
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
using System.Windows.Shapes;

namespace NiceAlerm
{
    /// <summary>
    /// CalendarWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CalendarWindow : Window
    {
        private DateTime _selectedDate;
        public bool IsSelected { get; set; }
        public CalendarWindow()
        {
            InitializeComponent();
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedDate = Calendar.SelectedDate.Value;
            IsSelected = true;
            this.Close();
        }
        public DateTime GetDate()
        {
            return _selectedDate;
        }
    }
}

using NiceAlerm.Models;
using System;
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
    /// AlermWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AlermWindow : Window
    {
        private Alerm alerm;
        List<Alerm> snoozeList;
        private SnoozeWindow snoozeWindow;


        public AlermWindow()
        {
            InitializeComponent();
        }

        internal void SetAlerm(Alerm a, List<Alerm> list)
        {
            alerm = a.DeepCopy();
            alerm.AlermDelete = true;
            alerm.ScheduleList.Clear();
            snoozeList = list;
        }

        private void CommitButton_Click(object sender, RoutedEventArgs e)
        {
            int hour = snoozeWindow.HourUpDown.Value.Value;
            int minute = snoozeWindow.MinuteUpDown.Value.Value;

            if(hour == 0 && minute == 0)
            {
                MessageBox.Show("指定した時刻では設定出来ません。", "スヌーズ設定エラー", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DateTime nextSchedule = DateTime.Now.AddHours(hour).AddMinutes(minute);
            Schedule schedule = new Schedule();
            schedule.StartTime = nextSchedule.ToString("HH:mm");
            schedule.StartAddTime = 0;
            schedule.Enable = true;
            schedule.ScheduleDelete = true;
            schedule.ScheduleType = "1回のみ";
            schedule.ScheduleTypeIndex = 0;
            schedule.ScheduleValue = nextSchedule.ToString("yyyy/MM/dd");
            alerm.ScheduleList.Add(schedule);
            snoozeList.Add(alerm);
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (snoozeWindow != null)
            {
                snoozeWindow.Close();
                snoozeWindow = null;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            snoozeWindow = new SnoozeWindow();
            snoozeWindow.Owner = this;
            snoozeWindow.Show();
            snoozeWindow.CommitButton.Click += CommitButton_Click;
            SetSnoozeWindowLocation();
        }

        private void SetSnoozeWindowLocation()
        {
            if (snoozeWindow == null) return;
            snoozeWindow.Left = this.Left + 8;
            snoozeWindow.Top = this.Top + this.Height - 7;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            SetSnoozeWindowLocation();
        }
    }
}

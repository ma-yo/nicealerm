using log4net;
using Newtonsoft.Json;
using NiceAlerm.Models;
using NiceAlerm.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NiceAlerm
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// ロガー
        /// </summary>
        ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// アラーム情報
        /// </summary>
        private List<Alerm> alermList = new List<Alerm>();
        /// <summary>
        /// 設定ファイル
        /// </summary>
        private const string SETTING_FILE = "setting.json";

        /// <summary>
        /// アラーム用スレッド
        /// </summary>
        private Task alermThread;
        /// <summary>
        /// スレッド状態ﾌﾗｸﾞ
        /// </summary>
        private volatile bool threadStart = false;
        /// <summary>
        /// スレッド中断フラグ
        /// </summary>
        private volatile bool threadPause = false;

        List<Alerm> snoozeList = new List<Alerm>();
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                CreateAppDir();
                LoadAlerm();
                LoadOutlookSchedule();


                //重複起動チェック等でアプリが開始OKでない場合は終了する
                if (!Singleton.GetInstance().AppEnabled)
                {
                    this.Close();
                    return;
                }

                TaskbarIcon.Icon = global::NiceAlerm.Properties.Resources.clock_stop;
                AlermStopMenu.IsEnabled = false;
                if (alermList.Count > 0)
                {
                    StartAlermThread();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadOutlookSchedule()
        {
            try
            {
                alermList.RemoveAll(a => a.OutlookSchedule);
                if (System.IO.File.Exists(PathUtil.GetOutlookMailPath()))
                {
                    OutlookUtil outlookManager = new OutlookUtil();
                    using (StreamReader sr = new StreamReader(PathUtil.GetOutlookMailPath()))
                    {
                        string json = sr.ReadToEnd();
                        List<string> mailList = JsonConvert.DeserializeObject<List<string>>(json);
                        foreach (var mail in mailList)
                        {
                            if (!string.IsNullOrEmpty(mail))
                            {
                                try
                                {
                                    var schedules = outlookManager.GetScheduleList(mail, DateTime.Today, DateTime.Today.AddDays(100));
                                    foreach (var s in schedules)
                                    {
                                        if(s.Start >= DateTime.Now)
                                        {
                                            Alerm alerm = new Alerm();
                                            alerm.Name = s.Title;
                                            alerm.Message = s.Title;
                                            alerm.ExecPath = "";
                                            alerm.AlermDelete = false;
                                            alerm.TimeAddUpDown = -3;
                                            alerm.OutlookSchedule = true;
                                            alerm.EdgeColor = new byte[] { 255, 65, 105, 255 };
                                            alerm.ForeColor = new byte[] { 255, 65, 105, 255 };
                                            alerm.LabelColor = new byte[] { 255, 240, 255, 255 };
                                            alerm.ExecTypeIndex = 0;
                                            alerm.ExecType = "アラーム";
                                            alerm.FontName = "Yu Mincho";
                                            alerm.Enable = true;
                                            alerm.EnableMsg = "有効";

                                            Schedule schedule = new Schedule();
                                            schedule.ScheduleTypeIndex = 0;
                                            schedule.ScheduleType = "1回のみ";
                                            schedule.ScheduleDelete = true;
                                            schedule.Enable = true;
                                            schedule.EnableMsg = "有効";
                                            schedule.ScheduleValue = s.Start.ToString("yyyy/MM/dd");
                                            schedule.StartTime = s.Start.ToString("HH:mm");
                                            schedule.LastAlerm = DateTime.Parse("2020-01-01T00:00:00");
                                            schedule.StartAddTime = -3;
                                            alerm.ScheduleList.Add(schedule);
                                            alermList.Add(alerm);
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Warn(ex.Message);
                                    logger.Warn(ex.StackTrace);
                                }

                            }

                        }
                    }
                }
 
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw ex;
            }
        }
        /// <summary>
        /// アラームスレッドを開始する
        /// </summary>
        private void StartAlermThread()
        {
            try
            {
                if (alermList.Count==0)
                {
                    MessageBox.Show("アラームは1件も設定されていません。", "スケジュール未設定", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                AlermStartMenu.IsEnabled = false;
                AlermStopMenu.IsEnabled = true;
                threadStart = true;
                TaskbarIcon.Icon = global::NiceAlerm.Properties.Resources.clock;
                alermThread = Task.Run(() => {
                    try
                    {
                        int loadOutlookCount = 0;
                        while (threadStart)
                        {
                            while (threadPause)
                            {
                                Thread.Sleep(100);
                            }
                            
                            loadOutlookCount++;

                            bool alermChanged = false;
                            DateTime currentDateTime = DateTime.Now;
                            List<Alerm> removeList = new List<Alerm>();

                            for (int i = 0; i < alermList.Count; ++i)
                            {
                                var a = alermList[i];
                                if (!a.Enable) continue;
                                List<Schedule> removeScheduleList = new List<Schedule>();

                                for (int j = 0; j < a.ScheduleList.Count; ++j)
                                {
                                    bool alermStarted = false;
                                    var s = a.ScheduleList[j];
                                    if (!s.Enable) continue;
                                    switch (s.ScheduleTypeIndex)
                                    {
                                        case 0: //指定日
                                            if (currentDateTime.ToString("yyyy/MM/dd") != s.ScheduleValue) continue;
                                            break;
                                        case 1: //毎月
                                            if (currentDateTime.Day != int.Parse(s.ScheduleValue))
                                            {
                                                //31日は月の末日として判定する
                                                if (currentDateTime.AddDays(1).Day != 1 || s.ScheduleValue != "31")
                                                {
                                                    continue;
                                                }
                                            }
                                            break;
                                        case 2: //毎週
                                            switch (currentDateTime.DayOfWeek)
                                            {
                                                case DayOfWeek.Monday:
                                                    if (!s.ScheduleValue.Contains("月")) continue;
                                                    break;
                                                case DayOfWeek.Tuesday:
                                                    if (!s.ScheduleValue.Contains("火")) continue;
                                                    break;
                                                case DayOfWeek.Wednesday:
                                                    if (!s.ScheduleValue.Contains("水")) continue;
                                                    break;
                                                case DayOfWeek.Thursday:
                                                    if (!s.ScheduleValue.Contains("木")) continue;
                                                    break;
                                                case DayOfWeek.Friday:
                                                    if (!s.ScheduleValue.Contains("金")) continue;
                                                    break;
                                                case DayOfWeek.Saturday:
                                                    if (!s.ScheduleValue.Contains("土")) continue;
                                                    break;
                                                case DayOfWeek.Sunday:
                                                    if (!s.ScheduleValue.Contains("日")) continue;
                                                    break;
                                            }
                                            break;
                                        case 3: //毎日
                                            break;
                                    }

                                    string currentTime = currentDateTime.ToString("HH:mm");
                                    string startTime = DateTime.Parse("2000/01/01 " + s.StartTime + ":00").AddMinutes(s.StartAddTime).ToString("HH:mm");
                                    if (currentTime == startTime)
                                    {
                                        if (s.LastAlerm.ToString("yyyy/MM/dd HH:mm:00") != currentDateTime.ToString("yyyy/MM/dd HH:mm:00"))
                                        {
                                            s.LastAlerm = currentDateTime;
                                            alermStarted = true;
                                            this.Dispatcher.Invoke((Action)(() =>
                                            {
                                                switch (a.ExecTypeIndex)
                                                {
                                                    case 0:
                                                        logger.Info("アラーム実行 ⇒ " + a.Name);
                                                        AlermWindow form = new AlermWindow();
                                                        form.MainGrid.Background = new SolidColorBrush(Color.FromArgb(a.EdgeColor[0], a.EdgeColor[1], a.EdgeColor[2], a.EdgeColor[3]));
                                                        form.MainBorder.Background = new SolidColorBrush(Color.FromArgb(a.LabelColor[0], a.LabelColor[1], a.LabelColor[2], a.LabelColor[3]));
                                                        form.MessageText.Foreground = new SolidColorBrush(Color.FromArgb(a.ForeColor[0], a.ForeColor[1], a.ForeColor[2], a.ForeColor[3]));
                                                        form.MessageText.Text = a.Message;
                                                        form.MessageText.FontFamily = new FontFamily(a.FontName);
                                                        form.Title = s.StartTime + " ⇒ " + a.Name;
                                                        form.SetAlerm(a, snoozeList);
                                                        form.Show();
                                                        break;
                                                    case 1:
                                                        try
                                                        {
                                                            logger.Info("外部アプリ実行 ⇒ " + a.Name);
                                                            logger.Info("実行パス ⇒ " + a.ExecPath);
                                                            Process.Start(a.ExecPath);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            logger.Error(ex.Message);
                                                            logger.Error(ex.StackTrace);
                                                        }
                                                        break;
                                                }

                                            }));
                                        }
                                    }
                                    //既に時刻を過ぎていたら処理したことにする
                                    if (int.Parse(currentTime.Replace(":", "")) > int.Parse(s.StartTime.Replace(":", "")))
                                    {
                                        alermStarted = true;
                                    }
                                    if (alermStarted)
                                    {
                                        if (s.ScheduleTypeIndex == 0 && s.ScheduleDelete)
                                        {
                                            removeScheduleList.Add(s);
                                        }
                                    }
                                }

                                foreach (var remove in removeScheduleList)
                                {
                                    alermChanged = true;
                                    a.ScheduleList.Remove(remove);
                                }
                                //スケジュールが0の場合は削除する
                                if (a.ScheduleList.Count == 0 && a.AlermDelete)
                                {
                                    removeList.Add(a);
                                }
                            }
                            foreach (Alerm r in removeList)
                            {
                                logger.Info("スケジュール削除 ⇒ " + r.Name);
                                alermList.Remove(r);
                                alermChanged = true;
                            }
                            //スヌーズが有った場合スケジュールい追加する
                            if(snoozeList.Count > 0)
                            {
                                for(int i = 0; i < snoozeList.Count; ++i)
                                {
                                    alermList.Add(snoozeList[i]);
                                }
                                snoozeList.Clear();
                            }
                            if (alermChanged)
                            {
                                SaveAlerm();
                            }

                            if (alermList.Count == 0)
                            {
                                threadStart = false;
                                this.Dispatcher.Invoke((Action)(() =>
                                {
                                    AlermStartMenu.IsEnabled = true;
                                    AlermStopMenu.IsEnabled = false;
                                    TaskbarIcon.Icon = global::NiceAlerm.Properties.Resources.clock_stop;
                                }));
                                break;
                            }
                            if(loadOutlookCount >= 120)
                            {
                                LoadOutlookSchedule();
                                loadOutlookCount = 0;
                            }
                            Thread.Sleep(500);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        logger.Error(ex.StackTrace);
                        throw ex;
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// アプリケーションフォルダを作成する
        /// </summary>
        private void CreateAppDir()
        {
            try
            {
                string appDirPath = AppUtil.GetAppDirPath();
                if (!System.IO.Directory.Exists(appDirPath))
                {
                    System.IO.Directory.CreateDirectory(appDirPath);
                }
                //ログフォルダを作成する
                if (!System.IO.Directory.Exists(appDirPath + @"\Logs"))
                {
                    System.IO.Directory.CreateDirectory(appDirPath + @"\Logs");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// アラームデータを読み込む
        /// </summary>
        private void LoadAlerm()
        {
            try
            {
                string filePath = AppUtil.GetAppDirPath() + @"\" + SETTING_FILE;
                if (!System.IO.File.Exists(filePath))
                {
                    string json = JsonConvert.SerializeObject(alermList);
                    using(StreamWriter sw = new StreamWriter(filePath))
                    {
                        sw.WriteLine(json);
                    }
                }
                else
                {
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        string json = sr.ReadToEnd();
                        alermList = JsonConvert.DeserializeObject<List<Alerm>>(json);
                    }
                }
                //不要アラームの削除
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw ex;
            }
        }
        /// <summary>
        /// アラームデータを保存する
        /// </summary>
        private void SaveAlerm()
        {
            try
            {
                string filePath = AppUtil.GetAppDirPath() + @"\" + SETTING_FILE;
                string json = JsonConvert.SerializeObject(alermList);
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.WriteLine(json);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw ex;
            }
        }
        /// <summary>
        /// アプリを終了する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppEndMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveAlerm();
                StopAlermThread();
                this.Close();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw ex;
            }
        }
        /// <summary>
        /// アラームスレッドを停止する
        /// </summary>
        private void StopAlermThread()
        {
            try
            {
                threadStart = false;
                while (alermThread != null && !alermThread.IsCompleted)
                {
                    Thread.Sleep(100);
                }
                AlermStartMenu.IsEnabled = true;
                AlermStopMenu.IsEnabled = false;
                TaskbarIcon.Icon = global::NiceAlerm.Properties.Resources.clock_stop;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw ex;
            }
        }



        /// <summary>
        /// アラームを停止する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlermStopMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StopAlermThread();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw ex; 
            }
        }
        /// <summary>
        /// アラームを開始する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlermStartMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartAlermThread();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw ex;
            }
        }
        /// <summary>
        /// アラーム設定を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlermSettingMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                threadPause = true;
                this.IsEnabled = false;

                bool[] flag = new bool[] { AlermSettingMenu.IsEnabled
                ,AppEndMenu.IsEnabled
                ,AlermStartMenu.IsEnabled
                ,AlermStopMenu.IsEnabled};
                AlermSettingMenu.IsEnabled = false;
                AppEndMenu.IsEnabled = false;
                AlermStartMenu.IsEnabled = false;
                AlermStopMenu.IsEnabled = false;
                AlermSettingWindow form = new AlermSettingWindow();
                form.Init(alermList);
                form.ShowDialog();
                SaveAlerm();

                this.IsEnabled = true;
                threadPause = false;
                AlermSettingMenu.IsEnabled = flag[0];
                AppEndMenu.IsEnabled = flag[1];
                AlermStartMenu.IsEnabled = flag[2];
                AlermStopMenu.IsEnabled = flag[3];

                if (!threadStart)
                {
                    if(alermList.Count > 0)
                    {
                        MessageBoxResult result = MessageBox.Show("アラームを開始しますか？", "アラーム開始確認", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                        if (result == MessageBoxResult.No) return;
                        StartAlermThread();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw ex;
            }
        }

    }
}

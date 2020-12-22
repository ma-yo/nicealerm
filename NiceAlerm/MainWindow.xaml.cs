using log4net;
using Newtonsoft.Json;
using NiceAlerm.Models;
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
                        while (threadStart)
                        {
                            while (threadPause)
                            {
                                Thread.Sleep(100);
                            }
                            bool alermChanged = false;
                            DateTime currentDateTime = DateTime.Now;
                            List<Alerm> removeList = new List<Alerm>();
                            foreach (var a in alermList.Where(a => a.Enable))
                            {
                                List<Schedule> removeScheduleList = new List<Schedule>();

                                bool alermStarted = false;
                                foreach (var s in a.ScheduleList.Where(b => b.Enable))
                                {
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
                                    if (currentTime == s.StartTime)
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
                                                        form.Title = s.StartTime + " ⇒ " + a.Name;
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

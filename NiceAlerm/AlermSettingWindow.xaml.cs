using Newtonsoft.Json;
using NiceAlerm.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// AlermSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AlermSettingWindow : Window
    {
        /// <summary>
        /// アラームデータを保持する
        /// </summary>
        private List<Alerm> alermList;

        /// <summary>
        /// スケジュール設定ファイル
        /// </summary>
        private const string SCHEDULE_SETTING_FILE = "schedule-setting.json";
        /// <summary>
        /// Constructor
        /// </summary>
        public AlermSettingWindow()
        {
            try
            {
                InitializeComponent();
                Title = "NiceAlerm - アラーム設定 ver " + AppUtil.GetVersion();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 初期化を実行する
        /// </summary>
        /// <param name="alerms"></param>
        public void Init(List<Alerm> alerms)
        {
            try
            {
                alermList = alerms;

                SetAlermGrid();

                SetButtonEnabled();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// アラームグリッドを作成する
        /// </summary>
        private void SetAlermGrid()
        {
            try
            {
                AlermGrid.Items.Clear();
                foreach (var alerm in alermList)
                {
                    AlermGrid.Items.Add(alerm);
                }
                if (AlermGrid.Items.Count > 0)
                {
                    AlermGrid.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// ボタンのEnabledを設定する
        /// </summary>
        private void SetButtonEnabled()
        {
            try
            {
                AddButton.IsEnabled = true;
                EditButton.IsEnabled = AlermGrid.Items.Count > 0;
                RemoveButton.IsEnabled = AlermGrid.Items.Count > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 新規作成ボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ScheduleSettingWindow form = new ScheduleSettingWindow();
                Alerm alerm = LoadFormSetting();
                form.Init(alerm);
                form.ShowDialog();
                if (form.Committed)
                {
                    AlermGrid.Items.Add(form.EditData);
                    alermList.Add(form.EditData);
                    SaveFormSetting(form.EditData);
                }
                AlermGrid.SelectedIndex = AlermGrid.Items.Count - 1;
                SetButtonEnabled();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 画面状態を保存する
        /// </summary>
        private void SaveFormSetting(Alerm alerm)
        {
            try
            {
                string appDir = AppUtil.GetAppDirPath() + @"\" + SCHEDULE_SETTING_FILE;

                ScheduleSetting setting = new ScheduleSetting();
                setting.EdgeColor = new byte[] { alerm.EdgeColor[0], alerm.EdgeColor[1], alerm.EdgeColor[2], alerm.EdgeColor[3] };
                setting.LabelColor = new byte[] { alerm.LabelColor[0], alerm.LabelColor[1], alerm.LabelColor[2], alerm.LabelColor[3] };
                setting.ForeColor = new byte[] { alerm.ForeColor[0], alerm.ForeColor[1], alerm.ForeColor[2], alerm.ForeColor[3] };
                setting.AlermDelete = alerm.AlermDelete;
                setting.FontName = alerm.FontName;
                setting.TimeAddUpDown = alerm.TimeAddUpDown;
                string jsonString = JsonConvert.SerializeObject(setting);
                using (StreamWriter sw = new StreamWriter(appDir, false))
                {
                    sw.WriteLine(jsonString);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        /// <summary>
        /// 画面状態を読み込む
        /// </summary>
        private Alerm LoadFormSetting()
        {
            try
            {
                string appDir = AppUtil.GetAppDirPath() + @"\" + SCHEDULE_SETTING_FILE;
                ScheduleSetting setting = new ScheduleSetting();
                if (!System.IO.File.Exists(appDir))
                {
                    string jsonString = JsonConvert.SerializeObject(setting);
                    using (StreamWriter sw = new StreamWriter(appDir, false))
                    {
                        sw.WriteLine(jsonString);
                    }
                }
                else
                {
                    using (StreamReader sr = new StreamReader(appDir))
                    {
                        string jsonString = sr.ReadToEnd();
                        setting = JsonConvert.DeserializeObject<ScheduleSetting>(jsonString);
                    }
                }

                Alerm alerm = new Alerm();
                alerm.EdgeColor = setting.EdgeColor;
                alerm.LabelColor = setting.LabelColor;
                alerm.ForeColor = setting.ForeColor;
                alerm.AlermDelete = setting.AlermDelete;
                alerm.FontName = setting.FontName;
                alerm.TimeAddUpDown = setting.TimeAddUpDown;
                return alerm;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 修正ボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExecuteEdit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 修正を実行する
        /// </summary>
        private void ExecuteEdit()
        {
            try
            {
                if (AlermGrid.SelectedIndex < 0) return;
                Alerm edit = (Alerm)AlermGrid.SelectedItem;
                ScheduleSettingWindow form = new ScheduleSettingWindow();
                form.Init(edit);
                form.ShowDialog();
                if (form.Committed)
                {
                    edit.Clone(form.EditData);
                    SaveFormSetting(form.EditData);
                    SetAlermGrid();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 削除ボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AlermGrid.SelectedIndex < 0) return;
                MessageBoxResult result = MessageBox.Show("アラームを削除しますか？", "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.No) return;

                Alerm remove = (Alerm)AlermGrid.Items[AlermGrid.SelectedIndex];
                AlermGrid.Items.Remove(remove);
                alermList.Remove(remove);
                SetButtonEnabled();
                if(AlermGrid.Items.Count > 0)
                {
                    AlermGrid.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// マウスダブルクリックで修正を起動する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlermGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ExecuteEdit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

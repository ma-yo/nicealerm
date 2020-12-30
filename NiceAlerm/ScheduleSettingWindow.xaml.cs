using Microsoft.Win32;
using Newtonsoft.Json;
using NiceAlerm.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// ScheduleSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ScheduleSettingWindow : Window
    {
        /// <summary>
        /// 編集用データ
        /// </summary>
        public Alerm EditData;
        /// <summary>
        /// 確定フラグ
        /// </summary>
        public bool Committed { get; set; }
        /// <summary>
        /// 変更フラグ
        /// </summary>
        private bool IsChanged { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        public ScheduleSettingWindow()
        {
            try
            {
                InitializeComponent();
                Title = "NiceAlerm - スケジュール設定 ver " + AppUtil.GetVersion();

                foreach(var font in Fonts.SystemFontFamilies)
                {
                    FontModel model = new FontModel(font.Source);
                    FontComboBox.Items.Add(model);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 初期化処理を行う
        /// </summary>
        /// <param name="alm"></param>
        public void Init(Alerm alm)
        {
            try
            {
                EditData = alm.DeepCopy();
                AlermEnableCheck.IsChecked = EditData.Enable;
                TorokuNameTextBox.Text = EditData.Name;
                AlermDeleteCheck.IsChecked = EditData.AlermDelete;
                switch (EditData.ExecTypeIndex)
                {
                    case 0:
                        AlermRadio.IsChecked = true;
                        AlermMessageTextBox.Text = EditData.Message;
                        EdgeColorColorPicker.SelectedColor = Color.FromArgb(EditData.EdgeColor[0], EditData.EdgeColor[1], EditData.EdgeColor[2], EditData.EdgeColor[3]);
                        LabelColorColorPicker.SelectedColor = Color.FromArgb(EditData.LabelColor[0], EditData.LabelColor[1], EditData.LabelColor[2], EditData.LabelColor[3]);
                        ForeColorColorPicker.SelectedColor = Color.FromArgb(EditData.ForeColor[0], EditData.ForeColor[1], EditData.ForeColor[2], EditData.ForeColor[3]);
                        SetSelectedFont(EditData.FontName);


                        SetFontSampleBorderColor();
                        SetFontSampleBackColor();
                        SetFontSampleForeColor();

                        break;
                    case 1:
                        ExecRadio.IsChecked = true;
                        ExecPathTextBox.Text = EditData.Message;
                        break;
                }

                SetScheduleGrid();
                SetButtonEnabled();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// サンプルラベルの文字色設定
        /// </summary>
        private void SetFontSampleForeColor()
        {
            if (!ForeColorColorPicker.SelectedColor.HasValue) return;
            if (FontSampleLabel == null) return;
            FontSampleLabel.Foreground = new SolidColorBrush(ForeColorColorPicker.SelectedColor.Value);
        }
        /// <summary>
        /// サンプルラベルの背景色設定
        /// </summary>
        private void SetFontSampleBackColor()
        {
            if (!LabelColorColorPicker.SelectedColor.HasValue) return;
            if (FontSampleBorder == null) return;
            FontSampleBorder.Background = new SolidColorBrush(LabelColorColorPicker.SelectedColor.Value);
        }
        /// <summary>
        /// サンプルラベルのボーダー色設定
        /// </summary>
        private void SetFontSampleBorderColor()
        {
            if (!EdgeColorColorPicker.SelectedColor.HasValue) return;
            if (FontSampleBorder == null) return;
            FontSampleBorder.BorderBrush = new SolidColorBrush(EdgeColorColorPicker.SelectedColor.Value);
        }

        //フォントを選択する
        private void SetSelectedFont(string fontName)
        {
            try
            {
                foreach(FontModel item in FontComboBox.Items)
                {
                    if(item.Name == fontName)
                    {
                        FontComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// スケジュールグリッドを作成する
        /// </summary>
        private void SetScheduleGrid()
        {
            try
            {
                InitScheduleControl();
                ScheduleGrid.Items.Clear();
                foreach (Schedule s in EditData.ScheduleList)
                {
                    ScheduleGrid.Items.Add(s);
                }
                if (ScheduleGrid.Items.Count > 0)
                {
                    ScheduleGrid.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// スケジュールコントロールを初期化する
        /// </summary>
        private void InitScheduleControl()
        {
            try
            {
                OnetimeRadio.IsChecked = true;
                OnetimeDateTextBox.Text = DateTime.Now.ToString("yyyy/MM/dd");
                MonthlyUpDown.Value = 1;
                ScheduleEnableCheck.IsChecked = true;
                ScheduleDeleteCheck.IsChecked = true;
                Week1CheckBox.IsChecked = false;
                Week1CheckBox.IsChecked = true;
                Week1CheckBox.IsChecked = true;
                Week1CheckBox.IsChecked = true;
                Week1CheckBox.IsChecked = true;
                Week1CheckBox.IsChecked = true;
                Week1CheckBox.IsChecked = false;
                TimeTextBox.Text = "08:00";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// ボタンEnabledを設定する
        /// </summary>
        private void SetButtonEnabled()
        {
            try
            {
                EditButton.IsEnabled = ScheduleGrid.Items.Count > 0;
                RemoveButton.IsEnabled = ScheduleGrid.Items.Count > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 追加ボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Schedule schedule = GetScheduleFromControl();
                if (schedule == null) return;

                IsChanged = true;
                ScheduleGrid.Items.Add(schedule);
                EditData.ScheduleList.Add(schedule);
                SetButtonEnabled();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// スケジュールをｺﾝﾄﾛｰﾙから取得する
        /// </summary>
        /// <returns></returns>
        private Schedule GetScheduleFromControl()
        {
            try
            {
                Schedule schedule = new Schedule();

                string startTime = TimeTextBox.Text;

                if (!DateTime.TryParse("2020/01/01 " + startTime + ":00", out DateTime result))
                {
                    MessageBox.Show("正しい時刻を入力してください。", "時刻入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
                schedule.StartTime = startTime;
                schedule.Enable = (bool)ScheduleEnableCheck.IsChecked;
                schedule.ScheduleDelete = (bool)ScheduleDeleteCheck.IsChecked;
                if ((bool)OnetimeRadio.IsChecked)
                {
                    string value = OnetimeDateTextBox.Text;
                    if (!DateTime.TryParse(value, out DateTime result2))
                    {
                        MessageBox.Show("正しい日付を入力してください。", "日付入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                    schedule.ScheduleType = OnetimeRadio.Content.ToString();
                    schedule.ScheduleTypeIndex = 0;
                    schedule.ScheduleValue = value;
                }
                if ((bool)MonthlyRadio.IsChecked)
                {
                    schedule.ScheduleType = MonthlyRadio.Content.ToString();
                    schedule.ScheduleTypeIndex = 1;
                    schedule.ScheduleValue = "" + MonthlyUpDown.Value;
                }
                if ((bool)WeeklyRadio.IsChecked)
                {
                    if (!(bool)Week1CheckBox.IsChecked
                        && !(bool)Week2CheckBox.IsChecked
                        && !(bool)Week3CheckBox.IsChecked
                        && !(bool)Week4CheckBox.IsChecked
                        && !(bool)Week5CheckBox.IsChecked
                        && !(bool)Week6CheckBox.IsChecked
                        && !(bool)Week7CheckBox.IsChecked)
                    {
                        MessageBox.Show("少なくとも1つの曜日を選択してください。", "曜日選択エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                    schedule.ScheduleType = WeeklyRadio.Content.ToString();
                    schedule.ScheduleTypeIndex = 2;
                    if ((bool)Week1CheckBox.IsChecked) schedule.ScheduleValue += "|日";
                    if ((bool)Week2CheckBox.IsChecked) schedule.ScheduleValue += "|月";
                    if ((bool)Week3CheckBox.IsChecked) schedule.ScheduleValue += "|火";
                    if ((bool)Week4CheckBox.IsChecked) schedule.ScheduleValue += "|水";
                    if ((bool)Week5CheckBox.IsChecked) schedule.ScheduleValue += "|木";
                    if ((bool)Week6CheckBox.IsChecked) schedule.ScheduleValue += "|金";
                    if ((bool)Week7CheckBox.IsChecked) schedule.ScheduleValue += "|土";
                    schedule.ScheduleValue = schedule.ScheduleValue.Substring(1);
                }
                if ((bool)DailyRadio.IsChecked)
                {
                    schedule.ScheduleType = DailyRadio.Content.ToString();
                    schedule.ScheduleTypeIndex = 3;
                }

                return schedule;
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
                Schedule schedule = GetScheduleFromControl();
                if (schedule == null) return;

                IsChanged = true;
                Schedule original = (Schedule)ScheduleGrid.SelectedItem;
                original.Clone(schedule);
                SetScheduleGrid();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// スケジュールを削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleGrid.SelectedIndex < 0) return;
            MessageBoxResult result = MessageBox.Show("スケジュールを削除してもよいですか？", "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.No) return;

            IsChanged = true;
            Schedule remove = (Schedule)ScheduleGrid.Items[ScheduleGrid.SelectedIndex];
            ScheduleGrid.Items.Remove(remove);
            EditData.ScheduleList.Remove(remove);
            SetButtonEnabled();
        }
        /// <summary>
        /// 確定ボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(EditData.ScheduleList.Count == 0)
                {
                    MessageBox.Show("少なくとも1件のスケジュールの登録が必要です。", "スケジュール登録エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string name = TorokuNameTextBox.Text.Trim();
                string message = AlermMessageTextBox.Text.Trim();
                string execPath = ExecPathTextBox.Text.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("登録名は入力必須項目です。", "登録名入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if ((bool)AlermRadio.IsChecked)
                {
                    if (string.IsNullOrEmpty(message))
                    {
                        MessageBox.Show("メッセージは入力必須項目です。", "メッセージ入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    EditData.ExecType = "アラーム";
                    EditData.ExecTypeIndex = 0;
                    EditData.Message = message;
                    EditData.EdgeColor = new byte[]{ EdgeColorColorPicker.SelectedColor.Value.A
                    ,EdgeColorColorPicker.SelectedColor.Value.R
                    ,EdgeColorColorPicker.SelectedColor.Value.G
                    ,EdgeColorColorPicker.SelectedColor.Value.B };
                    EditData.LabelColor = new byte[]{ LabelColorColorPicker.SelectedColor.Value.A
                        ,LabelColorColorPicker.SelectedColor.Value.R
                        ,LabelColorColorPicker.SelectedColor.Value.G
                        ,LabelColorColorPicker.SelectedColor.Value.B};
                    EditData.ForeColor = new byte[] { ForeColorColorPicker.SelectedColor.Value.A
                ,ForeColorColorPicker.SelectedColor.Value.R
                ,ForeColorColorPicker.SelectedColor.Value.G
                ,ForeColorColorPicker.SelectedColor.Value.B};
                    EditData.FontName = ((FontModel)FontComboBox.SelectedItem).Name;
                }

                if ((bool)ExecRadio.IsChecked)
                {
                    if (!System.IO.File.Exists(execPath))
                    {
                        MessageBox.Show("正しい実行パスを入力してください。", "実行パス入力エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    EditData.ExecType = "外部アプリ実行";
                    EditData.ExecTypeIndex = 1;
                    EditData.Message = execPath;
                }

                EditData.Enable = (bool)AlermEnableCheck.IsChecked;
                EditData.Name = name;
                EditData.AlermDelete = (bool)AlermDeleteCheck.IsChecked;
                Committed = true;
                IsChanged = false;
                this.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
 
        /// <summary>
        /// 選択変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void ScheduleGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ScheduleGrid.SelectedIndex < 0) return;

                Schedule schedule = (Schedule)ScheduleGrid.SelectedItem;
                InitScheduleControl();
                TimeTextBox.Text = schedule.StartTime;
                ScheduleEnableCheck.IsChecked = schedule.Enable;
                ScheduleDeleteCheck.IsChecked = schedule.ScheduleDelete;
                switch (schedule.ScheduleTypeIndex)
                {
                    case 0:
                        OnetimeRadio.IsChecked = true;
                        OnetimeDateTextBox.Text = schedule.ScheduleValue;
                        break;
                    case 1:
                        MonthlyRadio.IsChecked = true;
                        MonthlyUpDown.Value = int.Parse(schedule.ScheduleValue);
                        break;
                    case 2:
                        WeeklyRadio.IsChecked = true;
                        Week1CheckBox.IsChecked = (schedule.ScheduleValue.Contains("日"));
                        Week2CheckBox.IsChecked = (schedule.ScheduleValue.Contains("月"));
                        Week3CheckBox.IsChecked = (schedule.ScheduleValue.Contains("火"));
                        Week4CheckBox.IsChecked = (schedule.ScheduleValue.Contains("水"));
                        Week5CheckBox.IsChecked = (schedule.ScheduleValue.Contains("木"));
                        Week6CheckBox.IsChecked = (schedule.ScheduleValue.Contains("金"));
                        Week7CheckBox.IsChecked = (schedule.ScheduleValue.Contains("土"));
                        break;
                    case 3:
                        DailyRadio.IsChecked = true;
                        break;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 実行ファイルを選択します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.FilterIndex = 1;
                openFileDialog.Filter = "All Files (*.*)|*.*";
                bool? result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    ExecPathTextBox.Text = openFileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// アラームラジオのチェックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlermRadio_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ExecPathTextBox.IsEnabled = false;
                ExecChoiceButton.IsEnabled = false;

                AlermMessageTitleLabel.Visibility = Visibility.Visible;
                AlermMessageTextBox.Visibility = Visibility.Visible;
                AlermFormBackColor.Visibility = Visibility.Visible;
                EdgeColorColorPicker.Visibility = Visibility.Visible;
                AlermFormLabelColor.Visibility = Visibility.Visible;
                LabelColorColorPicker.Visibility = Visibility.Visible;
                AlermFormForeColor.Visibility = Visibility.Visible;
                ForeColorColorPicker.Visibility = Visibility.Visible;
                FontLabel.Visibility = Visibility.Visible;
                FontComboBox.Visibility = Visibility.Visible;
                FontSampleBorder.Visibility = Visibility.Visible;

                ExecLabel.Visibility = Visibility.Hidden;
                ExecPathTextBox.Visibility = Visibility.Hidden;
                ExecChoiceButton.Visibility = Visibility.Hidden;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 処理実行ラジオのチェックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecRadio_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ExecPathTextBox.IsEnabled = true;
                ExecChoiceButton.IsEnabled = true;

                AlermMessageTitleLabel.Visibility = Visibility.Hidden;
                AlermMessageTextBox.Visibility = Visibility.Hidden;
                AlermFormBackColor.Visibility = Visibility.Hidden;
                EdgeColorColorPicker.Visibility = Visibility.Hidden;
                AlermFormLabelColor.Visibility = Visibility.Hidden;
                LabelColorColorPicker.Visibility = Visibility.Hidden;
                AlermFormForeColor.Visibility = Visibility.Hidden;
                ForeColorColorPicker.Visibility = Visibility.Hidden;
                FontLabel.Visibility = Visibility.Hidden;
                FontComboBox.Visibility = Visibility.Hidden;
                FontSampleBorder.Visibility = Visibility.Hidden;

                ExecLabel.Visibility = Visibility.Visible;
                ExecPathTextBox.Visibility = Visibility.Visible;
                ExecChoiceButton.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// アラームの有効化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlermEnableCheck_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                AlermEnableCheck.Content = "有効";
                AlermEnableCheck.Foreground = new SolidColorBrush(Colors.Blue);
            }
            catch (Exception ex)
            {
                throw ex; 
            }
        }
        /// <summary>
        /// アラームの無効化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlermEnableCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                AlermEnableCheck.Content = "無効";
                AlermEnableCheck.Foreground = new SolidColorBrush(Colors.Red);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 終了時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (IsChanged)
                {
                    MessageBoxResult result = MessageBox.Show("データは編集中ですが、登録せずに終了しますか？", "終了確認", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
                    if(result == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 削除チェックのチェックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlermDeleteCheck_Checked(object sender, RoutedEventArgs e)
        {
            AlermDeleteCheck.Content = "全スケジュール終了時にアラームを削除する";
            AlermDeleteCheck.Foreground = new SolidColorBrush(Colors.Blue);
        }
        /// <summary>
        /// 削除チェックのアンチェックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlermDeleteCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            AlermDeleteCheck.Content = "全スケジュール終了時にアラームを削除しない";
            AlermDeleteCheck.Foreground = new SolidColorBrush(Colors.Black);
        }
        /// <summary>
        /// スケジュールの有効化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleEnableCheck_Checked(object sender, RoutedEventArgs e)
        {
            ScheduleEnableCheck.Content = "有効";
            ScheduleEnableCheck.Foreground = new SolidColorBrush(Colors.Blue);
        }
        /// <summary>
        /// スケジュールの無効化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleEnableCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            ScheduleEnableCheck.Content = "無効";
            ScheduleEnableCheck.Foreground = new SolidColorBrush(Colors.Black);
        }
        /// <summary>
        /// スケジュール終了時削除の有効化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleDeleteCheck_Checked(object sender, RoutedEventArgs e)
        {
            ScheduleDeleteCheck.Content = "スケジュール終了時に削除する";
            ScheduleDeleteCheck.Foreground = new SolidColorBrush(Colors.Blue);
        }
        /// <summary>
        /// スケジュール終了時削除の無効化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleDeleteCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            ScheduleDeleteCheck.Content = "スケジュール終了時に削除しない";
            ScheduleDeleteCheck.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                FontSampleLabel.FontFamily = ((FontModel)FontComboBox.SelectedItem).FontFamily;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// エッヂ色を設定する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EdgeColorColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SetFontSampleBorderColor();
        }
        /// <summary>
        /// ラベル色を設定する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LabelColorColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SetFontSampleBackColor();
        }
        /// <summary>
        /// フォント色を設定する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ForeColorColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SetFontSampleForeColor();
        }
    }
}

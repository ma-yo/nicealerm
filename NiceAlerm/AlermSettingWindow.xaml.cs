using NiceAlerm.Models;
using System;
using System.Collections.Generic;
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
                form.Init(new Alerm());
                form.ShowDialog();
                if (form.Committed)
                {
                    AlermGrid.Items.Add(form.EditData);
                    alermList.Add(form.EditData);
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

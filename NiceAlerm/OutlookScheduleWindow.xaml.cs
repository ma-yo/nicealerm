using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using NiceAlerm.Models;
using NiceAlerm.Utils;
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
    /// OutlookScheduleWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OutlookScheduleWindow : Window
    {
        /// <summary>
        /// ロガー
        /// </summary>
        ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public OutlookScheduleWindow()
        {
            InitializeComponent();
            if (System.IO.File.Exists(PathUtil.GetOutlookMailPath()))
            {
                using(StreamReader sr = new StreamReader(PathUtil.GetOutlookMailPath()))
                {
                    string json = sr.ReadToEnd();
                    List<string> mailList = JsonConvert.DeserializeObject<List<string>>(json);
                    MailAddressTextBox1.Text = mailList[0];
                    MailAddressTextBox2.Text = mailList[1];
                    MailAddressTextBox3.Text = mailList[2];
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                List<string> mailList = new List<string>
                {
                    MailAddressTextBox1.Text.Trim(),
                    MailAddressTextBox2.Text.Trim(),
                    MailAddressTextBox3.Text.Trim()
                };

                string json = JsonConvert.SerializeObject(mailList);
                using (StreamWriter sw = new StreamWriter(PathUtil.GetOutlookMailPath()))
                {
                    sw.WriteLine(json);
                }
                MessageBox.Show("保存しました。");

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

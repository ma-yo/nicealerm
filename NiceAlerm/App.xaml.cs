using NiceAlerm.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NiceAlerm
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private System.Threading.Mutex mutex = new System.Threading.Mutex(false, "NiceAlerm");

        /// <summary>
        /// アプリケーションの開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // ミューテックスの所有権を要求
            if (!mutex.WaitOne(0, false))
            {
                // 既に起動しているため終了させる
                MessageBox.Show("NiceAlerm は既に起動しています。", "二重起動チェック", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                mutex.Close();
                mutex = null;
                this.Shutdown();
            }
            Singleton.GetInstance().AppEnabled = true;
        }
        /// <summary>
        /// アプリケーションの終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Close();
            }
        }
    }
}

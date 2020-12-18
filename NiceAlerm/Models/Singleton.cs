using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceAlerm.Models
{
    public class Singleton
    {
        /// <summary>
        /// シングルトンデータ
        /// </summary>
        private static Singleton instance = new Singleton();
        /// <summary>
        /// アプリケーション起動OKフラグ
        /// </summary>
        public bool AppEnabled { get; set; } = false;
        /// <summary>
        /// シングルトンを取得する
        /// </summary>
        /// <returns></returns>
        public static Singleton GetInstance()
        {
            return instance;
        }
    }
}

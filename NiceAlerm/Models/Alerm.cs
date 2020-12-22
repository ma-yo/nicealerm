using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NiceAlerm.Models
{
    /// <summary>
    /// アラームクラス
    /// </summary>
    [Serializable]
    public class Alerm
    {
        /// <summary>
        /// アラーム名
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 通知メッセージ
        /// </summary>
        public string Message { get; set; } = "";
        /// <summary>
        /// アプリケーション実行パス
        /// </summary>
        public string ExecPath { get; set; } = "";
        /// <summary>
        /// 全スケジュール終了後にアラームを削除する
        /// </summary>
        public bool AlermDelete { get; set; } = true;
        /// <summary>
        /// 枠色
        /// </summary>
        public byte[] EdgeColor { get; set; } = new byte[] { Colors.Yellow.A, Colors.Yellow.R, Colors.Yellow.G, Colors.Yellow.B };
        /// <summary>
        /// ラベル背景色
        /// </summary>
        public byte[] LabelColor { get; set; } = new byte[] { Colors.Black.A, Colors.Black.R, Colors.Black.G, Colors.Black.B } ;
        /// <summary>
        /// 文字色
        /// </summary>
        public byte[] ForeColor { get; set; } = new byte[] { Colors.Tomato.A, Colors.Tomato.R, Colors.Tomato.G, Colors.Tomato.B };

        /// <summary>
        /// 処理タイプ番号
        /// </summary>
        public int ExecTypeIndex { get; set; } = 0;
        /// <summary>
        /// 処理タイプ
        /// </summary>
        public string ExecType { get; set; } = "アラーム";
        /// <summary>
        /// スケジュール一覧
        /// </summary>
        public List<Schedule> ScheduleList { get; set; } = new List<Schedule>();

        private bool _enable = true;
        /// <summary>
        /// 有効無効
        /// </summary>
        public bool Enable { 
            get { return _enable; } 
            set { _enable = value; if (value) { EnableMsg = "有効"; } else { EnableMsg = "無効"; } }
        }

        public string EnableMsg { get; set; }
        /// <summary>
        /// オブジェクトをコピーする
        /// </summary>
        /// <param name="editData"></param>
        internal void Clone(Alerm editData)
        {
            try
            {
                Name = editData.Name;
                Message = editData.Message;
                LabelColor = editData.LabelColor;
                EdgeColor = editData.EdgeColor;
                ForeColor = editData.ForeColor;
                Enable = editData.Enable;
                ExecPath = editData.ExecPath;
                ExecType = editData.ExecType;
                ExecTypeIndex = editData.ExecTypeIndex;

                ScheduleList.Clear();
                foreach (var s in editData.ScheduleList)
                {
                    ScheduleList.Add(s);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}

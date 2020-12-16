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
        /// 最終通知時刻
        /// </summary>
        public DateTime LastAlerm { get; set; } = DateTime.Now;
        /// <summary>
        /// 枠色
        /// </summary>
        public byte[] EdgeColor { get; set; } = new byte[] { Colors.Black.A, Colors.Black.R, Colors.Black.G, Colors.Black.B };
        /// <summary>
        /// ラベル背景色
        /// </summary>
        public byte[] LabelColor { get; set; } = new byte[] { Colors.Black.A, Colors.Black.R, Colors.Black.G, Colors.Black.B } ;
        /// <summary>
        /// 文字色
        /// </summary>
        public byte[] ForeColor { get; set; } = new byte[] { Colors.Tomato.A, Colors.Tomato.R, Colors.Tomato.G, Colors.Tomato.B };
        /// <summary>
        /// スケジュール一覧
        /// </summary>
        public List<Schedule> ScheduleList { get; set; } = new List<Schedule>();
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
                LastAlerm = editData.LastAlerm;
                LabelColor = editData.LabelColor;
                EdgeColor = editData.EdgeColor;
                ForeColor = editData.ForeColor;
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

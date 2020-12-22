using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceAlerm.Models
{
    /// <summary>
    /// スケジュールクラス
    /// </summary>
    [Serializable]
    public class Schedule
    {
        /// <summary>
        /// スケジュールタイプ番号
        /// </summary>
        public int ScheduleTypeIndex { get; set; } = 0;
        /// <summary>
        /// スケジュールタイプ
        /// </summary>
        public string ScheduleType { get; set; } = "1回のみ";

        /// <summary>
        /// スケジュール終了後に削除する
        /// </summary>
        public bool ScheduleDelete { get; set; } = true;

        private bool _enable = true;
        /// <summary>
        /// 有効無効
        /// </summary>
        public bool Enable
        {
            get { return _enable; }
            set { _enable = value; if (value) { EnableMsg = "有効"; } else { EnableMsg = "無効"; } }
        }

        public string EnableMsg { get; set; }
        /// <summary>
        /// スケジュール値
        /// </summary>
        public string ScheduleValue { get; set; }
        /// <summary>
        /// 開始時刻
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 最終通知時刻
        /// </summary>
        public DateTime LastAlerm { get; set; } = DateTime.Parse("2020/01/01 00:00:00");

        /// <summary>
        /// オブジェクトのコピーを行う
        /// </summary>
        /// <param name="schedule"></param>
        internal void Clone(Schedule schedule)
        {
            ScheduleTypeIndex = schedule.ScheduleTypeIndex;
            ScheduleType = schedule.ScheduleType;
            ScheduleValue = schedule.ScheduleValue;
            StartTime = schedule.StartTime;
            LastAlerm = schedule.LastAlerm;
            Enable = schedule.Enable;
            ScheduleDelete = schedule.ScheduleDelete;
        }
    }
}

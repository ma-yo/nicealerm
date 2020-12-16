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
        /// スケジュール値
        /// </summary>
        public string ScheduleValue { get; set; }
        /// <summary>
        /// 開始時刻
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// オブジェクトのコピーを行う
        /// </summary>
        /// <param name="schedule1"></param>
        internal void Clone(Schedule schedule1)
        {
            ScheduleTypeIndex = schedule1.ScheduleTypeIndex;
            ScheduleType = schedule1.ScheduleType;
            ScheduleValue = schedule1.ScheduleValue;
            StartTime = schedule1.StartTime;
        }
    }
}

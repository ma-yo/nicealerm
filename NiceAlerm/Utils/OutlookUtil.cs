using NetOffice.OutlookApi.Enums;
using NetOffice.OutlookApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceAlerm.Models;

namespace NiceAlerm.Utils
{
    public class OutlookUtil
    {
        public List<OutlookScheduleItem> GetScheduleList(string mailOrName, DateTime date)
        {
            return GetScheduleList(mailOrName, date, date.AddDays(1));
        }

        public List<OutlookScheduleItem> GetScheduleList(string mailOrName, DateTime start, DateTime end)
        {
            // 対象フォルダの取得
            var folder = GetCalenderFolder(mailOrName);

            // スケジュールの検索
            var items = GetScheduleItems(folder, start, end);

            // 定期的なスケジュールを展開して、ScheduleItemに変換
            var schedules = ExpansionAndConvert(items, start, end);

            return schedules;
        }

        private Application GetApplication()
        {
            return new Application();
        }

        private MAPIFolder GetCalenderFolder(string mailOrName)
        {
            var outlook = GetApplication();

            // var oFolder = outlook.Session.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);
            // var rObject = outlook.Session.CreateRecipient("rm20160917120031@jp.nssol.nssmc.com");
            var rObject = outlook.Session.CreateRecipient(mailOrName);
            var oFolder = outlook.Session.GetSharedDefaultFolder(rObject, OlDefaultFolders.olFolderCalendar);
            // oFolder.Items.IncludeRecurrences = true;
            // Console.WriteLine(oFolder.Name);

            return oFolder;
        }

        private _Items GetScheduleItems(MAPIFolder folder, DateTime start, DateTime end)
        {
            // Filter apply
            string startDate = start.ToString("yy/MM/dd");
            string endDate = end.ToString("yy/MM/dd");
            string filter = $"[Start] >= '{startDate}' AND [Start] <= '{endDate}'";
            // Console.WriteLine(filter);
            var list = folder.Items.Restrict(filter);

            // Set Sort
            // list.Sort("[Start]", false);

            return list;
        }

        private List<OutlookScheduleItem> ExpansionAndConvert(_Items items, DateTime start, DateTime end)
        {
            var ret = new List<OutlookScheduleItem>();

            foreach (AppointmentItem item in items)
            {
                if (item.IsRecurring)
                {
                    var pattern = item.GetRecurrencePattern();
                    DateTime first = new DateTime(start.Year, start.Month, start.Day, item.Start.Hour, item.Start.Minute, 0);
                    DateTime last = new DateTime(end.Year, end.Month, end.Day);
                    for (DateTime cur = first; cur <= last; cur = cur.AddDays(1))
                    {
                        try
                        {
                            // ここの取得が成功したらスケジュールが存在するって判定になる
                            var recur = pattern.GetOccurrence(cur);
                            var curEnd = new DateTime(cur.Year, cur.Month, cur.Day, item.End.Hour, item.End.Minute, 0);
                            ret.Add(new OutlookScheduleItem(item.Subject, cur, curEnd));
                        }
                        catch
                        { }
                    }
                }
                else
                {
                    ret.Add(new OutlookScheduleItem(item.Subject, item.Start, item.End));
                }
            }

            // 指定の日付幅だけにする、ついでにソート
            ret = ret.Where(x => start <= x.Start)
                     .Where(x => x.End <= end)
                     .OrderBy(x => x.Start)
                     .ToList();

            return ret;
        }
    }
}

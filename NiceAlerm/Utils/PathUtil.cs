using NiceAlerm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceAlerm.Utils
{
    public class PathUtil
    {
        public static string GetOutlookMailPath()
        {
            return AppUtil.GetAppDirPath() + @"\outlookmail.json";
        }
    }
}

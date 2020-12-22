using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NiceAlerm.Models
{
    [Serializable]
    public class ScheduleSetting
    {
        public byte[] ForeColor { get; set; }
        public byte[] LabelColor { get; set; }
        public byte[] EdgeColor { get; set; }
        public bool AlermDelete { get; set; }

        public ScheduleSetting()
        {
            EdgeColor = new byte[]{ Colors.Yellow.A
                    ,Colors.Yellow.R
                    ,Colors.Yellow.G
                    ,Colors.Yellow.B };
            LabelColor = new byte[]{ Colors.Black.A
                        ,Colors.Black.R
                        ,Colors.Black.G
                        ,Colors.Black.B};
            ForeColor = new byte[] { Colors.Orange.A
                ,Colors.Orange.R
                ,Colors.Orange.G
                ,Colors.Orange.B};
        }
    }
}

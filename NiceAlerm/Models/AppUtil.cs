using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NiceAlerm.Models
{
    public static class AppUtil
    {
        // DeepCopyメソッド
        public static T DeepCopy<T>(this T src)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, src);
                stream.Position = 0;

                return (T)formatter.Deserialize(stream);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NiceAlerm.Models
{
    /// <summary>
    /// フォントモデル
    /// </summary>
    [Serializable]
    public class FontModel
    {
        public FontFamily FontFamily { get; set; }

        public string Name { get; set; }
        public FontModel()
        {
            FontFamily = new FontFamily("Meiryo UI");
            Name = FontFamily.Source;
        }
        public FontModel(string fontName)
        {
            FontFamily = new FontFamily(fontName);
            Name = FontFamily.Source;
        }
        public override string ToString()
        {
            foreach(var kv in FontFamily.FamilyNames.Where(a => a.Key == System.Windows.Markup.XmlLanguage.GetLanguage("ja-jp")))
            {
                return kv.Value;
            }
            return FontFamily.Source;
        }
    }
}

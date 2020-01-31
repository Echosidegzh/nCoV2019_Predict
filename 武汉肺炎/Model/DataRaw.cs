using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 武汉肺炎
{
    public class DataRaw
    {
        public string date;
        public int confirm;
        public int suspect;
        public int dead;
        public int heal;

        public DateTime 日期
        {
            get
            {
                if (date.ToArray().Count(i => i == '.') == 1)
                {
                    return DateTime.Parse("2020." + date);
                }
                else
                {
                    return DateTime.Parse(date);
                }
            }
            set
            {
                date = value.ToString("yyyy.MM.dd");
            }
        }
        public int 确诊人数 { get { return confirm; } }
        public int 疑似人数 { get { return suspect; } }
        public int 治愈人数 { get { return heal; } }
        public int 死亡人数 { get { return dead; } }
    }
}
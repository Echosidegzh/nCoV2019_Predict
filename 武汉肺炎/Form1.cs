using MathNet.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace 武汉肺炎
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;

            var data = GetData().OrderBy(i => i.日期).ToList();
            if (data.Last().日期 == DateTime.Today) data.RemoveAt(data.Count - 1);

            var P = CalcP(data);
            Predict(P, data);

            dataGridView1.DataSource = data;

            new List<string> { "确诊人数", "疑似人数", "治愈人数", "死亡人数" }.ForEach(i => Plot(i, data));

            new Thread(() =>
            {
                while (true)
                {
                    var od = GetDataOns();
                    label1.Text = $"更新时间：{od.lastUpdateTime}，确诊：{od.chinaTotal.confirm}，疑似：{od.chinaTotal.suspect}，治愈：{od.chinaTotal.heal}，死亡：{od.chinaTotal.dead}";
                    Thread.Sleep(1000 * 60);
                }
            }).Start();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private double[][] CalcP(List<DataRaw> data)
        {
            var x = new List<double>();
            var yc = new List<double>();
            var ys = new List<double>();
            var yh = new List<double>();
            var yd = new List<double>();
            data.ForEach(i =>
            {
                var da = (i.日期 - DateTime.Parse("2020-1-12")).TotalDays;
                x.Add(da);
                yc.Add(i.确诊人数);
                ys.Add(i.疑似人数);
                yh.Add(i.治愈人数);
                yd.Add(i.死亡人数);
            });
            var Pc = Fit.Polynomial(x.ToArray(), yc.ToArray(), 3);
            var Ps = Fit.Polynomial(x.ToArray(), ys.ToArray(), 3);
            var Ph = Fit.Polynomial(x.ToArray(), yh.ToArray(), 3);
            var Pd = Fit.Polynomial(x.ToArray(), yd.ToArray(), 3);
            return new double[][]{ Pc, Ps, Ph, Pd };
        }
        private void Predict(double[][] P, List<DataRaw> data)
        {
            var ds = (int)((data.Last().日期 - DateTime.Parse("2020-1-12")).TotalDays);
            for (int i = ds; i < ds + 30 * 2; i++)
            {
                data.Add(new DataRaw()
                {
                    日期 = data[0].日期.AddDays(i),
                    confirm = (int)Polynomial.Evaluate(i + 1, P[0]),
                    suspect = (int)Polynomial.Evaluate(i + 1, P[1]),
                    heal = (int)Polynomial.Evaluate(i + 1, P[2]),
                    dead = (int)Polynomial.Evaluate(i + 1, P[3])
                });
            }
        }
        private void Plot(string name, List<DataRaw> data)
        {
            var series = new Series() { Name = name, ChartType = SeriesChartType.Point };
            switch (name)
            {
                case "确诊人数": data.ForEach(i => series.Points.AddXY(i.日期, i.确诊人数)); break;
                case "疑似人数": data.ForEach(i => series.Points.AddXY(i.日期, i.疑似人数)); break;
                case "治愈人数": data.ForEach(i => series.Points.AddXY(i.日期, i.治愈人数)); break;
                case "死亡人数": data.ForEach(i => series.Points.AddXY(i.日期, i.死亡人数)); break;
            }
            chart1.Series.Add(series);
        }

        private string Get(string url)
        {
            var r = new WebClient { Encoding = Encoding.UTF8 }.DownloadString(url);
            return JsonConvert.DeserializeObject<RootObject>(r).data;
        }
        private DataRaw[] GetData()
        {
            var r = Get("https://view.inews.qq.com/g2/getOnsInfo?name=wuwei_ww_cn_day_counts");
            return JsonConvert.DeserializeObject<DataRaw[]>(r);
        }
        private dynamic GetDataOns()
        {
            var r = Get("https://view.inews.qq.com/g2/getOnsInfo?name=disease_h5");
            return JValue.Parse(r);
        }
    }
}
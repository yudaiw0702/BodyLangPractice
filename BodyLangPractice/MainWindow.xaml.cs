using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using BodyLangPractice;

namespace BodyLangPractice
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        DateTime dt_start;

        //ログ用
        public static int next_push_count;
        public static int correct_count;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => { 
                dt_start = DateTime.Now;
                Trace.WriteLine("開始時刻：" + dt_start);
            };

            Uri uri = new Uri("/TitlePage.xaml", UriKind.Relative);
            frame.Source = uri;
        }


        public TimeSpan dt { get; private set; }

        public void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DateTime dt_close = DateTime.Now;
            Trace.WriteLine("終了時刻：" + dt_close);

            dt = dt_close - dt_start;
            Trace.WriteLine("起動時間：" + dt);

            Trace.WriteLine("[次へ]押下回数：" + next_push_count);
            Trace.WriteLine("正解回数：" + correct_count);

            Trace.WriteLine("---------------------------");
        }

        public void count (int x)
        {
            next_push_count = x;
        }

        public void countC(int x)
        {
            correct_count = x;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

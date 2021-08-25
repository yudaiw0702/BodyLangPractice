using System;
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
        public MainWindow()
        {
            InitializeComponent();

            Uri uri = new Uri("/TitlePage.xaml", UriKind.Relative);
            frame.Source = uri;
        }


        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

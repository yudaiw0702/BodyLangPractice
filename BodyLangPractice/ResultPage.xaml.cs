using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BodyLangPractice
{
    /// <summary>
    /// ResultPage.xaml の相互作用ロジック
    /// </summary>
    public partial class ResultPage : Page
    {
        public ResultPage(string str1, string str2)
        {
            InitializeComponent();

            //間違えた手話の表示
            incorrect_label.Content = str1;

            //正解数の表示
            correct.Content = str2;
        }

        private void TitleBtn_Click(object sender, RoutedEventArgs e)
        {
            var titlePage = new TitlePage();
            NavigationService.Navigate(titlePage);
        }
    }
}

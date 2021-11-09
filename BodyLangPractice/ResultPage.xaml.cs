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
        public ResultPage()
        {
            InitializeComponent();
        }

        private void TitleBtn_Click(object sender, RoutedEventArgs e)
        {
            var titlePage = new TitlePage();
            NavigationService.Navigate(titlePage);
        }

        private void titleBtn_Copy_Click(object sender, RoutedEventArgs e)
        {
            var testPage = new TestPage();

            
            correct.Content = testPage.IncorrectCount;

            Console.WriteLine(testPage.IncorrectCount);

            incorrect_label.Content = testPage.IncorrectList;
        }
    }
}

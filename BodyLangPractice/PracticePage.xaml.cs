using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BodyLangPractice
{
    /// <summary>
    /// PracticePage.xaml の相互作用ロジック
    /// </summary>
    public partial class PracticePage : Page
    {
        public PracticePage()
        {
            InitializeComponent();
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            var titlePage = new TitlePage();
            NavigationService.Navigate(titlePage);
        }
    }
}

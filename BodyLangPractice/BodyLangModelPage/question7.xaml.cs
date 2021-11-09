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

namespace BodyLangPractice.BodyLangModelPage
{
    /// <summary>
    /// question1.xaml の相互作用ロジック
    /// </summary>
    public partial class question7 : Page
    {
        public string TextBox1Str { get; private set; } = "";
        public question7()
        {
            InitializeComponent();
            TextBox1Str = "勉強";
        }
    }
}

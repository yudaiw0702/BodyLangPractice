using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System.Resources;
using System.ComponentModel.Design;
using System.Net;
using System.Threading;
using Microsoft.Win32;
using System.ComponentModel;

namespace BodyLangPractice.BodyLangModelPage
{
    /// <summary>
    /// question1.xaml の相互作用ロジック
    /// </summary>
    public partial class question1 : Page
    {
        public string TextBox1Str { get; private set; } = "";
        public question1()
        {
            InitializeComponent();
            TextBox1Str = "おはよう";
        }
    }
}

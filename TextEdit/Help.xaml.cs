using System.Windows;
using System.IO;
using System.Windows.Input;

namespace TextEdit
{
    /// <summary>
    /// Help.xaml 的交互逻辑
    /// </summary>
    public partial class Help : Window
    {
        public Help(string function)
        {
            InitializeComponent();
            Title = function;

            if (Title.StartsWith("帮助"))
            {
                if (!File.Exists("readme.txt"))
                {
                    MessageBox.Show("找不到帮助文本。", "提示");
                    Box.Text = "帮助文件缺失，请重新下载本软件。";
                    return;
                }
                else
                {
                    StreamReader reader = new StreamReader("readme.txt");
                    Box.Text = reader.ReadToEnd();
                    reader.Dispose();
                }
            }
            else if (Title.StartsWith("版本信息"))
            {
                if (!File.Exists("update.txt"))
                {
                    MessageBox.Show("找不到版本信息。", "提示");
                    Box.Text = "版本文件缺失，请重新下载本软件。";
                    return;
                }
                else
                {
                    StreamReader reader = new StreamReader("update.txt");
                    Box.Text = reader.ReadToEnd();
                    reader.Dispose();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CopyEmail(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("是否复制邮箱？", "提示", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                Clipboard.SetText("byjr_k@163.com");
        }        

        void CloseWindowByESC(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}

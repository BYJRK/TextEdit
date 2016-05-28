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
using System.Windows.Shapes;

namespace TextEdit
{
    /// <summary>
    /// InputBox.xaml 的交互逻辑
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 弹出输入框
        /// </summary>
        /// <param name="HintText">文本框上方的提示文字</param>
        /// <param name="DefaultText">文本框中的默认文字</param>
        /// <param name="Title">标题</param>
        /// <param name="ButtonText">按钮文字</param>
        /// <returns></returns>
        public static string Input(string HintText = "请输入：", string DefaultText = "",
            string Title = "提示", string ButtonText = "确认")
        {
            InputBox input = new InputBox();
            input.Title = Title;
            input.Notice.Text = HintText;
            input.Inputbox.Text = DefaultText;
            input.Confirm.Content = ButtonText;
            input.Inputbox.Focus();
            input.Inputbox.SelectAll();
            input.ShowDialog();
            return input.Inputbox.Text;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Close();
            }
            else if (e.Key == Key.Escape)
            {
                Inputbox.Text = "";
                Close();
            }
        }
    }
}

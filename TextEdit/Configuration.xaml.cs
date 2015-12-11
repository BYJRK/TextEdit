using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TextEdit
{
    /// <summary>
    /// Configuration.xaml 的交互逻辑
    /// </summary>
    public partial class Configuration : Window
    {
        private MainWindow mainWindow;
        public Configuration(MainWindow mainWindow)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            size.Text = ((int)mainWindow.Height).ToString() + "x" + ((int)mainWindow.Width).ToString();

            historyCount.Text = mainWindow.historyCount.ToString();
            if (mainWindow.clearAfterCopy) clearAfterCopy.IsChecked = true;
            if (mainWindow.clearAfterUse) clearAfterUse.IsChecked = true;
            if (mainWindow.autoSaveTemp) autoSaveTemp.IsChecked = true;
            if (mainWindow.showSpeed) showSpeed.IsChecked = true;
            if (mainWindow.alwaysShowBox2) alwaysShowBox2.IsChecked = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Name == "Cancel")
            {
                this.Close();
                mainWindow.Show();
            }
            else if(btn.Name=="Init")
            {
                clearAfterCopy.IsChecked = false;
                clearAfterUse.IsChecked = false;
                autoSaveTemp.IsChecked = true;
                showSpeed.IsChecked = false;
                historyCount.Text = "10";
            }
            else if (btn.Name == "Save")
            {
                // 检查historyCount的格式是否正确
                int count = -1;
                int temp = 0;
                try
                {
                    temp = Int32.Parse(historyCount.Text);
                }
                catch (FormatException)
                {
                    MessageBox.Show("您输入的历史记录数目有误，请检查后重试。\n其他内容已成功保存。");
                    temp = -1;
                }
                count = temp;
                // 如果historyCount格式正确，则保存
                if (count >= 0) mainWindow.historyCount = count;

                mainWindow.clearAfterCopy = clearAfterCopy.IsChecked.Value;
                mainWindow.clearAfterUse = clearAfterUse.IsChecked.Value;
                mainWindow.autoSaveTemp = autoSaveTemp.IsChecked.Value;
                mainWindow.showSpeed = showSpeed.IsChecked.Value;
                mainWindow.alwaysShowBox2 = alwaysShowBox2.IsChecked.Value;

                this.Close();
                mainWindow.Show();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mainWindow.UpdateDisplay();
        }

        private void historyCount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 不允许空格的输入
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        private void historyCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int result;
            if (!Int32.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
        }

        void CloseWindowByESC(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
            mainWindow.Show();
        }
    }
}

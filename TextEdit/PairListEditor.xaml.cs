using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// PairListEditor.xaml 的交互逻辑
    /// </summary>
    public partial class PairListEditor : Window
    {
        Pair pairList = new Pair();
        public PairListEditor(Pair pairList)
        {
            InitializeComponent();

            listbox.Items.Clear();
            for (int i = 0; i < pairList.From.Count; i++)
            {
                string temp = pairList.From[i] + " -> " + pairList.To[i];
                listbox.Items.Add(temp);
            }
            this.pairList = pairList;
            from.Text = pairList.From[0];
            to.Text = pairList.To[0];

            count.Text = "总计" + pairList.From.Count.ToString() + "项";
        }

        private void listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = listbox.SelectedIndex;
            from.Text = pairList.From[index];
            to.Text = pairList.To[index];
        }
    }
}

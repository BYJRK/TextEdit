using Microsoft.International.Converters.PinYinConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using WordEdit;

namespace TextEdit
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // 引入外部算法
        private WordEdit.Functions f = new WordEdit.Functions();

        // 在程序初始化结束后，进行窗口中元素的初始化
        private void Window_Initialized(object sender, EventArgs e)
        {
            InitializeElement();
            InitializeEveryFunction();
            // 读取配置文件
            ReadConfig();
            InitializeConfigElements();
            // 程序的初始位置以及高度
            InitSizeMenuItem_Click(this, null);
            // 更新界面内各元件的显示
            UpdateDisplay();
            // 载入历史
            if (File.Exists("temp1.txt"))
            {
                StreamReader r = new StreamReader("temp1.txt");
                Text1 = r.ReadToEnd();
                r.Dispose();
            }
            if (File.Exists("temp2.txt"))
            {
                StreamReader r = new StreamReader("temp2.txt");
                Text2 = r.ReadToEnd();
                r.Dispose();
            }
            // 读取自定义转换列表
            LoadTransformLists();
            // 初始化常见括号对列表
            BraListInitialize();
            // 初始化剪贴板辅助工具自动粘贴的按键列表
            PasteAutoKey.Items.Add("TAB");
            PasteAutoKey.Items.Add("ENTER");
            PasteAutoKey.Items.Add("DOWN");
            PasteAutoKey.Items.Add("NONE");
            PasteAutoKey.Text = "TAB";

            // 如果设置了自动检查更新，则检查最新版
            if (autoCheckUpdate)
            {
                Thread t = new Thread(CheckLatestVersion2);
                t.Start();
                t.Join();
            }

            // 不知道当时写这句是干啥用的，可能是用来注册全局热键的
            // 但是本程序的全局热键使用的是键盘钩子，所以删掉也可以
            //OnSourceInitialized(null);
        }
        // 程序关闭
        private void Window_Closed(object sender, EventArgs e)
        {
            // config文件
            SaveConfig();

            // temp文件
            if (!autoSaveTemp)
            {
                // 不仅不保存，而且删除现有的temp文件
                try
                {
                    File.Delete("temp.txt");
                    File.Delete("temp1.txt");
                    File.Delete("temp2.txt");
                }
                catch (Exception err)
                {
                    MessageBox.Show("未知错误。\n原因：" + err.Message);
                }
            }
            else
            {
                if (Text1.Length > 0)
                {
                    StreamWriter w = new StreamWriter(@"temp1.txt");
                    w.Write(Text1);
                    w.Dispose();
                }
                else
                {
                    // 如果关闭时，Text内容为空，则清除之前的历史
                    if (File.Exists("temp.txt"))
                    {
                        File.Delete("temp.txt");
                    }
                    else if (File.Exists("temp1.txt"))
                    {
                        File.Delete("temp1.txt");
                    }
                }
                if (Text2.Length > 0)
                {
                    StreamWriter w = new StreamWriter(@"temp2.txt");
                    w.Write(Text2);
                    w.Dispose();
                }
                else
                {
                    // 如果关闭时，Text内容为空，则清除之前的历史
                    if (File.Exists("temp2.txt"))
                    {
                        File.Delete("temp2.txt");
                    }
                }
            }

            try
            {
                if (isUsingPasteHelper)
                {
                    // 如果全局钩子没有正常关闭，则在此处再次关闭
                    kh.UnHook();
                }
            }
            catch (Exception)
            {
                // 暂时懒得设定一个变量，判断钩子是否在使用，所以如果出现异常，直接无视掉得了
            }
        }

        /* 初始化窗口成员
         * RadioButton默认只有第一个Checked
         * GroupBox默认只有第一个显示，并且Enabled
         */
        private void InitializeElement()
        {
            Replac.IsChecked = true;
            Bracket.IsChecked = false;
            Space.IsChecked = false;
            Return.IsChecked = false;
            ExchangePL.IsChecked = false;
            CopyTimes.IsChecked = false;
            AddByLine.IsChecked = false;
            AddIndex.IsChecked = false;
            Format.IsChecked = false;
            SymbolTrans.IsChecked = false;
            DeleteByLine.IsChecked = false;
            InsertByLine.IsChecked = false;
            SpecialInsert.IsChecked = false;
            Rex.IsChecked = false;
            Rename.IsChecked = false;
            PasteHelper.IsChecked = false;

            gReplac.IsEnabled = true;
            gBracket.IsEnabled = false;
            gSpace.IsEnabled = false;
            gReturn.IsEnabled = false;
            gExchangePL.IsEnabled = false;
            gCopyTimes.IsEnabled = false;
            gAddByLine.IsEnabled = false;
            gAddIndex.IsEnabled = false;
            gFormat.IsEnabled = false;
            gSymbolTrans.IsEnabled = false;
            gDeleteByLine.IsEnabled = false;
            gInsertByLine.IsEnabled = false;
            gSpecialInsert.IsEnabled = false;
            gRex.IsEnabled = false;
            gRename.IsEnabled = false;
            gPasteHelper.IsEnabled = false;

            gReplac.Visibility = Visibility.Visible;
            gBracket.Visibility = Visibility.Collapsed;
            gSpace.Visibility = Visibility.Collapsed;
            gReturn.Visibility = Visibility.Collapsed;
            gExchangePL.Visibility = Visibility.Collapsed;
            gCopyTimes.Visibility = Visibility.Collapsed;
            gAddByLine.Visibility = Visibility.Collapsed;
            gAddIndex.Visibility = Visibility.Collapsed;
            gFormat.Visibility = Visibility.Collapsed;
            gSymbolTrans.Visibility = Visibility.Collapsed;
            gDeleteByLine.Visibility = Visibility.Collapsed;
            gInsertByLine.Visibility = Visibility.Collapsed;
            gSpecialInsert.Visibility = Visibility.Collapsed;
            gRex.Visibility = Visibility.Collapsed;
            gRename.Visibility = Visibility.Collapsed;
            gPasteHelper.Visibility = Visibility.Collapsed;

            AddIndexDigitsTextBlock.Visibility = Visibility.Collapsed;
            AddIndexDigits.Visibility = Visibility.Collapsed;

            PasteNoticeText.Visibility = Visibility.Collapsed;

            Box1.Clear();
            Box2.Clear();
        }
        private void InitializeEveryFunction()
        {
            // 文本替换
            RepL.Clear();
            RepR.Clear();
            //括号内容
            BraL.Clear();
            BraR.Clear();
            BraV.IsChecked = false;
            BraIniList.SelectedIndex = 0;
            BraKeep.IsChecked = false;
            BraPair.IsChecked = false;
            BraPairNotice.Visibility = Visibility.Collapsed;
            // 删除空格
            SpaT.IsChecked = false;
            SpaF.IsChecked = false;
            // 换行符相关
            RetA.IsChecked = true;
            // 文字顺序调换
            ExchangeAll.IsChecked = true;
            // 复制当前内容
            CopyTimeCount.Text = "1";
            // 逐行添加内容
            AddC.Clear();
            AddP.Text = "0";
            AddIgnoreB.IsChecked = false;
            // 逐行添加序号
            AddIndexL.Clear();
            AddIndexR.Clear();
            AddIndexA.IsChecked = false;
            AddIndexI.IsChecked = false;
            AddIndexP.Text = "0";
            AddIndexS.Text = "1";
            AddIndexDigits.Text = "3";
            AddIndexNumber.IsChecked = true;
            // 文字格式转换
            Format1.IsChecked = true;
            // 自定义转换列表
            UpdatePairList(this, null);
            SymbolDirect.IsChecked = false;
            // 隔行删除
            DeleteValue1.Text = "1";
            DeleteValue2.Text = "1";
            KeepRemovedContent.IsChecked = false;
            // 隔行插入
            InsertValue1.Text = "1";
            InsertValue2.Text = "1";
            // 特殊插入
            SpecialAddP.Text = "0";
            SpecialIgnoreS.IsChecked = false;
            // 正则表达式
            RexFrom.Clear();
            RexTo.Clear();
            cs.IsChecked = false;
            ml.IsChecked = false;
            sl.IsChecked = false;
            ib.IsChecked = false;
            vc.IsChecked = false;
            ReplaceOnly.IsChecked = true;
            // 文件重命名 由于功能要求，不能清空此项的内容
            // 剪贴板辅助工具
            PasteCycle.IsChecked = false;
            PasteIgnoreBlank.IsChecked = false;
            PasteAuto.IsChecked = false;
            PasteAutoGroup.Visibility = Visibility.Collapsed;
            PasteAutoDelay.Text = "300";
            PasteAutoKey.Text = "TAB";
        }
        private void InitializeConfigElements()
        {
            AutoCheckUpdateCheckBox.IsChecked = autoCheckUpdate;
            AutoClearAfterCopyCheckBox.IsChecked = clearAfterCopy;
            AutoResetCheckBox.IsChecked = clearAfterUse;
            AutoSaveTempCheckBox.IsChecked = autoSaveTemp;
            ShowTimeCheckBox.IsChecked = showSpeed;
            ShowBothTextBoxCheckBox.IsChecked = alwaysShowBox2;

            HistoryCountComboBox.Items.Add("5");
            HistoryCountComboBox.Items.Add("10");
            HistoryCountComboBox.Items.Add("20");
            HistoryCountComboBox.Items.Add("50");
            HistoryCountComboBox.Items.Add("100");
            HistoryCountComboBox.Text = historyCount.ToString();
        }

        // 与界面中两个文本框的显示与否及各自的位置有关
        private bool isUsingBox2 = false;
        private double LowerGridHeight = 0;
        public void UpdateDisplay()
        {
            if (!isUsingBox2 && !alwaysShowBox2)
            {
                LowerGridHeight = lower.ActualHeight;
                Box2.Visibility = Visibility.Collapsed;
                UpsideDownButton.Visibility = Visibility.Collapsed;
                HorizontalSplitter.Visibility = Visibility.Collapsed;
                lower.Height = new GridLength(0);
                middle.Height = new GridLength(0);
            }
            else if (Box2.Visibility == Visibility.Collapsed)
            {
                Box2.Visibility = Visibility.Visible;
                UpsideDownButton.Visibility = Visibility.Visible;
                HorizontalSplitter.Visibility = Visibility.Visible;
                if (LowerGridHeight > 0)
                    lower.Height = new GridLength(LowerGridHeight);
                else
                {
                    upper.Height = new GridLength(1, GridUnitType.Star);
                    lower.Height = new GridLength(1, GridUnitType.Star);
                }
                middle.Height = GridLength.Auto;
            }
            else
            {
            }
            // 如果历史记录最大数量为0，则撤销键不显示
            if (historyCount == 0) CancelButton.Visibility = Visibility.Collapsed;
            else CancelButton.Visibility = Visibility.Visible;
            // 如果历史记录数量为0，则撤销键不可用
            if (history.Count == 0) CancelButton.IsEnabled = false;
            else CancelButton.IsEnabled = true;
            // 如果勾选在复制后自动清空，则按钮名称变为剪切
            if (clearAfterCopy) CopyButton.Content = "剪切";
            else CopyButton.Content = "复制";
            // 如果选择剪贴板辅助工具，则显示停止按钮
            if (PasteHelper.IsChecked.Value)
                StopPasteHelper.Visibility = Visibility.Visible;
            else
                StopPasteHelper.Visibility = Visibility.Collapsed;
            // 如果正在进行剪贴板辅助，则停止按钮可以点击
            if (isUsingPasteHelper)
            {
                PasteNoticeText.Visibility = Visibility.Visible;
                StopPasteHelper.IsEnabled = true;
            }
            else
            {
                PasteNoticeText.Visibility = Visibility.Collapsed;
                StopPasteHelper.IsEnabled = false;
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDisplay();
        }

        // 各功能的函数调用
        #region ApplyFunction

        private void ApplyFunction()
        {
            DateTime start_time = DateTime.Now;

            string originText = Text1;
            string newText1 = string.Empty;
            string newText2 = string.Empty;

            if (Replac.IsChecked.Value)
            {
                // 文本替换
                DoReplace(ref newText1);
            }
            else if (Bracket.IsChecked.Value)
            {
                // 括号内容
                DoRemoveBracket(ref newText1, ref newText2);
            }
            else if (Space.IsChecked.Value)
            {
                // 删除空格
                DoRemoveSpace(ref newText1);
            }
            else if (Return.IsChecked.Value)
            {
                // 换行符相关
                DoRemoveReturn(ref newText1);
            }
            else if (ExchangePL.IsChecked.Value)
            {
                // 文字顺序调换
                DoExchangePerLetter(ref newText1);
            }
            else if (CopyTimes.IsChecked.Value)
            {
                DoCopyTimes(ref newText1);
            }
            else if (AddByLine.IsChecked.Value)
            {
                // 逐行添加文字
                DoAddTextAt(ref newText1);
            }
            else if (AddIndex.IsChecked.Value)
            {
                // 逐行添加序号
                DoAddLineIndex(ref newText1);
            }
            else if (Format.IsChecked.Value)
            {
                // 调用VB的库进行编辑
                // 或者使用ChnCharInfo库进行汉字转拼音
                DoFormatEdit(ref newText1);
            }
            else if (SymbolTrans.IsChecked.Value)
            {
                // 自定义转换列表
                DoTransformList(ref newText1);
            }
            else if (DeleteByLine.IsChecked.Value)
            {
                // 隔行删除
                DoRemoveLines(ref newText1, ref newText2);
            }
            else if (InsertByLine.IsChecked.Value)
            {
                // 隔行插入
                DoInsertByLine(ref newText1);
            }
            else if (SpecialInsert.IsChecked.Value)
            {
                // 特殊插入
                DoSpecialAddTextAt(ref newText1);
            }
            else if (Rex.IsChecked.Value)
            {
                // 使用正则表达式
                DoUseRegExp(ref newText1, ref newText2);
            }
            else if (Rename.IsChecked.Value)
            {
                // 文件重命名
                TryRenameFiles();
                return;
            }
            else if (PasteHelper.IsChecked.Value)
            {
                // 剪贴板辅助工具
                DoPasteHelper();
                return;
            }

            // 效果执行完毕，如果没有问题，则对右边文本框进行修改
            if (newText1.Length > 0)
            {
                AddHistory();
                Text1 = newText1;
                Box1.Focus();
            }
            if (newText2.Length > 0)
            {
                Text2 = newText2;
            }

            DateTime stop_time = DateTime.Now;
            TimeSpan delay = stop_time.Subtract(start_time);
            if (showSpeed)
            {
                if (delay.TotalMilliseconds > 100)
                {
                    MessageBox.Show(string.Format("用时{0}毫秒", delay.TotalMilliseconds));
                }
            }
        }

        private void DoReplace(ref string T1)
        {
            T1 = f.Replace(Text1, RepL.Text, RepR.Text, true);
        }
        private void DoRemoveBracket(ref string T1, ref string T2)
        {
            T2 = string.Empty;
            if (!BraKeep.IsChecked.Value)
            {
                string temp = string.Empty;
                T1 = f.RemoveBracket(Text1, BraL.Text, BraR.Text, !BraV.IsChecked.Value, ref temp, BraPair.IsChecked.Value);
            }
            else
            {
                T1 = f.RemoveBracket(Text1, BraL.Text, BraR.Text, !BraV.IsChecked.Value, ref T2, BraPair.IsChecked.Value);
            }
        }
        private void DoRemoveSpace(ref string T1)
        {
            T1 = f.RemoveSpace(Text1, SpaT.IsChecked.Value, SpaF.IsChecked.Value);
        }
        private void DoRemoveReturn(ref string T1)
        {
            if (RetA.IsChecked.Value)
            {
                T1 = f.RemoveReturn(Text1, false);
            }
            else if (RetU.IsChecked.Value)
            {
                T1 = f.RemoveReturn(Text1, true);
            }
        }
        private void DoExchangePerLetter(ref string T1)
        {
            int selected = 0;
            if (ExchangeAll.IsChecked.Value) selected = 1;
            else if (ExchangeLine.IsChecked.Value) selected = 2;
            else if (ExchangeLineLetter.IsChecked.Value) selected = 3;
            T1 = f.ExchangePerLetter(Text1, selected);
        }
        private void DoCopyTimes(ref string T1)
        {
            T1 = f.CopyTimes(Text1, CopyTimeCount.Text);
        }
        private void DoAddTextAt(ref string T1)
        {
            T1 = f.AddTextAt(Text1, AddC.Text, AddP.Text, AddIgnoreB.IsChecked.Value);
        }
        private void DoAddLineIndex(ref string T1)
        {
            int digits;
            bool result = int.TryParse(AddIndexDigits.Text, out digits);
            int style = 0;
            if (AddIndexChinese.IsChecked.Value)
                style = 1;
            if (!result) return;
            T1 = f.AddLineIndex(Text1, AddIndexL.Text, AddIndexR.Text, AddIndexP.Text, AddIndexS.Text,
                       AddIndexA.IsChecked.Value, AddIndexI.IsChecked.Value, digits, style);
        }
        private void DoFormatEdit(ref string T1)
        {
            int i;
            for (i = 1; i <= 5; i++)
            {
                if (((RadioButton)(FindName("Format" + i.ToString()))).IsChecked.Value)
                {
                    T1 = f.TransformStr(Text1, i);
                    break;
                }
            }
            if (i > 5 && ((RadioButton)FindName("Pinyin")).IsChecked.Value)
            {
                string temp = string.Empty;
                foreach (char c in Text1)
                {
                    if (Regex.IsMatch(c.ToString(), @"[\u4e00-\u9fbb]"))
                    {
                        ChineseChar ch = new ChineseChar(c);
                        string s = ch.Pinyins[0];
                        s = s.ToLower();
                        s = s.Substring(0, s.Length - 1);
                        temp += s + ' ';
                    }
                    else
                    {
                        temp += c;
                    }
                }
                T1 = temp;
            }
        }
        private void DoRemoveLines(ref string T1, ref string T2)
        {
            try
            {
                int per = Int32.Parse(DeleteValue1.Text);
                int delete = Int32.Parse(DeleteValue2.Text);
                if (per < 1 || delete < 1)
                {
                    MessageBox.Show("请在文本框中输入大于0的整数。", "提示");
                    return;
                }
                if (!KeepRemovedContent.IsChecked.Value)
                {
                    string ttt = "";
                    T1 = f.RemoveLines(Text1, per, delete, ref ttt);
                }
                else
                {
                    T1 = f.RemoveLines(Text1, per, delete, ref T2);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("请在文本框中输入正确的数字。", "提示");
                T1 = Text1;
            }
            catch (Exception err)
            {
                MessageBox.Show("隔行删除功能失败。\n原因：" + err.Message, "提示");
                T1 = Text1;
            }
        }
        private void DoInsertByLine(ref string T1)
        {
            try
            {
                int per = Int32.Parse(InsertValue1.Text);
                int insert = Int32.Parse(InsertValue2.Text);
                if (per < 1 || insert < 1) return;
                T1 = f.InsertLines(Text1, Text2, per, insert);
            }
            catch (FormatException)
            {
                MessageBox.Show("请在文本框中输入正确的数字。", "提示");
                T1 = Text1;
            }
            catch (Exception)
            {
                MessageBox.Show("请在文本框中输入大于0的整数。", "提示");
                T1 = Text1;
            }
        }
        private void DoSpecialAddTextAt(ref string T1)
        {
            T1 = f.SpecialAddTextAt(Text1, Text2, SpecialAddP.Text, SpecialIgnoreS.IsChecked.Value);
        }
        private void DoUseRegExp(ref string T1, ref string T2)
        {
            if (ReplaceOnly.IsChecked.Value)
            {
                T1 = f.UseRegExp(Text1, RexFrom.Text, RexTo.Text, GetRexOptions());
            }
            else if (ShowMatchAndReplace.IsChecked.Value)
            {
                T1 = f.UseRegExp(Text1, RexFrom.Text, RexTo.Text, out T2, GetRexOptions());
            }
            else if (ShowMatch.IsChecked.Value)
            {
                f.UseRegExp(Text1, RexFrom.Text, out T2, GetRexOptions());
            }
        }
        private void DoTransformList(ref string T1)
        {
            Pair current = GetTransfromList(SymbolList.Text);
            List<string> from = current.From, to = current.To;
            bool usingRex = current.usingRex;

            // 以防万一，再次判断转换列表长度是否相等
            if ((from.Count != to.Count) || from.Count == 0)
                return;

            if (usingRex)
            {
                if (SymbolDirect.IsChecked.Value)
                {
                    List<string> temp = from;
                    from = to;
                    to = temp;
                }
                try
                {
                    T1 = Text1;
                    for (int i = 0; i < from.Count; i++)
                    {
                        T1 = f.UseRegExp(T1, from[i], to[i], current.RegOptions);
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show("错误，使用正则表达式失败。错误原因：" + err.Message, "提示");
                }
            }
            else
            {
                T1 = f.Transform(Text1, from, to, !SymbolDirect.IsChecked.Value);
            }

        }
        private void DoPasteHelper()
        {
            if (isUsingPasteHelper)
            {
                kh.UnHook();
            }
            kh = new KeyboardHook();
            kh.SetHook();
            kh.OnKeyDownEvent += HookOnKeyDownEvent;
            isUsingPasteHelper = true;
            PasteLineIndex = 0;
            PasteLines = f.SplitByStr(Text1, Environment.NewLine, !PasteIgnoreBlank.IsChecked.Value);
            Clipboard.SetText(PasteLines[PasteLineIndex]);
            MessageBox.Show("剪贴板辅助工具已启用，\n共采集到文本信息 " + PasteLines.Count.ToString() + " 行。");

            // 用来显示停止按钮
            UpdateDisplay();
        }

        #endregion

        // 剪贴板辅助工具相关
        #region PasteHelper

        KeyboardHook kh;
        private void HookOnKeyDownEvent(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (isUsingPasteHelper)
            {
                // 如果按下Ctrl+V
                if (e.KeyData == (System.Windows.Forms.Keys.V | System.Windows.Forms.Keys.Control))
                {
                    try
                    {
                        // 如果还没有循环一遍
                        if (PasteLineIndex < PasteLines.Count)
                        {
                            Clipboard.SetText(PasteLines[PasteLineIndex]);
                            PasteLineIndex++;
                        }
                        // 如果已经粘贴完全部的内容
                        if (PasteLineIndex >= PasteLines.Count)
                        {
                            // 如果开启了粘贴循环，则重头开始
                            if (PasteCycle.IsChecked.Value)
                            {
                                PasteLineIndex = 0;
                            }
                            // 否则，关闭剪贴板辅助工具
                            else
                            {
                                kh.UnHook();
                                ClearPasteHelper();
                                //MessageBox.Show("剪贴板辅助工具已自动停止");
                            }
                        }

                    }
                    catch (Exception err)
                    {
                        MessageBox.Show("剪贴板辅助工具出现错误。\n原因：" + err.Message);
                        kh.UnHook();
                        ClearPasteHelper();
                    }
                    try
                    {
                        if (PasteAuto.IsChecked.Value)
                        {
                            kh.UnHook();
                            Thread.Sleep(800);
                            int delay = int.Parse(PasteAutoDelay.Text) / 2;
                            string key = "{" + PasteAutoKey.Text + "}";
                            IsEnabled = false;
                            Clipboard.SetText(PasteLines[PasteLineIndex]);
                            while (PasteLineIndex < PasteLines.Count)
                            {
                                if (key != "{NONE}")
                                    System.Windows.Forms.SendKeys.SendWait(key);
                                Thread.Sleep(delay);
                                System.Windows.Forms.SendKeys.SendWait("^{V}");
                                Thread.Sleep(delay);
                                PasteLineIndex++;
                                if (PasteLineIndex >= PasteLines.Count)
                                {
                                    ClearPasteHelper();
                                    IsEnabled = true;
                                    return;
                                }
                                Clipboard.SetText(PasteLines[PasteLineIndex]);
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show("剪贴板辅助工具出现错误。\n原因：" + err.Message);
                        ClearPasteHelper();
                        IsEnabled = true;
                        return;
                    }
                }
            }
        }

        private List<string> PasteLines = new List<string>();
        private int PasteLineIndex = 0;
        private bool isUsingPasteHelper = false;
        private void ClearPasteHelper()
        {
            isUsingPasteHelper = false;
            PasteLineIndex = 0;
            PasteLines.Clear();

            // 用于将停止按钮Disable
            UpdateDisplay();
        }

        #endregion

        // 用来获取当前正则表达式的配置
        private RegexOptions GetRexOptions()
        {
            RegexOptions option = new RegexOptions();
            bool caseSensitive = cs.IsChecked.Value;
            bool multiLine = ml.IsChecked.Value;
            bool singleLine = sl.IsChecked.Value;
            bool ignoreBlank = ib.IsChecked.Value;
            bool explicitCapture = vc.IsChecked.Value;
            if (caseSensitive) option |= RegexOptions.IgnoreCase;
            if (multiLine) option |= RegexOptions.Multiline;
            if (singleLine) option |= RegexOptions.Singleline;
            if (ignoreBlank) option |= RegexOptions.IgnorePatternWhitespace;
            if (explicitCapture) option |= RegexOptions.ExplicitCapture;
            return option;
        }

        // 用来get和set两个文本框的内容
        private string Text1
        {
            get { return Box1.Text; }
            set { Box1.Text = value; }
        }
        private string Text2
        {
            get { return Box2.Text; }
            set { Box2.Text = value; }
        }

        // 程序中自定义的快捷键
        #region ShortcutSettings

        void OpenFileShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            打开MenuItem_Click(this, null);
        }
        void SettingsShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            选项MenuItem_Click(this, null);
        }
        void HelpShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            帮助MenuItem_Click(HelpMenuItem, null);
        }
        void CountShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            统计MenuItem_Click(HelpMenuItem, null);
        }
        void InitFuncShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            InitFuncMenuItem_Click(this, null);
        }
        private void CarryOutFuncShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ApplyFunction();
        }
        private void UndoShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Undo_Click(this, null);
        }
        private void CopyToClipboardShortcutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Copy_Click(this, null);
        }

        #endregion

        // 右侧上方的按钮事件
        #region RightButtonEvent

        // 执行功能
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            ApplyFunction();
        }
        // 清空两个文本框的内容
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            AddHistory();
            Box1.Clear();
            Box2.Clear();

            if (isUsingPasteHelper)
            {
                ClearPasteHelper();
                kh.UnHook();
                MessageBox.Show("剪贴板辅助工具已手动停止");
            }
        }
        // 撤销更改
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (history.Count == 0)
            {
                //MessageBox.Show("当前没有历史记录。", "提示");
                return;
            }
            else
            {
                Text1 = GetLatestHistory();
                if (history.Count == 0)
                {
                    CancelButton.IsEnabled = false;
                }
            }
        }
        // 复制到剪贴板
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            string temp = "";

            temp += Text1;
            if (isUsingBox2)
            {
                temp += Environment.NewLine + Environment.NewLine;
                temp += Text2;
            }

            try
            {
                Clipboard.SetDataObject(temp);
            }
            finally
            {
                if (clearAfterCopy)
                {
                    AddHistory();
                    Box1.Clear();
                    if (isUsingBox2)
                        Box2.Clear();
                }
            }
        }
        // 替换两个Box的内容
        private void UpsideDown_CLick(object sender, RoutedEventArgs e)
        {
            string temp = Text1;
            Text1 = Text2;
            Text2 = temp;
        }
        // 停止剪贴板辅助工具
        private void StopPasteHelper_CLick(object sender, RoutedEventArgs e)
        {
            if (isUsingPasteHelper)
            {
                ClearPasteHelper();
                kh.UnHook();
                MessageBox.Show("剪贴板辅助工具已手动停止");
            }
        }

        #endregion

        // 程序配置选项的相关事件
        #region ConfigEvents

        // 菜单栏中，关于配置的选项的属性改变
        private void ConfigBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox c = sender as CheckBox;
            bool result = c.IsChecked.Value;
            switch (c.Name)
            {
                case "AutoCheckUpdateCheckBox": autoCheckUpdate = result; break;
                case "AutoClearAfterCopyCheckBox": clearAfterCopy = result; break;
                case "AutoResetCheckBox": clearAfterUse = result; break;
                case "AutoSaveTempCheckBox": autoSaveTemp = result; break;
                case "ShowTimeCheckBox": showSpeed = result; break;
                case "ShowBothTextBoxCheckBox": alwaysShowBox2 = result; break;
            }
            UpdateDisplay();
        }
        private void ConfigBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ConfigBox_Checked(sender, null);
        }

        private void HistoryCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int value;
            if (int.TryParse(HistoryCountComboBox.SelectedItem.ToString(), out value))
            {
                historyCount = value;
            }
            else
            {
                MessageBox.Show(HistoryCountComboBox.SelectedItem.ToString());
            }
        }

        #endregion

        // 历史记录相关
        #region HistoryFunction

        private List<string> history = new List<string>();// 历史记录
        public int historyCount = 10;// 最大历史记录条数
        // 添加历史记录
        private void AddHistory()
        {
            AddHistory(Text1);
        }
        private void AddHistory(string temp)
        {
            if (historyCount == 0) return;
            // 判断当前存储的历史记录数量是否超过最大值
            if (history.Count >= historyCount)
            {
                history.RemoveAt(0);
            }
            history.Add(temp);
            UpdateDisplay();
        }
        // 获取最新历史记录，并自动清除最新历史记录
        private string GetLatestHistory()
        {
            if (history.Count == 0)
            {
                return string.Empty;
            }
            string temp = history[history.Count - 1];
            history.RemoveAt(history.Count - 1);
            return temp;
        }


        #endregion

        // 软件参数相关
        #region Configuration

        // 软件参数
        public bool clearAfterUse;
        public bool clearAfterCopy;
        public bool autoSaveTemp;
        public bool showSpeed;
        private string version = string.Empty;
        public bool alwaysShowBox2 = false;
        public bool autoCheckUpdate;
        // 获取参数
        private void ReadConfig()
        {
            string filePath = "config.xml";
            if (!File.Exists(filePath))
            {
                MessageBox.Show("未找到配置文件。默认配置文件已创建。", "提示", MessageBoxButton.OK);

                WriteInitialConfig(filePath);
            }

            XmlDocument doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(filePath, settings);
            doc.Load(filePath);
            reader.Close();

            // 获取根节点
            XmlNode xn = doc.SelectSingleNode("property");
            // 获取全部子节点
            //XmlNodeList xnl = xn.ChildNodes;

            try
            {
                XmlElement xe0 = (XmlElement)(xn.SelectSingleNode("version"));
                XmlElement xe0_1 = (XmlElement)(xn.SelectSingleNode("number"));
                Title = "文本编辑器 " + xe0.InnerText + " v" + xe0_1.InnerText;
                version = xe0_1.InnerText;

                XmlElement xe1 = (XmlElement)(xn.SelectSingleNode("autoClear"));
                clearAfterUse = Convert.ToBoolean(xe1.GetAttribute("value"));

                XmlElement xe2 = (XmlElement)(xn.SelectSingleNode("memoryCount"));
                historyCount = Convert.ToInt32(xe2.GetAttribute("value"));

                XmlElement xe3 = (XmlElement)(xn.SelectSingleNode("clearAfterCopy"));
                clearAfterCopy = Convert.ToBoolean(xe3.GetAttribute("value"));

                XmlElement xe4 = (XmlElement)(xn.SelectSingleNode("textBoxColor"));
                SetColor(xe4.GetAttribute("value"));

                XmlElement xe5 = (XmlElement)(xn.SelectSingleNode("autoSaveTemp"));
                autoSaveTemp = Convert.ToBoolean(xe5.GetAttribute("value"));

                XmlElement xe6 = (XmlElement)(xn.SelectSingleNode("showSpeed"));
                showSpeed = Convert.ToBoolean(xe6.GetAttribute("value"));

                XmlElement xe7 = (XmlElement)(xn.SelectSingleNode("alwaysShowBox2"));
                alwaysShowBox2 = Convert.ToBoolean(xe7.GetAttribute("value"));

                XmlElement xe8 = (XmlElement)(xn.SelectSingleNode("autoCheckUpdate"));
                autoCheckUpdate = Convert.ToBoolean(xe8.GetAttribute("value"));
            }
            catch (Exception)
            {
                MessageBoxResult result = MessageBox.Show("配置文件内容错误，是否删除原有config文件并重建？", "提示", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    File.Delete(filePath);
                    ReadConfig();
                }
                else
                {
                    Close();
                }
            }
        }
        private void SaveConfig()
        {
            string filePath = "config.xml";
            // 判断config文件是否存在
            if (!File.Exists(filePath))
            {
                MessageBox.Show("未找到配置文件。默认配置文件已创建。", "提示", MessageBoxButton.OK);
                WriteInitialConfig(filePath);
            }

            XmlDocument doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(filePath, settings);
            doc.Load(filePath);
            reader.Close();

            try
            {
                // 获取根节点
                XmlNode xn = doc.SelectSingleNode("property");
                // 获取全部子节点
                //XmlNodeList xnl = xn.ChildNodes;
                XmlElement AC = (XmlElement)xn.SelectSingleNode("autoClear");
                AC.SetAttribute("value", clearAfterUse.ToString());

                XmlElement MC = (XmlElement)xn.SelectSingleNode("memoryCount");
                MC.SetAttribute("value", historyCount.ToString());

                XmlElement CC = (XmlElement)xn.SelectSingleNode("clearAfterCopy");
                CC.SetAttribute("value", clearAfterCopy.ToString());

                XmlElement TBC = (XmlElement)xn.SelectSingleNode("textBoxColor");
                TBC.SetAttribute("value", Color);

                XmlElement AST = (XmlElement)xn.SelectSingleNode("autoSaveTemp");
                AST.SetAttribute("value", autoSaveTemp.ToString());

                XmlElement SS = (XmlElement)xn.SelectSingleNode("showSpeed");
                SS.SetAttribute("value", showSpeed.ToString());

                XmlElement AS2 = (XmlElement)(xn.SelectSingleNode("alwaysShowBox2"));
                AS2.SetAttribute("value", alwaysShowBox2.ToString());

                XmlElement AC2 = (XmlElement)(xn.SelectSingleNode("autoCheckUpdate"));
                AC2.SetAttribute("value", autoCheckUpdate.ToString());
            }
            catch (Exception error)
            {
                MessageBox.Show("保存Config出现错误。\n原因：" + error.Message);
                //Close();
            }

            doc.Save(filePath);
        }
        private void WriteInitialConfig(string path)
        {
            Stream sm = Assembly.GetExecutingAssembly().GetManifestResourceStream("TextEdit.config.xml");
            StreamWriter sw = new StreamWriter(path);
            StreamReader sr = new StreamReader(sm);
            sw.Write(sr.ReadToEnd());
            //sw.Write(Properties.Resources.InitialConfig); 以前使用的是Resources，2.5.1版本改用嵌入的资源
            sw.Close();
            sw.Dispose();
            sr.Dispose();
        }
        // 设置文本框的颜色
        private string Color = "白色";
        private void SetColor(object sender, RoutedEventArgs e)
        {
            // 判断什么颜色
            MenuItem m = sender as MenuItem;
            // 判断对什么进行颜色调整
            SetColor((string)m.Header);
            // 如果选择了黑色，则需要调整文字颜色为白色
            if (m.Header.ToString() == "黑色")
            {
                Box1.Foreground = Box2.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                Box1.Foreground = Box2.Foreground = new SolidColorBrush(Colors.Black);
            }

            Color = m.Header.ToString();
        }
        private void SetColor(string color_name)
        {
            Box1.Foreground = Box2.Foreground = new SolidColorBrush(Colors.Black);
            switch (color_name)
            {
                case "green":
                case "Green":
                case "绿色":
                    Box1.Background = Box2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCAFFCA"));
                    break;
                case "red":
                case "Red":
                case "红色":
                    Box1.Background = Box2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFCACA"));
                    break;
                case "blue":
                case "Blue":
                case "蓝色":
                    Box1.Background = Box2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCACAFF"));
                    break;
                case "yellow":
                case "Yellow":
                case "黄色":
                    Box1.Background = Box2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFCA"));
                    break;
                case "black":
                case "Black":
                case "黑色":
                    Box1.Background = Box2.Background = new SolidColorBrush(Colors.Black);
                    Box1.Foreground = Box2.Foreground = new SolidColorBrush(Colors.White);
                    break;
                case "white":
                case "White":
                case "白色":
                    Box1.Background = Box2.Background = new SolidColorBrush(Colors.White);
                    break;
            }
            Color = color_name;
        }

        #endregion

        // 软件功能函数
        #region UsefulFunctions

        // 使用默认浏览器打开网页
        public void OpenBrowser(string url)
        {
            //RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command\");
            //string s = key.GetValue("").ToString();
            //string browserpath = null;
            //if (s.StartsWith("\""))
            //{
            //    browserpath = s.Substring(1, s.IndexOf('\"', 1) - 1);
            //}
            //else
            //{
            //    browserpath = s.Substring(0, s.IndexOf(" "));
            //}
            //return System.Diagnostics.Process.Start(browserpath, url) != null;

            // 上面的方法在高版本Windows中并不可用，会打开IE，原因不明
            System.Diagnostics.Process.Start(url);
        }

        // 获取最新版软件的版本号
        public void CheckLatestVersion(bool mode)
        {
            // mode = true ：如果是最新版，或者网络连接失败，都会给出提示
            // mode = false：只有发现最新版时，才会给出提示
            string url = @"https://www.zybuluo.com/byjr-k/note/267993";
            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                var response = request.GetResponse() as HttpWebResponse;
                var sr = new StreamReader(response.GetResponseStream());
                string info = sr.ReadToEnd();
                sr.Dispose();

                Regex r = new Regex(@"(?<=当前最新版：v )\d+\.\d+\.\d+");
                int latestversion = TransformVersion(r.Match(info).ToString());
                int currentversion = TransformVersion(version);

                if (latestversion > currentversion)
                {
                    MessageBox.Show("文本编辑器有最新版更新，您可以使用帮助中提供的下载地址进行下载。", "提示");
                }
                else if (latestversion == currentversion)
                {
                    if (mode)
                        MessageBox.Show("您当前使用的是最新版软件，无需更新。", "提示");
                }
                else
                {
                    if (mode)
                        MessageBox.Show("您当前使用的最新的内测版。", "提示");
                }
            }
            catch (Exception)
            {
                // 除非是分享链接出了问题，否则只能是因为无法联网
                if (mode)
                    MessageBox.Show("当前网络连接失败，请重试。", "提示");
            }
        }

        public void CheckLatestVersion2()
        {
            CheckLatestVersion(false);
        }

        // 将版本号换算为整数
        private int TransformVersion(string version)
        {
            Regex r = new Regex(@"\d+");
            MatchCollection mc = r.Matches(version);
            if (mc.Count != 3) return -1;

            int A = int.Parse(mc[0].ToString()),
                B = int.Parse(mc[1].ToString()),
                C = int.Parse(mc[2].ToString());

            return A * 1000000 + B * 1000 + C;
        }

        #endregion

        // 左侧元件的事件
        #region LeftElementEvent

        // 文本替换的按钮
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            string temp = RepL.Text;
            RepL.Text = RepR.Text;
            RepR.Text = temp;
        }
        // 括号内容的按钮
        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            string temp = BraL.Text;
            BraL.Text = BraR.Text;
            BraR.Text = temp;
        }
        // 左侧大多数TextBox在获取焦点时，自动全选
        private void LeftTextBox_Focused(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }
        // 括号内容中，预设括号对下拉菜单的点击功能
        private void BraIniList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string temp = BraIniList.SelectedItem.ToString();
            //MessageBox.Show(string.Format("SelectionChangedEvent Happens!\nText: {0}\nLength: {1}", temp, temp.Length));
            if (temp.Length == 2)
            {
                BraL.Text = temp[0].ToString();
                BraR.Text = temp[1].ToString();
            }
        }
        private void BraListInitialize()
        {
            BraIniList.Items.Clear();
            BraIniList.Items.Add("常见括号：");
            BraIniList.Items.Add("()");
            BraIniList.Items.Add("（）");
            BraIniList.Items.Add("[]");
            BraIniList.Items.Add("【】");
            BraIniList.Items.Add("〖〗");
            BraIniList.Items.Add("「」");
            BraIniList.Items.Add("{}");
            BraIniList.Items.Add("<>");
            BraIniList.Items.Add("《》");
            BraIniList.SelectedIndex = 0;
        }

        // 左侧用于选择相应功能的RadioButton的相关事件
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Panel p = (Panel)(FindName("g" + ((RadioButton)sender).Name));
            p.IsEnabled = true;
            p.Visibility = Visibility.Visible;
            RadioButton r = (RadioButton)sender;
            r.Padding = new Thickness(4, -5, 0, 0);
            r.FontWeight = FontWeights.Bold;
            r.FontSize = 18.0;
            r.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0066CC"));

            isUsingBox2 = false;
            if (r.Name == "InsertByLine")
            {
                isUsingBox2 = true;
                UpdateDisplay();
            }
            else if (r.Name == "DeleteByLine")
            {
                if (((CheckBox)FindName("KeepRemovedContent")).IsChecked.Value)
                {
                    isUsingBox2 = true;
                    UpdateDisplay();
                }
            }
            else if (r.Name == "SpecialInsert")
            {
                isUsingBox2 = true;
                UpdateDisplay();
            }
            else if (r.Name == "Bracket" && BraKeep.IsChecked.Value)
            {
                isUsingBox2 = true;
                UpdateDisplay();
            }
            else if (r.Name == "Rex" && (ShowMatch.IsChecked.Value || ShowMatchAndReplace.IsChecked.Value))
            {
                isUsingBox2 = true;
                UpdateDisplay();
            }
            else if (r.Name == "PasteHelper")
            {
                UpdateDisplay();
            }

            // 自动还原默认值
            if (clearAfterUse)
            {
                InitializeEveryFunction();
            }
        }
        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Panel p = (Panel)(FindName("g" + ((RadioButton)sender).Name));
            p.IsEnabled = false;
            p.Visibility = Visibility.Collapsed;
            RadioButton r = (RadioButton)sender;
            r.FontWeight = FontWeights.Regular;
            r.FontSize = 15.0;
            r.Foreground = new SolidColorBrush(Colors.Black);
            r.Padding = new Thickness(4, -1, 0, 0);

            if (r.Name == "InsertByLine")
            {
                isUsingBox2 = false;
                UpdateDisplay();
            }
            else if (r.Name == "DeleteByLine")
            {
                isUsingBox2 = false;
                UpdateDisplay();
            }
            else if (r.Name == "SpecialInsert")
            {
                isUsingBox2 = false;
                UpdateDisplay();
            }
            else if (r.Name == "Bracket" && BraKeep.IsChecked.Value)
            {
                isUsingBox2 = false;
                UpdateDisplay();
            }
            else if (r.Name == "Rex" && ShowMatch.IsChecked.Value)
            {
                isUsingBox2 = false;
                UpdateDisplay();
            }
            else if (r.Name == "PasteHelper")
            {
                UpdateDisplay();
            }
        }
        // 所有能够激活Box2的CheckBox都用这个event
        private void CheckBoxUseBox2_Checked(object sender, RoutedEventArgs e)
        {
            isUsingBox2 = true;
            UpdateDisplay();
        }
        private void CheckBoxUseBox2_Unchecked(object sender, RoutedEventArgs e)
        {
            isUsingBox2 = false;
            UpdateDisplay();
        }
        // 正则表达式功能中的CheckBox用这个event
        private void RexCheckBoxUseBox2_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            switch (rb.Name)
            {
                case "ReplaceOnly": isUsingBox2 = false; break;
                case "ShowMatch": isUsingBox2 = true; RexSecondTextBox.Visibility = Visibility.Collapsed; break;
                case "ShowMatchAndReplace": isUsingBox2 = true; break;
                default: /* 这怎么可能？ */ break;
            }
            UpdateDisplay();
        }
        private void RexCheckBoxUseBox2_UnChecked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            switch (rb.Name)
            {
                case "ShowMatch": isUsingBox2 = true; RexSecondTextBox.Visibility = Visibility.Visible; break;
                default: /* 这怎么可能？ */ break;
            }
            UpdateDisplay();
        }

        // 识别括号对的文字提示
        private void BraPair_Checked(object sender, RoutedEventArgs e)
        {
            BraPairNotice.Visibility = Visibility.Visible;
        }
        private void BraPair_Unchecked(object sender, RoutedEventArgs e)
        {
            BraPairNotice.Visibility = Visibility.Collapsed;
        }

        // 剪贴板辅助工具的自动粘贴功能的设置显示
        private void PasteAuto_Checked(object sender, RoutedEventArgs e)
        {
            PasteAutoGroup.Visibility = Visibility.Visible;
        }
        private void PasteAuto_Unchecked(object sender, RoutedEventArgs e)
        {
            PasteAutoGroup.Visibility = Visibility.Collapsed;
        }

        // 中间分割线的双击，隐藏/显示左侧的功能栏
        private double LeftToolBarWidth = 0;
        private void VerticalSplitter_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LeftToolBar.Width.Value > 0)
            {
                LeftToolBarWidth = LeftToolBar.Width.Value;
                LeftToolBar.MinWidth = 0;
                LeftToolBar.MaxWidth = 0;
                LeftToolBar.Width = new GridLength(0);
            }
            else
            {
                LeftToolBar.Width = new GridLength(LeftToolBarWidth);
                LeftToolBar.MinWidth = 220;
                LeftToolBar.MaxWidth = 360;
            }
        }

        // 逐行插入序号的对齐数字复选框控制显示或隐藏相应功能
        private void AddIndexAlign_Checked(object sender, RoutedEventArgs e)
        {
            AddIndexDigitsTextBlock.Visibility = Visibility.Visible;
            AddIndexDigits.Visibility = Visibility.Visible;
        }
        private void AddIndexAlign_Unchecked(object sender, RoutedEventArgs e)
        {
            AddIndexDigitsTextBlock.Visibility = Visibility.Collapsed;
            AddIndexDigits.Visibility = Visibility.Collapsed;
        }

        #endregion

        // 文件重命名功能
        #region FileRenameFunction

        List<string> fileNameList = new List<string>();
        private List<string> newFileNameList = new List<string>();
        private void CopyToRight_Click(object sender, RoutedEventArgs e)
        {
            if (fileNameList.Count == 0) return;
            else
            {
                Box1.Clear();
                for (int i = 0; i < fileNameList.Count - 1; i++)
                {
                    Box1.AppendText(Path.GetFileName(fileNameList[i]) + Environment.NewLine);
                }
                Box1.AppendText(Path.GetFileName(fileNameList[fileNameList.Count - 1]));
            }
        }
        private void ClearFileList_Click(object sender, RoutedEventArgs e)
        {
            RenameBox.Clear();
            fileNameList.Clear();
        }
        private void TryRenameFiles()
        {
            try
            {
                newFileNameList = f.SplitByStr(Text1, Environment.NewLine, false);
                // 判断最后一行是否为空行，如果是，则清除
                if (newFileNameList[newFileNameList.Count - 1].Length == 0)
                    newFileNameList.RemoveAt(newFileNameList.Count - 1);
                // 如果文件列表的长度小于新列表的长度，则放弃
                if (fileNameList.Count != newFileNameList.Count)
                {
                    MessageBox.Show("错误，新旧文件列表数量不同。", "提示");
                    return;
                }
                for (int i = 0; i < fileNameList.Count; i++)
                {
                    string newdic = Path.GetDirectoryName(fileNameList[i]) + @"\" + newFileNameList[i];
                    FileInfo fi = new FileInfo(fileNameList[i]);
                    fi.MoveTo(newdic);
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("重命名错误。\n原因：" + error.Message);
                return;
            }
            // 如果能运行到这里，则表示重命名成功。清空文件列表
            fileNameList.Clear();
            newFileNameList.Clear();
            RenameBox.Clear();
        }

        #endregion

        // 自定义转换列表相关
        #region PairExchangeFunction

        public static List<Pair> pairList = new List<Pair>();
        public PairExchangeReader reader = new PairExchangeReader();
        private void LoadTransformLists()
        {
            CheckFolderExist();
            // 每次读取时，先清空原有列表的内容
            SymbolList.Items.Clear();
            pairList.Clear();

            pairList = reader.LoadPairListFromFiles(@"PairExchange\");
            if (pairList.Count == 0)
            {
                SymbolList.Items.Clear();
                SymbolList.Text = "<暂无功能>";
                return;
            }
            foreach (string str in reader.GetNames(pairList))
            {
                SymbolList.Items.Add(str);
            }
            SymbolList.Text = SymbolList.Items[0] as string;
        }
        private Pair GetTransfromList(string name)
        {
            return reader.GetTransfromList(name, pairList);
        }
        private void UpdatePairList(object sender, RoutedEventArgs e)
        {
            LoadTransformLists();
        }
        private void ShowPairList(object sender, RoutedEventArgs e)
        {
            string n = SymbolList.Text;
            foreach (Pair p in pairList)
            {
                if (p.Name == n)
                {
                    PairListEditor editor = new PairListEditor(p);
                    editor.Left = this.Left + 100;
                    editor.Top = this.Top + 100;
                    editor.ShowDialog();
                    //System.Windows.MessageBox.Show(p.Info(), "转换列表信息");
                }
            }
        }
        private void OpenPairFile(object sender, RoutedEventArgs e)
        {
            string filePath = Environment.CurrentDirectory + @"\pairexchange\" + SymbolList.Text + ".txt";
            if (!File.Exists(filePath))
            {
                MessageBox.Show("该路径下的文件无效。\n路径：" + filePath);
                return;
            }
            else
            {
                System.Diagnostics.Process.Start(filePath);
            }
        }
        private void CheckFolderExist()
        {
            if (!Directory.Exists("PairExchange"))
            {
                MessageBox.Show("PairExchange文件夹不存在，自动创建新的文件夹。");
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\PairExchange");
            }
        }

        #endregion

        // 菜单项的点击事件
        #region MenuItemClickEvent

        private void 关闭MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void 帮助MenuItem_Click(object sender, RoutedEventArgs e)
        {

            string header = ((MenuItem)sender).Header.ToString();
            if (header.StartsWith("帮助"))
            {
                OpenBrowser(@"https://www.zybuluo.com/byjr-k/note/262468");
            }
            else if (header.StartsWith("版本"))
            {
                OpenBrowser(@"https://www.zybuluo.com/byjr-k/note/267993");
            }

            //Help help = new Help(((MenuItem)sender).Header.ToString());
            //help.WindowStartupLocation = WindowStartupLocation.Manual;
            //help.Left = Left + 50;
            //help.Top = Top + 50;
            //help.Width = Width - 100;
            //help.Height = Height - 100;
            //help.ShowDialog();
        }
        private void 更新MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CheckLatestVersion(true);
        }
        private void 打开MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Rename.IsChecked.Value)
            {
                System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();

                string FileName = "";
                open.FileName = "";
                open.Filter = "txt文件(*.txt)|*.txt|其他文件(*.*)|*.*";
                open.Title = "打开文本文件";
                open.Multiselect = false;
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileName = open.FileName;
                    //this.textBox1.Text = FileName;
                    StreamReader txtReader = new StreamReader(FileName, System.Text.Encoding.Default);
                    Text1 = txtReader.ReadToEnd();
                }

                // 如果当前正在使用两个文本框，则打开第二个文本文件
                if (!isUsingBox2) return;
                open.Title = "打开文本文件二";
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileName = open.FileName;
                    //this.textBox1.Text = FileName;
                    StreamReader txtReader = new StreamReader(FileName, System.Text.Encoding.Default);
                    Text2 = txtReader.ReadToEnd();
                }
            }
            else
            {
                // 正在使用文件重命名功能
                System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();

                openFileDialog1.FileName = "";
                openFileDialog1.Filter = "所有文件(*.*)|*.*";
                openFileDialog1.Title = "导入文件路径";
                openFileDialog1.Multiselect = true;
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //fileNameList.Clear();
                    RenameBox.Clear();
                    string[] list = openFileDialog1.FileNames;
                    foreach (string str in list)
                    {
                        if (!fileNameList.Contains(str))
                        {
                            fileNameList.Add(str);
                        }
                    }
                    // 重新刷新列表，保证顺序
                    foreach (string str in fileNameList)
                    {
                        RenameBox.AppendText(str + Environment.NewLine);
                    }
                }
            }
        }
        private void 文件目录MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Environment.CurrentDirectory);
        }
        private void 调整替换列表MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //PairListEditor form = new PairListEditor();
            //form.WindowStartupLocation = WindowStartupLocation.Manual;
            //form.Left = this.Left + 100;
            //form.Top = this.Top + 100;
            //form.Show();
        }
        private void 选项MenuItem_Click(object sender, RoutedEventArgs e)
        {
        }
        private void 统计MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Regex r1 = new Regex(@"^", RegexOptions.Multiline);
            Regex r2 = new Regex(@"\w");
            MatchCollection c1 = r1.Matches(Text1);
            MatchCollection c2 = r2.Matches(Text1);
            if (!isUsingBox2)
            {
                string info = string.Format("总字符数：{0}\n总字数：{1}\n总行数：{2}", Text1.Length, c2.Count, c1.Count);
                MessageBox.Show(info, "字数统计");
            }
            else
            {
                MatchCollection c3 = r1.Matches(Text2);
                MatchCollection c4 = r2.Matches(Text2);
                string info = string.Format("文本框1：\n总字符数：{0}\n总字数：{1}\n总行数：{2}\n\n文本框2：\n总字符数：{3}\n总字数：{4}\n总行数：{5}\n\n",
                    Text1.Length, c2.Count, c1.Count, Text2.Length, c4.Count, c3.Count);
                MessageBox.Show(info, "字数统计");
            }
        }
        private void InitSizeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // 程序的初始位置以及高度
            int height = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height * 0.8),
                width = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width * 0.65),
                top = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height * 0.08),
                left = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width * 0.175);

            // 以下内容只是为了让数值好看些，个位数为0，仅此而已
            Height = height / 10 * 10;
            Width = width / 10 * 10;
            Top = top / 10 * 10;
            Left = left / 10 * 10;
        }
        private void InitFuncMenuItem_Click(object sender, RoutedEventArgs e)
        {
            InitializeEveryFunction();
            UpdateDisplay();
        }
        // 文本框内容更新，从而自动更新字数统计信息
        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            Regex r1 = new Regex(@"\w");
            Regex r2 = new Regex(@"^", RegexOptions.Multiline);
            StringBuilder info = new StringBuilder("字数统计(E)：");
            info.AppendFormat("{0}/{1}", r1.Matches(Text1).Count, r2.Matches(Text1).Count);
            if (isUsingBox2)
            {
                info.AppendFormat(", {0}/{1}", r1.Matches(Text2).Count, r2.Matches(Text2).Count);
            }
            wordCount.Header = info.ToString();
        }

        #endregion

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Text.RegularExpressions;

namespace TextEdit
{
    public class PairExchangeReader
    {
        public PairExchangeReader() { }
        public Pair LoadPairFromFile(string filename)
        {
            Pair p = new Pair(Path.GetFileNameWithoutExtension(filename));
            List<string> lines = GetLinesFromFile(filename);
            // 总共有两种类型，只有两行的单字符对照，以及多行的字符串对照
            if (lines.Count == 2)
            {
                // 对于只有两行的单字符对照进行读取，返回一个Pair
                int length = lines[0].Length <= lines[1].Length ? lines[0].Length : lines[1].Length;
                for (int i = 0; i < length; i++)
                {
                    p.From.Add(lines[0][i].ToString());
                    p.To.Add(lines[1][i].ToString());
                }
                p.usingRex = false;
            }
            else if (lines.Count > 2)
            {
                Regex bracketCheck = new Regex(@"^\[.*?\]$");
                Regex keyCheck = new Regex(@"(?<=^\[).+(?==)");
                Regex valueCheck = new Regex(@"(?<==).+(?=\]$)");
                string separator = "->";
                foreach (string str in lines)
                {
                    if (bracketCheck.IsMatch(str))
                    {
                        string key = keyCheck.Match(str).ToString();
                        string value = valueCheck.Match(str).ToString();
                        try
                        {
                            if (key == "UseRex") p.usingRex = Convert.ToBoolean(value);
                            else if (key == "Separator") separator = value;
                            else if (key == "Space") p.Space = CheckRegExpStyle(value);
                            else if (key == "Empty") p.Empty = CheckRegExpStyle(value);
                            else if (key == "IgnoreCase" && Convert.ToBoolean(value)) p.RegOptions |= RegexOptions.IgnoreCase;
                            else if (key == "Multiline" && Convert.ToBoolean(value)) p.RegOptions |= RegexOptions.Multiline;
                            else if (key == "Singleline" && Convert.ToBoolean(value)) p.RegOptions |= RegexOptions.Singleline;
                            else if (key == "IgnorePatternWhitespace" && Convert.ToBoolean(value)) p.RegOptions |= RegexOptions.IgnorePatternWhitespace;
                            else if (key == "ExplicitCapture" && Convert.ToBoolean(value)) p.RegOptions |= RegexOptions.ExplicitCapture;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(key + " : " + value);
                        }
                    }
                    else
                    {
                        Regex pairCheck = new Regex(separator);
                        string[] ss = pairCheck.Split(str);
                        if (ss.Length < 2) continue;
                        p.From.Add(ss[0]);
                        p.To.Add(ss[1]);
                    }
                }
                p.CheckSpaceAndEmpty();
            }
            else
            {
                // 表明外部文件有问题，内容不足两行
                return null;
            }
            return p;
        }
        // 尝试读取指定文件夹下的所有txt
        public List<Pair> LoadPairListFromFiles(string folderPath)
        {
            List<Pair> tempList = new List<Pair>();
            try
            {
                string[] filelist = Directory.GetFiles(folderPath);
                foreach (string filename in filelist)
                {
                    if (Path.GetExtension(filename).ToLower() == ".txt")
                    {
                        Pair p = LoadPairFromFile(filename);
                        if (p != null) tempList.Add(p);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("文件夹访问失败，原因：" + e.Message);
            }
            return tempList;
        }
        // 逐行读取外部txt的内容，自动无视空行
        private List<string> GetLinesFromFile(string filename)
        {
            List<string> lines = new List<string>();
            if (!File.Exists(filename))
            {
                throw new Exception("试图读取文本文件失败，路径有误。");
            }
            StreamReader r = new StreamReader(filename, Encoding.Default);
            while (!r.EndOfStream)
            {
                string temp = r.ReadLine();
                if (temp.Length < 1) continue;
                else lines.Add(temp);
            }
            r.Close();
            return lines;
        }
        // 使用正则表达式进行简易的替换操作
        private void Replace(ref string origin, string from, string to)
        {
            Regex r = new Regex(from);
            origin = r.Replace(origin, to);
        }
        public List<string> GetNames(List<Pair> list)
        {
            List<string> temp = new List<string>();
            foreach (Pair p in list)
            {
                temp.Add(p.Name);
            }
            return temp;
        }
        public Pair GetTransfromList(string name, List<Pair> pairList)
        {
            foreach (Pair p in pairList)
            {
                if (name == p.Name)
                {
                    return p;
                }
            }
            throw new Exception("错误，没有找到相应名称的自定义转换列表。");
        }
        // 用于在使用正则表达式的时候，将能够被正则表达式识别到的特殊符号变为非转义字符
        private string CheckRegExpStyle(string origin)
        {
            string list = @"\.*+?!=()[]|-{}<>'#^$";
            foreach (char s in list)
            {
                Regex r = new Regex(@"\" + s);
                origin = r.Replace(origin, @"\" + s);
            }
            return origin;
        }
    }
}

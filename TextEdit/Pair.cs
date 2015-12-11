using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TextEdit
{
    public class Pair
    {
        public List<string> From;
        public List<string> To;
        public string Name;
        public bool usingRex;
        public RegexOptions RegOptions = RegexOptions.None;
        public string Empty = @"\N";
        public string Space = @"\_";
        public Pair(string Name)
        {
            this.Name = Name;
            From = new List<string>();
            To = new List<string>();
            usingRex = false;
        }
        public Pair()
            : this(string.Empty)
        {
        }
        public string Info()
        {
            string nl = Environment.NewLine;
            StringBuilder sb = new StringBuilder(Name);
            sb.Append(nl);
            sb.Append("UseRex: ");
            sb.Append(usingRex.ToString());
            sb.Append(nl);
            if (From.Count < 20)
            {
                for (int i = 0; i < From.Count; i++)
                {
                    sb.Append(From[i]);
                    sb.Append("->");
                    sb.Append(To[i]);
                    sb.Append(nl);
                }
            }
            else
            {
                for (int i = 0; i < From.Count; i++)
                {
                    sb.Append(From[i]);
                    sb.Append("->");
                    sb.Append(To[i]);
                    sb.Append("\t");
                }
            }
            return sb.ToString();
        }
        public void CheckSpaceAndEmpty()
        {
            Regex ss = new Regex(Space);
            Regex ee = new Regex(Empty);
            for (int i = 0; i < From.Count; i++)
            {
                From[i] = ss.Replace(From[i], " ");
                From[i] = ee.Replace(From[i], "");
                To[i] = ss.Replace(To[i], " ");
                To[i] = ee.Replace(To[i], "");
            }
        }
    }
}

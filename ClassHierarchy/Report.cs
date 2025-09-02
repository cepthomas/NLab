using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Text;


namespace ClassHierarchy
{
    public class Report
    {
        public string Footer { get; private set; } = "";

        public Dictionary<string, string> ReplacementStrings { get; private set; } = new Dictionary<string, string>();

        public List<Page> Pages { get; private set; } = new List<Page>();

        public string FontName { get; private set; } = "";
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Text;


namespace ClassHierarchy
{
    public class Table : Element
    {
        public DataTable RawTable { get; set; }

        public bool IncludeHeader { get; set; } = true;

        public Dictionary<Rectangle, Color> ShadedRegions { get; set; } = [];

        public int ContentFontSize { get; set; } = 8;
    }
}

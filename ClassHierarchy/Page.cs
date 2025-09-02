
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Text;


namespace ClassHierarchy
{
    public class Page
    {
        public string Name { get; set; }

        public string Header { get; set; }

        public bool Landscape { get; set; }

        public bool GridLines { get; set; } = true;

        public int RowHeight { get; set; } = 0;

        public bool AutoFitColumns { get; set; } = true;

        public bool AutoFitRows { get; set; } = true;

        public List<Table> Tables { get; set; }

        //public List<Picture> Pictures { get; set; }

        //public List<ValueBox> ValueBoxes { get; set; }

        public List<Chart> Charts { get; set; }
    }
}

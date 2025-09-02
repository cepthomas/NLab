
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Text;


namespace ClassHierarchy
{
    public abstract class Element
    {
        public string Name { get; set; }

        public string AliasName { get; set; }

        public int Row { get; set; }

        public int Col { get; set; }

        public int Width { get; set; }

        public int ColumnPixelWidth { get; set; }

        public int Height { get; set; }

        public bool Multiline { get; set; }

        public Color FontColor { get; set; }

        public int FontSize { get; set; }

        public Color BackColor { get; set; }

        public bool PageBreak { get; set; } = false;

        public int RowHeight { get; set; } 

        public Color BorderColor { get; set; } = Color.Black;

        public bool CheckBox { get; set; } = false;

        public string CellComment { get; set; }
    }
}

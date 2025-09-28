using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Text;


namespace ClassHierarchy
{
    public class Chart : Element
    {
        public List<ChartSeries> DataSeries { get; set; } = new List<ChartSeries>();

        public string TitleText { get; set; } = Definitions.UNKNOWN_STRING;

        public string TitleFontName { get; set; } = "Arial Black";

        public string XAxisLabelText { get; set; } = Definitions.UNKNOWN_STRING;

        public string XAxisFontName { get; set; } = "Arial";

        public string YAxisLabelText { get; set; } = Definitions.UNKNOWN_STRING;

        public string YAxisFontName { get; set; } = "Arial";
    }

    public class ChartSeries
    {
        public string Name { get; set; }

        public Definitions.ChartType ChartType { get; set; }

        public Definitions.ChartFitCurve FitCurve { get; set; }

        public Definitions.ChartPointType PointType { get; set; }

        public Color Color { get; set; }

        public int NumDataRows { get; set; }

        public int FirstDataRow { get; set; }

        public int YAxisCol { get; set; }

        public string YRange { get; set; }

        public int XAxisCol { get; set; }

        public string XRange { get; set; }
    }
}

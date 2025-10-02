using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;


namespace ClassHierarchy
{
    public abstract class Element
    {
        public string Name { get; set; }

        public int Row { get; set; }

        public int Col { get; set; }

        public Color FontColor { get; set; }
    }

    public class ChartSeries
    {
        public string Name { get; set; }

        public Color Color { get; set; }

        public string YRange { get; set; }

        public string XRange { get; set; }
    }

    public class Chart : Element
    {
        public List<ChartSeries> DataSeries { get; set; } = new List<ChartSeries>();

        public string TitleText { get; set; } = Definitions.UNKNOWN_STRING;

        public string XAxisLabelText { get; set; } = Definitions.UNKNOWN_STRING;

        public string YAxisLabelText { get; set; } = Definitions.UNKNOWN_STRING;
    }

    public class Table : Element
    {
        public DataTable RawTable { get; set; }

        public bool IncludeHeader { get; set; } = true;

        public Dictionary<Rectangle, Color> ShadedRegions { get; set; } = [];
    }

    public class Report
    {
        public string Header { get; private set; } = "???";

        public Dictionary<string, string> ReplacementStrings { get; private set; } = new Dictionary<string, string>();

        public List<Page> Pages { get; private set; } = new List<Page>();
    }

    public class Page
    {
        public string Name { get; set; }

        public string Header { get; set; }

        public List<Table> Tables { get; set; }

        public List<Chart> Charts { get; set; }
    }


    public ClassHierarchy()
    {
        // Create an instance of this.

    }






    ///////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////

    public enum AnimalFamily
    {
        Felidae, // (Cats)
        Canidae, // (Dogs, Wolves, Coyotes, African Wild Dogs, etc.)
        Ursidae, // (Bears)
        Leporidae, // (Rabbits and Hares)
        Mustelidae, //  (Weasels, Badgers, Otters, etc.)
        Procyonidae, //  (Raccoons, Coatis, Olingos, etc.)
        Mephitidae, //  (Skunks, Stink Badgers)
    }

    public abstract class Animal
    {
        public AnimalFamily Family;

        public string Name;

        public Color Shade;

        public string Range;

        public List<Animal> Eats;
    }
}

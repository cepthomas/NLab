using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;


namespace ClassHierarchy
{
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

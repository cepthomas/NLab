using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
//using Ephemera.NBagOfTricks;


namespace NLab
{
    #region Types
    /// <summary>General categories, mainly for logging.</summary>
    public enum Cat { None, Error, Info }

    // /// <summary>Comm has something to tell the user.</summary>
    // public class NotifEventArgs(Cat cat, string msg) : EventArgs
    // {
    //     public Cat Cat { get; init; } = cat;
    //     public string Message { get; init; } = msg;
    // }
    #endregion
}

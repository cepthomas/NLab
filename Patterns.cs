namespace NLab
{
    public class Patterns
    {
        public void DoIt()
        {



        }


#if _SWITCH
        public static string GetInstrumentName(int which)
        {
            string ret = which switch
            {
                -1 => "NoPatch",
                >= 0 and < MAX_MIDI => _instrumentNames[which],
                _ => throw new ArgumentException(nameof(which)),
            };
            return ret;
        }

        IEnumerable<EventDesc> GetFilteredEvents(string patternName, List<int> channels, bool sortTime)
        {
            IEnumerable<EventDesc> descs = ((uint)patternName.Length, (uint)channels.Count) switch
            {
                (0, 0) => AllEvents.AsEnumerable(),
                (0, > 0) => AllEvents.Where(e => channels.Contains(e.ChannelNumber)),
                ( > 0, 0) => AllEvents.Where(e => patternName == e.PatternName),
                ( > 0, > 0) => AllEvents.Where(e => patternName == e.PatternName && channels.Contains(e.ChannelNumber))
            };

            // Always order.
            return sortTime ? descs.OrderBy(e => e.AbsoluteTime) : descs;
        }

        void SwitchAndPattern()
        {
            _ = key switch
            {
                Keys.Key_Reset => ProcessEvent(E.Reset, key),
                Keys.Key_Set => ProcessEvent(E.SetCombo, key),
                Keys.Key_Power => ProcessEvent(E.Shutdown, key),
                _ => ProcessEvent(E.DigitKeyPressed, key)
            };

            tmsec = snap switch
            {
                SnapType.Coarse => MathUtils.Clamp(tmsec, MSEC_PER_SECOND, true), // second
                SnapType.Fine => MathUtils.Clamp(tmsec, MSEC_PER_SECOND / 10, true), // tenth second
                _ => tmsec, // none
            };

            switch (e.Button, ControlPressed(), ShiftPressed())
            {
                case (MouseButtons.None, true, false): // Zoom in/out at mouse position
                    break;
                case (MouseButtons.None, false, true): // Shift left/right
                    break;
            }

            string s = ArrayType switch ???
            {
                Type it when it == typeof(int) => "",
                Type it when it == typeof(int) => "",
            };


            string Format(EventArgs e) => e switch
            {
                LogArgs le => $"Log level:{le.level} msg:{le.msg}",
                SetTempoArgs te => $"SetTempo Bpm:{te.bpm}",
            };


            switch (ArrayType)
            {
                case Type it when it == typeof(int):
                    List<string> lvals = new();
                    _elements.ForEach(f => lvals.Add(f.Value.ToString()!));
                    ls.Add($"{sindent}{tableName}(IntArray):[ {string.Join(", ", lvals)} ]");
                    break;
                case Type dt when dt == typeof(double):
                    stype = "DoubleArray";
                    break;
                case Type ds when ds == typeof(string):
                    stype = "StringArray";
                    break;
                default:
                    stype = "Dictionary";
                    break;
            }
#endif
}
}


    // //You should create a struct that overrides the implicit conversion operator:

    // struct PackedValue
    // {
    //     private ushort _value;

    //     private PackedValue(ushort val)
    //     {
    //          if(val >= (1<<12)) throw new ArgumentException("val");
    //          this._value = val;
    //     }

    //     public static explicit operator PackedValue(ushort value)
    //     {
    //         return new PackedValue(value);
    //     }

    //     public static implicit operator ushort(PackedValue me)
    //     {
    //         return me._value;
    //     }
    // }



    // // Licensed to the .NET Foundation under one or more agreements.
    // // The .NET Foundation licenses this file to you under the MIT license.

    // using System.Runtime.CompilerServices;

    // namespace System
    // {
    //     public partial class Object
    //     {
    //         // Returns a Type object which represent this object instance.
    //         [Intrinsic]
    //         [MethodImpl(MethodImplOptions.InternalCall)]
    //         public extern Type GetType();

    //         // Returns a new object instance that is a memberwise copy of this
    //         // object.  This is always a shallow copy of the instance. The method is protected
    //         // so that other object may only call this method on themselves.  It is intended to
    //         // support the ICloneable interface.
    //         [Intrinsic]
    //         protected unsafe object MemberwiseClone()
    //         {
    //             return clone;
    //         }
    //     }
    // }


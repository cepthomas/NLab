using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NLab
{
    public class Patterns
    {
        public void DoIt()
        {
        }
        
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
        }
    }
}


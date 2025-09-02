
namespace NLab
{
    public class LinqPlay
    {


#if _SWITCH

        public void KillAll()
        {
            //var allOrderIds = _outputs.SelectMany(op => op.ChannelState);

           //TODO? linq-y _outputs.SelectMany(op => op.ChannelState).ForEach(ch => op.Send(new ControlChangeEvent(0, ch + 1, MidiController.AllNotesOff, 0))); ;

            _outputs.ForEach(op =>
            {
                Enumerable.Range(0, Common.NUM_MIDI_CHANNELS).ForEach(ch => op.Send(new ControlChangeEvent(0, ch + 1, MidiController.AllNotesOff, 0)));
            });

            // Hard reset.
            State.Instance.ExecState = ExecState.Idle;
        }

        void PopulateFileMenu()
        {
            List<string> options = [];
            options.Add("Open...");
            if (UserSettings.Current.RecentFiles.Count > 0)
            {
                options.Add("");
                UserSettings.Current.RecentFiles.ForEach(options.Add);
            }

            ddbtnFile.SetOptions(options);
        }

        public void SetLuaPath(List<string> paths)
        {
            List<string> parts = ["?", "?.lua"];
            paths.ForEach(p => parts.Add(Path.Join(p, "?.lua").Replace('\\', '/')));
            string s = string.Join(';', parts);
            s = $"package.path = \"{s}\"";
            DoString(s);
        }

        bool anySolo = _channelControls.Where(c => c.State == PlayState.Solo).Any();

        var qry = from ca in CommandArguments where ca.Name == argName select ca;

        public static List<int> Levels(this List<RepDataPoint> data)
        {
            List<int> levels = (from cdp in data select cdp.Level).Distinct().ToList();
            levels.Remove(0);
            levels.Sort();
            return levels;
        }

        /// <summary>
        /// Get all responses in data set by level.
        /// </summary>
        public static List<double> ResponseValues(this List<RepDataPoint> data, bool average, int level)
        {
            List<double> resps = (from cdp in data where (true) && cdp.Level == level select cdp.Response).ToList();

            if (average)
            {
                double avg = resps.Average();
                resps = new List<double>() { avg };
            }

            return resps;
        }

        /// <summary>
        /// Get all responses in data set by concentration.
        /// </summary>
        public static List<double> ResponseValues(this List<RepDataPoint> data, bool average, double conc = double.MinValue)
        {
            List<double> resps = new List<double>();

            if (conc == double.MinValue)
            {
                resps = (from cdp in data where (true) select cdp.Response).ToList();
            }
            else
            {
                resps = (from cdp in data where (true) && cdp.Concentration == conc select cdp.Response).ToList();
            }

            if (average)
            {
                double avg = resps.Average();
                resps = new List<double>() { avg };
            }

            return resps;
        }

        public void RemoveCursor(int id)
        {
            var qry = from c in _cursors where c.Id == id select c;

            if (qry.Any()) // found it
            {
                _cursors.Remove(qry.First());
            }
        }

        public void MoveCursor(int id, double position)
        {
            var qry = from c in _cursors where c.Id == id select c;

            if (qry.Any()) // found it
            {
                qry.First().Position = position;
            }
        }

        public void DoIt()
        {
            List<SlideCalcDetailRecord> ungroupedSlideCalcRecords = 
               (from row in unsortedSlideCalcRecords
                orderby row.AnalyzerTestNumber, row.Day, row.ChemCode, row.Run
                select row).ToList();


            List<TestDesignFluidRecord> fluidRecords = _adTestDesign.TestDesignFluidRecords.FindAll(f =>
                    (f.FLUID_ID_TD == runSheetRec.FluidId) &&
                    (f.VIAL_TD == runSheetRec.Vial) &&
                    (f.LEVEL_TD == runSheetRec.Level) &&
                    (f.TRAY_LOCATION_TD == runSheetRec.TrayLocation) &&
                    (f.TRAY_TD == runSheetRec.Tray) &&
                    (f.DAY_TD == runSheetRec.Day) &&
                    (f.CUP_TD == runSheetRec.Cup)).ToList();

            // Get the tray linkage for this analyzer, day, run row.
            List<TrayLinkage> thisTrayLinkage =
                (from trayLink in _adTestDesign.TrayLinkages
                 where trayLink.AnalyzerTestNumber == analyzerTestNumber
                 where trayLink.Day == day
                 where trayLink.Run == run
                 select trayLink).ToList();

            // Query the runsheet details for the test design trays that were setup during test design
            List<RunSheetDetailRecord> runSheetDetailRecords =
               (from runsheet in _adTestDesign.RunSheetDetailRecords
                where runsheet.AnalyzerTestNumber == analyzerTestNumber
                where runsheet.Day == day
                where runsheet.Run == run
                select runsheet).DistinctBy(x => x.Tray).ToList();
        }

#endif
    }
}

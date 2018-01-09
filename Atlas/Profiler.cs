using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Atlas
{
    class Profiler
    {
        class ProfileItem
        {
            private string _id;
            private uint _numCalls;
            private double _totalSeconds, _maximumSeconds, _minimumSeconds;
            private long _startTime;

            public ProfileItem(string id)
            {
                _id = id;
                _numCalls = 0;
                _totalSeconds = 0.0;
                _maximumSeconds = double.NegativeInfinity;
                _minimumSeconds = double.PositiveInfinity;
            }

            public void Start()
            {
                _numCalls++;
                _startTime = Stopwatch.GetTimestamp();
            }

            public void End()
            {
                long time = Stopwatch.GetTimestamp() - _startTime;
                double timespan = (double)time / (double)Stopwatch.Frequency;
                _maximumSeconds = Math.Max(_maximumSeconds, timespan);
                _minimumSeconds = Math.Min(_minimumSeconds, timespan);
                _totalSeconds += timespan;
            }

            public string ID { get { return _id; } }
            public uint NumCalls { get { return _numCalls; } }
            public double TotalSeconds { get { return _totalSeconds; } }
            public double MaximumSeconds { get { return _maximumSeconds; } }
            public double MinimumSeconds { get { return _minimumSeconds; } }
        }

        private Dictionary<string, ProfileItem> _profiles;
        private bool _isProfiling;
        private long _profileStopTime;

        static readonly Profiler _instance = new Profiler();
        public static Profiler Instance { get { return _instance; } }

        private Profiler()
        {
            _profiles = new Dictionary<string, ProfileItem>();
            _isProfiling = false;
        } 

        public void StartProfiling(double num_seconds)
        {
            _isProfiling = true;
            _profiles = new Dictionary<string, ProfileItem>();
            //Console.WriteLine("Starting profile...");
            HUD.Instance.AddMessage("Starting profile\n");
            _profileStopTime = Stopwatch.GetTimestamp() + (long)(num_seconds * (double)Stopwatch.Frequency);
            EnterSection("TOTAL PROFILE");
        }

        public void EnterSection(string ID)
        {
            if (!_isProfiling) return;
            if (Stopwatch.GetTimestamp() >= _profileStopTime)
            {
                EndProfiling();
            }
            if (!_profiles.ContainsKey(ID)) _profiles[ID] = new ProfileItem(ID);
            _profiles[ID].Start();
        }

        public void ExitSection(string ID)
        {
            if (!_isProfiling) return;
            if (Stopwatch.GetTimestamp() >= _profileStopTime)
            {
                EndProfiling();
            }
            if (!_profiles.ContainsKey(ID)) return;
            _profiles[ID].End();
        }

        public void EndProfiling()
        {
            if (!_isProfiling) return;
            _isProfiling = false;
            _profiles["TOTAL PROFILE"].End();
            //Console.WriteLine("Finished profile...");
            HUD.Instance.AddMessage("Finished profile\n");
            
            //turn to list
            List<ProfileItem> list = new List<ProfileItem>(_profiles.Keys.Count);
            foreach (ProfileItem item in _profiles.Values)
            {
                list.Add(item);
            }

            //order by total time
            list.Sort(CompareProfileItems);

            WriteToFile(list);
            WriteToTrace(list);
        }

        private void WriteToFile(List<ProfileItem> list)
        {
            StreamWriter writer = new StreamWriter("profile.txt", false);
            //print out info
            writer.WriteLine("{0,-30} | {1,-15} | {2,-10} | {3,-17} | {4,-15} | {5,-15}",
                "Name", "Total time (ms)", "Num Calls", "Average Time (ms)", "Max Time (ms)", "Min Time (ms)");
            writer.WriteLine("-------------------------------+-----------------+------------+-------------------+-----------------+--------------");
            for (int i = 0; i < list.Count; i++)
            {
                ProfileItem item = list[i];
                writer.WriteLine("{0,-30} | {1,-15:F} | {2,-10} | {3,-17:F} | {4,-15:F} | {5,-15:F}",
                    item.ID,
                    item.TotalSeconds * 1000.0,
                    item.NumCalls,
                    item.TotalSeconds / (double)item.NumCalls * 1000.0,
                    item.MaximumSeconds * 1000.0,
                    item.MinimumSeconds * 1000.0);
            }
            writer.Close();
        }

        private void WriteToTrace(List<ProfileItem> list)
        {
            String line = String.Format("{0,-30} | {1,-15} | {2,-10} | {3,-17} | {4,-15} | {5,-15}",
                "Name", "Total time (ms)", "Num Calls", "Average Time (ms)", "Max Time (ms)", "Min Time (ms)");
            Trace.WriteLine(line);
            Trace.WriteLine("-------------------------------+-----------------+------------+-------------------+-----------------+--------------");
            for (int i = 0; i < list.Count; i++)
            {
                ProfileItem item = list[i];
                line = String.Format("{0,-30} | {1,-15:F} | {2,-10} | {3,-17:F} | {4,-15:F} | {5,-15:F}",
                    item.ID,
                    item.TotalSeconds * 1000.0,
                    item.NumCalls,
                    item.TotalSeconds / (double)item.NumCalls * 1000.0,
                    item.MaximumSeconds * 1000.0,
                    item.MinimumSeconds * 1000.0);
                Trace.WriteLine(line);
            }
            Trace.Flush();
        }

        private static int CompareProfileItems(ProfileItem x, ProfileItem y)
        {
            double diff = y.TotalSeconds - x.TotalSeconds;
            return Math.Sign(diff);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.IO;

namespace Atlas
{
    class Statistics
    {
        private const int INTERVAL = 1000;
        private const String scoreFile = "Config/stats";
        private int _time;

        private MissionStats _currentMission;

        static readonly Statistics _instance = new Statistics();

        public static Statistics Instance
        {
            get { return _instance; }
        }

        public Statistics()
        {
        }

        public void Initialize()
        {
            _currentMission = new MissionStats();
            _time = INTERVAL;
        }

        public void ComboAt(float mult)
        {
            _currentMission.MaxCombo = mult;
        }

        public void TableClearAt()
        {
            _currentMission.TableClears = 1;
        }

        public void BombAt()
        {
            _currentMission.Bombs = 1;
        }

        public void RemainingPiecesAt(int numPieces)
        {
            _currentMission.NumEndPcs = numPieces;
        }

        public void LiveLostAt()
        {
            _currentMission.LostLives = 1;
        }

        public void Update(GameTime gt)
        {
            _time -= (int)((float)gt.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);
            int levelId = ResourceMgr.Instance.Game.LEVEL.ID;
            int missionId = ResourceMgr.Instance.Game.LEVEL.ACTIVEMISSION.ID;
            _currentMission.Duration = (int)((float)gt.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);
            if (_time <= 0)
            {
                _time = INTERVAL;
                _currentMission.AddDangerPerc(ResourceMgr.Instance.Game.Board.CurrentHeightPercentage);
                _currentMission.AddPoints(Player.Instance.CurrentMissionPoints);
            }
        }

        public void DeleteFileContents()
        {
            try
            {
                StreamWriter writer = new StreamWriter(scoreFile, false);
            }
            catch (Exception ex)
            {
            }
        }

        public void AppendToFile(int lId, int mId)
        {
            try
            {
                StreamWriter writer = new StreamWriter(scoreFile, true);
                writer.WriteLine("Level: " + lId + ", Mission: " + mId);
                writer.WriteLine(_currentMission.PrintStaticStats());
                writer.WriteLine(_currentMission.PrintDistanceList());
                writer.WriteLine(_currentMission.PrintPointList());
                writer.Close();
                Initialize();
            }
            catch (Exception ex)
            {
            }
        }
    }

    class MissionStats
    {
        int _lostLives, _numEndPcs, _tableClrs, _bombs, _duration;
        float _maxCombo;
        List<float> _dangerPercentage;
        List<int> _points;

        public MissionStats()
        {
            _duration = 0;
            _lostLives = 0;
            _numEndPcs = 0;
            _tableClrs = 0;
            _bombs = 0;
            _maxCombo = 0;
            _dangerPercentage = new List<float>();
            _points = new List<int>();
        }

        public void AddDangerPerc(float percentage)
        {
            _dangerPercentage.Add(percentage);
        }

        public void AddPoints(int points)
        {
            _points.Add(points);
        }

        public int Duration
        {
            get { return _duration; }
            set { _duration += value; }
        }

        public int LostLives
        {
            get { return _lostLives; }
            set { _lostLives += value; }
        }

        public int NumEndPcs
        {
            get { return _numEndPcs; }
            set { _numEndPcs += value; }
        }

        public int TableClears
        {
            get { return _tableClrs; }
            set { _tableClrs += value; }
        }
        public int Bombs
        {
            get { return _bombs; }
            set { _bombs += value; }
        }
        public float MaxCombo
        {
            get { return _maxCombo; }
            set { _maxCombo = MathHelper.Max(value, _maxCombo); }
        }

        public string PrintDistanceList()
        {
            string res = "";
            foreach (float f in _dangerPercentage)
                res += f.ToString("0.00") + " ";
            return res;
        }

        public string PrintPointList()
        {
            string res = "";
            foreach (int i in _points)
                res += i + " ";
            return res;
        }

        public string PrintStaticStats()
        {
            return "Duration: " + _duration + " Lost Lives: " + _lostLives + " NumEndPcs: " + _numEndPcs + " Table Clears: " + _tableClrs + " Bombs: " + _bombs + " MaxCombo: " + _maxCombo;
        }
    }
}

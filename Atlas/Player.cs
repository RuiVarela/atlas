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

namespace Atlas
{
    class Player
    {
        private int _globalPoints;
        private int _currentMissionPoints;

        static readonly Player _instance = new Player();

        public static Player Instance
        {
            get { return _instance; }
        }

        public Player()
        {
            _globalPoints = 0;
            _currentMissionPoints = 0;
        }

        public void IncreasePoints(int val)
        {
            _currentMissionPoints += val;
            _globalPoints += val;
        }

        public void DecreasePoints(int val)
        {
            _currentMissionPoints -= val;
            if (_currentMissionPoints < 0) _currentMissionPoints = 0;
            _globalPoints -= val;
            if (_globalPoints < 0) _globalPoints = 0;
        }

        public int GlobalPoints
        {
            get { return _globalPoints; }
            set { _globalPoints = value; }
        }

        public int CurrentMissionPoints
        {
            get { return _currentMissionPoints; }
            set { _currentMissionPoints = value; }
        }
    }
}

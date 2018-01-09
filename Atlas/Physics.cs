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
    class Physics
    {
        private float _currentSpeed, _baseSpeed, _fastSpeed, _maxSpeed;
        private bool _sUp;

        public Physics(float baseSpeed, float maxSpeed, float fastSpeed)
        {
            _baseSpeed = baseSpeed;
            _fastSpeed = fastSpeed;
            _currentSpeed = baseSpeed;
            _maxSpeed = maxSpeed;
            _sUp = false;
        }

        public void Restart()
        {
            _currentSpeed = _baseSpeed;
            _sUp = false;
        }

        public void SpeedUp()
        {
            _sUp = true;
            //_currentSpeed = _fastSpeed;//.06f;
        }

        public void NormalSpeed()
        {
            _sUp = false;
            //_currentSpeed = _baseSpeed;//.01f;
        }

        public void IncrementSpeed()
        {
            _currentSpeed += 0.00005f;
            if (_currentSpeed > _maxSpeed)
            {
                _currentSpeed = _maxSpeed;
                //Console.WriteLine("Max Speed attained");
            }
        }

        public int Apply(Piece p, GameTime gt)
        {
            if (p.State == PieceState.STABLE) return 0;
            float h;
            if (_sUp)
                h = p.PositionY - _fastSpeed * ((float)gt.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);
            else
                h = p.PositionY - _currentSpeed * ((float)gt.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);
            int y = p.IndexPositionY;
            if (h > y * Piece.EdgeSize)
            {
                p.PositionY = h;
                return 0;
            }
            if (h <= y * Piece.EdgeSize)
            {
                p.PositionY = y * Piece.EdgeSize;
                p.State = PieceState.STABLE;
                return 1;
            }
            return 0;
        }
    }
}

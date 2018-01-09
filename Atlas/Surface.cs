using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Atlas
{
    class Surface
    {
        protected float _delta;//variaçao de altura desde o zero;
        protected float _heightOffset;
        protected float _heightOffsetTarget;
        protected float _height;
        protected const float SPEED = .0005f;

        public Surface(float initialHeight)
        {
            _height = initialHeight;
            _delta = Math.Abs(initialHeight) - 1;
            _heightOffset = 0;
            _heightOffsetTarget = 0;
        }

        public virtual void Initialize() { }

        public virtual void LoadContent() { }

        public virtual void Restart()
        {
            _heightOffset = 0f;

            //reset vertices to Y = _height
        }

        public void HeightPercentage(float amount)
        {
            _heightOffsetTarget = _delta * amount;
        }

        public float HeightOffset
        {
            get { return _heightOffset; }
        }

        public float Height
        {
            get { return _height + _heightOffset; }
        }

        public virtual BoundingBox GetBoundingBox() { return new BoundingBox(Vector3.Zero, Vector3.Zero); }

        public virtual void Update(GameTime gameTime)
        {
            if (_heightOffset - SPEED > _heightOffsetTarget) _heightOffset -= SPEED * ((float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond) * 2;//lava "desce" 2x mais rapido do k sobe
            if (_heightOffset + SPEED < _heightOffsetTarget) _heightOffset += SPEED * ((float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);

            //do other stuff
        }

        public virtual void DrawOpaque(Matrix view, Matrix projection) { }
        public virtual void DrawTransparent(Matrix view, Matrix projection) { }
    }
}

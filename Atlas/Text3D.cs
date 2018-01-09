using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Atlas
{
    //small manager class
    class Text3DManager
    {
        static readonly Text3DManager _instance = new Text3DManager();
        List<Text3D> _textList;

        public static Text3DManager Instance { get { return _instance; } }

        private Text3DManager()
        {
            _textList = new List<Text3D>();
        }

        public void AddText3D(Text3D text3D) { _textList.Add(text3D); }
        public void ClearAllTexts() { _textList.Clear(); }

        public void Draw(Matrix view, Matrix projection, SpriteBatch sb)
        {
            if (_textList.Count == 0) return;
            sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            foreach (Text3D t in _textList) t.Draw(view, projection, sb);
            sb.End();
        }

        public void Update(GameTime gameTime)
        {
            //update texts in list
            foreach (Text3D t in _textList) t.Update(gameTime);

            //remove expired texts
            List<Text3D> toRemove = new List<Text3D>();
            foreach (Text3D t in _textList)
                if (t.TimeLeftToDisplay < 0.0f) toRemove.Add(t);
            foreach (Text3D t in toRemove) _textList.Remove(t);
        }
    }

    public class Text3D //texto genérico, fazer subclasses para comportamento específico
    {
        #region Fields

        protected String _text;
        protected Vector3 _position;
        protected float _timeLeftToDisplay;
        protected bool _fitInWindow;
        protected Vector2 _textHalfSize, _textSize;
        protected SpriteFont _font;
        protected Color _color;
        protected float _scale;

        #endregion
        #region Properties

        public float TimeLeftToDisplay
        {
            get { return _timeLeftToDisplay; }
        }

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public String Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public bool FitInWindow
        {
            get { return _fitInWindow; }
            set { _fitInWindow = value; }
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        #endregion

        public Text3D(String text, Vector3 position, float timeToDisplay, bool fitInWindow, Color color, float scale, SpriteFont font)
        {
            _text = text;
            _position = position;
            _timeLeftToDisplay = timeToDisplay;
            _fitInWindow = fitInWindow;
            _color = color;
            _font = font;
            _scale = scale;
            _textSize = _font.MeasureString(_text) * _scale;
            _textHalfSize = _textSize * 0.5f;
        }

        public virtual void Draw(Matrix view, Matrix projection, SpriteBatch sb)
        {
            if (_timeLeftToDisplay < 0.0f) return;
            Vector3 projected = ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Project(_position, projection, view, Matrix.Identity);
            Vector2 textPosition = new Vector2(projected.X, projected.Y) - _textHalfSize;
            Vector2 viewportLimits = new Vector2(ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height);
            if (_fitInWindow)
            {
                //align with left side of screen
                if (textPosition.X < 0.0f) textPosition.X = 0.0f;

                //align with top of screen
                if (textPosition.Y < 0.0f) textPosition.Y = 0.0f;

                //align with right side of screen
                if (textPosition.X > viewportLimits.X - _textSize.X) textPosition.X = viewportLimits.X - _textSize.X;

                //align with bottom of screen
                if (textPosition.Y > viewportLimits.Y - _textSize.Y) textPosition.Y = viewportLimits.Y - _textSize.Y;
            }
            sb.DrawString(_font, _text, textPosition, _color, 0.0f, Vector2.Zero, _scale, SpriteEffects.None, 0.0f);
        }

        public virtual void Update(GameTime gameTime)
        {
            //remove time to display
            _timeLeftToDisplay -= (float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerSecond;
        }
    }

    public class FloatUpText3D : Text3D //um exemplo de subclasse
    {
        protected float _upSpeed;

        public FloatUpText3D(String text, Vector3 position, float timeToDisplay, bool fitInWindow, Color color, float scale, SpriteFont font,
            float upSpeed)
            : base(text, position, timeToDisplay, fitInWindow, color, scale, font)
        {
            _upSpeed = upSpeed;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Vector3 newPosition = Position + Vector3.UnitY * _upSpeed * ((float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerSecond);
            Position = newPosition;
        }
    }
}

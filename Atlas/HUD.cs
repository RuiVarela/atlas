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
    class HUD
    {
        SpriteFont _font;
        Texture2D _weightMeter, _score, _level, _mission, _goal, _life, _viewport, _goalBar, _heart, _help;
        Rectangle _weightR, _scoreR, _levelR, _missionR, _goalR, _lifeR, _viewportR, _heartR, _allR;
        Rectangle _goalBarR, _goalPercR;
        Texture2D _cursor, _white;
        Rectangle _bar, _futureBar;

        int SCALE = 8;

        float _weightPercentage, _futureWeightPercentage;
        float _alertDelay;
        bool _alertState, _alertOn;

        List<string> _combos;
        float _comboTimer;
        const int COMBO_STRING_DELAY = 3000;
        const int FRAMES_SKIP_TIME = 500;

        static readonly HUD _instance = new HUD();

        public static HUD Instance
        {
            get { return _instance; }
        }

        public void Initialize()
        {
            _combos = new List<string>();
            _alertDelay = FRAMES_SKIP_TIME;
            _alertState = false;
            int vHeight = ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height;
            int vWidth = ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width;

            _font = ResourceMgr.Instance.Game.Content.Load<SpriteFont>("gameFont");
            _weightMeter = ResourceMgr.Instance.Game.Content.Load<Texture2D>("weightmeter");
            _cursor = ResourceMgr.Instance.Game.Content.Load<Texture2D>("cursor");
            _score = ResourceMgr.Instance.Game.Content.Load<Texture2D>("score");
            _level = ResourceMgr.Instance.Game.Content.Load<Texture2D>("level");
            _mission = ResourceMgr.Instance.Game.Content.Load<Texture2D>("mission");
            _goal = ResourceMgr.Instance.Game.Content.Load<Texture2D>("goal");
            _life = ResourceMgr.Instance.Game.Content.Load<Texture2D>("life");
            _viewport = ResourceMgr.Instance.Game.Content.Load<Texture2D>("viewport");
            _goalBar = ResourceMgr.Instance.Game.Content.Load<Texture2D>("goal_indicator");
            _heart = ResourceMgr.Instance.Game.Content.Load<Texture2D>("heart");
            _help = ResourceMgr.Instance.Game.Content.Load<Texture2D>("help");
            _white = ResourceMgr.Instance.Game.Content.Load<Texture2D>("square");

            _scoreR = new Rectangle(15, vHeight - vHeight / SCALE, vHeight * 3 / SCALE, vHeight / SCALE);
            _levelR = new Rectangle(15, 0, vHeight * 3 / SCALE, vHeight / SCALE);
            _missionR = new Rectangle(15, _levelR.Height, vHeight * 3 / SCALE, vHeight / SCALE);
            _goalR = new Rectangle(15, _levelR.Height + _missionR.Height, vHeight * 3 / SCALE, vHeight / SCALE);
            _lifeR = new Rectangle(15, _levelR.Height + _missionR.Height + _goalR.Height, vHeight * 3 / SCALE, vHeight / SCALE);
            int offset = (int)((_goalR.Right - _goalR.Left) / 2.3f);
            _goalBarR = new Rectangle(offset, (int)(_goalR.Top + vHeight / 22.5f), _goalR.Right - offset, _goalBar.Height);
            _goalPercR = new Rectangle(offset + vHeight / 45, _goalR.Top + vHeight / 18, (int)(_goalR.Right - offset - vHeight / 25.7f), _goalBar.Height - vHeight / 36);
            _heartR = new Rectangle(offset, _levelR.Height + _missionR.Height + _goalR.Height + vHeight / 36, vHeight / 2 / SCALE, vHeight / 2 / SCALE);
            _viewportR = new Rectangle(vWidth - vHeight * 2 / SCALE - 5, 5, vHeight * 2 / SCALE, vHeight * 2 / SCALE);
            _weightR = new Rectangle(vWidth - vHeight / SCALE - 10, _viewportR.Bottom + vHeight / 30, vHeight / SCALE, vHeight - _viewportR.Bottom - vHeight / 15);
            _allR = new Rectangle(0, 0, vWidth, vHeight);

            _bar = new Rectangle(_weightR.Center.X - vWidth / 30, _weightR.Top + 10, vWidth / 15, vHeight / 30);
            _futureBar = new Rectangle(_weightR.Center.X - vWidth / 22, _weightR.Top + 10 - 4, vWidth / 11, (int)(vHeight / 22.5f));
        }

        public void AddComboBonus(int extraPoints)
        {
            _combos.Add("Bonus: +" + extraPoints + " points\n");
        }

        public void AddTableClearBonus(int bonusPoints)
        {
            _combos.Add("Table Clear Bonus:\n+" + bonusPoints + " points\n");
        }

        public void AddBombBonus()
        {
            _combos.Add("Bomb awarded!\n");
        }

        public void AddMessage(string message)
        {
            _combos.Add(message);
        }

        public void RemoveAllMessages()
        {
            _combos.Clear();
        }

        public float WeightPercentage
        {
            set { _weightPercentage = value; }
            get { return _weightPercentage; }
        }

        public float FutureWeightPercentage
        {
            set { _futureWeightPercentage = value; }
            get { return _futureWeightPercentage; }
        }

        public void Update(GameTime gameTime)
        {
            int vHeight = ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height;
            if (ResourceMgr.Instance.Game.DangerPercentage > 0)
            {
                _alertState = true;
                _alertDelay -= ((float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond) * _futureWeightPercentage * 3;
                if (_alertDelay < 0)
                {
                    _alertOn = !_alertOn;
                    _alertDelay = FRAMES_SKIP_TIME;
                }
            }
            else
                _alertState = false;

            _bar.Y = (int)(_weightR.Top + 10 + (_weightR.Bottom - vHeight / 20 - _weightR.Top) * _weightPercentage);
            _futureBar.Y = (int)(_weightR.Top + 5 + (_weightR.Bottom - vHeight / 20 - _weightR.Top) * _futureWeightPercentage);

            _goalPercR.Width = (int)((_goalBarR.Right - _goalBarR.Left - vHeight / 25.7f) * ResourceMgr.Instance.Game.LEVEL.ACTIVEMISSION.GoalPercentage);

            if (_comboTimer - ((float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond) > 0) _comboTimer -= ((float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);
            else if (_comboTimer != 0 && _combos.Count > 0)
            {
                _comboTimer = 0;
                _combos.RemoveAt(0);
            }
            if (_combos.Count != 0 && _comboTimer == 0)
            {
                _comboTimer = COMBO_STRING_DELAY;
            }
        }

        public void DrawMissionEnd(SpriteBatch sb)
        {
            int vHeight = ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height;
            sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            sb.Draw(_score, _scoreR, Color.White);
            sb.DrawString(_font, Player.Instance.GlobalPoints.ToString(), new Vector2(_scoreR.Center.X, _scoreR.Top + vHeight / 25.7f), Color.White, 0, Vector2.Zero, vHeight / 900f, SpriteEffects.None, 0);

            Vector2 size = _font.MeasureString("You achieved the goal!") * (vHeight / 900f) * 0.5f;
            Vector2 position = new Vector2(ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width / 2 - size.X, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height / 5f);
            sb.DrawString(_font, "You achieved the goal!", position, Color.White, 0, Vector2.Zero, vHeight / 900f, SpriteEffects.None, 0);

            if (ResourceMgr.Instance.Game.LEVEL.ACTIVEMISSION.BoardEmpty)
            {
                size = _font.MeasureString("Press SPACE to continue...") * (vHeight / 900f) * 0.5f;
                position = new Vector2(ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width / 2 - size.X, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height / 5f + size.Y * 3);
                sb.DrawString(_font, "Press SPACE to continue...", position, Color.GreenYellow, 0, Vector2.Zero, vHeight / 900f, SpriteEffects.None, 0);
            }
            else
            {
                size = _font.MeasureString("You are being penalized...") * (vHeight / 900f) * 0.5f;
                position = new Vector2(ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width / 2 - size.X, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height / 5f + size.Y * 3);
                sb.DrawString(_font, "You are being penalized...", position, Color.Red, 0, Vector2.Zero, vHeight / 900f, SpriteEffects.None, 0);
            }
            sb.End();
        }

        public void DrawHelp(SpriteBatch sb)
        {
            sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            sb.Draw(_help, _allR, Color.White);
            sb.End();
        }

        public void Draw(SpriteBatch sb)
        {
            int vHeight = ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height;
            int vidas = ResourceMgr.Instance.Game.Lives;
            sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            sb.Draw(_score, _scoreR, Color.White);
            sb.Draw(_level, _levelR, Color.White);
            sb.Draw(_mission, _missionR, Color.White);
            sb.Draw(_goal, _goalR, Color.White);
            sb.Draw(_life, _lifeR, Color.White);
            sb.Draw(_goalBar, _goalBarR, Color.White);
            sb.Draw(_white, _goalPercR, Color.GreenYellow);

            int bkup = _heartR.X;
            for (int i = 0; i < vidas; i++)
            {
                _heartR.X += _heartR.Width * i;
                sb.Draw(_heart, _heartR, Color.White);
                _heartR.X = bkup;
            }

            sb.Draw(_viewport, _viewportR, Color.White);

            sb.DrawString(_font, Player.Instance.GlobalPoints.ToString(), new Vector2(_scoreR.Center.X, _scoreR.Top + vHeight / 25.7f), Color.White, 0, Vector2.Zero, vHeight / 900f, SpriteEffects.None, 0);
            sb.DrawString(_font, ResourceMgr.Instance.Game.LEVEL.ID.ToString(), new Vector2((_levelR.Right - _levelR.Left) * (3f / 4f), _levelR.Top + vHeight / 25.7f), Color.White, 0, Vector2.Zero, vHeight / 900f, SpriteEffects.None, 0);
            sb.DrawString(_font, ResourceMgr.Instance.Game.LEVEL.ACTIVEMISSION.ID.ToString(), new Vector2((_missionR.Right - _missionR.Left) * (3f / 4f), _missionR.Top + vHeight / 25.7f), Color.White, 0, Vector2.Zero, vHeight / 900f, SpriteEffects.None, 0);

            float goalPercentage = ResourceMgr.Instance.Game.LEVEL.ACTIVEMISSION.GoalPercentage;

            if (_comboTimer > 0 && _combos.Count == 1)
            {
                Vector2 size = _font.MeasureString(_combos.First<string>()) * (vHeight / 900f) * 0.5f;
                Vector2 position = new Vector2(ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width / 2 - size.X, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height / 9f);
                sb.DrawString(_font, _combos.First<string>(), position, Color.Gold, 0, Vector2.Zero, vHeight / 900f, SpriteEffects.None, 0);
            }
            if (_comboTimer > 0 && _combos.Count > 1)
            {
                string st = "";
                foreach (string s in _combos)
                {
                    st += s;
                }
                Vector2 size = _font.MeasureString(st) * (vHeight / 900f) * 0.5f;
                Vector2 position = new Vector2(ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width / 2 - size.X, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height / 9f);
                sb.DrawString(_font, st, position, Color.Gold, 0, Vector2.Zero, vHeight / 900f, SpriteEffects.None, 0);
            }
            if ((_alertState && _alertOn) || !_alertState)
            {
                sb.Draw(_weightMeter, _weightR, Color.White);
                sb.Draw(_cursor, _futureBar, Color.Gray);//BARRA PESO
                sb.Draw(_cursor, _bar, Color.White);//BARRA PESO
            }
            sb.End();
        }
    }
}

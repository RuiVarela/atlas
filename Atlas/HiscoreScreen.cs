using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Atlas
{
    class HiscoreScreen
    {
        private SpriteFont _font;
        private const int NAME_MAX_CHARS = 12;
        private int[] _currentName = new int[NAME_MAX_CHARS];
        private int _index;
        private bool _enteringName;
        private bool _drawUnderline = false;
        private float _timeToChangeUnderline;
        private const float _underlineBlinkSpeed = 0.5f; //seconds per toggle

        private char[] _possibleChars = {
            'A', 'B', 'C', 'D', 'E', 'F',
            'G', 'H', 'I', 'J', 'K', 'L',
            'M', 'N', 'O', 'P', 'Q', 'R',
            'S', 'T', 'U', 'V', 'W', 'X',
            'Y', 'Z', ' ', '0', '1', '2', '3',
            '4', '5', '6', '7', '8', '9'
        };

        public void Initialize()
        {
            Reset();
        }

        public void LoadContent()
        {
            _font = ResourceMgr.Instance.Game.Content.Load<SpriteFont>("ScoreFont");
        }

        public void StartEnteringName() { _enteringName = true; }
        public bool IsEnteringName() { return _enteringName; }

        public void Reset()
        {
            for (int i = 0; i < NAME_MAX_CHARS; i++) _currentName[i] = -1;
            _index = 0;
            _enteringName = false;
            _drawUnderline = false;
            _timeToChangeUnderline = _underlineBlinkSpeed;
        }

        public void Update(GameTime gameTime)
        {
            if (!_enteringName) return;
            _timeToChangeUnderline -= ((float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond) / 1000.0f;
            if (_timeToChangeUnderline < 0.0f)
            {
                _drawUnderline = !_drawUnderline;
                while (_timeToChangeUnderline < 0.0f) _timeToChangeUnderline += _underlineBlinkSpeed;
            }
        }

        //convem ser chamado dentro de um draw pk foram tirados os begin e end
        public void Draw(SpriteBatch sb)
        {
            String lineUnder = "";
            for (int i = 0; i < _index; i++) lineUnder += " ";
            if (_index < NAME_MAX_CHARS) lineUnder += "_";
            String text = "";
            if (_enteringName)
            {
                text = "";
                for (int i = 0; i < NAME_MAX_CHARS; i++)
                {
                    if (_currentName[i] < 0) continue;
                    text += _possibleChars[_currentName[i]];
                }
            }
            Vector2 size = _font.MeasureString(text) * 0.5f;
            Vector2 position = new Vector2(ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width / 2 - size.X, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height * .7f);
            sb.DrawString(_font, text, position, Color.White);
            if (_drawUnderline) sb.DrawString(_font, lineUnder, position, Color.White);
        }

        public void IncreaseLetter()
        {
            _currentName[_index]++;
            if (_currentName[_index] == _possibleChars.Length) _currentName[_index] = 0;
        }

        public void IncreaseIndex()
        {
            if (_index == NAME_MAX_CHARS - 1) return; //already at the end
            if (_currentName[_index] < 0) return; //didn't enter a char at the current position
            else _index++;
        }

        public void FinishedName()
        {
            //if (_index == 0) return; //require at least 1 letter
            bool oneLetterFound = false;
            for (int i = 0; i < NAME_MAX_CHARS; i++)
                if (_currentName[i] > 0) { oneLetterFound = true; break; }
            if (!oneLetterFound) return; //require at least 1 letter
            String name = "";
            for (int i = 0; i < NAME_MAX_CHARS; i++)
                if (_currentName[i] >= 0) name += _possibleChars[_currentName[i]];
                else break;
            name = name.Trim();
            HiscoreManager.Instance.AddHighScore(name, Player.Instance.GlobalPoints);
            _enteringName = false;
            _drawUnderline = false;
        }

        public void DecreaseIndex()
        {
            if (_index == 0) return; //already at start
            _index--;
        }

        public void DecreaseLetter()
        {
            _currentName[_index]--;
            if (_currentName[_index] < 0) _currentName[_index] = _possibleChars.Length - 1;

        }
    }
}

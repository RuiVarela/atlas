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
    delegate void Del();

    class Menu
    {
        List<Screen> _screenList;
        Screen _activeScreen;
        int SCALE = 8;
        int _gameOverIndex, _insertTopNameIndex, _lifeLostIndex, _mmenuIndex;

        static readonly Menu _instance = new Menu();

        public static Menu Instance
        {
            get { return _instance; }
        }

        public void Initialize()
        {
            _screenList = new List<Screen>();
        }

        public void LoadContent()
        {
            int vHeight = ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height;
            int vWidth = ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width;
            int bHeight = vHeight / SCALE;
            int bWidth = (int)(vHeight * 3.75f / SCALE);

            Screen mainMenu = new Screen("fundo_atlas");
            Screen credits = new Screen("credits");
            Screen top10 = new ScreenTop10("top10");
            Screen paused = new Screen("paused");
            Screen gameOver = new Screen("try_again");
            Screen insertTopName = new ScreenInputTop10("top10_insert_name");
            Screen lifeLost = new Screen("lost_life");
            //Screen padCtrl = new Screen("tutorial/page11");
            Screen kbCtrl = new Screen("tutorial/8");

            Del exitHandler = ResourceMgr.Instance.Game.Exit;
            Del restartHandler = ResourceMgr.Instance.Game.Restart;

            mainMenu.AddButton(new Button("new_game_highlight", "new_game", vWidth - bWidth - 30, vHeight - bHeight * 4, bWidth, bHeight, paused, GameState.LEVELINTRO));
            mainMenu.AddButton(new Button("top10_highlight", "top10_button", vWidth - bWidth - 30, vHeight - bHeight * 3 - 10, bWidth, bHeight, top10, GameState.MENU));
            mainMenu.AddButton(new Button("credits_highlight", "credits_button", vWidth - bWidth - 30, vHeight - bHeight * 2 - 20, bWidth, bHeight, credits, GameState.MENU));
            mainMenu.AddButton(new Button("exit_highlight", "exit_button", vWidth - bWidth - 30, vHeight - bHeight - 30, bWidth, bHeight, mainMenu, GameState.MENU, exitHandler));

            paused.AddButton(new Button("continue_hl_button", "continue_button", vWidth / 2 - bWidth / 2, vHeight / 3, bWidth, bHeight, paused, GameState.RUNNING));
            paused.AddButton(new Button("ctrls_hl_button", "ctrls_button", vWidth / 2 - bWidth / 2, vHeight / 3 + bHeight, bWidth, bHeight, kbCtrl, GameState.MENU));
            paused.AddButton(new Button("mmenu_hl_button", "mmenu_button", vWidth / 2 - bWidth / 2, vHeight / 3 + bHeight * 2, bWidth, bHeight, mainMenu, GameState.MENU, restartHandler));

            credits.SetNoButtonsFire(mainMenu, GameState.MENU);
            top10.SetNoButtonsFire(mainMenu, GameState.MENU, restartHandler);
            gameOver.SetNoButtonsFire(mainMenu, GameState.MENU, restartHandler);
            insertTopName.SetNoButtonsFire(top10, GameState.MENU);
            lifeLost.SetNoButtonsFire(paused, GameState.RUNNING);
            //padCtrl.SetNoButtonsFire(kbCtrl, GameState.MENU);
            kbCtrl.SetNoButtonsFire(paused, GameState.MENU);

            _screenList.Add(mainMenu);
            _screenList.Add(credits);
            _screenList.Add(top10);
            _screenList.Add(paused);
            _screenList.Add(gameOver);
            _screenList.Add(insertTopName);
            _screenList.Add(lifeLost);
            //_screenList.Add(padCtrl);
            _screenList.Add(kbCtrl);
            _activeScreen = _screenList[0];
            _gameOverIndex = 4;
            _insertTopNameIndex = 5;
            _lifeLostIndex = 6;
            _mmenuIndex = 0;

        }

        public bool isMainMenu()
        {
            return _activeScreen == _screenList[_mmenuIndex];
        }

        public void SelectGameOver()
        {
            _activeScreen = _screenList[_gameOverIndex];
        }

        public void SelectInsertTopName()
        {
            _activeScreen = _screenList[_insertTopNameIndex];
        }

        public void SelectLifeLost()
        {
            _activeScreen = _screenList[_lifeLostIndex];
        }

        public void NavigateUp()
        {
            _activeScreen.NavigateUp();
        }

        public void NavigateDown()
        {
            _activeScreen.NavigateDown();
        }

        public void Fire()
        {
            _activeScreen = _activeScreen.Fire();
        }

        public void Draw(SpriteBatch sb)
        {
            _activeScreen.Draw(sb);
        }
    }

    class Screen
    {
        protected List<Button> _buttonList;
        protected Texture2D _backTex;
        protected Rectangle _allR;
        protected int _activeButton;
        protected Screen _defaultNextScr;
        protected GameState _defaultNextGameStt;
        protected SpriteFont _font;
        protected Del _defaultCallback;

        public Screen() { }

        public Screen(string backImg)
        {
            _buttonList = new List<Button>();
            _backTex = ResourceMgr.Instance.Game.Content.Load<Texture2D>(backImg);
            _allR = new Rectangle(0, 0, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height);
            _font = ResourceMgr.Instance.Game.Content.Load<SpriteFont>("ScoreFont");
        }

        public void AddButton(Button b)
        {
            _buttonList.Add(b);
            _activeButton = 0;
        }

        public void NavigateUp()
        {
            if (_buttonList.Count == 0) return;
            if (_activeButton - 1 >= 0)
            {
                _activeButton--;
                ResourceMgr.Instance.Game.Sounds.PlayCue("Menu_Nav");
            }
        }

        public void NavigateDown()
        {
            if (_buttonList.Count == 0) return;
            if (_activeButton + 1 < _buttonList.Count)
            {
                _activeButton++;
                ResourceMgr.Instance.Game.Sounds.PlayCue("Menu_Nav");
            }
        }

        public void SetNoButtonsFire(Screen nextScr, GameState nextGameStt, Del callback)
        {
            _defaultNextScr = nextScr;
            _defaultNextGameStt = nextGameStt;
            _defaultCallback = callback;
        }

        public void SetNoButtonsFire(Screen nextScr, GameState nextGameStt)
        {
            _defaultNextScr = nextScr;
            _defaultNextGameStt = nextGameStt;
            _defaultCallback = null;
        }

        public Screen Fire()
        {
            ResourceMgr.Instance.Game.Sounds.PlayCue("menu_validate");
            if (_buttonList.Count == 0)
            {
                ResourceMgr.Instance.Game.State = _defaultNextGameStt;
                if (_defaultCallback != null) _defaultCallback();
                return _defaultNextScr;
            }
            ResourceMgr.Instance.Game.State = _buttonList[_activeButton].NextState;
            _buttonList[_activeButton].Fire();
            Screen res = _buttonList[_activeButton].NextScreen;
            _activeButton = 0;
            return res;

        }

        public virtual void Draw(SpriteBatch sb)
        {
            sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            sb.Draw(_backTex, _allR, Color.White);
            int count = _buttonList.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                    _buttonList[i].Draw(sb, i == _activeButton);
            }
            sb.End();
        }
    }

    class ScreenTop10 : Screen
    {
        public ScreenTop10(string backImg)
        {
            _buttonList = new List<Button>();
            _backTex = ResourceMgr.Instance.Game.Content.Load<Texture2D>(backImg);
            _allR = new Rectangle(0, 0, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height);
            _font = ResourceMgr.Instance.Game.Content.Load<SpriteFont>("ScoreFont");
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            sb.Draw(_backTex, _allR, Color.White);
            int count = _buttonList.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                    _buttonList[i].Draw(sb, i == _activeButton);
            }
            string text = HiscoreManager.Instance.GetHiscoreText();
            Vector2 size = _font.MeasureString(text) * 0.5f;
            Vector2 position = new Vector2(ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width / 2 - size.X, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height / 1.6f - size.Y);
            sb.DrawString(_font, text, position, Color.Black);
            sb.End();
        }
    }

    class ScreenInputTop10 : Screen
    {
        public ScreenInputTop10(string backImg)
        {
            _buttonList = new List<Button>();
            _backTex = ResourceMgr.Instance.Game.Content.Load<Texture2D>(backImg);
            _allR = new Rectangle(0, 0, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height);
            _font = ResourceMgr.Instance.Game.Content.Load<SpriteFont>("ScoreFont");
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            sb.Draw(_backTex, _allR, Color.White);
            int count = _buttonList.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                    _buttonList[i].Draw(sb, i == _activeButton);
            }
            ResourceMgr.Instance.Game.HighScoreScreen.Draw(sb);
            sb.End();
        }
    }

    struct Button
    {
        Rectangle _buttonR;
        Texture2D _selected, _unselected;
        Screen _nextScr;
        GameState _nextGameStt;
        Del _callback;

        public Button(string selectedImg, string unselectedImg, int x, int y, int width, int height, Screen nextScr, GameState nextGameStt, Del callback)//bool restart, bool exit)
        {
            _buttonR = new Rectangle(x, y, width, height);
            _selected = ResourceMgr.Instance.Game.Content.Load<Texture2D>(selectedImg);
            _unselected = ResourceMgr.Instance.Game.Content.Load<Texture2D>(unselectedImg);
            _nextGameStt = nextGameStt;
            _nextScr = nextScr;
            _callback = callback;
        }

        public Button(string selectedImg, string unselectedImg, int x, int y, int width, int height, Screen nextScr, GameState nextGameStt)
        {
            _buttonR = new Rectangle(x, y, width, height);
            _selected = ResourceMgr.Instance.Game.Content.Load<Texture2D>(selectedImg);
            _unselected = ResourceMgr.Instance.Game.Content.Load<Texture2D>(unselectedImg);
            _nextGameStt = nextGameStt;
            _nextScr = nextScr;
            _callback = null;
        }

        public void Draw(SpriteBatch sb, bool selected)
        {
            if (selected) sb.Draw(_selected, _buttonR, Color.White);
            else sb.Draw(_unselected, _buttonR, Color.White);
        }

        public Screen NextScreen
        {
            get { return _nextScr; }
        }

        public GameState NextState
        {
            get { return _nextGameStt; }
        }

        public void Fire()
        {
            if (_callback == null) return;
            _callback();
        }
    }
}

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
    class Controller
    {
        private KeyboardState _oldState;
        private GamePadState _gamePad;

        private Camera _camera;

        private const int DELTA = 140;
        private float MAX_VIBRATION = .5f;

        float _interval;

        public Controller(Camera cam)
        {
            _camera = cam;
        }

        public void Initialize()
        {
            _oldState = Keyboard.GetState();
            _gamePad = GamePad.GetState(PlayerIndex.One);
            _interval = 0;
            MAX_VIBRATION = ConfigReader.Instance.GetValueAsInt("MaxVibration") * 0.01f;
        }

        public void Update(GameTime gameTime)
        {
            if (_interval - ((float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond) > 0) _interval -= ((float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);
            else
                _interval = 0;
            KeyboardState newState = Keyboard.GetState();
            GamePadState newGamePadState = GamePad.GetState(PlayerIndex.One);
            Board board = ResourceMgr.Instance.Game.Board;
            GameState gs = ResourceMgr.Instance.Game.State;

            //Hack to skip mission
            if (newState.IsKeyDown(Keys.LeftAlt) && newState.IsKeyDown(Keys.N) && !_oldState.IsKeyDown(Keys.N))
                ResourceMgr.Instance.Game.Gain1000p();

            //Profile for N seconds
            if (newState.IsKeyDown(Keys.P) && !_oldState.IsKeyDown(Keys.P)) Profiler.Instance.StartProfiling(5.0);

            if (gs == GameState.MENU)
            {
                if ((newState.IsKeyDown(Keys.Escape) || newGamePadState.Buttons.Back == ButtonState.Pressed) && Menu.Instance.isMainMenu())
                    ResourceMgr.Instance.Game.Exit();
                if ((newState.IsKeyDown(Keys.Up) && !_oldState.IsKeyDown(Keys.Up)) || (newGamePadState.DPad.Up == ButtonState.Pressed && _gamePad.DPad.Up != ButtonState.Pressed))
                    Menu.Instance.NavigateUp();
                if ((newState.IsKeyDown(Keys.Down) && !_oldState.IsKeyDown(Keys.Down)) || (newGamePadState.DPad.Down == ButtonState.Pressed && _gamePad.DPad.Down != ButtonState.Pressed))
                    Menu.Instance.NavigateDown();
                if ((newState.IsKeyDown(Keys.Space) && !_oldState.IsKeyDown(Keys.Space)) || (newGamePadState.Buttons.A == ButtonState.Pressed && _gamePad.Buttons.A != ButtonState.Pressed))
                    Menu.Instance.Fire();
                if (ResourceMgr.Instance.Game.HighScoreScreen.IsEnteringName())
                    NameEntry(newState, newGamePadState);
            }

            if (gs == GameState.LEVELINTRO)
            {
                if ((newState.IsKeyDown(Keys.Space) && !_oldState.IsKeyDown(Keys.Space)) || (newGamePadState.Buttons.A == ButtonState.Pressed && _gamePad.Buttons.A != ButtonState.Pressed))
                {
                    ResourceMgr.Instance.Game.Sounds.PlayCue("menu_validate");
                    ResourceMgr.Instance.Game.LEVEL.NextIntro();
                }
                if ((newGamePadState.Buttons.X == ButtonState.Pressed && _gamePad.Buttons.X != ButtonState.Pressed) || (newState.IsKeyDown(Keys.Escape) && !_oldState.IsKeyDown(Keys.Escape)))
                {
                    ResourceMgr.Instance.Game.Sounds.PlayCue("menu_back");
                    ResourceMgr.Instance.Game.SkipTutorial();
                }
            }
            if (gs == GameState.MISSION_END)
            {
                if (((newState.IsKeyDown(Keys.Space) && !_oldState.IsKeyDown(Keys.Space)) || (newGamePadState.Buttons.A == ButtonState.Pressed && _gamePad.Buttons.A != ButtonState.Pressed)) && ResourceMgr.Instance.Game.LEVEL.ACTIVEMISSION.BoardEmpty)
                {
                    ResourceMgr.Instance.Game.Sounds.PlayCue("menu_validate");
                    ResourceMgr.Instance.Game.LEVEL.ACTIVEMISSION.ISOVER = true;
                    ResourceMgr.Instance.Game.State = GameState.RUNNING;
                }
                CameraMovement(newState, newGamePadState, gameTime);
            }

            if (gs == GameState.RUNNING)
            {
                if (newState.IsKeyDown(Keys.Escape) || (newGamePadState.Buttons.Start == ButtonState.Pressed && _gamePad.Buttons.Start != ButtonState.Pressed))
                    ResourceMgr.Instance.Game.State = GameState.MENU;
                CameraMovement(newState, newGamePadState, gameTime);
                if (!ResourceMgr.Instance.Game.DisplayHelp)
                    ShapeMovement(newState, newGamePadState, board);
                if ((newState.IsKeyDown(Keys.H) && !_oldState.IsKeyDown(Keys.H)) || (newGamePadState.Buttons.Y == ButtonState.Pressed && _gamePad.Buttons.Y != ButtonState.Pressed))
                    ResourceMgr.Instance.Game.DisplayHelp = !ResourceMgr.Instance.Game.DisplayHelp;
            }

            float aux = 0f;
            if (gs == GameState.RUNNING) aux = ResourceMgr.Instance.Game.DangerPercentage * MAX_VIBRATION;
            GamePad.SetVibration(PlayerIndex.One, aux, aux);

            _oldState = newState;
            _gamePad = newGamePadState;

        }

        private void NameEntry(KeyboardState newState, GamePadState newGamePadState)
        {
            if ((newState.IsKeyDown(Keys.Space) && !_oldState.IsKeyDown(Keys.Space)) || (newGamePadState.Buttons.A == ButtonState.Pressed && _gamePad.Buttons.A != ButtonState.Pressed))
            {
                ResourceMgr.Instance.Game.HighScoreScreen.FinishedName();
            }
            if ((newState.IsKeyDown(Keys.Up) && !_oldState.IsKeyDown(Keys.Up)) || (newGamePadState.DPad.Up == ButtonState.Pressed && _gamePad.DPad.Up != ButtonState.Pressed))
            {
                ResourceMgr.Instance.Game.HighScoreScreen.IncreaseLetter();
            }
            if ((newState.IsKeyDown(Keys.Down) && !_oldState.IsKeyDown(Keys.Down)) || (newGamePadState.DPad.Down == ButtonState.Pressed && _gamePad.DPad.Down != ButtonState.Pressed))
            {
                ResourceMgr.Instance.Game.HighScoreScreen.DecreaseLetter();
            }
            if ((newState.IsKeyDown(Keys.Left) && !_oldState.IsKeyDown(Keys.Left)) || (newGamePadState.DPad.Left == ButtonState.Pressed && _gamePad.DPad.Left != ButtonState.Pressed))
            {
                ResourceMgr.Instance.Game.HighScoreScreen.DecreaseIndex();
            }
            if ((newState.IsKeyDown(Keys.Right) && !_oldState.IsKeyDown(Keys.Right)) || (newGamePadState.DPad.Right == ButtonState.Pressed && _gamePad.DPad.Right != ButtonState.Pressed))
            {
                ResourceMgr.Instance.Game.HighScoreScreen.IncreaseIndex();
            }
        }

        private void CameraMovement(KeyboardState newKS, GamePadState newGPS, GameTime gt)
        {
            //CAMERA MOVEMENT PAD
            if (newGPS.ThumbSticks.Right.Y > 0f)
                _camera.MoveUp(newGPS.ThumbSticks.Right.Y, gt);

            if (newGPS.ThumbSticks.Right.Y < 0f)
                _camera.MoveDown(newGPS.ThumbSticks.Right.Y, gt);

            if (newGPS.ThumbSticks.Right.X != 0f)
                if (Math.Abs(newGPS.ThumbSticks.Right.X) < 0.95f)
                    _camera.MoveRight(newGPS.ThumbSticks.Right.X, gt);
                else
                    if (newGPS.ThumbSticks.Right.X < 0)
                        _camera.QuickTurnLeft();
                    else
                        _camera.QuickTurnRight();

            //CAMERA MOVEMENT KEYBOARD
            if (newKS.IsKeyDown(Keys.W))
                _camera.MoveUp(1f, gt);

            if (newKS.IsKeyDown(Keys.S))
                _camera.MoveDown(1f, gt);

            if (newKS.IsKeyDown(Keys.D))
                _camera.MoveRight(1f, gt);

            if (newKS.IsKeyDown(Keys.A))
                _camera.MoveRight(-1f, gt);

            //QUICKTURN
            if (newKS.IsKeyDown(Keys.J) && !_oldState.IsKeyDown(Keys.J))
                _camera.QuickTurnLeft();
            if (newKS.IsKeyDown(Keys.K) && !_oldState.IsKeyDown(Keys.K))
                _camera.QuickTurnRight();
        }

        private void ShapeMovement(KeyboardState newKS, GamePadState newGPS, Board board)
        {
            //SHAPE MOVEMENT
            if ((newKS.IsKeyDown(Keys.Right) && _interval == 0) || (newKS.IsKeyDown(Keys.Right) && !_oldState.IsKeyDown(Keys.Right)) || (newGPS.DPad.Right == ButtonState.Pressed && _interval == 0) || (newGPS.DPad.Right == ButtonState.Pressed && _gamePad.DPad.Right != ButtonState.Pressed))
            {
                _camera.MoveShapeRight();
                _interval = DELTA;
            }

            if ((newKS.IsKeyDown(Keys.Left) && _interval == 0) || (newKS.IsKeyDown(Keys.Left) && !_oldState.IsKeyDown(Keys.Left)) || (newGPS.DPad.Left == ButtonState.Pressed && _interval == 0) || (newGPS.DPad.Left == ButtonState.Pressed && _gamePad.DPad.Left != ButtonState.Pressed))
            {
                _camera.MoveShapeLeft();
                _interval = DELTA;
            }

            if ((newKS.IsKeyDown(Keys.Up) && _interval == 0) || (newKS.IsKeyDown(Keys.Up) && !_oldState.IsKeyDown(Keys.Up)) || (newGPS.DPad.Up == ButtonState.Pressed && _interval == 0) || (newGPS.DPad.Up == ButtonState.Pressed && _gamePad.DPad.Up != ButtonState.Pressed))
            {
                _camera.MoveShapeBack();
                _interval = DELTA;
            }

            if ((newKS.IsKeyDown(Keys.Down) && _interval == 0) || (newKS.IsKeyDown(Keys.Down) && !_oldState.IsKeyDown(Keys.Down)) || (newGPS.DPad.Down == ButtonState.Pressed && _interval == 0) || (newGPS.DPad.Down == ButtonState.Pressed && _gamePad.DPad.Down != ButtonState.Pressed))
            {
                _camera.MoveShapeFront();
                _interval = DELTA;
            }
            //Shape Rotation
            if ((newKS.IsKeyDown(Keys.E) && _interval == 0) || (newKS.IsKeyDown(Keys.E) && !_oldState.IsKeyDown(Keys.E)) || (newGPS.Triggers.Right == 1.0f && _gamePad.Triggers.Right != 1.0f) || (newGPS.Buttons.RightShoulder == ButtonState.Pressed && _gamePad.Buttons.RightShoulder == ButtonState.Released))
            {
                board.CurrentShape.RotateRight();
                _interval = DELTA;
            }
            if ((newKS.IsKeyDown(Keys.Q) && _interval == 0) || (newKS.IsKeyDown(Keys.Q) && !_oldState.IsKeyDown(Keys.Q)) || (newGPS.Triggers.Left == 1.0f && _gamePad.Triggers.Left != 1.0f) || (newGPS.Buttons.LeftShoulder == ButtonState.Pressed && _gamePad.Buttons.LeftShoulder == ButtonState.Released))
            {
                board.CurrentShape.RotateLeft();
                _interval = DELTA;
            }
            //Shape Speed Up
            if (newKS.IsKeyDown(Keys.Space) && !_oldState.IsKeyDown(Keys.Space) || (newGPS.Buttons.A == ButtonState.Pressed && _gamePad.Buttons.A != ButtonState.Pressed))
                board.Physics.SpeedUp();

            if (newKS.IsKeyUp(Keys.Space) && _oldState.IsKeyDown(Keys.Space) || (newGPS.Buttons.A == ButtonState.Released && _gamePad.Buttons.A != ButtonState.Released))
                board.Physics.NormalSpeed();
        }
    }
}

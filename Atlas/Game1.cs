using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Net;

namespace Atlas
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        #region Particulas
        ParticleSystem explosionParticles;
        ParticleSystem explosionSmokeParticles;
        ParticleSystem projectileTrailParticles;
        ParticleSystem smokePlumeParticles;
        ParticleSystem fireParticles;


        // The sample can switch between three different visual effects.
        enum ParticleState
        {
            Explosions,
            SmokePlume,
            RingOfFire,
        };

        //NOT BEING USED
        //ParticleState currentState = ParticleState.Explosions;


        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        List<Projectile> projectiles = new List<Projectile>();

        TimeSpan timeToNextProjectile = TimeSpan.Zero;

        Random random = new Random();
        #endregion

        #region Sound
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;
        Cue _music;
        #endregion

        float _currentDanger;
        const float DANGER_THRESHOLD = 0.6f;
        const int LIVES = 3;
        int _lives;
        bool _help, _highQualityGraphics;
        List<int> _usedLevels;

        GameState _gameState;

        Camera _camera;
        Controller _ctrlr;
        int _activeLevel;
        HiscoreScreen _hiscoreScreen;
        List<Level> _levelList;

        int _supersample;

        SpriteFont _Text3DFont;
        SafeArea _safeArea;

        public Level LEVEL
        {
            get { return _levelList[_activeLevel]; }
        }

        public int Lives
        {
            get { return _lives; }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            this.Exiting += new EventHandler(AtlasExiting);

            _supersample = ConfigReader.Instance.GetValueAsInt("UseAA");
            if (_supersample == 0) _supersample = 1; //not defined

            if (_supersample > 1)
            {
                graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            }

            int width = ConfigReader.Instance.GetValueAsInt("ResolutionWidth");
            int height = ConfigReader.Instance.GetValueAsInt("ResolutionHeight");

            if (width == 0) graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            else graphics.PreferredBackBufferWidth = width;
            if (height == 0) graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            else graphics.PreferredBackBufferHeight = height;
            if (ConfigReader.Instance.GetValueAsBool("Fullscreen")) graphics.ToggleFullScreen();

            _highQualityGraphics = ConfigReader.Instance.GetValueAsBool("HighQualityGraphics");
            this.IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;

            Content.RootDirectory = "Content";

            _camera = new Camera();
            _ctrlr = new Controller(_camera);
            _hiscoreScreen = new HiscoreScreen();

            _usedLevels = new List<int>();
            _usedLevels.Add(0);

            _levelList = new List<Level>();
            Level tmp = new Level(0, "level0");
            _levelList.Add(tmp);
            tmp = new Level(1, "level1");
            _levelList.Add(tmp);
            tmp = new Level(2, "level5plus5");
            _levelList.Add(tmp);
            tmp = new Level(3, "levelBlack");
            _levelList.Add(tmp);
            tmp = new Level(4, "levelCMplusWeight");
            _levelList.Add(tmp);
            _activeLevel = 0;

            _gameState = GameState.MENU;
            _currentDanger = 0f;
            _lives = LIVES;
            _help = false;

            _safeArea = new SafeArea();
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            //XBOX
            //e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 0;
            //e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType = MultiSampleType.FourSamples; //AA 4x
            //e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType = MultiSampleType.TwoSamples; //AA 2x

            //PC
            GraphicsAdapter adapter = e.GraphicsDeviceInformation.Adapter;
            SurfaceFormat format = adapter.CurrentDisplayMode.Format;

            if (_supersample == 16) //try 16x AA
            {
                if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format, ConfigReader.Instance.GetValueAsBool("Fullscreen"), MultiSampleType.SixteenSamples))
                {
                    //supports 16x AA
                    graphics.PreferMultiSampling = true;
                    e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 0;
                    e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType = MultiSampleType.SixteenSamples;
                }
                else _supersample = 8; //8x AA not supported, go to 4x AA
            }

            if (_supersample == 8) //try 8x AA
            {
                if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format, ConfigReader.Instance.GetValueAsBool("Fullscreen"), MultiSampleType.EightSamples))
                {
                    //supports 8x AA
                    graphics.PreferMultiSampling = true;
                    e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 0;
                    e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType = MultiSampleType.EightSamples;
                }
                else _supersample = 4; //8x AA not supported, go to 4x AA
            }

            //if (_supersample > 4) _supersample = 4; //max supported for render targets...

            if (_supersample == 4) //try 4x AA
            {
                if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format, ConfigReader.Instance.GetValueAsBool("Fullscreen"), MultiSampleType.FourSamples))
                {
                    //supports 4x AA
                    graphics.PreferMultiSampling = true;
                    e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 0;
                    e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType = MultiSampleType.FourSamples;
                }
                else _supersample = 2; //4x AA not supported, go to 2x AA
            }

            if (_supersample == 2) //try 2x AA
            {
                if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format, ConfigReader.Instance.GetValueAsBool("Fullscreen"), MultiSampleType.FourSamples))
                {
                    //supports 2x AA
                    graphics.PreferMultiSampling = true;
                    e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 0;
                    e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType = MultiSampleType.TwoSamples;
                }
                else _supersample = 1; //2x AA not supported, use no AA
            }
            
            return;
        }

        void CheckRenderTargetSupport()
        {
            RenderTarget2D temp;
            if (_supersample == 16)
            {
                try
                {
                    temp = new RenderTarget2D(GraphicsDevice, 1, 1, 1, SurfaceFormat.Vector4, MultiSampleType.SixteenSamples, 0);
                    GraphicsDevice.PresentationParameters.MultiSampleType = MultiSampleType.SixteenSamples;
                    GraphicsDevice.PresentationParameters.MultiSampleQuality = 0;
                }
                catch (InvalidOperationException e)
                {
                    _supersample = 8;
                }
            }
            if (_supersample == 8)
            {
                try
                {
                    temp = new RenderTarget2D(GraphicsDevice, 1, 1, 1, SurfaceFormat.Vector4, MultiSampleType.EightSamples, 0);
                    GraphicsDevice.PresentationParameters.MultiSampleType = MultiSampleType.EightSamples;
                    GraphicsDevice.PresentationParameters.MultiSampleQuality = 0;
                }
                catch (InvalidOperationException e)
                {
                    _supersample = 4;
                }
            }
            if (_supersample == 4)
            {
                try
                {
                    temp = new RenderTarget2D(GraphicsDevice, 1, 1, 1, SurfaceFormat.Vector4, MultiSampleType.FourSamples, 0);
                    GraphicsDevice.PresentationParameters.MultiSampleType = MultiSampleType.FourSamples;
                    GraphicsDevice.PresentationParameters.MultiSampleQuality = 0;
                }
                catch (InvalidOperationException e)
                {
                    _supersample = 2;
                }
            }
            if (_supersample == 2)
            {
                try
                {
                    temp = new RenderTarget2D(GraphicsDevice, 1, 1, 1, SurfaceFormat.Vector4, MultiSampleType.TwoSamples, 0);
                    GraphicsDevice.PresentationParameters.MultiSampleType = MultiSampleType.TwoSamples;
                    GraphicsDevice.PresentationParameters.MultiSampleQuality = 0;
                }
                catch (InvalidOperationException e)
                {
                    _supersample = 1;
                }
            }
            if (_supersample == 1)
            {
                graphics.PreferMultiSampling = false;
                GraphicsDevice.PresentationParameters.MultiSampleType = MultiSampleType.None;
                GraphicsDevice.PresentationParameters.MultiSampleQuality = 0;
            }
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            CheckRenderTargetSupport();
            // TODO: Add your initialization logic here
            ResourceMgr.Instance.Initialize(this);
            HUD.Instance.Initialize();
            Menu.Instance.Initialize();

            _ctrlr.Initialize();
            _camera.Initialize();

            _levelList[_activeLevel].Initialize();
            _hiscoreScreen.Initialize();

            Statistics.Instance.Initialize();

            #region Particulas

            // Construct our particle system components.
            explosionParticles = new ExplosionParticleSystem(this, Content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(this, Content);
            projectileTrailParticles = new ProjectileTrailParticleSystem(this, Content);
            smokePlumeParticles = new SmokePlumeParticleSystem(this, Content);
            fireParticles = new FireParticleSystem2(this, Content);


            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            smokePlumeParticles.DrawOrder = 100;
            explosionSmokeParticles.DrawOrder = 200;
            projectileTrailParticles.DrawOrder = 300;
            explosionParticles.DrawOrder = 400;
            fireParticles.DrawOrder = 500;

            // Register the particle system components.
            Components.Add(explosionParticles);
            Components.Add(explosionSmokeParticles);
            Components.Add(projectileTrailParticles);
            Components.Add(smokePlumeParticles);
            Components.Add(fireParticles);
            #endregion

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            #region SOUND
            audioEngine = new AudioEngine("Content/atlas_sound.xgs");
            waveBank = new WaveBank(audioEngine, "Content/atlas.xwb");
            soundBank = new SoundBank(audioEngine, "Content/atlas_soundbank.xsb");
            #endregion

            _levelList[0].AddIntro("tutorial/1");
            _levelList[0].AddIntro("tutorial/2");
            _levelList[0].AddIntro("tutorial/3");
            _levelList[0].AddIntro("tutorial/4");
            _levelList[0].AddIntro("tutorial/5");
            _levelList[0].AddIntro("tutorial/6");
            _levelList[0].AddIntro("tutorial/7");
            _levelList[0].AddIntro("tutorial/8");
            _levelList[0].AddIntro("level0");

            _levelList[_activeLevel].LoadContent();
            _hiscoreScreen.LoadContent();
            Menu.Instance.LoadContent();

            _Text3DFont = Content.Load<SpriteFont>("gameFont");
            _safeArea.LoadGraphicsContent(GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public SpriteFont Text3D
        {
            get { return _Text3DFont; }
        }

        private void AtlasExiting(object sender, EventArgs args)
        {
            bool sendStatsByMail = ConfigReader.Instance.GetValueAsBool("SendStatsByEmail");
            if (sendStatsByMail)
            {
                if (SendStatsByMail())
                    Statistics.Instance.DeleteFileContents();
            }
            Profiler.Instance.EndProfiling();
        }

        #region particulas

        /// <summary>
        /// Helper used by the UpdateFire method. Chooses a random location
        /// around a circle, at which a fire particle will be created.
        /// </summary>
        Vector3 RandomPointOnCircle()
        {
            const float radius = 30;
            const float height = 40;

            double angle = random.NextDouble() * Math.PI * 2;

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            return new Vector3(x * radius, y * radius + height, 0);
        }

        /// <summary>
        /// Helper for updating the explosions effect.
        /// </summary>
        void UpdateExplosions(GameTime gameTime)
        {
            timeToNextProjectile -= gameTime.ElapsedGameTime;

            if (timeToNextProjectile <= TimeSpan.Zero)
            {
                // Create a new projectile once per second. The real work of moving
                // and creating particles is handled inside the Projectile class.
                //explosionParticles.AddParticle(new Vector3(-12.0f, 1.0f, -12.0f), Vector3.Zero);

                //projectiles.Add(new Projectile(explosionParticles,
                //                               explosionSmokeParticles,
                //                               projectileTrailParticles));

                timeToNextProjectile += TimeSpan.FromSeconds(1);
            }
        }

        /// <summary>
        /// Helper for updating the list of active projectiles.
        /// </summary>
        void UpdateProjectiles(GameTime gameTime)
        {
            int i = 0;

            while (i < projectiles.Count)
            {
                if (!projectiles[i].Update(gameTime))
                {
                    // Remove projectiles at the end of their life.
                    projectiles.RemoveAt(i);
                }
                else
                {
                    // Advance to the next projectile.
                    i++;
                }
            }
        }

        /// <summary>
        /// Helper for updating the smoke plume effect.
        /// </summary>
        void UpdateSmokePlume()
        {
            // This is trivial: we just create one new smoke particle per frame.
            smokePlumeParticles.AddParticle(new Vector3(-12.0f, 1.0f, -12.0f), Vector3.Zero);
        }

        /// <summary>
        /// Helper for updating the fire effect.
        /// </summary>
        void UpdateFire()
        {
            const int fireParticlesPerFrame = 100;
            if (_levelList[_activeLevel].ACTIVEMISSION.BOARD.REMOVEPIECES.Count != 0)
                soundBank.PlayCue("fada");
            if (_levelList[_activeLevel].ACTIVEMISSION.BOARD.InvalidRemovePieces.Count != 0)
                soundBank.PlayCue("bad_balls");

            foreach (Piece p in _levelList[_activeLevel].ACTIVEMISSION.BOARD.REMOVEPIECES)
            {
                fireParticles.SETINGS.MaxColor = p.Color;
                fireParticles.SETINGS.MinColor = p.Color;
                fireParticles.UpdateColor();
                for (int i = 0; i < fireParticlesPerFrame; i++)
                    fireParticles.AddParticle(new Vector3(p.PositionX, p.PositionY, p.PositionZ), Vector3.Zero);
                Text3DManager.Instance.AddText3D(
                    new FloatUpText3D("+5", new Vector3(p.PositionX, p.PositionY, p.PositionZ), 2.0f, false, p.Color, 0.5f, _Text3DFont, 1.0f)
                    );
            }
            foreach (Piece p in _levelList[_activeLevel].ACTIVEMISSION.BOARD.InvalidRemovePieces)
            {
                fireParticles.SETINGS.MaxColor = Color.Red;
                fireParticles.SETINGS.MinColor = Color.Red;
                fireParticles.UpdateColor();
                for (int i = 0; i < fireParticlesPerFrame; i++)
                    fireParticles.AddParticle(new Vector3(p.PositionX, p.PositionY, p.PositionZ), Vector3.Zero);
            }

            // Create one smoke particle per frmae, too.
            //smokePlumeParticles.AddParticle(new Vector3(-12.0f, 1.0f, -12.0f), Vector3.Zero);
        }
        #endregion

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            audioEngine.Update();
            _ctrlr.Update(gameTime);

            if (_gameState == GameState.RUNNING)
            {
                if (!DisplayHelp)
                    _levelList[_activeLevel].Update(gameTime);
                _camera.Update(gameTime);
                UpdateProjectiles(gameTime);
                UpdateExplosions(gameTime); UpdateFire(); UpdateSmokePlume();
                HUD.Instance.Update(gameTime);
                Statistics.Instance.Update(gameTime);
                Text3DManager.Instance.Update(gameTime);

                //if (_levelList.Count > _activeLevel + 1 && _levelList[_activeLevel].ISOVER)
                if (_levelList[_activeLevel].ISOVER && _levelList.Count != _usedLevels.Count)
                {
                    Statistics.Instance.AppendToFile(LEVEL.ID, LEVEL.ACTIVEMISSION.ID);
                    if (_activeLevel == 0)
                    {
                        Player.Instance.GlobalPoints = 0;
                        _lives = LIVES;
                        _activeLevel++;
                        _usedLevels.Add(1);
                    }
                    else
                    {
                        while (true)
                        {
                            int nextLevel = random.Next(2, 5);
                            if (!_usedLevels.Contains(nextLevel))
                            {
                                _activeLevel = nextLevel;
                                _usedLevels.Add(nextLevel);
                                break;
                            }
                        }
                    }
                    Player.Instance.CurrentMissionPoints = 0;
                    _levelList[_activeLevel].Initialize();
                    _levelList[_activeLevel].LoadContent();
                    State = GameState.LEVELINTRO;
                }
                //else if (_levelList.Count == _activeLevel + 1 && _levelList[_activeLevel].ISOVER)
                else if (_levelList[_activeLevel].ISOVER && _levelList.Count == _usedLevels.Count)
                    GameOver(true);
            }
            if (_gameState == GameState.MISSION_END)
            {
                _levelList[_activeLevel].Update(gameTime);
                _camera.Update(gameTime);
                UpdateProjectiles(gameTime);
                UpdateExplosions(gameTime); UpdateFire(); UpdateSmokePlume();
                Text3DManager.Instance.Update(gameTime);
            }
            if (_gameState == GameState.MENU && _hiscoreScreen.IsEnteringName())
            {
                _hiscoreScreen.Update(gameTime);
            }

            base.Update(gameTime);
        }

        public void Gain1000p()
        {
            Player.Instance.IncreasePoints(1000);
        }

        public void SkipTutorial()
        {
            if (_activeLevel == 0)
            {
                Player.Instance.CurrentMissionPoints = 0;
                _activeLevel++;
                _levelList[_activeLevel].Initialize();
                _levelList[_activeLevel].LoadContent();
                State = GameState.LEVELINTRO;
                _usedLevels.Add(1);
            }
        }

        //CHEAT
        public void SkipLevel()
        {
            if (_levelList.Count > _activeLevel + 1)
            {
                Player.Instance.CurrentMissionPoints = 0;
                _activeLevel++;
                _levelList[_activeLevel].Initialize();
                _levelList[_activeLevel].LoadContent();
                State = GameState.LEVELINTRO;
            }
        }

        private bool SendStatsByMail()
        {
            try
            {
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                System.Net.NetworkCredential cred = new System.Net.NetworkCredential("atlasXNA@gmail.com", "atlasXNA123456");
                mail.To.Add("atlasXNA@gmail.com");
                mail.Subject = "XNA 24 evaluation";
                mail.From = new System.Net.Mail.MailAddress("atlasXNA@gmail.com");
                mail.IsBodyHtml = true;
                mail.Body = "message";
                mail.Attachments.Add(new System.Net.Mail.Attachment("Config/stats"));

                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.UseDefaultCredentials = true;
                client.Credentials = cred;
                client.Send(mail);
                mail.Attachments[0].ContentStream.Close();
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }

        public void GameOver(bool terminal)
        {
            if (_lives <= 1 || terminal)
            {
                Statistics.Instance.AppendToFile(LEVEL.ID, LEVEL.ACTIVEMISSION.ID);
                if (Player.Instance.GlobalPoints > HiscoreManager.Instance.GetLowestScore())
                {
                    Menu.Instance.SelectInsertTopName();
                    _gameState = GameState.MENU;
                    EnterHighScore();
                }
                else
                {
                    Menu.Instance.SelectGameOver();
                    _gameState = GameState.MENU;
                }

            }
            else
            {
                _lives--;
                Statistics.Instance.LiveLostAt();
                Statistics.Instance.AppendToFile(LEVEL.ID, LEVEL.ACTIVEMISSION.ID);
                LEVEL.ACTIVEMISSION.Restart();
                //Tirar os pontos feitos ate perder a vida
                Player.Instance.GlobalPoints -= Player.Instance.CurrentMissionPoints;
                Player.Instance.CurrentMissionPoints = 0;
                Menu.Instance.SelectLifeLost();
                ResourceMgr.Instance.Game.State = GameState.MENU;
            }
        }

        public void EnterHighScore()
        {
            _hiscoreScreen.Reset();
            _hiscoreScreen.StartEnteringName();
        }

        public HiscoreScreen HighScoreScreen { get { return _hiscoreScreen; } }

        public void Restart()
        {
            Window.Title = "Atlas";
            Player.Instance.CurrentMissionPoints = 0;
            Player.Instance.GlobalPoints = 0;
            //for (int i = 0; i <= _activeLevel; i++)
            //    _levelList[i].Restart();
            foreach (int i in _usedLevels)
                _levelList[i].Restart();
            _activeLevel = 0;
            _levelList[_activeLevel].Initialize();
            _levelList[_activeLevel].LoadContent();

            _usedLevels.Clear();
            _usedLevels.Add(0);

            _gameState = GameState.MENU;
            _lives = LIVES;
            _help = false;
        }

        public GameState State
        {
            get { return _gameState; }
            set
            {
                _gameState = value;
                if (_gameState == GameState.RUNNING)
                {
                    if (_music.IsPaused)
                        _music.Resume();
                    if (_music.IsPrepared)
                        _music.Play();
                }
                else
                {
                    if (_music.IsPlaying)
                        _music.Pause();
                }
            }
        }

        public SoundBank Sounds
        {
            get { return soundBank; }
        }

        public Cue ActiveMusic
        {
            get { return _music; }
        }

        public string Music
        {
            set
            {
                if (_music != null) _music.Stop(AudioStopOptions.Immediate);
                _music = soundBank.GetCue(value);
            }
        }

        public bool DisplayHelp
        {
            get { return _help; }
            set { _help = value; }
        }

        public float DangerPercentage
        {
            get { return _currentDanger; }
            set
            {
                _currentDanger = value - DANGER_THRESHOLD;
                if (_currentDanger < 0) { _currentDanger = 0; return; }
                _currentDanger = _currentDanger / (1 - _currentDanger);
            }
        }

        public Surface Surface
        {
            get { return _levelList[_activeLevel].ACTIVEMISSION.SURFACE; }
        }

        public Camera Camera
        {
            get { return _camera; }
        }

        public Board Board
        {
            get { return _levelList[_activeLevel].ACTIVEMISSION.BOARD; }
        }

        public bool HighQualityGraphics
        {
            get { return _highQualityGraphics; }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);

            GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            if (State == GameState.RUNNING || State == GameState.MISSION_END)
            {
                _levelList[_activeLevel].DrawOpaque();
                _levelList[_activeLevel].DrawTransparent();
            }

            #region particulas

            explosionParticles.SetCamera(_camera.View, _camera.Projection);
            explosionSmokeParticles.SetCamera(_camera.View, _camera.Projection);
            projectileTrailParticles.SetCamera(_camera.View, _camera.Projection);
            //smokePlumeParticles.SetCamera(_camera.View, _camera.Projection);

            fireParticles.SetCamera(_camera.View, _camera.Projection);
            //projectileTrailParticles.Visible = true;

            #endregion

            //ExtraFunctions.DrawBoundingBox(Board.CollisionBB, _camera.View, _camera.Projection);
            //ExtraFunctions.DrawBoundingBox(Surface.GetBoundingBox(), _camera.View, _camera.Projection);

            if (State == GameState.LEVELINTRO)
                LEVEL.DrawIntro(spriteBatch);

            if (_gameState == GameState.MENU)
            {
                Menu.Instance.Draw(spriteBatch);
            }

            if (_gameState == GameState.RUNNING)
            {
                Text3DManager.Instance.Draw(_camera.View, _camera.Projection, spriteBatch);
                HUD.Instance.Draw(spriteBatch);
                if (DisplayHelp) HUD.Instance.DrawHelp(spriteBatch);
            }
            if (_gameState == GameState.MISSION_END)
            {
                HUD.Instance.DrawMissionEnd(spriteBatch);
                Text3DManager.Instance.Draw(_camera.View, _camera.Projection, spriteBatch);
            }

            //draw viewport da peça
            if (_gameState == GameState.RUNNING)
            {
                Viewport original = GraphicsDevice.Viewport;
                Viewport vp = GraphicsDevice.Viewport;
                vp.Height /= 4;
                vp.Height -= 25;
                vp.Width = vp.Height;
                vp.X = original.Width - vp.Width - 18;
                vp.Y = 18;
                GraphicsDevice.Viewport = vp;
                GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
                _levelList[_activeLevel].ACTIVEMISSION.BOARD.NextShape.Draw(_camera.NextShapeView, _camera.ShapeProjection, Matrix.Identity);
                GraphicsDevice.Viewport = original;
            }

            //draw safe area
            //_safeArea.Draw();

            base.Draw(gameTime);
        }
    }
}

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
    struct LevelConfig
    {
        public int _maxWeight;
        public int _sequenceLength;
        public int _inclinationFactor;
        public float _inclinationSpeed;
        public int _numCores;
        public int _numCoresNaoEliminaveis;
        public float _physicsSpeed;
        public float _physicsFastSpeed;
        public bool _invertCM;

        public LevelConfig(int maxWeight, int sequenceLength, int inclinationFactor, float inclinationSpeed, int numCores, int numCoresNaoEliminaveis, float physicsSpeed, float physicsFastSpeed, bool invertCM)
        {
            _maxWeight = maxWeight;
            _sequenceLength = sequenceLength;
            _inclinationFactor = inclinationFactor;
            _inclinationSpeed = inclinationSpeed;
            _numCores = numCores;
            _numCoresNaoEliminaveis = numCoresNaoEliminaveis;
            _physicsSpeed = physicsSpeed;
            _physicsFastSpeed = physicsFastSpeed;
            _invertCM = invertCM;
        }
    }

    class Level
    {
        int _id;
        List<Mission> _missions;
        List<Texture2D> _intros;
        int missaoActiva;
        bool isOver;
        string _introTexName;
        Texture2D _whiteTexture;
        Rectangle _introTexR, _fullscreen;
        int _activeIntro;
        public const float SURFACE_HEIGHT = 15f; //copia da definicao no Mission.cs

        /*static MissionConfig[] missions =
        {
            new MissionConfig("square", "simpleTile", "simpleTile", "simpleTile", "vulcao", Color.White, Color.Black, new Color(117, 77, 36), 1, new Color(0.5f, 0, 0), new Color(0.6f, 0, 0), new Color(0.7f, 0.6f, 0), new Color(1, 1, 1),.005f,.015f), //missao 1
            //new MissionConfig("rectangle", "simpleTile", "simpleTile", "simpleTile", "oasis", Color.White, Color.Black, Color.Brown, 1, new Color(0, 0, 0.5f), new Color(0, 0, 0.6f), new Color(0, 0.3f, 0.9f), new Color(0, 0.5f, 1.0f)), //missao 2
            new MissionConfig("cross", "simpleTile", "simpleTile", "simpleTile", "esgotos", Color.White, Color.Black, Color.Brown, 1, new Color(96, 57, 19), new Color(177, 76, 36), new Color(140, 98, 57), new Color(166, 124, 82),.005f,.015f), //missao 3
            new MissionConfig("eight", "simpleTile", "simpleTile", "simpleTile", "reactor", Color.White, Color.Black, Color.Brown, 1, new Color(0, 1, 0), new Color(0.5f, 1, 0.5f), new Color(0.5f, 1, 0), new Color(0, 1, 0),.005f,.015f), //missao 4
            new MissionConfig("alternator", "simpleTile", "simpleTile", "simpleTile", "oasis", Color.White, Color.Black, Color.Brown, 1, new Color(0, 0, 0.5f), new Color(0, 0, 0.6f), new Color(0, 0.3f, 0.9f), new Color(0, 0.5f, 1.0f), .005f,.015f), //missao 7
            new MissionConfig("quads", "simpleTile", "simpleTile", "simpleTile", "iceberg", Color.White, Color.Black, Color.Brown, 400, new Color(0, 255, 255), new Color(255, 255, 255), new Color(192, 255, 255), new Color(64, 255, 255),.005f,.015f), //missao 5
            new MissionConfig("asymmetric", "simpleTile", "simpleTile", "simpleTile", "vulcao", Color.White, Color.Black, new Color(117, 77, 36), 900, new Color(0.5f, 0, 0), new Color(0.6f, 0, 0), new Color(0.7f, 0.6f, 0), new Color(1, 1, 1),.005f,.015f) //missao 6
        };*/

        static MissionConfig[] missions =
        {
            new MissionConfig("square", "simpleTile", "simpleTile", "simpleTile", "vulcao", null, Color.White, Color.Black, new Color(117, 77, 36), 1, .005f,.015f,
                //new Lava(63.0f * 4.0f, -SURFACE_HEIGHT, 0.0005f, new Color(0.5f, 0, 0), new Color(0.6f, 0, 0), new Color(0.7f, 0.6f, 0), new Color(1, 1, 1))), //missao 1
                new HotLava(-SURFACE_HEIGHT),"Ambient_loop"),
            new MissionConfig("cross", "simpleTile", "simpleTile", "simpleTile", "esgotos", null, Color.White, Color.Black, Color.Brown, 1, .005f,.015f,
                new Lava(63.0f * 4.0f, -SURFACE_HEIGHT, 0.0005f, new Color(96, 57, 19), new Color(177, 76, 36), new Color(140, 98, 57), new Color(166, 124, 82)),"loop4"), //missao 3
            new MissionConfig("eight", "simpleTile", "simpleTile", "simpleTile", "reactor", null, Color.White, Color.Black, Color.Brown, 1, .005f,.015f,
                new Lava(63.0f * 4.0f, -SURFACE_HEIGHT, 0.0005f, new Color(0, 1, 0), new Color(0.5f, 1, 0.5f), new Color(0.5f, 1, 0), new Color(0, 1, 0)),"Ambient_loop"), //missao 4
            new MissionConfig("alternator", "simpleTile", "simpleTile", "simpleTile", "oasis", null, Color.White, Color.Black, Color.Brown, 1, .005f,.015f,
                new Water(-SURFACE_HEIGHT),"loop4"), //missao 7
            new MissionConfig("quads", "simpleTile", "simpleTile", "simpleTile", "iceberg", null, Color.White, Color.Black, Color.Brown, 400, .005f,.015f,
                //new Lava(63.0f * 4.0f, -SURFACE_HEIGHT, 0.0005f, new Color(0, 255, 255), new Color(255, 255, 255), new Color(192, 255, 255), new Color(64, 255, 255)),"Ambient_loop"), //missao 5
                new Ice(-SURFACE_HEIGHT), "Ambient_loop"),
            new MissionConfig("asymmetric", "simpleTile", "simpleTile", "simpleTile", "vulcao", null, Color.White, Color.Black, Color.Brown, 900, .005f,.015f,
                //new Lava(63.0f * 4.0f, -SURFACE_HEIGHT, 0.0005f, new Color(0.5f, 0, 0), new Color(0.6f, 0, 0), new Color(0.7f, 0.6f, 0), new Color(1, 1, 1))), //missao 6
                new HotLava(-SURFACE_HEIGHT),"loop4")
        };

        static LevelConfig[] levels =
        {
            new LevelConfig(200, 4, 400, 0.00001f, 4, 0, .005f, .06f, false), //0 - tutorial
            new LevelConfig(150, 4, 150, 0.00005f, 4, 0, .007f, .06f, false), //1 - normal
            new LevelConfig(150, 5, 150, 0.00005f, 5, 0, .007f, .06f, false), //2 - 5 cores + cadeia de 5
            new LevelConfig(150, 4, 150, 0.00005f, 4, 1, .007f, .06f, false), //3 - não remover uma cor
            new LevelConfig(70, 4, 150, 0.00005f, 4, 0, .007f, .06f, true) //4 - CM invertido e mais peso
        };

        public bool ISOVER
        {
            get { return isOver; }
        }
        public Mission ACTIVEMISSION
        {
            get { return _missions[missaoActiva]; }
        }

        public Level(int id, string introTex)
        {
            _id = id;
            _missions = new List<Mission>();
            _intros = new List<Texture2D>();
            isOver = false;
            _introTexName = introTex;
            _activeIntro = 0;

            Board aux;

            if (_id == 0) //tutorial, caso especial
            {
                aux = new Board(missions[0], 20, levels[0]);
                _missions.Add(new Mission(aux, 1, 250, missions[0]));
            }
            else
            {
                for (int i = 0; i < missions.Length; i++)
                {
                    aux = new Board(missions[i], 20, levels[_id]);
                    _missions.Add(new Mission(aux, i + 1, 1000, missions[i]));
                }
            }

            missaoActiva = 0;
        }

        public int ID
        {
            get { return _id; }
        }

        public void GameOver()
        {
            _missions[missaoActiva].BOARD.ApplyColor(Color.Black);
        }

        public void Restart()
        {
            for (int i = 0; i <= missaoActiva; i++)
                _missions[i].Restart();
            missaoActiva = 0;
            isOver = false;
            Player.Instance.CurrentMissionPoints = 0;
            _activeIntro = 0;
        }

        public void AddIntro(string intro)
        {
            Texture2D introTex = ResourceMgr.Instance.Game.Content.Load<Texture2D>(intro);
            _intros.Add(introTex);
        }

        public void NextIntro()
        {
            _activeIntro++;
            if (_activeIntro >= _intros.Count)
            {
                _activeIntro = 0;
                ResourceMgr.Instance.Game.State = GameState.RUNNING;
            }
        }

        public void Initialize()
        {
            _missions[missaoActiva].Initialize();
            _fullscreen = new Rectangle(0, 0, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height);
        }

        public void LoadContent()
        {
            _missions[missaoActiva].LoadContent();
            _whiteTexture = ResourceMgr.Instance.Game.Content.Load<Texture2D>("whiteTexture");
            Texture2D introTex = ResourceMgr.Instance.Game.Content.Load<Texture2D>(_introTexName);
            if (_intros.Count == 0)
                _intros.Add(introTex);
            /*float scaleWidth = ((float)ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width) / 1920.0f;
            float scaleHeight = ((float)ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height) / 1080.0f;
            float scale = Math.Min(scaleWidth, scaleHeight);
            _introTexR = new Rectangle(0, 0, (int)(introTex.Width * scale), (int)(introTex.Height * scale));
            _introTexR.Offset(ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width - _introTexR.Width,
                ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height - _introTexR.Height);*/
            _introTexR = new Rectangle(0, 0, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Width, ResourceMgr.Instance.Game.GraphicsDevice.Viewport.Height);
        }

        public void DrawOpaque()
        {
            _missions[missaoActiva].DrawOpaque();
        }

        public void DrawTransparent()
        {
            _missions[missaoActiva].DrawTransparent();
        }

        public void DrawIntro(SpriteBatch sb)
        {
            sb.Begin(SpriteBlendMode.None, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            //sb.Draw(_whiteTexture, _fullscreen, new Color(44, 46, 56));
            sb.Draw(_intros[_activeIntro], _introTexR, Color.White);
            sb.End();
        }

        public void Update(GameTime gameTime)
        {
            _missions[missaoActiva].Update(gameTime);
            if (_missions[missaoActiva].ISOVER)
            {
                if (missaoActiva + 1 < _missions.Count)
                {
                    Statistics.Instance.AppendToFile(ID, ACTIVEMISSION.ID);
                    Player.Instance.CurrentMissionPoints = 0;
                    missaoActiva++;
                    _missions[missaoActiva].Initialize();
                    _missions[missaoActiva].LoadContent();
                }
                else
                {
                    isOver = true;
                }
            }

        }
    }
}

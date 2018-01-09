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
    struct MissionConfig
    {
        public string _maskTex;
        public string _tileModel;
        public string _borderSideModel;
        public string _borderCornerModel;
        public Color _tile1Color;
        public Color _tile2Color;
        public Color _borderColor;
        public string _sceneryModelOpaque, _sceneryModelTransparent;
        public int _naturalInclinationFactor;
        public float _baseSpeed, _maxSpeed;
        public Surface _surface;
        public string _music;

        public MissionConfig(string maskTex, string tileModel, string borderSideModel, string borderCornerModel, string sceneryModelOpaque, string sceneryModelTransparent, Color tile1Color, Color tile2Color, Color borderColor, int naturalInclinationFactor, float baseSpeed, float maxSpeed, Surface surface, string music)
        {
            _maskTex = maskTex;
            _tileModel = tileModel;
            _borderSideModel = borderSideModel;
            _borderCornerModel = borderCornerModel;
            _tile1Color = tile1Color;
            _tile2Color = tile2Color;
            _borderColor = borderColor;
            _sceneryModelOpaque = sceneryModelOpaque;
            _sceneryModelTransparent = sceneryModelTransparent;
            _naturalInclinationFactor = naturalInclinationFactor;
            _baseSpeed = baseSpeed;
            _maxSpeed = maxSpeed;
            _surface = surface;
            _music = music;
        }
    }

    class Mission
    {
        int _id;
        Board tabuleiro;
        int maxScore;
        bool isOver;

        Surface _surface;
        Model _hand;
        Model _cenarioOpaco, _cenarioTransparente;
        Model _skydome;
        string _sceneryPathOpaque, _sceneryPathTransparent, _music;
        public const float SURFACE_HEIGHT = 15f; //tambem definido em Level.cs

        public float SURFACEHEIGHT
        {
            get { return SURFACE_HEIGHT; }
        }

        public bool ISOVER
        {
            get { return isOver; }
            set { isOver = value; }
        }

        public bool BoardEmpty
        {
            get { return tabuleiro.IsEmpty; }
        }

        public int ID
        {
            get { return _id; }
        }

        public Board BOARD
        {
            get { return tabuleiro; }
        }

        public Surface SURFACE
        {
            get { return _surface; }
        }

        public string Music
        {
            get { return _music; }
        }

        public float GoalPercentage
        {
            get
            {
                float res = Player.Instance.CurrentMissionPoints / (float)maxScore;
                if (res > 1f) return 1f;
                else return res;
            }
        }

        public Mission(Board tab, int id, int _maxScore, MissionConfig config)
        {
            tabuleiro = tab;
            _id = id;
            maxScore = _maxScore;
            isOver = false;
            _music = config._music;

            //_surface = new Lava(63.0f, -SURFACE_HEIGHT, 0.0005f);
            //_surface = new Lava(630.0f, -SURFACE_HEIGHT, 0.0005f);
            //_surface = new Lava(63.0f * 4.0f, -SURFACE_HEIGHT, 0.0005f, config._surfaceColor1, config._surfaceColor2, config._surfaceColor3, config._surfaceColor4);
            _surface = config._surface;
            _sceneryPathOpaque = config._sceneryModelOpaque;
            _sceneryPathTransparent = config._sceneryModelTransparent;
        }

        public void Initialize()
        {
            tabuleiro.Initialize();
            _surface.Initialize();
        }

        public void LoadContent()
        {
            tabuleiro.LoadContent();
            _hand = ResourceMgr.Instance.Game.Content.Load<Model>("hand");
            _cenarioOpaco = ResourceMgr.Instance.Game.Content.Load<Model>(_sceneryPathOpaque);
            _surface.LoadContent();
            _skydome = ResourceMgr.Instance.Game.Content.Load<Model>("skydome");
            if (_sceneryPathTransparent != null) _cenarioTransparente = ResourceMgr.Instance.Game.Content.Load<Model>(_sceneryPathTransparent);
            else _cenarioTransparente = null;
            ResourceMgr.Instance.Game.Music = Music;
        }

        public void Restart()
        {
            tabuleiro.Restart();
            _surface.Restart();
            isOver = false;
        }

        public void Update(GameTime gameTime)
        {
            GameState gs = ResourceMgr.Instance.Game.State;
            if (gs == GameState.RUNNING && !ISOVER)
            {
                Cue c = ResourceMgr.Instance.Game.ActiveMusic;
                if (c.IsPrepared) c.Play();

                tabuleiro.Update(gameTime);
                _surface.Update(gameTime);

                if (Player.Instance.CurrentMissionPoints >= maxScore)
                {
                    ResourceMgr.Instance.Game.State = GameState.MISSION_END;
                    Statistics.Instance.RemainingPiecesAt(ResourceMgr.Instance.Game.Board.NumPieces);
                    HUD.Instance.RemoveAllMessages();
                    Text3DManager.Instance.ClearAllTexts();
                    return;
                }
            }
            if (gs == GameState.MISSION_END)
            {
                tabuleiro.UpdateAfterEnd(gameTime);
                _surface.Update(gameTime);
            }
        }

        public void DrawOpaque()
        {
            Camera cam = ResourceMgr.Instance.Game.Camera;
            DrawModelAmbient(_skydome, cam.View, cam.Projection, Matrix.CreateTranslation(0, _surface.HeightOffset, 0));
            DrawModel(_hand, cam.View, cam.Projection, Matrix.Identity);
            DrawModel(_cenarioOpaco, cam.View, cam.Projection, Matrix.CreateTranslation(0, _surface.HeightOffset, 0));
            _surface.DrawOpaque(cam.View, cam.Projection);
            tabuleiro.DrawOpaque(cam.View, cam.Projection, cam.CameraPosition);
        }

        public void DrawTransparent()
        {
            Camera cam = ResourceMgr.Instance.Game.Camera;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.CullMode = CullMode.None;
            if (_cenarioTransparente != null) DrawModel(_cenarioTransparente, cam.View, cam.Projection, Matrix.CreateTranslation(0, _surface.HeightOffset, 0));
            _surface.DrawTransparent(cam.View, cam.Projection);
            tabuleiro.DrawTransparent(cam.View, cam.Projection, cam.CameraPosition);
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
        }

        public void DrawModel(Model model, Matrix view, Matrix projection, Matrix transform)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            bool highQuality = ResourceMgr.Instance.Game.HighQualityGraphics;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = highQuality;
                    effect.SpecularPower = 100;
                    effect.World = transforms[mesh.ParentBone.Index] * transform;

                    // Use the matrices provided by the chase camera
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }

        public void DrawModelAmbient(Model model, Matrix view, Matrix projection, Matrix transform)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.AmbientLightColor = Color.White.ToVector3();
                    effect.World = transforms[mesh.ParentBone.Index] * transform;

                    // Use the matrices provided by the chase camera
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }
    }
}

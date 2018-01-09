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
    class Piece
    {
        private Vector3 _3dPosition;
        private int[] _indexPosition = new int[3];
        private Model _interior;
        //private Model _frame;
        private int _type;
        private static float _edgeSize = 2.7f;
        private PieceState _state;
        private Color _color;
        private static int _points = 5;

        private bool _visited;

        public Piece(Vector3 pos, int x, int y, int z, int type)
        {
            //_frame = ResourceMgr.Instance.Game.Content.Load<Model>("arame");
            _interior = ResourceMgr.Instance.Game.Content.Load<Model>("esfera");
            _3dPosition = pos;
            _indexPosition[0] = x;
            _indexPosition[1] = y;
            _indexPosition[2] = z;
            _type = type;
            _state = PieceState.INSTABLE;
            _visited = false;
            UpdateColor();
        }

        public Piece Clone()
        {
            Piece res = new Piece(_3dPosition, IndexPositionX, IndexPositionY, IndexPositionZ, _type);
            res._state = _state;
            res._visited = _visited;
            res.UpdateColor();
            return res;
        }

        private void UpdateColor()
        {
            if (_type == 0) _color = Color.DeepPink;
            if (_type == 1) _color = Color.ForestGreen;
            if (_type == 2) _color = Color.RoyalBlue;
            if (_type == 3) _color = Color.Goldenrod;
            if (_type == 4) _color = Color.LightCyan;
            if (_type == 9) _color = Color.Red;//BOMB COLOR
            if (_type < ResourceMgr.Instance.Game.Board.NumColorsNotRemovable) _color = Color.Black;
        }

        public PieceState State
        {
            get { return _state; }
            set { _state = value; }
        }

        public int IndexPositionX
        {
            get { return _indexPosition[0]; }
            set { _indexPosition[0] = value; }
        }

        public int IndexPositionY
        {
            get { return _indexPosition[1]; }
            set { _indexPosition[1] = value; }
        }

        public int IndexPositionZ
        {
            get { return _indexPosition[2]; }
            set { _indexPosition[2] = value; }
        }

        public float PositionX
        {
            get { return _3dPosition.X; }
            set { _3dPosition.X = value; }
        }

        public float PositionY
        {
            get { return _3dPosition.Y; }
            set { _3dPosition.Y = value; }
        }

        public float PositionZ
        {
            get { return _3dPosition.Z; }
            set { _3dPosition.Z = value; }
        }

        public bool Visited
        {
            get { return _visited; }
            set { _visited = value; }
        }

        public int Type
        {
            get { return _type; }
            set
            {
                _type = value;
                UpdateColor();
            }
        }

        public static int Points
        {
            get { return _points; }
        }

        public Color Color
        {
            get
            {
                return _color;
            }
            set { _color = value; }
        }

        public static float EdgeSize
        {
            get { return _edgeSize; }
        }

        public void DrawProjection(Matrix view, Matrix projection, Matrix rotation, float alpha)
        {
            float scale = 0.5f + alpha * 0.5f;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            DrawModel(_interior, view, projection, Matrix.CreateScale(scale, scale, scale) * Matrix.CreateTranslation(PositionX, IndexPositionY * Piece.EdgeSize, PositionZ) * rotation, alpha);
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
        }

        public void Draw(Matrix view, Matrix projection, Matrix rotation)
        {
            DrawModel(_interior, view, projection, Matrix.CreateTranslation(_3dPosition) * rotation, 1);
        }

        private void DrawModel(Model model, Matrix view, Matrix projection, Matrix transform, float alpha)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            bool highQuality = ResourceMgr.Instance.Game.HighQualityGraphics;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.Alpha = alpha;
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = highQuality;
                    effect.SpecularPower = 75;
                    effect.World = transforms[mesh.ParentBone.Index] * transform;

                    effect.DiffuseColor = _color.ToVector3();

                    // Use the matrices provided by the chase camera
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }
    }
}

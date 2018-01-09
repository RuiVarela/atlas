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
    class Shape
    {
        protected Board _board;
        protected Physics _physics;
        protected Piece[] _pieces;
        protected int[] _inactivePieces;
        protected int _halt;//numero de peças k ja colidiram
        protected int _type, _numCores;
        protected int _type2Rotations, _type3Rotations;
        protected float _alpha;
        protected bool _goingUp;
        protected const float ALPHA_SPEED = 0.002f;
        protected const float MIN_ALPHA = 0.2f;
        protected const float MAX_ALPHA = 1f;

        protected Model _line;

        public Shape() { }

        //nao abusar no numero de cores (so suporta 5 + 1 por agora)
        public Shape(Board board, Physics physics, float dropHeight, Random rand, int numCores)
        {
            _board = board;
            _physics = physics;
            _halt = 0;
            _type2Rotations = 0;
            _type3Rotations = 0;
            _numCores = numCores;
            //LOAD CONTENT FUNCTION:?
            _line = ResourceMgr.Instance.Game.Content.Load<Model>("cilindro");
            _goingUp = true;
            _alpha = MIN_ALPHA;


            _type = rand.Next(1, 4);
            if (_type == 1)//2x2
            {
                _pieces = new Piece[4];
                _inactivePieces = new int[4];

                _pieces[0] = new Piece(new Vector3(0, dropHeight, 0), 0, 0, 0, rand.Next(0, _numCores));
                _pieces[1] = new Piece(new Vector3(0, dropHeight, 0), 0, 0, 1, rand.Next(0, _numCores));
                _pieces[3] = new Piece(new Vector3(0, dropHeight, 0), 1, 0, 0, rand.Next(0, _numCores));
                _pieces[2] = new Piece(new Vector3(0, dropHeight, 0), 1, 0, 1, rand.Next(0, _numCores));

                int incX = rand.Next(_board.DimensionX - 1);
                int incZ = rand.Next(_board.DimensionZ - 1);
                while (true)
                {
                    bool invalid = false;
                    foreach (Piece p in _pieces)
                    {
                        if (!_board.ValidTile(p.IndexPositionX + incX, p.IndexPositionZ + incZ))
                            invalid = true;
                    }
                    if (!invalid) break;
                    incX = rand.Next(_board.DimensionX - 1);
                    incZ = rand.Next(_board.DimensionZ - 1);
                }
                for (int i = 0; i < 4; i++)
                {
                    int x = _pieces[i].IndexPositionX += incX;
                    int z = _pieces[i].IndexPositionZ += incZ;
                    _pieces[i].IndexPositionY = _board.GetLowestFreeY(x, z);
                    _pieces[i].PositionX = _board.GetCoord(x, z).X;
                    _pieces[i].PositionZ = _board.GetCoord(x, z).Y;
                }
                return;
            }

            if (_type == 2)//1x4
            {
                _pieces = new Piece[4];
                _inactivePieces = new int[4];

                _pieces[0] = new Piece(new Vector3(0, dropHeight, 0), 0, 0, 0, rand.Next(0, _numCores));
                _pieces[1] = new Piece(new Vector3(0, dropHeight, 0), 0, 0, 1, rand.Next(0, _numCores));
                _pieces[2] = new Piece(new Vector3(0, dropHeight, 0), 0, 0, 2, rand.Next(0, _numCores));
                _pieces[3] = new Piece(new Vector3(0, dropHeight, 0), 0, 0, 3, rand.Next(0, _numCores));

                int incX = rand.Next(_board.DimensionX);
                int incZ = rand.Next(_board.DimensionZ - 3);
                while (true)
                {
                    bool invalid = false;
                    foreach (Piece p in _pieces)
                    {
                        if (!_board.ValidTile(p.IndexPositionX + incX, p.IndexPositionZ + incZ))
                            invalid = true;
                    }
                    if (!invalid) break;
                    incX = rand.Next(_board.DimensionX - 1);
                    incZ = rand.Next(_board.DimensionZ - 1);
                }
                for (int i = 0; i < 4; i++)
                {
                    int x = _pieces[i].IndexPositionX += incX;
                    int z = _pieces[i].IndexPositionZ += incZ;
                    _pieces[i].IndexPositionY = _board.GetLowestFreeY(x, z);
                    _pieces[i].PositionX = _board.GetCoord(x, z).X;
                    _pieces[i].PositionZ = _board.GetCoord(x, z).Y;
                }
                return;
            }

            if (_type == 3)//1x2
            {
                _pieces = new Piece[2];
                _inactivePieces = new int[2];

                _pieces[0] = new Piece(new Vector3(0, dropHeight, 0), 0, 0, 0, rand.Next(0, _numCores));
                _pieces[1] = new Piece(new Vector3(0, dropHeight, 0), 0, 0, 1, rand.Next(0, _numCores));

                int incX = rand.Next(_board.DimensionX);
                int incZ = rand.Next(_board.DimensionZ - 1);
                while (true)
                {
                    bool invalid = false;
                    foreach (Piece p in _pieces)
                    {
                        if (!_board.ValidTile(p.IndexPositionX + incX, p.IndexPositionZ + incZ))
                            invalid = true;
                    }
                    if (!invalid) break;
                    incX = rand.Next(_board.DimensionX - 1);
                    incZ = rand.Next(_board.DimensionZ - 1);
                }
                for (int i = 0; i < 2; i++)
                {
                    int x = _pieces[i].IndexPositionX += incX;
                    int z = _pieces[i].IndexPositionZ += incZ;
                    _pieces[i].IndexPositionY = _board.GetLowestFreeY(x, z);
                    _pieces[i].PositionX = _board.GetCoord(x, z).X;
                    _pieces[i].PositionZ = _board.GetCoord(x, z).Y;
                }
                return;
            }
        }

        public virtual bool isBomb
        {
            get
            {
                return false;
            }
        }

        public Vector3 Center
        {
            get
            {
                int length = _pieces.Length;
                float x = 0; float z = 0;
                for (int i = 0; i < length; i++)
                {
                    x += _pieces[i].PositionX;
                    z += _pieces[i].PositionZ;
                }
                x /= length;
                z /= length;
                return new Vector3(x, _pieces[0].PositionY + Piece.EdgeSize / 2f, z);
            }
        }

        public virtual Vector3 DiscreteCenter
        {
            get
            {
                int length = _pieces.Length;
                float x = 0; float z = 0;
                for (int i = 0; i < length; i++)
                {
                    x += _pieces[i].IndexPositionX;
                    z += _pieces[i].IndexPositionZ;
                }
                x /= length;
                z /= length;
                return new Vector3(x, _pieces[0].IndexPositionY, z);
            }
        }

        private void UpdateCoords()
        {
            foreach (Piece p in _pieces)
            {
                int x = p.IndexPositionX;
                int z = p.IndexPositionZ;
                p.IndexPositionY = _board.GetLowestFreeY(x, z);
                p.PositionX = _board.GetCoord(x, z).X;
                p.PositionZ = _board.GetCoord(x, z).Y;
            }
        }

        private void CorrectIndexes()
        {
            Board b = ResourceMgr.Instance.Game.Board;
            int minX = int.MaxValue;
            int maxX = 0;
            int minZ = int.MaxValue;
            int maxZ = 0;
            foreach (Piece p in _pieces)
            {
                minX = (int)MathHelper.Min(p.IndexPositionX, minX);
                maxX = (int)MathHelper.Max(p.IndexPositionX, maxX);
                minZ = (int)MathHelper.Min(p.IndexPositionZ, minZ);
                maxZ = (int)MathHelper.Max(p.IndexPositionZ, maxZ);
            }
            foreach (Piece p in _pieces)
            {
                if (minX < 0) p.IndexPositionX += -minX;
                if (minZ < 0) p.IndexPositionZ += -minZ;
                if (maxX > b.DimensionX - 1) p.IndexPositionX -= maxX - (b.DimensionX - 1);
                if (maxZ > b.DimensionZ - 1) p.IndexPositionZ -= maxZ - (b.DimensionZ - 1);
            }
        }

        //usar apenas no tipo de peça 2
        private void ApplyOrientation(int rotation)
        {
            switch (rotation)
            {
                case 0:
                    _pieces[0].IndexPositionX = _pieces[1].IndexPositionX = _pieces[2].IndexPositionX = _pieces[3].IndexPositionX = 0;
                    _pieces[0].IndexPositionZ = -1;
                    _pieces[1].IndexPositionZ = 0;
                    _pieces[2].IndexPositionZ = 1;
                    _pieces[3].IndexPositionZ = 2;
                    break;
                case 1:
                    _pieces[0].IndexPositionZ = _pieces[1].IndexPositionZ = _pieces[2].IndexPositionZ = _pieces[3].IndexPositionZ = 0;
                    _pieces[0].IndexPositionX = -1;
                    _pieces[1].IndexPositionX = 0;
                    _pieces[2].IndexPositionX = 1;
                    _pieces[3].IndexPositionX = 2;
                    break;
                case 2:
                    _pieces[0].IndexPositionX = _pieces[1].IndexPositionX = _pieces[2].IndexPositionX = _pieces[3].IndexPositionX = 0;
                    _pieces[0].IndexPositionZ = 2;
                    _pieces[1].IndexPositionZ = 1;
                    _pieces[2].IndexPositionZ = 0;
                    _pieces[3].IndexPositionZ = -1;
                    break;
                case 3:
                    _pieces[0].IndexPositionZ = _pieces[1].IndexPositionZ = _pieces[2].IndexPositionZ = _pieces[3].IndexPositionZ = 0;
                    _pieces[0].IndexPositionX = 2;
                    _pieces[1].IndexPositionX = 1;
                    _pieces[2].IndexPositionX = 0;
                    _pieces[3].IndexPositionX = -1;
                    break;
            }
        }

        //usar apenas no tipo de peça 3
        private void ApplyType3Orientation(int rotation)
        {
            switch (rotation)
            {
                case 0:
                    _pieces[0].IndexPositionX = _pieces[1].IndexPositionX = 0;
                    _pieces[0].IndexPositionZ = 0;
                    _pieces[1].IndexPositionZ = 1;
                    break;
                case 1:
                    _pieces[0].IndexPositionZ = _pieces[1].IndexPositionZ = 0;
                    _pieces[0].IndexPositionX = 0;
                    _pieces[1].IndexPositionX = 1;
                    break;
                case 2:
                    _pieces[0].IndexPositionX = _pieces[1].IndexPositionX = 0;
                    _pieces[0].IndexPositionZ = 1;
                    _pieces[1].IndexPositionZ = 0;
                    break;
                case 3:
                    _pieces[0].IndexPositionZ = _pieces[1].IndexPositionZ = 0;
                    _pieces[0].IndexPositionX = 1;
                    _pieces[1].IndexPositionX = 0;
                    break;
            }
        }

        public virtual void RotateRight()
        {
            if (CollidedOnce()) return;
            ResourceMgr.Instance.Game.Sounds.PlayCue("Laser8");
            if (_type == 1)
            {
                int length = _pieces.Length - 1;
                int temp = _pieces[0].Type;
                for (int i = 0; i < length; i++)
                {
                    _pieces[i].Type = _pieces[i + 1].Type;
                }
                _pieces[length].Type = temp;
                return;
            }
            if (_type == 2)
            {
                int prevRot = _type2Rotations;
                if (_type2Rotations == 0) _type2Rotations = 3;
                else _type2Rotations--;
                Vector3 offsets = DiscreteCenter;
                /*Piece[] prevState = (Piece[])_pieces.Clone();
                for (int i = 0; i < _pieces.Length; i++)
                {
                    prevState[i] = _pieces[i].Clone();
                }*/
                ApplyOrientation(_type2Rotations);
                foreach (Piece p in _pieces)
                {
                    p.IndexPositionX += (int)offsets.X;
                    p.IndexPositionZ += (int)offsets.Z;
                }
                CorrectIndexes();
                /*foreach (Piece p in _pieces)
                {
                    if (!_board.ValidTile(p.IndexPositionX, p.IndexPositionZ))
                    {
                        _pieces = prevState;
                        _type2Rotations = prevRot;
                        return;
                    }
                }*/
                UpdateCoords();
                return;
            }
            if (_type == 3)
            {
                int prevRot = _type3Rotations;
                if (_type3Rotations == 0) _type3Rotations = 3;
                else _type3Rotations--;
                Vector3 offsets = DiscreteCenter;
                /*Piece[] prevState = (Piece[])_pieces.Clone();
                for (int i = 0; i < _pieces.Length; i++)
                {
                    prevState[i] = _pieces[i].Clone();
                }*/
                ApplyType3Orientation(_type3Rotations);
                foreach (Piece p in _pieces)
                {
                    p.IndexPositionX += (int)offsets.X;
                    p.IndexPositionZ += (int)offsets.Z;
                }
                CorrectIndexes();
                /*foreach (Piece p in _pieces)
                {
                    if (!_board.ValidTile(p.IndexPositionX, p.IndexPositionZ))
                    {
                        _pieces = prevState;
                        _type3Rotations = prevRot;
                        return;
                    }
                }*/
                UpdateCoords();
                return;
            }
        }

        public virtual void RotateLeft()
        {
            if (CollidedOnce()) return;
            ResourceMgr.Instance.Game.Sounds.PlayCue("Laser8");
            if (_type == 1)
            {
                int length = _pieces.Length - 1;
                int temp = _pieces[length].Type;
                for (int i = length; i > 0; i--)
                {
                    _pieces[i].Type = _pieces[i - 1].Type;
                }
                _pieces[0].Type = temp;
                return;
            }
            if (_type == 2)
            {
                int prevRot = _type2Rotations;
                _type2Rotations = (_type2Rotations + 1) % 4;
                Vector3 offsets = DiscreteCenter;
                /*Piece[] prevState = (Piece[])_pieces.Clone();
                for (int i = 0; i < _pieces.Length; i++)
                {
                    prevState[i] = _pieces[i].Clone();
                }*/
                ApplyOrientation(_type2Rotations);
                foreach (Piece p in _pieces)
                {
                    p.IndexPositionX += (int)offsets.X;
                    p.IndexPositionZ += (int)offsets.Z;
                }
                CorrectIndexes();
                /*foreach (Piece p in _pieces)
                {
                    if (!_board.ValidTile(p.IndexPositionX, p.IndexPositionZ))
                    {
                        _pieces = prevState;
                        _type2Rotations = prevRot;
                        return;
                    }
                }*/
                UpdateCoords();
                return;
            }
            if (_type == 3)
            {
                int prevRot = _type3Rotations;
                _type3Rotations = (_type3Rotations + 1) % 4;
                Vector3 offsets = DiscreteCenter;
                /*Piece[] prevState = (Piece[])_pieces.Clone();
                for (int i = 0; i < _pieces.Length; i++)
                {
                    prevState[i] = _pieces[i].Clone();
                }*/
                ApplyType3Orientation(_type3Rotations);
                foreach (Piece p in _pieces)
                {
                    p.IndexPositionX += (int)offsets.X;
                    p.IndexPositionZ += (int)offsets.Z;
                }
                CorrectIndexes();
                /*
                foreach (Piece p in _pieces)
                {
                    if (!_board.ValidTile(p.IndexPositionX, p.IndexPositionZ))
                    {
                        _pieces = prevState;
                        _type3Rotations = prevRot;
                        return;
                    }
                }*/
                UpdateCoords();
                return;
            }
        }

        public void MoveUpX()
        {
            if (CollidedOnce()) return;

            foreach (Piece p in _pieces)
            {
                if (!_board.PartiallyValidTile(p.IndexPositionX + 1, p.IndexPositionZ)) return;
                Piece p2 = _board.GetHighestPiece(p.IndexPositionX + 1, p.IndexPositionZ);
                if (p2 == null) continue;
                if (p.PositionY < p2.PositionY + Piece.EdgeSize) return;
            }

            ResourceMgr.Instance.Game.Sounds.PlayCue("move");
            foreach (Piece p in _pieces)
            {
                p.IndexPositionX += 1;
                p.IndexPositionY = _board.GetLowestFreeY(p.IndexPositionX, p.IndexPositionZ);
                p.PositionX = _board.GetCoord((int)p.IndexPositionX, (int)p.IndexPositionZ).X;
            }
        }

        public void MoveDownX()
        {
            if (CollidedOnce()) return;

            foreach (Piece p in _pieces)
            {
                if (!_board.PartiallyValidTile(p.IndexPositionX - 1, p.IndexPositionZ)) return;
                Piece p2 = _board.GetHighestPiece(p.IndexPositionX - 1, p.IndexPositionZ);
                if (p2 == null) continue;
                if (p.PositionY < p2.PositionY + Piece.EdgeSize) return;
            }

            ResourceMgr.Instance.Game.Sounds.PlayCue("move");
            foreach (Piece p in _pieces)
            {
                p.IndexPositionX -= 1;
                p.IndexPositionY = _board.GetLowestFreeY(p.IndexPositionX, p.IndexPositionZ);
                p.PositionX = _board.GetCoord((int)p.IndexPositionX, (int)p.IndexPositionZ).X;
            }
        }

        public void MoveUpZ()
        {
            if (CollidedOnce()) return;

            foreach (Piece p in _pieces)
            {
                if (!_board.PartiallyValidTile(p.IndexPositionX, p.IndexPositionZ + 1)) return;
                Piece p2 = _board.GetHighestPiece(p.IndexPositionX, p.IndexPositionZ + 1);
                if (p2 == null) continue;
                if (p.PositionY < p2.PositionY + Piece.EdgeSize) return;
            }

            ResourceMgr.Instance.Game.Sounds.PlayCue("move");
            foreach (Piece p in _pieces)
            {
                p.IndexPositionZ += 1;
                p.IndexPositionY = _board.GetLowestFreeY(p.IndexPositionX, p.IndexPositionZ);
                p.PositionZ = _board.GetCoord((int)p.IndexPositionX, (int)p.IndexPositionZ).Y;
            }
        }

        public void MoveDownZ()
        {
            if (CollidedOnce()) return;

            foreach (Piece p in _pieces)
            {
                if (!_board.PartiallyValidTile(p.IndexPositionX, p.IndexPositionZ - 1)) return;
                Piece p2 = _board.GetHighestPiece(p.IndexPositionX, p.IndexPositionZ - 1);
                if (p2 == null) continue;
                if (p.PositionY < p2.PositionY + Piece.EdgeSize) return;
            }

            ResourceMgr.Instance.Game.Sounds.PlayCue("move");
            foreach (Piece p in _pieces)
            {
                p.IndexPositionZ -= 1;
                p.IndexPositionY = _board.GetLowestFreeY(p.IndexPositionX, p.IndexPositionZ);
                p.PositionZ = _board.GetCoord((int)p.IndexPositionX, (int)p.IndexPositionZ).Y;
            }
        }

        public bool CompletelyStopped()
        {
            if (_halt == _pieces.Length) return true;
            return false;
        }

        public bool CollidedOnce()
        {
            if (_halt > 0) return true;
            return false;
        }

        public void Update(GameTime gameTime)
        {
            int len = _pieces.Length;
            for (int i = 0; i < len; i++)
            {
                if (_inactivePieces[i] != 0) continue;
                //para evitar problemas devido ao facto de a peça ser lançada enquanto tao a haver remoçoes em cadeia:
                _pieces[i].IndexPositionY = _board.GetLowestFreeY(_pieces[i].IndexPositionX, _pieces[i].IndexPositionZ);
                int res = _physics.Apply(_pieces[i], gameTime);
                if (res == 1) _board.AddPiece(_pieces[i]);
                _halt += res;
                _inactivePieces[i] = res;
            }
            float delta = ALPHA_SPEED * ((float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);
            if (_goingUp)
                _alpha += delta;
            else
                _alpha -= delta;
            if (_alpha >= MAX_ALPHA)
            {
                _alpha = MAX_ALPHA;
                _goingUp = false;
            }
            if (_alpha <= MIN_ALPHA)
            {
                _alpha = MIN_ALPHA;
                _goingUp = true;
            }
        }

        public void DrawProjBalls(Matrix view, Matrix projection, Matrix rotation)
        {
            foreach (Piece p in _pieces)
            {
                p.DrawProjection(view, projection, rotation, _alpha);
            }
        }

        public void DrawLines(Matrix view, Matrix projection, Matrix rotation)
        {
            int len = _pieces.Length;
            for (int i = 0; i < len; i++)
            {
                if (_inactivePieces[i] != 0) continue;
                DrawModelAmbientOnly(_line, view, projection,
                    Matrix.CreateScale(.25f, (_pieces[i].PositionY / Piece.EdgeSize) - 1 - _pieces[i].IndexPositionY, .25f) *
                    Matrix.CreateTranslation(_pieces[i].PositionX, Piece.EdgeSize * (_pieces[i].IndexPositionY + 1), _pieces[i].PositionZ) *
                    rotation, _pieces[i].Color);
            }
        }

        public void Draw(Matrix view, Matrix projection, Matrix rotation)
        {
            int len = _pieces.Length;
            for (int i = 0; i < len; i++)
            {
                if (_inactivePieces[i] != 0) continue;
                _pieces[i].Draw(view, projection, rotation);
            }
        }

        private void DrawModelAmbientOnly(Model model, Matrix view, Matrix projection, Matrix transform, Color col)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.PreferPerPixelLighting = ResourceMgr.Instance.Game.HighQualityGraphics;
                    effect.World = transforms[mesh.ParentBone.Index] * transform;
                    effect.DiffuseColor = col.ToVector3();

                    // Use the matrices provided by the chase camera
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }
    }
}

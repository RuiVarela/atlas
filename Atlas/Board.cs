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
    class Board
    {
        protected Physics _physics;
        protected Model _model, _borderSideModel, _borderCornerModel;
        protected Shape _currentShape, _nextShape;
        protected Random _rand = new Random((int)System.DateTime.Now.Ticks);

        protected Piece[, ,] _grid;
        protected Vector2[,] _gridCoords;
        protected List<Vector3> _cornerPoints;
        protected int _dimensionX;
        protected int _dimensionY;//ALTURA
        protected int _dimensionZ, _inclinationFactor, _naturalInclinationFactor;
        protected Vector2 _cm;
        protected List<Piece> _removeStack, _invalidStack;

        protected float _angleX;
        protected float _angleY;
        protected int _sequenceLength;
        protected bool _invertCM;

        protected float _heightPercentage;
        protected int _weight;
        protected int _maxWeight, _pieceNumColors, _pieceNumColorsNotRemovable;

        protected static float _incSpeed = 0.00005f;

        const float DURATION = 100;
        protected float _time;

        Texture2D _mask;
        string _maskTex, _tileModelPath, _borderSideModelPath, _borderCornerModelPath;
        Color _tile1Color, _tile2Color, _borderColor;
        bool[,] _maskTiles;
        ModelCoord[,] _tileGrid;

        //optimized draw
        VertexBuffer _vbTile1, _vbTile2, _vbBorderSide, _vbBorderCorner;
        IndexBuffer _ibTile1, _ibTile2, _ibBorderSide, _ibBorderCorner;
        BasicEffect _boardBasicEffect;
        VertexDeclaration _vdTile1, _vdTile2, _vdBorderSide, _vdBorderCorner;
        int _strideTile1, _strideTile2, _strideBorderSide, _strideBorderCorner;
        int _vCountTile1, _vCountTile2, _vCountBorderSide, _vCountBorderCorner;
        int _countTile1, _countTile2, _countBorderSide, _countBorderCorner;
        int _pCountTile1, _pCountTile2, _pCountBorderSide, _pCountBorderCorner;

        protected Matrix _rotation;
        protected Vector2 _naturalCM;

        public enum TILE_TYPE
        {
            NONE, NORMAL, BORDER_SIDE, BORDER_CORNER
        };

        public Board(MissionConfig mission, int dimY, LevelConfig level)
        {
            _physics = new Physics(mission._baseSpeed, mission._maxSpeed, level._physicsFastSpeed);
            _maskTex = mission._maskTex;
            _tileModelPath = mission._tileModel;
            _borderCornerModelPath = mission._borderCornerModel;
            _borderSideModelPath = mission._borderSideModel;
            _tile1Color = mission._tile1Color;
            _tile2Color = mission._tile2Color;
            _borderColor = mission._borderColor;
            _dimensionY = dimY;
            _maxWeight = level._maxWeight;
            _inclinationFactor = level._inclinationFactor;
            _incSpeed = level._inclinationSpeed;
            _pieceNumColors = level._numCores;
            _pieceNumColorsNotRemovable = level._numCoresNaoEliminaveis;
            _naturalInclinationFactor = mission._naturalInclinationFactor;
            _sequenceLength = level._sequenceLength;
            _invertCM = level._invertCM;
            _time = DURATION;

            _cornerPoints = new List<Vector3>();
            _removeStack = new List<Piece>();
            _invalidStack = new List<Piece>();
        }

        public void LoadContent()
        {
            _mask = ResourceMgr.Instance.Game.Content.Load<Texture2D>(_maskTex);
            _dimensionX = _mask.Width + 2;
            _dimensionZ = _mask.Height + 2;
            _grid = new Piece[_dimensionX, _dimensionY, _dimensionZ];
            _gridCoords = new Vector2[_dimensionX, _dimensionZ];
            _tileGrid = new ModelCoord[_dimensionX, _dimensionZ];

            CreateTileGrid(_mask);

            _model = ResourceMgr.Instance.Game.Content.Load<Model>(_tileModelPath);
            _borderSideModel = ResourceMgr.Instance.Game.Content.Load<Model>(_borderSideModelPath);
            _borderCornerModel = ResourceMgr.Instance.Game.Content.Load<Model>(_borderCornerModelPath);
            _boardBasicEffect = new BasicEffect(ResourceMgr.Instance.Game.GraphicsDevice, null);

            //fill the coordinate grid
            for (int i = 0; i < _dimensionX; i++)
                for (int j = 0; j < _dimensionZ; j++)
                {
                    if (_dimensionX % 2 == 1)//IMPAR?
                        _gridCoords[i, j].X = -Piece.EdgeSize * (i - _dimensionX / 2);
                    else
                        _gridCoords[i, j].X = -Piece.EdgeSize * (i - _dimensionX / 2) - Piece.EdgeSize / 2;

                    if (_dimensionZ % 2 == 1)//IMPAR?
                        _gridCoords[i, j].Y = -Piece.EdgeSize * (j - _dimensionZ / 2);
                    else
                        _gridCoords[i, j].Y = -Piece.EdgeSize * (j - _dimensionZ / 2) - Piece.EdgeSize / 2;
                }
            CreateTileGridCoord(_mask);

            _rand = new Random((int)System.DateTime.Now.Ticks);
            _currentShape = new Shape(this, _physics, 50, _rand, _pieceNumColors);
            _rand = new Random((int)System.DateTime.Now.Ticks + 1);
            _nextShape = new Shape(this, _physics, 50, _rand, _pieceNumColors);

            _naturalCM = CalculateNaturalCM(_invertCM);

            CreateBuffers();
        }

        public void CreateBuffers()
        {
            List<Vector3> positions = new List<Vector3>();

            //create tile1 buffers
            foreach (ModelCoord mc in _tileGrid)
            {
                if (mc.Type == TILE_TYPE.NORMAL && mc.Color == _tile1Color)
                {
                    positions.Add(mc.Coord);
                }
            }
            _countTile1 = positions.Count;
            _vdTile1 = _model.Meshes[0].MeshParts[0].VertexDeclaration;
            _strideTile1 = _vdTile1.GetVertexStrideSize(0);
            _vCountTile1 = _model.Meshes[0].MeshParts[0].NumVertices;
            _pCountTile1 = _model.Meshes[0].MeshParts[0].PrimitiveCount;
            ReplicateVertexBuffer(_model, positions, out _vbTile1);
            ReplicateIndexBuffer(_model, positions.Count, out _ibTile1);
            positions.Clear();

            //create tile2 buffers
            foreach (ModelCoord mc in _tileGrid)
            {
                if (mc.Type == TILE_TYPE.NORMAL && mc.Color == _tile2Color)
                {
                    positions.Add(mc.Coord);
                }
            }
            _countTile2 = positions.Count;
            _vdTile2 = _model.Meshes[0].MeshParts[0].VertexDeclaration;
            _strideTile2 = _vdTile2.GetVertexStrideSize(0);
            _vCountTile2 = _model.Meshes[0].MeshParts[0].NumVertices;
            _pCountTile2 = _model.Meshes[0].MeshParts[0].PrimitiveCount;
            ReplicateVertexBuffer(_model, positions, out _vbTile2);
            ReplicateIndexBuffer(_model, positions.Count, out _ibTile2);
            positions.Clear();

            //create side tile buffers
            foreach (ModelCoord mc in _tileGrid)
            {
                if (mc.Type == TILE_TYPE.BORDER_SIDE)
                {
                    positions.Add(mc.Coord);
                }
            }
            _countBorderSide = positions.Count;
            _vdBorderSide = _borderSideModel.Meshes[0].MeshParts[0].VertexDeclaration;
            _strideBorderSide = _vdBorderSide.GetVertexStrideSize(0);
            _vCountBorderSide = _borderSideModel.Meshes[0].MeshParts[0].NumVertices;
            _pCountBorderSide = _borderSideModel.Meshes[0].MeshParts[0].PrimitiveCount;
            ReplicateVertexBuffer(_borderSideModel, positions, out _vbBorderSide);
            ReplicateIndexBuffer(_borderSideModel, positions.Count, out _ibBorderSide);
            positions.Clear();

            //create side tile buffers
            foreach (ModelCoord mc in _tileGrid)
            {
                if (mc.Type == TILE_TYPE.BORDER_CORNER)
                {
                    positions.Add(mc.Coord);
                }
            }
            _countBorderCorner = positions.Count;
            _vdBorderCorner = _borderCornerModel.Meshes[0].MeshParts[0].VertexDeclaration;
            _strideBorderCorner = _vdBorderCorner.GetVertexStrideSize(0);
            _vCountBorderCorner = _borderCornerModel.Meshes[0].MeshParts[0].NumVertices;
            _pCountBorderCorner = _borderCornerModel.Meshes[0].MeshParts[0].PrimitiveCount;
            ReplicateVertexBuffer(_borderCornerModel, positions, out _vbBorderCorner);
            ReplicateIndexBuffer(_borderCornerModel, positions.Count, out _ibBorderCorner);
            positions.Clear();
        }

        public void ReplicateVertexBuffer(Model m, List<Vector3> positions, out VertexBuffer vbOut)
        {
            VertexDeclaration vd = m.Meshes[0].MeshParts[0].VertexDeclaration;
            VertexElement[] elements = vd.GetVertexElements();
            int offsetToPosition = -1, offsetToNormal = -1, stride = vd.GetVertexStrideSize(0), vertexCount = m.Meshes[0].MeshParts[0].NumVertices;
            foreach (VertexElement element in elements)
            {
                if (element.VertexElementUsage == VertexElementUsage.Position) offsetToPosition = element.Offset;
                if (element.VertexElementUsage == VertexElementUsage.Normal) offsetToNormal = element.Offset;
            }
            if (offsetToPosition == -1) //vertex buffer nao tem posicoes... quase impossivel, mas just in case
            {
                throw new Exception("Model VB did not contain any position information.");
            }
            byte[] oldVertices = new byte[vertexCount * stride];
            m.Meshes[0].VertexBuffer.GetData(oldVertices);
            byte[] newVertices = new byte[vertexCount * stride * positions.Count];
            int indexToWrite = 0;

            for (int instance = 0; instance < positions.Count; instance++) //for each instance
            {
                //calculate transformation matrix
                Matrix[] bones = new Matrix[m.Bones.Count]; ;
                m.CopyAbsoluteBoneTransformsTo(bones);
                Matrix transformation = bones[0] * Matrix.CreateTranslation(positions[instance]);
                Matrix normalTransform = bones[0];
                normalTransform.Translation = Vector3.Zero; //appears unnecessary, but just in case

                for (int vertex = 0; vertex < vertexCount; vertex++) //for each vertex
                {
                    //read the vertex
                    byte[] vertexBytes = new byte[stride];
                    Array.Copy(oldVertices, vertex * stride, vertexBytes, 0, stride);

                    //CONVERT POSITION
                    //convert to Vector3
                    Vector3 positionV3 = new Vector3(BitConverter.ToSingle(vertexBytes, offsetToPosition + 0),
                        BitConverter.ToSingle(vertexBytes, offsetToPosition + 4),
                        BitConverter.ToSingle(vertexBytes, offsetToPosition + 8));

                    //transform
                    Vector3 newPositionV3 = Vector3.Transform(positionV3, transformation);

                    //write back
                    BitConverter.GetBytes(newPositionV3.X).CopyTo(vertexBytes, offsetToPosition + 0);
                    BitConverter.GetBytes(newPositionV3.Y).CopyTo(vertexBytes, offsetToPosition + 4);
                    BitConverter.GetBytes(newPositionV3.Z).CopyTo(vertexBytes, offsetToPosition + 8);

                    //CONVERT NORMAL
                    if (offsetToNormal != -1) //must exist
                    {
                        //convert to Vector3
                        Vector3 normalV3 = new Vector3(BitConverter.ToSingle(vertexBytes, offsetToNormal + 0),
                            BitConverter.ToSingle(vertexBytes, offsetToNormal + 4),
                            BitConverter.ToSingle(vertexBytes, offsetToNormal + 8));

                        //transform
                        Vector3 newNormalV3 = Vector3.Transform(normalV3, normalTransform);
                        newNormalV3.Normalize();

                        //write back
                        BitConverter.GetBytes(newNormalV3.X).CopyTo(vertexBytes, offsetToNormal + 0);
                        BitConverter.GetBytes(newNormalV3.Y).CopyTo(vertexBytes, offsetToNormal + 4);
                        BitConverter.GetBytes(newNormalV3.Z).CopyTo(vertexBytes, offsetToNormal + 8);
                    }

                    //write in vertex array
                    vertexBytes.CopyTo(newVertices, indexToWrite);
                    indexToWrite += stride;
                }
            }

            //vertex array created, turn into vertex buffer
            vbOut = new VertexBuffer(ResourceMgr.Instance.Game.GraphicsDevice, newVertices.Length, BufferUsage.WriteOnly);
            vbOut.SetData(newVertices);
        }

        public void ReplicateIndexBuffer(Model m, int count, out IndexBuffer ibOut)
        {
            ushort[] oldIndices = new ushort[m.Meshes[0].MeshParts[0].PrimitiveCount * 3];
            m.Meshes[0].IndexBuffer.GetData(oldIndices);
            ushort[] newIndices = new ushort[oldIndices.Length * count];
            int indexToWrite = 0;
            int vertexCount = m.Meshes[0].MeshParts[0].NumVertices;
            if (vertexCount * count > 65536) //we're screwed
            {
                throw new Exception("Modelo com " + vertexCount + " vertices * " + count + " instancias > 65536...");
            }
            for (int instance = 0; instance < count; instance++) //for each instance
            {
                int offsetToApply = instance * vertexCount;
                for (int i = 0; i < oldIndices.Length; i++) //for each index in index buffer
                {
                    newIndices[indexToWrite++] = (ushort)(oldIndices[i] + offsetToApply);
                }
            }
            ibOut = new IndexBuffer(ResourceMgr.Instance.Game.GraphicsDevice, sizeof(uint) * newIndices.Length, BufferUsage.None, IndexElementSize.SixteenBits);
            ibOut.SetData<ushort>(newIndices);
        }

        public void Initialize() { }

        public void Restart()
        {
            _physics.Restart();
            _grid = new Piece[_dimensionX, _dimensionY, _dimensionZ];
            _rand = new Random((int)System.DateTime.Now.Ticks);
            _currentShape = new Shape(this, _physics, 50, _rand, _pieceNumColors);
            _rand = new Random((int)System.DateTime.Now.Ticks + 1);
            _nextShape = new Shape(this, _physics, 50, _rand, _pieceNumColors);
            _angleX = 0;
            _angleY = 0;
            _weight = 0;
            _time = DURATION;
        }

        //retorna true quando tiver terminado as eliminaçoes
        public void UpdateAfterEnd(GameTime gameTime)
        {
            _time -= ((float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);

            ResourceMgr.Instance.Game.Surface.HeightPercentage((float)_weight / (float)_maxWeight);
            CalculateCM(_invertCM);
            CalculateRotationMatrix(gameTime);

            if (_time <= 0)
            {
                _time = DURATION;
                for (int y = DimensionY - 1; y >= 0; y--)
                    for (int x = 0; x < DimensionX; x++)
                        for (int z = 0; z < DimensionZ; z++)
                        {
                            if (_grid[x, y, z] == null) continue;
                            _invalidStack.Add(_grid[x, y, z]);
                            Text3DManager.Instance.AddText3D(new FloatUpText3D("-2",
                                new Vector3(_grid[x, y, z].PositionX, _grid[x, y, z].PositionY, _grid[x, y, z].PositionZ), 2.0f, false, _grid[x, y, z].Color, 0.5f, ResourceMgr.Instance.Game.Text3D, 1.0f)
                                );
                            _grid[x, y, z] = null;
                            _weight--;
                            Player.Instance.DecreasePoints(2);
                            return;
                        }
            }
        }

        public void Update(GameTime gameTime)
        {
            ResourceMgr.Instance.Game.Surface.HeightPercentage((float)_weight / (float)_maxWeight);

            _currentShape.Update(gameTime);
            if (_currentShape.CollidedOnce()) _physics.SpeedUp();//talvez seja melhor usar um speedUp menor?

            //aplica fisica
            int numLandings = 0;
            Profiler.Instance.EnterSection("Aplica fisica");
            foreach (Piece p in _grid)
            {
                if (p == null) continue;
                numLandings += _physics.Apply(p, gameTime);
            }
            Profiler.Instance.ExitSection("Aplica fisica");

            //so aplica remoçao quando alguma peça "aterra" ou a shape cai completamente
            if (numLandings > 0 || _currentShape.CompletelyStopped())
                RemoveAdjacentPieces(_sequenceLength);
            if (_currentShape.CompletelyStopped() && _currentShape.isBomb)
            {
                Bombing(1, _currentShape.DiscreteCenter);
            }

            Profiler.Instance.EnterSection("Calculate CM and rotation");
            CalculateCM(_invertCM);
            CalculateRotationMatrix(gameTime);
            Profiler.Instance.ExitSection("Calculate CM and rotation");

            float futureDelta = Math.Abs(ResourceMgr.Instance.Game.Surface.Height) - Math.Abs(FutureBoardOnlyBB.Min.Y);
            float delta = Math.Abs(ResourceMgr.Instance.Game.Surface.Height) - Math.Abs(CollisionBB.Min.Y);
            float wp = 1 - (delta / (ResourceMgr.Instance.Game.LEVEL.ACTIVEMISSION.SURFACEHEIGHT - 1));//1 = grossura do tabuleiro
            float futurewp = 1 - (futureDelta / (ResourceMgr.Instance.Game.LEVEL.ACTIVEMISSION.SURFACEHEIGHT - 1));//1 = grossura do tabuleiro
            if (futurewp > 1) futurewp = 1;
            HUD.Instance.WeightPercentage = wp;
            _heightPercentage = wp;
            HUD.Instance.FutureWeightPercentage = futurewp;
            ResourceMgr.Instance.Game.DangerPercentage = futurewp;
            if (CollisionBB.Contains(ResourceMgr.Instance.Game.Surface.GetBoundingBox()) == ContainmentType.Intersects)
                ResourceMgr.Instance.Game.GameOver(false);

            if (_currentShape.CompletelyStopped())
            {
                _physics.NormalSpeed();
                _currentShape = _nextShape;
                _rand = new Random((int)System.DateTime.Now.Ticks);
                _nextShape = new Shape(this, _physics, 50, _rand, _pieceNumColors);
            }
        }

        private void CreateTileGridCoord(Texture2D configuration)
        {
            Color[] configData = new Color[configuration.Width * configuration.Height];
            _mask.GetData<Color>(configData);

            int index = 0;

            //fill in coords and basic tile models
            for (int y = _dimensionZ - 1; y >= 0; y--)
            {
                for (int x = 0; x < _dimensionX; x++)
                {
                    if (x == 0 || y == 0 || x == _dimensionX - 1 || y == _dimensionZ - 1)
                    {
                        _tileGrid[x, y] = new ModelCoord(CalculateTileCoord(x, y), null, Color.White, TILE_TYPE.NONE);
                        continue;
                    }
                    Color tileConfig = configData[index++];
                    if (tileConfig.R == 255)
                    {
                        Color c;
                        if ((x + y) % 2 == 0) c = _tile1Color;
                        else c = _tile2Color;
                        _tileGrid[x, y] = new ModelCoord(CalculateTileCoord(x, y), _model, c, TILE_TYPE.NORMAL);
                    }
                    else
                    {
                        _tileGrid[x, y] = new ModelCoord(CalculateTileCoord(x, y), null, Color.White, TILE_TYPE.NONE);
                    }
                }
            }

            //now put in corners and sides
            for (int y = _dimensionZ - 1; y >= 0; y--)
            {
                for (int x = 0; x < _dimensionX; x++)
                {
                    if (GetTileType(x, y) != TILE_TYPE.NONE) continue; //occupied

                    //count normal tiles in the 4 normal directions
                    int count = 0;
                    if (GetTileType(x - 1, y) == TILE_TYPE.NORMAL) count++;
                    if (GetTileType(x + 1, y) == TILE_TYPE.NORMAL) count++;
                    if (GetTileType(x, y - 1) == TILE_TYPE.NORMAL) count++;
                    if (GetTileType(x, y + 1) == TILE_TYPE.NORMAL) count++;

                    if (count == 1) //side
                    {
                        _tileGrid[x, y].Modelo = _borderSideModel;
                        _tileGrid[x, y].Type = TILE_TYPE.BORDER_SIDE;
                        _tileGrid[x, y].Color = _borderColor;
                    }
                    else if (count == 2) //interior corner
                    {
                        _tileGrid[x, y].Modelo = _borderCornerModel;
                        _tileGrid[x, y].Type = TILE_TYPE.BORDER_CORNER;
                        _tileGrid[x, y].Color = _borderColor;
                    }
                    else
                    {
                        if (GetTileType(x - 1, y - 1) == TILE_TYPE.NORMAL ||
                            GetTileType(x - 1, y + 1) == TILE_TYPE.NORMAL ||
                            GetTileType(x + 1, y - 1) == TILE_TYPE.NORMAL ||
                            GetTileType(x + 1, y + 1) == TILE_TYPE.NORMAL) //exterior corner
                        {
                            _tileGrid[x, y].Modelo = _borderCornerModel;
                            _tileGrid[x, y].Type = TILE_TYPE.BORDER_CORNER;
                            _tileGrid[x, y].Color = _borderColor;
                            //_cornerPoints.Add(Vector3.Transform(_borderCornerModel.Meshes[0].BoundingSphere.Center, Matrix.CreateTranslation(_tileGrid[x, y].Coord)));
                            _cornerPoints.Add(Vector3.Transform(new Vector3(-1.35f, -0.9f, -1.35f), Matrix.CreateTranslation(_tileGrid[x, y].Coord)));
                            _cornerPoints.Add(Vector3.Transform(new Vector3(-1.35f, -0.9f, 1.35f), Matrix.CreateTranslation(_tileGrid[x, y].Coord)));
                            _cornerPoints.Add(Vector3.Transform(new Vector3(1.35f, -0.9f, -1.35f), Matrix.CreateTranslation(_tileGrid[x, y].Coord)));
                            _cornerPoints.Add(Vector3.Transform(new Vector3(1.35f, -0.9f, 1.35f), Matrix.CreateTranslation(_tileGrid[x, y].Coord)));
                        }
                    }
                }
            }
        }

        private Board.TILE_TYPE GetTileType(int x, int z)
        {
            if (x < 0 || z < 0 || x >= _dimensionX || z >= _dimensionZ) return TILE_TYPE.NONE;
            return _tileGrid[x, z].Type;
        }

        private Vector3 CalculateTileCoord(int x, int z)
        {
            float resX = 0f, resZ = 0f;
            resX = _gridCoords[0, 0].X - 2.7f * x;
            resZ = _gridCoords[0, 0].Y - 2.7f * z;
            return new Vector3(resX, 0, resZ);
        }

        private void CreateTileGrid(Texture2D configuration)
        {
            Color[] configData = new Color[configuration.Width * configuration.Height];
            _mask.GetData<Color>(configData);
            _maskTiles = new bool[_dimensionX, _dimensionZ];

            for (int i = 0; i < _dimensionX; i++)
                for (int j = 0; j < _dimensionZ; j++)
                    _maskTiles[i, j] = false;

            int index = 0;
            for (int y = _dimensionZ - 2; y >= 1; y--)
            {
                for (int x = 1; x < _dimensionX - 1; x++)
                {
                    Color tileConfig = configData[index++];
                    if (tileConfig.R == 255)
                        _maskTiles[x, y] = true;
                }
            }
        }

        public bool ValidTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _dimensionX || y >= _dimensionZ) return false;
            return _maskTiles[x, y];
        }

        public bool PartiallyValidTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _dimensionX || y >= _dimensionZ) return false;
            return true;
        }

        public Matrix CalculateDestinationRotationMatrix()
        {
            float newAngleX = (float)Math.Atan2(_cm.X, _inclinationFactor) + (float)Math.Atan2(_naturalCM.X, _naturalInclinationFactor);
            float newAngleY = (float)Math.Atan2(_cm.Y, _inclinationFactor) + (float)Math.Atan2(_naturalCM.Y, _naturalInclinationFactor);
            return Matrix.CreateRotationZ(newAngleX) * Matrix.CreateRotationX(-newAngleY);
        }

        protected Vector2 CalculateRotationMatrix(GameTime gt)
        {
            float newAngleX = (float)Math.Atan2(_cm.X, _inclinationFactor) + (float)Math.Atan2(_naturalCM.X, _naturalInclinationFactor);
            float newAngleY = (float)Math.Atan2(_cm.Y, _inclinationFactor) + (float)Math.Atan2(_naturalCM.Y, _naturalInclinationFactor);

            float tmp = _incSpeed * ((float)gt.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);
            if (Math.Abs(newAngleX - _angleX) > 0.001f)
            {
                if (newAngleX - tmp > _angleX) _angleX += tmp;
                if (newAngleX + tmp < _angleX) _angleX -= tmp;
            }
            if (Math.Abs(newAngleY - _angleY) > 0.001f)
            {
                if (newAngleY - tmp > _angleY) _angleY += tmp;
                if (newAngleY + tmp < _angleY) _angleY -= tmp;
            }

            _rotation = Matrix.CreateRotationZ(_angleX) * Matrix.CreateRotationX(-_angleY);
            return new Vector2((float)Math.Tan(_angleX) * _inclinationFactor, (float)Math.Tan(_angleY) * _inclinationFactor);
        }

        public void CalculateCM(bool invert)
        {
            _cm = new Vector2();
            foreach (Piece p in _grid)
            {
                if (p == null) continue;
                int x = p.IndexPositionX;
                int y = p.IndexPositionZ;
                int offsetX = _dimensionX / 2;
                int offsetY = _dimensionZ / 2;

                if (_dimensionX % 2 == 1)//IMPAR?
                    _cm.X += x - offsetX;
                else
                {
                    if (x < offsetX) _cm.X += x - offsetX;
                    if (x >= offsetX) _cm.X += x - (offsetX - 1);
                }

                if (_dimensionZ % 2 == 1)//IMPAR?
                    _cm.Y += y - offsetY;
                else
                {
                    if (y < offsetY) _cm.Y += y - offsetY;
                    if (y >= offsetY) _cm.Y += y - (offsetY - 1);
                }
            }
            if (invert)
            {
                _cm.X = -_cm.X;
                _cm.Y = -_cm.Y;
            }
        }

        public Vector2 CalculateNaturalCM(bool invert)
        {
            int x = 0, y = 0;
            int offsetX = _dimensionX / 2;
            int offsetY = _dimensionZ / 2;
            for (int xx = 0; xx < _dimensionX; xx++)
                for (int yy = 0; yy < _dimensionZ; yy++)
                {
                    if (!_maskTiles[xx, yy]) continue;
                    if (_dimensionX % 2 == 1)//IMPAR?
                        x += xx - offsetX;
                    else
                    {
                        if (xx < offsetX) x += xx - offsetX;
                        if (xx >= offsetX) x += xx - (offsetX - 1);
                    }
                    if (_dimensionZ % 2 == 1)//IMPAR?
                        y += yy - offsetY;
                    else
                    {
                        if (yy < offsetY) y += yy - offsetY;
                        if (yy >= offsetY) y += yy - (offsetY - 1);
                    }
                }
            if (invert) return new Vector2(-x, -y);
            return new Vector2(x, y);
        }

        public bool IsEmpty
        {
            get
            {
                if (_weight == 0) return true;
                else return false;
            }
        }

        public int NumPieces
        {
            get { return _weight; }
        }

        public float CurrentHeightPercentage
        {
            get { return _heightPercentage; }
        }

        public int DimensionX
        {
            get { return _dimensionX; }
        }
        public int DimensionY
        {
            get { return _dimensionY; }
        }
        public int DimensionZ
        {
            get { return _dimensionZ; }
        }

        public int NumColorsNotRemovable
        {
            get { return _pieceNumColorsNotRemovable; }
        }

        public Vector3[] BoardCornerPoints
        {
            get
            {
                int count = _cornerPoints.Count;
                float maxY = 50;//Height + Piece.EdgeSize;
                float minY = ResourceMgr.Instance.Game.Surface.Height;
                Vector3[] res = new Vector3[count * 2];
                for (int i = 0; i < count; i++)
                {
                    //res[i] = Vector3.Transform(_cornerPoints[i], _rotation);
                    res[i] = new Vector3(_cornerPoints[i].X, minY, _cornerPoints[i].Z);
                    res[count + i] = Vector3.Transform(new Vector3(_cornerPoints[i].X, maxY, _cornerPoints[i].Z), _rotation);
                }
                return res;
            }
        }

        public BoundingBox CollisionBB
        {
            get
            {
                return GetRotatedBoundingBox(_rotation);
            }
        }

        public BoundingBox GetRotatedBoundingBox(Matrix rotation)
        {
            List<Vector3> transformed = new List<Vector3>();
            foreach (Vector3 v in _cornerPoints)
            {
                transformed.Add(Vector3.Transform(v, rotation));
            }
            return BoundingBox.CreateFromPoints(transformed);
        }

        public BoundingBox FutureBoardOnlyBB
        {
            get
            {
                return GetRotatedBoundingBox(CalculateDestinationRotationMatrix());
            }
        }

        public Shape CurrentShape
        {
            get { return _currentShape; }
        }

        public Shape NextShape
        {
            get { return _nextShape; }
        }

        //recebe grid index's
        public Piece GetPiece(int x, int y, int z)
        {
            return _grid[x, y, z];
        }

        public List<Piece> REMOVEPIECES
        {
            get { return _removeStack; }
        }

        public List<Piece> InvalidRemovePieces
        {
            get { return _invalidStack; }
        }

        public Piece GetHighestPiece(int x, int z)
        {
            int y = 0;
            while (y < DimensionY && _grid[x, y, z] != null) y++;
            if (y == 0) return _grid[x, y, z];
            else return _grid[x, --y, z];
        }

        public float Height
        {
            get
            {
                float max = 0;
                foreach (Piece p in _grid)
                {
                    if (p == null) continue;
                    max = MathHelper.Max(max, p.PositionY + Piece.EdgeSize);
                }
                return max;
            }
        }

        public int GetLowestFreeY(int x, int z)
        {
            int y = 0;
            while (y < DimensionY && _grid[x, y, z] != null) y++;
            return y;
        }

        public Vector2 GetCoord(int x, int z)
        {
            return _gridCoords[x, z];
        }

        public Physics Physics
        {
            get { return _physics; }
        }

        protected void RemoveMarkedPieces(Piece p)
        {
            int x = p.IndexPositionX;
            int y = p.IndexPositionY;
            int z = p.IndexPositionZ;
            if (p.State == PieceState.REMOVABLE)
            {
                _weight--;
                if (_weight == 0)
                {
                    Player.Instance.IncreasePoints(1000);
                    HUD.Instance.AddTableClearBonus(1000);
                    Statistics.Instance.TableClearAt();
                }
                _removeStack.Add(p);
                if (y + 1 < DimensionY && _grid[x, y + 1, z] == null)
                {
                    _grid[x, y, z] = null;
                    return;
                }
                int i = 1;
                while (y + i < DimensionY && _grid[x, y + i, z] != null)
                {
                    _grid[x, y + i - 1, z] = _grid[x, y + i, z];
                    _grid[x, y + i, z] = null;
                    _grid[x, y + i - 1, z].State = PieceState.INSTABLE;
                    _grid[x, y + i - 1, z].IndexPositionY--;
                    i++;
                }
            }
        }

        protected int CheckIfRemovable(Piece p)//so para peças estaveis
        {
            int count = 1;
            int x = p.IndexPositionX;
            int y = p.IndexPositionY;
            int z = p.IndexPositionZ;
            Piece p2;
            p.Visited = true;
            //UP
            if (y + 1 < DimensionY)
            {
                p2 = _grid[x, y + 1, z];
                if (p2 != null && p.Type == p2.Type && p2.State != PieceState.INSTABLE && !p2.Visited)
                    count += CheckIfRemovable(p2);
            }
            //DOWN
            if (y > 0)
            {
                p2 = _grid[x, y - 1, z];
                if (p2 != null && p.Type == p2.Type && p2.State != PieceState.INSTABLE && !p2.Visited)
                    count += CheckIfRemovable(p2);
            }
            //HORIZONTAL
            if (x > 0)
            {
                p2 = _grid[x - 1, y, z];
                if (p2 != null && p.Type == p2.Type && p2.State != PieceState.INSTABLE && !p2.Visited)
                    count += CheckIfRemovable(p2);
            }

            if (x < DimensionX - 1)
            {
                p2 = _grid[x + 1, y, z];
                if (p2 != null && p.Type == p2.Type && p2.State != PieceState.INSTABLE && !p2.Visited)
                    count += CheckIfRemovable(p2);
            }

            if (z > 0)
            {
                p2 = _grid[x, y, z - 1];
                if (p2 != null && p.Type == p2.Type && p2.State != PieceState.INSTABLE && !p2.Visited)
                    count += CheckIfRemovable(p2);
            }

            if (z < DimensionZ - 1)
            {
                p2 = _grid[x, y, z + 1];
                if (p2 != null && p.Type == p2.Type && p2.State != PieceState.INSTABLE && !p2.Visited)
                    count += CheckIfRemovable(p2);
            }

            return count;
        }

        protected void ResetVisitedState()
        {
            foreach (Piece p in _grid)
            {
                if (p == null) continue;
                p.Visited = false;
            }
        }

        public void ApplyColor(Color col)
        {
            foreach (Piece p in _grid)
            {
                if (p == null) continue;
                p.Color = col;
            }
        }

        protected void Bombing(int radius, Vector3 source)
        {
            int x = (int)source.X;
            int y = (int)source.Y;
            int z = (int)source.Z;

            for (int xi = x - radius; xi <= x + radius; xi++)
            {
                if (xi < 0 || xi >= DimensionX) continue;
                for (int yi = y - radius; yi <= y + radius; yi++)
                {
                    if (yi < 0 || yi >= DimensionY) continue;
                    for (int zi = z - radius; zi <= z + radius; zi++)
                    {
                        if (zi < 0 || zi >= DimensionZ || _grid[xi, yi, zi] == null) continue;
                        _grid[xi, yi, zi].State = PieceState.REMOVABLE;
                    }
                }
            }

            for (y = DimensionY - 1; y >= 0; y--)
                for (x = 0; x < DimensionX; x++)
                    for (z = 0; z < DimensionZ; z++)
                    {
                        if (_grid[x, y, z] == null) continue;
                        RemoveMarkedPieces(_grid[x, y, z]);
                    }
        }

        protected void RemoveAdjacentPieces(int count)
        {
            Profiler.Instance.EnterSection("Remove adjacentes");
            int sequenceSize;
            List<Piece> removeList = new List<Piece>();
            foreach (Piece p in _grid)
            {
                if (p == null || p.State == PieceState.INSTABLE || p.Visited) continue;
                if (p.Type < _pieceNumColorsNotRemovable) continue;
                sequenceSize = CheckIfRemovable(p);
                if (sequenceSize >= count)
                {
                    removeList.Add(p);
                    if (sequenceSize > count)
                    {
                        //COMBO
                        float mult = (float)sequenceSize / (float)count;
                        Statistics.Instance.ComboAt(mult);
                        float points = sequenceSize * Piece.Points * mult;
                        Player.Instance.IncreasePoints((int)points);
                        HUD.Instance.AddComboBonus((int)(points - sequenceSize * Piece.Points));
                        if (mult >= 2)//GANHA BOMBA
                        {
                            HUD.Instance.AddBombBonus();
                            Statistics.Instance.BombAt();
                            _rand = new Random((int)System.DateTime.Now.Ticks);
                            _nextShape = new Bomb(this, _physics, 50, _rand);
                        }
                    }
                    else
                        Player.Instance.IncreasePoints(sequenceSize * Piece.Points);
                }
            }
            ResetVisitedState();
            foreach (Piece p in removeList) MarkRemovable(p);

            ResetVisitedState();
            for (int y = DimensionY - 1; y >= 0; y--)
                for (int x = 0; x < DimensionX; x++)
                    for (int z = 0; z < DimensionZ; z++)
                    {
                        if (_grid[x, y, z] == null) continue;
                        RemoveMarkedPieces(_grid[x, y, z]);
                    }
            Profiler.Instance.ExitSection("Remove adjacentes");
        }

        protected void MarkRemovable(Piece p)
        {
            int x = p.IndexPositionX;
            int y = p.IndexPositionY;
            int z = p.IndexPositionZ;
            Piece p2;
            p.Visited = true;
            p.State = PieceState.REMOVABLE;
            //UP
            if (y + 1 < DimensionY)
            {
                p2 = _grid[x, y + 1, z];
                if (p2 != null && p.Type == p2.Type && p2.State != PieceState.INSTABLE && !p2.Visited)
                    MarkRemovable(p2);
            }
            //DOWN
            if (y > 0)
            {
                p2 = _grid[x, y - 1, z];
                if (p2 != null && p.Type == p2.Type && p2.State != PieceState.INSTABLE && !p2.Visited)
                    MarkRemovable(p2);
            }
            //HORIZONTAL
            if (x > 0)
            {
                p2 = _grid[x - 1, y, z];
                if (p2 != null && p.Type == p2.Type && p2.State != PieceState.INSTABLE && !p2.Visited)
                    MarkRemovable(p2);
            }

            if (x < DimensionX - 1)
            {
                p2 = _grid[x + 1, y, z];
                if (p2 != null && p.Type == p2.Type && p2.State != PieceState.INSTABLE && !p2.Visited)
                    MarkRemovable(p2);
            }

            if (z > 0)
            {
                p2 = _grid[x, y, z - 1];
                if (p2 != null && p.Type == p2.Type && p2.State != PieceState.INSTABLE && !p2.Visited)
                    MarkRemovable(p2);
            }

            if (z < DimensionZ - 1)
            {
                p2 = _grid[x, y, z + 1];
                if (p2 != null && p.Type == p2.Type && p2.State != PieceState.INSTABLE && !p2.Visited)
                    MarkRemovable(p2);
            }
        }

        public void AddPiece(Piece p)
        {
            int x = p.IndexPositionX;
            int y = p.IndexPositionY;
            int z = p.IndexPositionZ;
            Physics.IncrementSpeed();
            if (y < DimensionY && ValidTile(x, z))
            {
                _grid[x, y, z] = p;
                _weight++;
            }
            else
            {
                //add hud msg?
                Player.Instance.DecreasePoints(Piece.Points);
                Text3DManager.Instance.AddText3D(new FloatUpText3D("-5",
                    new Vector3(p.PositionX, p.PositionY, p.PositionZ), 2.0f, false, p.Color, 0.5f, ResourceMgr.Instance.Game.Text3D, 1.0f)
                    );
                _invalidStack.Add(p);
            }
        }

        public void DrawTransparent(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            Game1 gm = ResourceMgr.Instance.Game;
            Profiler.Instance.EnterSection("Draw projected shape");
            if (gm.State == GameState.RUNNING)
                _currentShape.DrawProjBalls(view, projection, _rotation);
            Profiler.Instance.ExitSection("Draw projected shape");
        }

        public void DrawOpaque(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            _removeStack.Clear();
            _invalidStack.Clear();

            Game1 gm = ResourceMgr.Instance.Game;
            Profiler.Instance.EnterSection("Draw board");
            //setup common stuff
            _boardBasicEffect.EnableDefaultLighting();
            _boardBasicEffect.PreferPerPixelLighting = gm.HighQualityGraphics;
            _boardBasicEffect.SpecularPower = 2000.0f;
            _boardBasicEffect.View = view;
            _boardBasicEffect.Projection = projection;
            _boardBasicEffect.World = _rotation;
            _boardBasicEffect.Alpha = 1.0f;
            GraphicsDevice gd = gm.GraphicsDevice;

            //draw tile1
            gd.VertexDeclaration = _vdTile1;
            gd.Vertices[0].SetSource(_vbTile1, 0, _strideTile1);
            gd.Indices = _ibTile1;
            _boardBasicEffect.DiffuseColor = _tile1Color.ToVector3();
            _boardBasicEffect.Begin();
            foreach (EffectPass pass in _boardBasicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vCountTile1 * _countTile1, 0, _countTile1 * _pCountTile1);
                pass.End();
            }
            _boardBasicEffect.End();

            //draw tile2
            gd.VertexDeclaration = _vdTile2;
            gd.Vertices[0].SetSource(_vbTile2, 0, _strideTile2);
            gd.Indices = _ibTile2;
            _boardBasicEffect.DiffuseColor = _tile2Color.ToVector3();
            _boardBasicEffect.Begin();
            foreach (EffectPass pass in _boardBasicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vCountTile2 * _countTile2, 0, _countTile2 * _pCountTile2);
                pass.End();
            }
            _boardBasicEffect.End();

            //draw sides
            gd.VertexDeclaration = _vdBorderSide;
            gd.Vertices[0].SetSource(_vbBorderSide, 0, _strideBorderSide);
            gd.Indices = _ibBorderSide;
            _boardBasicEffect.DiffuseColor = _borderColor.ToVector3();
            _boardBasicEffect.Begin();
            foreach (EffectPass pass in _boardBasicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vCountBorderSide * _countBorderSide, 0, _countBorderSide * _pCountBorderSide);
                pass.End();
            }
            _boardBasicEffect.End();

            //draw corners
            gd.VertexDeclaration = _vdBorderCorner;
            gd.Vertices[0].SetSource(_vbBorderCorner, 0, _strideBorderCorner);
            gd.Indices = _ibBorderCorner;
            _boardBasicEffect.DiffuseColor = _borderColor.ToVector3();
            _boardBasicEffect.Begin();
            foreach (EffectPass pass in _boardBasicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vCountBorderCorner * _countBorderCorner, 0, _countBorderCorner * _pCountBorderCorner);
                pass.End();
            }
            _boardBasicEffect.End();
            Profiler.Instance.ExitSection("Draw board");

            //draw pieces
            Profiler.Instance.EnterSection("Draw shape and lines");
            if (gm.State == GameState.RUNNING)
            {
                _currentShape.Draw(view, projection, _rotation);
                //_currentShape.DrawLines(view, projection, _rotation);
            }
            Profiler.Instance.ExitSection("Draw shape and lines");
            Profiler.Instance.EnterSection("Draw pieces");
            foreach (Piece p in _grid)
            {
                if (p == null) continue;
                p.Draw(view, projection, _rotation);
            }
            Profiler.Instance.ExitSection("Draw pieces");
        }

        public void DrawShadow(Matrix view, Matrix projection, Vector3 cameraPosition, float height)
        {
            Profiler.Instance.EnterSection("Draw board shadow");
            Matrix projectToPlane = Matrix.CreateScale(1.0f, 0.0f, 1.0f) * Matrix.CreateTranslation(0.0f, height, 0.0f);

            //setup common stuff
            _boardBasicEffect.AmbientLightColor = Color.Black.ToVector3();
            _boardBasicEffect.DiffuseColor = Color.Black.ToVector3();
            _boardBasicEffect.View = view;
            _boardBasicEffect.Projection = projection;
            _boardBasicEffect.World = _rotation * projectToPlane;
            GraphicsDevice gd = ResourceMgr.Instance.Game.GraphicsDevice;

            //draw tile1
            gd.VertexDeclaration = _vdTile1;
            gd.Vertices[0].SetSource(_vbTile1, 0, _strideTile1);
            gd.Indices = _ibTile1;
            _boardBasicEffect.Begin();
            foreach (EffectPass pass in _boardBasicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vCountTile1 * _countTile1, 0, _countTile1 * _pCountTile1);
                pass.End();
            }
            _boardBasicEffect.End();

            //draw tile2
            gd.VertexDeclaration = _vdTile2;
            gd.Vertices[0].SetSource(_vbTile2, 0, _strideTile2);
            gd.Indices = _ibTile2;
            _boardBasicEffect.Begin();
            foreach (EffectPass pass in _boardBasicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vCountTile2 * _countTile2, 0, _countTile2 * _pCountTile2);
                pass.End();
            }
            _boardBasicEffect.End();

            //draw sides
            gd.VertexDeclaration = _vdBorderSide;
            gd.Vertices[0].SetSource(_vbBorderSide, 0, _strideBorderSide);
            gd.Indices = _ibBorderSide;
            _boardBasicEffect.Begin();
            foreach (EffectPass pass in _boardBasicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vCountBorderSide * _countBorderSide, 0, _countBorderSide * _pCountBorderSide);
                pass.End();
            }
            _boardBasicEffect.End();

            //draw corners
            gd.VertexDeclaration = _vdBorderCorner;
            gd.Vertices[0].SetSource(_vbBorderCorner, 0, _strideBorderCorner);
            gd.Indices = _ibBorderCorner;
            _boardBasicEffect.Begin();
            foreach (EffectPass pass in _boardBasicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vCountBorderCorner * _countBorderCorner, 0, _countBorderCorner * _pCountBorderCorner);
                pass.End();
            }
            _boardBasicEffect.End();

            Profiler.Instance.ExitSection("Draw board shadow");
        }
    }

    struct ModelCoord
    {
        private Vector3 _coord;
        private Model _model;
        private Color _color;
        private Board.TILE_TYPE _type;

        public ModelCoord(Vector3 coord, Model modelo, Color color, Board.TILE_TYPE type)
        {
            _coord = coord;
            _model = modelo;
            _color = color;
            _type = type;
        }

        public Model Modelo
        {
            get { return _model; }
            set { _model = value; }
        }

        public Vector3 Coord { get { return _coord; } }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public Board.TILE_TYPE Type
        {
            get { return _type; }
            set { _type = value; }
        }
    }
}

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
    class Camera
    {
        Matrix _view;
        Matrix _projection;
        Matrix _projectionOriginal;

        Matrix _ShapeView;
        Matrix _shapeProjection;

        // Set field of view of the camera in radians (pi/4 is 45 degrees).
        static float _fov = MathHelper.PiOver4;

        // Set distance from the camera of the near and far clipping planes.
        static float _nearClip = 1.0f;
        static float _farClip = 1000.0f;

        const float ZOOM_SPEED = 0.025f;
        const float QUICKTURN_SPEED = 0.005f;

        Vector3 _camPosition;
        Vector3 _shapePos;
        Vector3 _camLookAt;
        float _distance;
        float _shapeDistance;
        float _rotationSpeed = .003f;

        float _qtDestinationAngle;
        bool _quickTurning;

        float _azimuthalAngle;//teta
        float _zenithAngle;//phi

        public void Initialize()
        {
            _camPosition = new Vector3();
            _shapePos = new Vector3();
            _azimuthalAngle = MathHelper.ToRadians(70);
            _zenithAngle = MathHelper.ToRadians(-90);
            _distance = 100;
            _camLookAt = new Vector3(0, 25, 0);
            _quickTurning = false;

            Viewport viewport = ResourceMgr.Instance.Game.GraphicsDevice.Viewport;
            float aspectRatio = (float)viewport.Width / (float)viewport.Height;
            _projectionOriginal = Matrix.CreatePerspectiveFieldOfView(_fov, aspectRatio, _nearClip, _farClip);
            _projection = Matrix.CreatePerspectiveFieldOfView(_fov, aspectRatio, _nearClip, _farClip);
            _shapeProjection = Matrix.CreatePerspectiveFieldOfView(_fov, 1, _nearClip, _farClip);//1 hardcoded
        }

        private bool Contains(BoundingFrustum bf, Vector3[] pts)
        {
            foreach (Vector3 p in pts)
            {
                if (bf.Contains(p) != ContainmentType.Contains)
                    return false;
            }
            return true;
        }

        public void Update(GameTime gt)
        {
            Board board = ResourceMgr.Instance.Game.Board;

            if (_quickTurning)
            {
                if (_zenithAngle > _qtDestinationAngle)
                {
                    _zenithAngle -= QUICKTURN_SPEED * ((float)gt.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);
                    if (_zenithAngle <= _qtDestinationAngle)
                    {
                        _zenithAngle = _qtDestinationAngle;
                        _quickTurning = false;
                    }
                }
                if (_zenithAngle < _qtDestinationAngle)
                {
                    _zenithAngle += QUICKTURN_SPEED * ((float)gt.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);
                    if (_zenithAngle >= _qtDestinationAngle)
                    {
                        _zenithAngle = _qtDestinationAngle;
                        _quickTurning = false;
                    }
                }
            }

            //auto-zoom
            Viewport vp = ResourceMgr.Instance.Game.GraphicsDevice.Viewport;
            float ar = (float)vp.Width / (float)vp.Height;
            float diff = (float)vp.TitleSafeArea.Width / (float)vp.Width;
            BoundingFrustum bf;
            Matrix newFovProj = Matrix.Identity;
            if (diff < 1)
            {
                //safe area
                float h = 1f / (float)Math.Sin(_fov);
                float newFov = (float)Math.Asin(diff / h);
                newFovProj = Matrix.CreatePerspectiveFieldOfView(newFov, ar, _nearClip, _farClip);

                bf = new BoundingFrustum(_view * newFovProj);
            }
            else
                bf = new BoundingFrustum(_view * _projectionOriginal);

            Vector3[] bcps = board.BoardCornerPoints;
            _camLookAt.Y = (50 + ResourceMgr.Instance.Game.Surface.Height) / 2f;

            if (Contains(bf, bcps))
            {
                //see if I can zoom in and still contain
                Vector3 displacement;
                displacement.X = (float)(Math.Sin(_azimuthalAngle) * Math.Cos(_zenithAngle));
                displacement.Z = (float)(Math.Sin(_azimuthalAngle) * Math.Sin(_zenithAngle));
                displacement.Y = (float)Math.Cos(_azimuthalAngle);
                displacement *= (_distance - ZOOM_SPEED * ((float)gt.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond));
                if (displacement.Y <= ResourceMgr.Instance.Game.Surface.Height + 30) displacement.Y = ResourceMgr.Instance.Game.Surface.Height + 30;
                if (diff < 1)
                    bf = new BoundingFrustum(Matrix.CreateLookAt(displacement, _camLookAt, Vector3.UnitY) * newFovProj);
                else
                    bf = new BoundingFrustum(Matrix.CreateLookAt(displacement, _camLookAt, Vector3.UnitY) * _projectionOriginal);
                if (Contains(bf, bcps))
                {
                    //can zoom in
                    _distance -= ZOOM_SPEED * ((float)gt.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);
                }
            }
            else _distance += ZOOM_SPEED * ((float)gt.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);

            //distance calculated, finally update camera position
            _camPosition.X = _distance * (float)(Math.Sin(_azimuthalAngle) * Math.Cos(_zenithAngle));
            _camPosition.Z = _distance * (float)(Math.Sin(_azimuthalAngle) * Math.Sin(_zenithAngle));
            _camPosition.Y = _distance * (float)Math.Cos(_azimuthalAngle);
            if (_camPosition.Y <= ResourceMgr.Instance.Game.Surface.Height + 30) _camPosition.Y = ResourceMgr.Instance.Game.Surface.Height + 30;

            //shape
            _shapeDistance = 15;
            _shapePos.X = _shapeDistance * (float)(Math.Sin(_azimuthalAngle) * Math.Cos(_zenithAngle)) + board.NextShape.Center.X;
            _shapePos.Z = _shapeDistance * (float)(Math.Sin(_azimuthalAngle) * Math.Sin(_zenithAngle)) + board.NextShape.Center.Z;
            _shapePos.Y = _shapeDistance * (float)Math.Cos(_azimuthalAngle) + board.NextShape.Center.Y;

            //update view matrix
            _view = Matrix.CreateLookAt(_camPosition, _camLookAt, Vector3.UnitY);
            _ShapeView = Matrix.CreateLookAt(_shapePos, board.NextShape.Center, Vector3.UnitY);

            //update projection
            float near = float.PositiveInfinity;
            foreach (Vector3 point in bcps)
            {
                near = Math.Min(near, (point - _camPosition).Length());
            }
            near *= 0.7f; //a safety factor
            Viewport viewport = ResourceMgr.Instance.Game.GraphicsDevice.Viewport;
            float aspectRatio = (float)viewport.Width / (float)viewport.Height;
            _projection = Matrix.CreatePerspectiveFieldOfView(_fov, aspectRatio, near, _farClip);
        }

        public void MoveUp(float percentage, GameTime gt)
        {
            _azimuthalAngle -= _rotationSpeed * ((float)gt.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond) * Math.Abs(percentage);
            if (_azimuthalAngle < 0.001f) _azimuthalAngle = 0.001f;

        }

        public void MoveDown(float percentage, GameTime gt)
        {
            float newAngle = _azimuthalAngle + _rotationSpeed * ((float)gt.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond) * Math.Abs(percentage);
            float y = _distance * (float)Math.Cos(newAngle);
            if (y <= ResourceMgr.Instance.Game.Surface.Height + 30) return;
            _azimuthalAngle = newAngle;
        }

        public void QuickTurnRight()
        {
            if (_quickTurning) return;
            float cos = (float)Math.Round(Math.Cos(_zenithAngle), 4);
            float sin = (float)Math.Round(Math.Sin(_zenithAngle), 4);

            if (cos == 0 || cos == 1 || cos == -1)
            {
                _qtDestinationAngle = _zenithAngle - MathHelper.PiOver2;
                _quickTurning = true;
                return;
            }

            if (cos < 0 && sin < 0) { _qtDestinationAngle = -MathHelper.Pi; _quickTurning = true; }
            if (cos > 0 && sin < 0) { _qtDestinationAngle = -MathHelper.PiOver2; _quickTurning = true; }
            if (cos > 0 && sin > 0) { _qtDestinationAngle = 0; _quickTurning = true; }
            if (cos < 0 && sin > 0) { _qtDestinationAngle = MathHelper.PiOver2; _quickTurning = true; }
            _zenithAngle = MathHelper.WrapAngle(_zenithAngle);
        }

        public void QuickTurnLeft()
        {
            if (_quickTurning) return;
            float cos = (float)Math.Round(Math.Cos(_zenithAngle), 4);
            float sin = (float)Math.Round(Math.Sin(_zenithAngle), 4);

            if (cos == 0 || cos == 1 || cos == -1)
            {
                _qtDestinationAngle = _zenithAngle + MathHelper.PiOver2;
                _quickTurning = true;
                return;
            }

            if (cos < 0 && sin < 0) { _qtDestinationAngle = -MathHelper.PiOver2; _quickTurning = true; }
            if (cos > 0 && sin < 0) { _qtDestinationAngle = 0; _quickTurning = true; }
            if (cos > 0 && sin > 0) { _qtDestinationAngle = MathHelper.PiOver2; _quickTurning = true; }
            if (cos < 0 && sin > 0) { _qtDestinationAngle = MathHelper.Pi; _quickTurning = true; }
            _zenithAngle = MathHelper.WrapAngle(_zenithAngle);
        }

        public void MoveRight(float percentage, GameTime gt)
        {
            if (_quickTurning) return;
            _zenithAngle -= _rotationSpeed * ((float)gt.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond) * percentage;
        }

        public void MoveShapeFront()
        {
            Shape shape = ResourceMgr.Instance.Game.Board.CurrentShape;
            float angle = MathHelper.WrapAngle(_zenithAngle + MathHelper.PiOver2);
            //frente
            if (angle <= MathHelper.PiOver4 && angle > -MathHelper.PiOver4)
            {
                shape.MoveUpZ();
                return;
            }
            //direita
            if (angle <= -MathHelper.PiOver4 && angle > -(MathHelper.Pi - MathHelper.PiOver4))
            {
                shape.MoveUpX();
                return;
            }
            //tras
            if (angle <= -(MathHelper.Pi - MathHelper.PiOver4) || angle > MathHelper.Pi - MathHelper.PiOver4)
            {
                shape.MoveDownZ();
                return;
            }
            //esquerda
            if (angle <= MathHelper.Pi - MathHelper.PiOver4 && angle > MathHelper.PiOver4)
            {
                shape.MoveDownX();
                return;
            }
        }

        public void MoveShapeBack()
        {
            Shape shape = ResourceMgr.Instance.Game.Board.CurrentShape;
            float angle = MathHelper.WrapAngle(_zenithAngle + MathHelper.PiOver2);
            //frente
            if (angle <= MathHelper.PiOver4 && angle > -MathHelper.PiOver4)
            {
                shape.MoveDownZ();
                return;
            }
            //direita
            if (angle <= -MathHelper.PiOver4 && angle > -(MathHelper.Pi - MathHelper.PiOver4))
            {
                shape.MoveDownX();
                return;
            }
            //tras
            if (angle <= -(MathHelper.Pi - MathHelper.PiOver4) || angle > MathHelper.Pi - MathHelper.PiOver4)
            {
                shape.MoveUpZ();
                return;
            }
            //esquerda
            if (angle <= MathHelper.Pi - MathHelper.PiOver4 && angle > MathHelper.PiOver4)
            {
                shape.MoveUpX();
                return;
            }
        }

        public void MoveShapeLeft()
        {
            Shape shape = ResourceMgr.Instance.Game.Board.CurrentShape;
            float angle = MathHelper.WrapAngle(_zenithAngle + MathHelper.PiOver2);
            //frente
            if (angle <= MathHelper.PiOver4 && angle > -MathHelper.PiOver4)
            {
                shape.MoveDownX();
                return;
            }
            //direita
            if (angle <= -MathHelper.PiOver4 && angle > -(MathHelper.Pi - MathHelper.PiOver4))
            {
                shape.MoveUpZ();
                return;
            }
            //tras
            if (angle <= -(MathHelper.Pi - MathHelper.PiOver4) || angle > MathHelper.Pi - MathHelper.PiOver4)
            {
                shape.MoveUpX();
                return;
            }
            //esquerda
            if (angle <= MathHelper.Pi - MathHelper.PiOver4 && angle > MathHelper.PiOver4)
            {
                shape.MoveDownZ();
                return;
            }
        }

        public void MoveShapeRight()
        {
            Shape shape = ResourceMgr.Instance.Game.Board.CurrentShape;
            float angle = MathHelper.WrapAngle(_zenithAngle + MathHelper.PiOver2);
            //frente
            if (angle <= MathHelper.PiOver4 && angle > -MathHelper.PiOver4)
            {
                shape.MoveUpX();
                return;
            }
            //direita
            if (angle <= -MathHelper.PiOver4 && angle > -(MathHelper.Pi - MathHelper.PiOver4))
            {
                shape.MoveDownZ();
                return;
            }
            //tras
            if (angle <= -(MathHelper.Pi - MathHelper.PiOver4) || angle > MathHelper.Pi - MathHelper.PiOver4)
            {
                shape.MoveDownX();
                return;
            }
            //esquerda
            if (angle <= MathHelper.Pi - MathHelper.PiOver4 && angle > MathHelper.PiOver4)
            {
                shape.MoveUpZ();
                return;
            }
        }

        public Matrix View
        {
            get { return _view; }
        }

        public Matrix NextShapeView
        {
            get { return _ShapeView; }
        }

        public Matrix Projection
        {
            get { return _projection; }
        }

        public Matrix ShapeProjection
        {
            get { return _shapeProjection; }
        }

        public Vector3 CameraPosition
        {
            get { return _camPosition; }
        }

        public float RotationAngle
        {
            get { return _zenithAngle; }
        }
    }
}

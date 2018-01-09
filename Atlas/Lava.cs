using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Atlas
{
    class Lava : Surface
    {
        VertexPositionTexture[] _vertices;
        Effect _lavaEffect;
        VertexDeclaration _vertexDeclaration;
        float _size, _animationTime, _animationSpeed;
        bool _animationForward;
        Color _surfaceColor1;
        Color _surfaceColor2;
        Color _surfaceColor3;
        Color _surfaceColor4;

        public Lava(float size, float initialHeight, float animationSpeed, Color surfaceColor1, Color surfaceColor2, Color surfaceColor3, Color surfaceColor4)
            : base(initialHeight)
        {
            _size = size;
            _animationSpeed = animationSpeed;
            _animationForward = true;
            _animationTime = 0.0f;
            _surfaceColor1 = surfaceColor1;
            _surfaceColor2 = surfaceColor2;
            _surfaceColor3 = surfaceColor3;
            _surfaceColor4 = surfaceColor4;
        }

        public override void Initialize()
        {
            base.Initialize();
            _vertices = new VertexPositionTexture[6];
            VertexPositionTexture topLeft, topRight, bottomLeft, bottomRight;
            topLeft = new VertexPositionTexture(new Vector3(-_size, _height, _size), new Vector2(0.0f, 1.0f));
            topRight = new VertexPositionTexture(new Vector3(_size, _height, _size), new Vector2(1.0f, 1.0f));
            bottomLeft = new VertexPositionTexture(new Vector3(-_size, _height, -_size), new Vector2(0.0f, 0.0f));
            bottomRight = new VertexPositionTexture(new Vector3(_size, _height, -_size), new Vector2(1.0f, 0.0f));
            _vertices[0] = bottomLeft;
            _vertices[1] = bottomRight;
            _vertices[2] = topRight;

            _vertices[3] = topRight;
            _vertices[4] = topLeft;
            _vertices[5] = bottomLeft;

            _vertexDeclaration = new VertexDeclaration(ResourceMgr.Instance.Game.GraphicsDevice, VertexPositionTexture.VertexElements);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            _lavaEffect = ResourceMgr.Instance.Game.Content.Load<Effect>("lava");
            _lavaEffect.Parameters["CloudTexture1"].SetValue(ResourceMgr.Instance.Game.Content.Load<Texture>("clouds1"));
            _lavaEffect.Parameters["CloudTexture2"].SetValue(ResourceMgr.Instance.Game.Content.Load<Texture>("clouds2"));
            _lavaEffect.Parameters["Color1"].SetValue(_surfaceColor1.ToVector3());
            _lavaEffect.Parameters["Color2"].SetValue(_surfaceColor2.ToVector3());
            _lavaEffect.Parameters["Color3"].SetValue(_surfaceColor3.ToVector3());
            _lavaEffect.Parameters["Color4"].SetValue(_surfaceColor4.ToVector3());
        }

        public override BoundingBox GetBoundingBox()
        {
            return new BoundingBox(_vertices[0].Position, _vertices[3].Position);
        }

        public override void Restart()
        {
            base.Restart();
            _vertices[0].Position.Y = _height;
            _vertices[1].Position.Y = _height;
            _vertices[2].Position.Y = _height;
            _vertices[3].Position.Y = _height;
            _vertices[4].Position.Y = _height;
            _vertices[5].Position.Y = _height;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _vertices[0].Position.Y = _height + _heightOffset;
            _vertices[1].Position.Y = _height + _heightOffset;
            _vertices[2].Position.Y = _height + _heightOffset;
            _vertices[3].Position.Y = _height + _heightOffset;
            _vertices[4].Position.Y = _height + _heightOffset;
            _vertices[5].Position.Y = _height + _heightOffset;

            if (_animationForward)
            {
                _animationTime += ((float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond) * _animationSpeed;
                if (_animationTime > 1.0f)
                {
                    _animationForward = false;
                    _animationTime = 1.0f;
                }
            }
            else
            {
                _animationTime -= ((float)gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond) * _animationSpeed;
                if (_animationTime < 0.0f)
                {
                    _animationForward = true;
                    _animationTime = 0.0f;
                }
            }
            _lavaEffect.Parameters["AnimationTime"].SetValue(_animationTime);
        }

        public override void DrawOpaque(Matrix view, Matrix projection)
        {
            VertexDeclaration old = ResourceMgr.Instance.Game.GraphicsDevice.VertexDeclaration;
            ResourceMgr.Instance.Game.GraphicsDevice.VertexDeclaration = _vertexDeclaration;

            _lavaEffect.Parameters["World"].SetValue(Matrix.Identity);
            _lavaEffect.Parameters["View"].SetValue(view);
            _lavaEffect.Parameters["Projection"].SetValue(projection);

            _lavaEffect.Begin();
            foreach (EffectPass pass in _lavaEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                ResourceMgr.Instance.Game.GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, _vertices, 0, 2);

                pass.End();
            }
            _lavaEffect.End();

            ResourceMgr.Instance.Game.GraphicsDevice.VertexDeclaration = old;

            ResourceMgr.Instance.Game.Board.DrawShadow(view, projection, ResourceMgr.Instance.Game.Camera.CameraPosition, _height + _heightOffset + 0.1f);
        }
    }
}

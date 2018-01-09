using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Atlas
{
    class ExtraFunctions
    {
        static VertexBuffer _boxVertexBuffer = null;
        static VertexBuffer _boxVertexBufferRotate = null;
        static IndexBuffer _boxIndexBuffer = null;
        static VertexDeclaration _boxVertexDeclaration = null;
        static BasicEffect _boxEffect = null;
        static BasicEffect _boxEffectRotate = null;

        public static Vector3 BoxCenter(BoundingBox box)
        {
            return (box.Max + box.Min) / 2.0f;
        }

        public static void DrawBoundingBox(BoundingBox boundingBox, Matrix view, Matrix projection)
        {
            if (_boxVertexBuffer == null) //was never setup
            {
                float alpha = 0.5f;
                if(_boxVertexDeclaration == null) _boxVertexDeclaration = new VertexDeclaration(ResourceMgr.Instance.Game.GraphicsDevice, VertexPositionColor.VertexElements);
                _boxVertexBuffer = new VertexBuffer(ResourceMgr.Instance.Game.GraphicsDevice, VertexPositionColor.SizeInBytes * 24, BufferUsage.WriteOnly);
                if (_boxIndexBuffer == null)
                {
                    _boxIndexBuffer = new IndexBuffer(ResourceMgr.Instance.Game.GraphicsDevice, 2 /*bytes*/ * 6 * 6, BufferUsage.WriteOnly, IndexElementSize.SixteenBits);
                    short[] indices = new short[36];
                    indices[0] = 0;
                    indices[1] = 1;
                    indices[2] = 2;
                    indices[3] = 1;
                    indices[4] = 3;
                    indices[5] = 2;
                    indices[6] = 4;
                    indices[7] = 5;
                    indices[8] = 6;
                    indices[9] = 6;
                    indices[10] = 5;
                    indices[11] = 7;
                    indices[12] = 8;
                    indices[13] = 9;
                    indices[14] = 10;
                    indices[15] = 8;
                    indices[16] = 11;
                    indices[17] = 9;
                    indices[18] = 12;
                    indices[19] = 13;
                    indices[20] = 14;
                    indices[21] = 12;
                    indices[22] = 14;
                    indices[23] = 15;
                    indices[24] = 16;
                    indices[25] = 17;
                    indices[26] = 18;
                    indices[27] = 19;
                    indices[28] = 17;
                    indices[29] = 16;
                    indices[30] = 20;
                    indices[31] = 21;
                    indices[32] = 22;
                    indices[33] = 23;
                    indices[34] = 20;
                    indices[35] = 22;
                    _boxIndexBuffer.SetData<short>(indices);
                }
                _boxEffect = new BasicEffect(ResourceMgr.Instance.Game.GraphicsDevice, null);
                _boxEffect.VertexColorEnabled = true;
                _boxEffect.Alpha = alpha;
                VertexPositionColor[] vertices = new VertexPositionColor[24];

                Vector3 topLeftFront = new Vector3(-0.5f, 0.5f, 0.5f);
                Vector3 bottomLeftFront = new Vector3(-0.5f, -0.5f, 0.5f);
                Vector3 topRightFront = new Vector3(0.5f, 0.5f, 0.5f);
                Vector3 bottomRightFront = new Vector3(0.5f, -0.5f, 0.5f);
                Vector3 topLeftBack = new Vector3(-0.5f, 0.5f, -0.5f);
                Vector3 topRightBack = new Vector3(0.5f, 0.5f, -0.5f);
                Vector3 bottomLeftBack = new Vector3(-0.5f, -0.5f, -0.5f);
                Vector3 bottomRightBack = new Vector3(0.5f, -0.5f, -0.5f);

                //front
                vertices[0] = new VertexPositionColor(topLeftFront, Color.Red);
                vertices[1] = new VertexPositionColor(bottomLeftFront, Color.Red);
                vertices[2] = new VertexPositionColor(topRightFront, Color.Red);
                vertices[3] = new VertexPositionColor(bottomRightFront, Color.Red);

                //back
                vertices[4] = new VertexPositionColor(topLeftBack, Color.Orange);
                vertices[5] = new VertexPositionColor(topRightBack, Color.Orange);
                vertices[6] = new VertexPositionColor(bottomLeftBack, Color.Orange);
                vertices[7] = new VertexPositionColor(bottomRightBack, Color.Orange);

                //top
                vertices[8] = new VertexPositionColor(topLeftFront, Color.Yellow);
                vertices[9] = new VertexPositionColor(topRightBack, Color.Yellow);
                vertices[10] = new VertexPositionColor(topLeftBack, Color.Yellow);
                vertices[11] = new VertexPositionColor(topRightFront, Color.Yellow);

                //bottom
                vertices[12] = new VertexPositionColor(bottomLeftFront, Color.Purple);
                vertices[13] = new VertexPositionColor(bottomLeftBack, Color.Purple);
                vertices[14] = new VertexPositionColor(bottomRightBack, Color.Purple);
                vertices[15] = new VertexPositionColor(bottomRightFront, Color.Purple);

                //left
                vertices[16] = new VertexPositionColor(topLeftFront, Color.Blue);
                vertices[17] = new VertexPositionColor(bottomLeftBack, Color.Blue);
                vertices[18] = new VertexPositionColor(bottomLeftFront, Color.Blue);
                vertices[19] = new VertexPositionColor(topLeftBack, Color.Blue);

                //right
                vertices[20] = new VertexPositionColor(topRightFront, Color.Green);
                vertices[21] = new VertexPositionColor(bottomRightFront, Color.Green);
                vertices[22] = new VertexPositionColor(bottomRightBack, Color.Green);
                vertices[23] = new VertexPositionColor(topRightBack, Color.Green);

                _boxVertexBuffer.SetData<VertexPositionColor>(vertices);
            }

            Matrix scale = Matrix.CreateScale(boundingBox.Max.X - boundingBox.Min.X,
                                              boundingBox.Max.Y - boundingBox.Min.Y,
                                              boundingBox.Max.Z - boundingBox.Min.Z);
            Matrix translation = Matrix.CreateTranslation(BoxCenter(boundingBox));
            Matrix adjustment = scale * translation;

            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.CullMode = CullMode.None;
            ResourceMgr.Instance.Game.GraphicsDevice.Vertices[0].SetSource(_boxVertexBuffer, 0, VertexPositionColor.SizeInBytes);
            ResourceMgr.Instance.Game.GraphicsDevice.Indices = _boxIndexBuffer;
            VertexDeclaration old = ResourceMgr.Instance.Game.GraphicsDevice.VertexDeclaration;
            ResourceMgr.Instance.Game.GraphicsDevice.VertexDeclaration = _boxVertexDeclaration;

            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;

            _boxEffect.Begin();
            _boxEffect.View = view;
            _boxEffect.Projection = projection;
            _boxEffect.World = adjustment;
            foreach (EffectPass pass in _boxEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                ResourceMgr.Instance.Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);
                pass.End();
            }
            _boxEffect.End();

            ResourceMgr.Instance.Game.GraphicsDevice.VertexDeclaration = old;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
        }

        public static void DrawRotatedColoredBox(BoundingBox boundingBox, Matrix view, Matrix projection, Matrix rotation, float alpha, Color color)
        {
            if (_boxVertexBufferRotate == null) //was never setup
            {
                if (_boxVertexDeclaration == null) _boxVertexDeclaration = new VertexDeclaration(ResourceMgr.Instance.Game.GraphicsDevice, VertexPositionColor.VertexElements);
                _boxVertexBufferRotate = new VertexBuffer(ResourceMgr.Instance.Game.GraphicsDevice, VertexPositionColor.SizeInBytes * 24, BufferUsage.WriteOnly);
                if (_boxIndexBuffer == null)
                {
                    _boxIndexBuffer = new IndexBuffer(ResourceMgr.Instance.Game.GraphicsDevice, 2 /*bytes*/ * 6 * 6, BufferUsage.WriteOnly, IndexElementSize.SixteenBits);
                    short[] indices = new short[36];
                    indices[0] = 0;
                    indices[1] = 1;
                    indices[2] = 2;
                    indices[3] = 1;
                    indices[4] = 3;
                    indices[5] = 2;
                    indices[6] = 4;
                    indices[7] = 5;
                    indices[8] = 6;
                    indices[9] = 6;
                    indices[10] = 5;
                    indices[11] = 7;
                    indices[12] = 8;
                    indices[13] = 9;
                    indices[14] = 10;
                    indices[15] = 8;
                    indices[16] = 11;
                    indices[17] = 9;
                    indices[18] = 12;
                    indices[19] = 13;
                    indices[20] = 14;
                    indices[21] = 12;
                    indices[22] = 14;
                    indices[23] = 15;
                    indices[24] = 16;
                    indices[25] = 17;
                    indices[26] = 18;
                    indices[27] = 19;
                    indices[28] = 17;
                    indices[29] = 16;
                    indices[30] = 20;
                    indices[31] = 21;
                    indices[32] = 22;
                    indices[33] = 23;
                    indices[34] = 20;
                    indices[35] = 22;
                    _boxIndexBuffer.SetData<short>(indices);
                }
                _boxEffectRotate = new BasicEffect(ResourceMgr.Instance.Game.GraphicsDevice, null);
                _boxEffectRotate.VertexColorEnabled = true;
                VertexPositionColor[] vertices = new VertexPositionColor[24];

                Vector3 topLeftFront = new Vector3(-0.5f, 0.5f, 0.5f);
                Vector3 bottomLeftFront = new Vector3(-0.5f, -0.5f, 0.5f);
                Vector3 topRightFront = new Vector3(0.5f, 0.5f, 0.5f);
                Vector3 bottomRightFront = new Vector3(0.5f, -0.5f, 0.5f);
                Vector3 topLeftBack = new Vector3(-0.5f, 0.5f, -0.5f);
                Vector3 topRightBack = new Vector3(0.5f, 0.5f, -0.5f);
                Vector3 bottomLeftBack = new Vector3(-0.5f, -0.5f, -0.5f);
                Vector3 bottomRightBack = new Vector3(0.5f, -0.5f, -0.5f);

                //front
                vertices[0] = new VertexPositionColor(topLeftFront, Color.White);
                vertices[1] = new VertexPositionColor(bottomLeftFront, Color.White);
                vertices[2] = new VertexPositionColor(topRightFront, Color.White);
                vertices[3] = new VertexPositionColor(bottomRightFront, Color.White);

                //back
                vertices[4] = new VertexPositionColor(topLeftBack, Color.White);
                vertices[5] = new VertexPositionColor(topRightBack, Color.White);
                vertices[6] = new VertexPositionColor(bottomLeftBack, Color.White);
                vertices[7] = new VertexPositionColor(bottomRightBack, Color.White);

                //top
                vertices[8] = new VertexPositionColor(topLeftFront, Color.White);
                vertices[9] = new VertexPositionColor(topRightBack, Color.White);
                vertices[10] = new VertexPositionColor(topLeftBack, Color.White);
                vertices[11] = new VertexPositionColor(topRightFront, Color.White);

                //bottom
                vertices[12] = new VertexPositionColor(bottomLeftFront, Color.White);
                vertices[13] = new VertexPositionColor(bottomLeftBack, Color.White);
                vertices[14] = new VertexPositionColor(bottomRightBack, Color.White);
                vertices[15] = new VertexPositionColor(bottomRightFront, Color.White);

                //left
                vertices[16] = new VertexPositionColor(topLeftFront, Color.White);
                vertices[17] = new VertexPositionColor(bottomLeftBack, Color.White);
                vertices[18] = new VertexPositionColor(bottomLeftFront, Color.White);
                vertices[19] = new VertexPositionColor(topLeftBack, Color.White);

                //right
                vertices[20] = new VertexPositionColor(topRightFront, Color.White);
                vertices[21] = new VertexPositionColor(bottomRightFront, Color.White);
                vertices[22] = new VertexPositionColor(bottomRightBack, Color.White);
                vertices[23] = new VertexPositionColor(topRightBack, Color.White);

                _boxVertexBufferRotate.SetData<VertexPositionColor>(vertices);
            }

            _boxEffectRotate.Alpha = alpha;
            _boxEffectRotate.DiffuseColor = color.ToVector3();

            Matrix scale = Matrix.CreateScale(boundingBox.Max.X - boundingBox.Min.X,
                                              boundingBox.Max.Y - boundingBox.Min.Y,
                                              boundingBox.Max.Z - boundingBox.Min.Z);
            Matrix translation = Matrix.CreateTranslation(BoxCenter(boundingBox));
            Matrix adjustment = scale * translation * rotation;

            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.CullMode = CullMode.None;
            ResourceMgr.Instance.Game.GraphicsDevice.Vertices[0].SetSource(_boxVertexBufferRotate, 0, VertexPositionColor.SizeInBytes);
            ResourceMgr.Instance.Game.GraphicsDevice.Indices = _boxIndexBuffer;
            VertexDeclaration old = ResourceMgr.Instance.Game.GraphicsDevice.VertexDeclaration;
            ResourceMgr.Instance.Game.GraphicsDevice.VertexDeclaration = _boxVertexDeclaration;

            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;

            _boxEffectRotate.Begin();
            _boxEffectRotate.View = view;
            _boxEffectRotate.Projection = projection;
            _boxEffectRotate.World = adjustment;
            foreach (EffectPass pass in _boxEffectRotate.CurrentTechnique.Passes)
            {
                pass.Begin();
                ResourceMgr.Instance.Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);
                pass.End();
            }
            _boxEffectRotate.End();

            ResourceMgr.Instance.Game.GraphicsDevice.VertexDeclaration = old;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Atlas
{
    class Water : Surface
    {
        public struct VertexData
        {
            public Vector3 Position;
            public Vector2 TextureCoords;

            public static int SizeInBytes = (3 + 2) * 4;
            public static VertexElement[] VertexElements = new VertexElement[]
            {
                 new VertexElement(0, sizeof(float) * 3 * 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                 new VertexElement(0, sizeof(float) * 3 * 1, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0)
            };
        }

        // configuration
        private Vector3 position;
        private float length;
        private int geometry_size;

        //data
        private VertexData[] vertex_data;
        private VertexBuffer vertex_buffer;
        private IndexBuffer index_buffer;
        private short[] vertex_indices;
        VertexDeclaration vertex_declaration;

        private Effect effect;


        // uniforms
        private float wave_frequency;
        private float wave_amplitude;
        private float texture_scale;
        private float bump_height;
        private Vector2 flow_speed;

        private float water_amount;
        private float reflection_amount;
        private Vector4 deep_water_color;
        private Vector4 surface_water_color;
        private Vector4 reflection_color;

        private float fresnel_bias;
        private float fresnel_power;

        private float totalSeconds;

        public Water(float initialHeight)
            : base(initialHeight)
        {

            position = new Vector3(0.0f, _height, 0.0f);
            length = 200.0f;
            geometry_size = 32;
            texture_scale = 16.0f;

            wave_frequency = 0.8f;
            wave_amplitude = 0.2f;
            bump_height = 0.2f;
            flow_speed = new Vector2(0.01f, 0.02f);

            deep_water_color = new Vector4(0.4f, 0.7f, 0.8f, 1.0f);
            surface_water_color = new Vector4(0.5f, 0.8f, 0.9f, 1.0f);
            reflection_color = new Vector4(0.99f, 1.0f, 0.99f, 1.0f);
            water_amount = 0.5f;
            reflection_amount = 0.5f;

            fresnel_bias = 0.025f;
            fresnel_power = 1.0f;

            totalSeconds = 0.0f;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            buildGeometry();

            GraphicsDevice gd = ResourceMgr.Instance.Game.GraphicsDevice;

            vertex_buffer = new VertexBuffer(gd, VertexCount * VertexData.SizeInBytes, BufferUsage.WriteOnly);
            vertex_buffer.SetData(vertex_data);

            index_buffer = new IndexBuffer(gd, typeof(short), IndexCount, BufferUsage.WriteOnly);
            index_buffer.SetData(vertex_indices);

            vertex_declaration = new VertexDeclaration(gd, VertexData.VertexElements);

            effect = ResourceMgr.Instance.Game.Content.Load<Effect>("Water/Water");

            effect.Parameters["cube_map"].SetValue(ResourceMgr.Instance.Game.Content.Load<TextureCube>("Water/cube_map"));
            effect.Parameters["normal_map"].SetValue(ResourceMgr.Instance.Game.Content.Load<Texture2D>("Water/water_normal_map"));

            effect.Parameters["texture_scale"].SetValue(texture_scale);
            effect.Parameters["bump_height"].SetValue(bump_height);
            effect.Parameters["flow_speed"].SetValue(flow_speed);
            effect.Parameters["deep_water_color"].SetValue(deep_water_color);
            effect.Parameters["surface_water_color"].SetValue(surface_water_color);
            effect.Parameters["reflection_color"].SetValue(reflection_color);
            effect.Parameters["water_amount"].SetValue(water_amount);
            effect.Parameters["reflection_amount"].SetValue(reflection_amount);
            effect.Parameters["fresnel_bias"].SetValue(fresnel_bias);
            effect.Parameters["fresnel_power"].SetValue(fresnel_power);

            effect.Parameters["wave_frequency"].SetValue(wave_frequency);
            effect.Parameters["wave_amplitude"].SetValue(wave_amplitude);
        }

        private void buildGeometry()
        {
            vertex_data = new VertexData[VertexCount];

            float increment = length / (float)geometry_size;
            float halfsize = length / 2.0f;

            for (int x = 0; x != geometry_size; ++x)
            {
                for (int z = 0; z != geometry_size; ++z)
                {
                    float position_x = position.X - halfsize + increment * (float)x;
                    float position_z = position.Z - halfsize + increment * (float)z;
                    float position_y = position.Y;
                    vertex_data[z * geometry_size + x].Position = new Vector3(position_x, position_y, position_z);
                    //vertex_data[z * geometry_size + x].Normal = new Vector3(0.0f, 1.0f, 0.0f);
                    vertex_data[z * geometry_size + x].TextureCoords.X = (float)x / (float)geometry_size;
                    vertex_data[z * geometry_size + x].TextureCoords.Y = (float)z / (float)geometry_size;
                }
            }

            vertex_indices = new short[IndexCount];

            for (short x = 0; x != (geometry_size - 1); x++)
            {
                for (short z = 0; z != (geometry_size - 1); z++)
                {
                    vertex_indices[(z * (geometry_size - 1) + x) * 6 + 0] = (short)(x + z * geometry_size);
                    vertex_indices[(z * (geometry_size - 1) + x) * 6 + 1] = (short)((x + 1) + z * geometry_size);
                    vertex_indices[(z * (geometry_size - 1) + x) * 6 + 2] = (short)((x + 1) + (z + 1) * geometry_size);


                    vertex_indices[(z * (geometry_size - 1) + x) * 6 + 3] = (short)((x + 1) + (z + 1) * geometry_size);
                    vertex_indices[(z * (geometry_size - 1) + x) * 6 + 4] = (short)(x + (z + 1) * geometry_size);
                    vertex_indices[(z * (geometry_size - 1) + x) * 6 + 5] = (short)(x + z * geometry_size);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            float finalHeight = _height + _heightOffset;
            for(int i = 0; i < vertex_data.Length; i++)
            {
                vertex_data[i].Position.Y = finalHeight;
            }
            vertex_buffer.SetData(vertex_data);
            totalSeconds = (float)gameTime.TotalRealTime.TotalSeconds;
        }

        public override BoundingBox GetBoundingBox()
        {
            return new BoundingBox(new Vector3(position.X - length, _height + _heightOffset - 1.0f, position.Z - length),
                new Vector3(position.X + length, _height + _heightOffset, position.Z + length)); 
        }

        public override void Restart()
        {
            base.Restart();
            for (int i = 0; i < vertex_data.Length; i++)
            {
                vertex_data[i].Position.Y = _height;
            }
            vertex_buffer.SetData(vertex_data);
        }

        public override void DrawOpaque(Matrix view, Matrix projection)
        {
            GraphicsDevice gd = ResourceMgr.Instance.Game.GraphicsDevice;
            gd.VertexDeclaration = vertex_declaration;
            gd.Vertices[0].SetSource(vertex_buffer, 0, VertexData.SizeInBytes);
            gd.Indices = index_buffer;

            Matrix view_inverted = Matrix.Invert(view);

            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["ViewInverted"].SetValue(view_inverted);
            effect.Parameters["Projection"].SetValue(projection);

            effect.Parameters["time"].SetValue(totalSeconds);


            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexCount, 0, PrimitiveCount);

                pass.End();
            }
            effect.End();
        }

        /**
         * Properties
         */

        private int VertexCount
        {
            get { return geometry_size * geometry_size; }
        }

        private int PrimitiveCount
        {
            get { return (geometry_size - 1) * (geometry_size - 1) * 2; }
        }

        private int IndexCount
        {

            get { return (geometry_size - 1) * (geometry_size - 1) * 6; }
        }
    }
}

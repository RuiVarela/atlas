using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Atlas
{
    class HotLava : Surface
    {
        public struct VertexData
        {
            public Vector3 Position;
            public Vector2 TextureCoords;

            public static int SizeInBytes = (3 + 2) * 4;
            public static VertexElement[] VertexElements = new VertexElement[]
            {
                 new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                 new VertexElement(0, sizeof(float) * 3 * 1, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0)
            };
        }

        public struct ParticleData
        {
            public Vector3 Position;
            public Vector4 Color;

            public static int SizeInBytes = (3 + 4) * 4;
            public static VertexElement[] VertexElements = new VertexElement[]
            {
                 new VertexElement(0, sizeof(float) * 3 * 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                 new VertexElement(0, sizeof(float) * 3 * 1, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.Color, 0),
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
        private VertexDeclaration vertex_declaration;

        private ParticleData[] particle_vertex_data;
        private VertexDeclaration particle_vertex_declaration;
        private VertexBuffer particles_vertex_buffer;

        private int particle_count;
        private Effect effect;
        private Effect point_sprite_effect;

        private float wave_frequency;
        private float wave_amplitude;
        private float texture_scale;
        private Vector2 flow_speed;

        private float diffuse_amount;


        private Texture2D random_texture;
        private RenderTarget2D temporaryRT;
        private RenderTarget2D positionRT;
        private RenderTarget2D velocityRT;
        private DepthStencilBuffer simulationDepthBuffer;
        private SpriteBatch spriteBatch;
        private Effect physics_effect;
        private bool reset_physics = false;


        private float totalSeconds;
        private float last_update;

        public HotLava(float initialHeight)
            : base(initialHeight)
        {
            position = new Vector3(0.0f, 0.0f, 0.0f);
            length = 150.0f;
            geometry_size = 32;
            texture_scale = 2.0f;

            wave_frequency = 3.0f;
            wave_amplitude = 0.3f;
            flow_speed = new Vector2(0.01f, 0.005f);
            diffuse_amount = 0.2f;

            particle_count = 32;

            reset_physics = true;

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
            particle_vertex_declaration = new VertexDeclaration(ResourceMgr.Instance.Game.GraphicsDevice, ParticleData.VertexElements);

            temporaryRT = new RenderTarget2D(ResourceMgr.Instance.Game.GraphicsDevice, particle_count, particle_count, 1, SurfaceFormat.Vector4,
                ResourceMgr.Instance.Game.GraphicsDevice.PresentationParameters.MultiSampleType, 0);
            positionRT = new RenderTarget2D(ResourceMgr.Instance.Game.GraphicsDevice, particle_count, particle_count, 1, SurfaceFormat.Vector4,
                ResourceMgr.Instance.Game.GraphicsDevice.PresentationParameters.MultiSampleType, 0);
            velocityRT = new RenderTarget2D(ResourceMgr.Instance.Game.GraphicsDevice, particle_count, particle_count, 1, SurfaceFormat.Vector4,
                ResourceMgr.Instance.Game.GraphicsDevice.PresentationParameters.MultiSampleType, 0);
            simulationDepthBuffer = new DepthStencilBuffer(ResourceMgr.Instance.Game.GraphicsDevice, particle_count, particle_count, ResourceMgr.Instance.Game.GraphicsDevice.DepthStencilBuffer.Format,
                ResourceMgr.Instance.Game.GraphicsDevice.PresentationParameters.MultiSampleType, 0);
            spriteBatch = new SpriteBatch(ResourceMgr.Instance.Game.GraphicsDevice);


            effect = ResourceMgr.Instance.Game.Content.Load<Effect>("HotLava/HotLava");

            effect.Parameters["diffuse_map"].SetValue(ResourceMgr.Instance.Game.Content.Load<Texture2D>("HotLava/hotlava_texture"));
            effect.Parameters["heat_map"].SetValue(ResourceMgr.Instance.Game.Content.Load<Texture2D>("HotLava/heat"));
            effect.Parameters["fire_map"].SetValue(ResourceMgr.Instance.Game.Content.Load<Texture2D>("HotLava/fire"));

            effect.Parameters["texture_scale"].SetValue(texture_scale);
            effect.Parameters["flow_speed"].SetValue(flow_speed);
            effect.Parameters["diffuse_amount"].SetValue(diffuse_amount);

            effect.Parameters["wave_frequency"].SetValue(wave_frequency);
            effect.Parameters["wave_amplitude"].SetValue(wave_amplitude);



            physics_effect = ResourceMgr.Instance.Game.Content.Load<Effect>("HotLava/Physics");

            Random rand = new Random();

            random_texture = new Texture2D(ResourceMgr.Instance.Game.GraphicsDevice, particle_count, particle_count, 1, TextureUsage.None, SurfaceFormat.Vector4);
            Vector4[] pointsarray = new Vector4[particle_count * particle_count];
            for (int i = 0; i < particle_count * particle_count; i++)
            {
                pointsarray[i] = new Vector4();
                pointsarray[i].X = (float)rand.NextDouble() - 0.5f;
                pointsarray[i].Y = (float)rand.NextDouble() - 0.5f;
                pointsarray[i].Z = (float)rand.NextDouble() - 0.5f;
                pointsarray[i].W = (float)rand.NextDouble() - 0.5f;
            }
            random_texture.SetData<Vector4>(pointsarray);


            point_sprite_effect = ResourceMgr.Instance.Game.Content.Load<Effect>("HotLava/PointSprite");
            point_sprite_effect.Parameters["sprite_map"].SetValue(ResourceMgr.Instance.Game.Content.Load<Texture2D>("HotLava/firesprite"));

            reset_physics = true;
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


            particle_vertex_data = new ParticleData[particle_count * particle_count];
            for (int x = 0; x != particle_count; ++x)
            {
                for (int z = 0; z != particle_count; ++z)
                {
                    particle_vertex_data[z * particle_count + x].Position = new Vector3((float)x / (float)particle_count, 0.0f, (float)z / (float)particle_count);
                    particle_vertex_data[z * particle_count + x].Color = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                }
            }

            particles_vertex_buffer = new VertexBuffer(ResourceMgr.Instance.Game.GraphicsDevice, typeof(ParticleData), particle_count * particle_count, BufferUsage.Points);
            particles_vertex_buffer.SetData<ParticleData>(particle_vertex_data);
        }

        private void DoPhysicsPass(string technique, RenderTarget2D resultTarget)
        {
            ResourceMgr.Instance.Game.GraphicsDevice.SetRenderTarget(0, temporaryRT);
            DepthStencilBuffer oldDepthStencil = ResourceMgr.Instance.Game.GraphicsDevice.DepthStencilBuffer;
            ResourceMgr.Instance.Game.GraphicsDevice.DepthStencilBuffer = simulationDepthBuffer;
            ResourceMgr.Instance.Game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1, 0);

            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);

            physics_effect.CurrentTechnique = physics_effect.Techniques[technique];
            physics_effect.Begin();

            if (!reset_physics)
            {
                physics_effect.Parameters["positionMap"].SetValue(positionRT.GetTexture());
                physics_effect.Parameters["velocityMap"].SetValue(velocityRT.GetTexture());
            }

            physics_effect.CurrentTechnique.Passes[0].Begin();
            // the positionMap and velocityMap are passed through parameters
            // We need to pass a texture to the spriteBatch.Draw() funciton, even if we won't be using it some times.
            spriteBatch.Draw(random_texture, new Rectangle(0, 0, particle_count, particle_count), Color.White);
            physics_effect.CurrentTechnique.Passes[0].End();
            physics_effect.End();

            spriteBatch.End();



            ResourceMgr.Instance.Game.GraphicsDevice.SetRenderTarget(0, resultTarget);

            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            physics_effect.CurrentTechnique = physics_effect.Techniques["CopyTexture"];
            physics_effect.Begin();

            physics_effect.CurrentTechnique.Passes[0].Begin();
            spriteBatch.Draw(temporaryRT.GetTexture(), new Rectangle(0, 0, particle_count, particle_count), Color.White);
            physics_effect.CurrentTechnique.Passes[0].End();
            physics_effect.End();
            spriteBatch.End();

            ResourceMgr.Instance.Game.GraphicsDevice.SetRenderTarget(0, null);
            ResourceMgr.Instance.Game.GraphicsDevice.DepthStencilBuffer = oldDepthStencil;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float finalHeight = _height + _heightOffset;
            for (int i = 0; i < vertex_data.Length; i++)
            {
                vertex_data[i].Position.Y = finalHeight;
            }
            vertex_buffer.SetData(vertex_data);
            totalSeconds = (float)gameTime.TotalRealTime.TotalSeconds;


            float delta_to_update = 1.0f / 40.0f;

            if (((last_update + delta_to_update) < ((float)gameTime.TotalGameTime.TotalSeconds)) || last_update == 0.0f)
            {
                float delta = ((float)gameTime.TotalGameTime.TotalSeconds) - last_update;
                last_update = ((float)gameTime.TotalGameTime.TotalSeconds);
                physics_effect.Parameters["elapsedTime"].SetValue(delta);

                if (reset_physics)
                {
                    DoPhysicsPass("ResetPositions", positionRT);
                    DoPhysicsPass("ResetVelocities", velocityRT);
                    reset_physics = false;
                }

                DoPhysicsPass("UpdateVelocities", velocityRT);
                DoPhysicsPass("UpdatePositions", positionRT);
            }
        }

        public override void Restart()
        {
            base.Restart();
            for (int i = 0; i < vertex_data.Length; i++)
            {
                vertex_data[i].Position.Y = _height;
            }
            vertex_buffer.SetData(vertex_data);
            reset_physics = true;
        }

        public override BoundingBox GetBoundingBox()
        {
            return new BoundingBox(new Vector3(position.X - length, _height + _heightOffset - 1.0f, position.Z - length),
                new Vector3(position.X + length, _height + _heightOffset, position.Z + length));
        }

        public override void DrawOpaque(Matrix view, Matrix projection)
        {
            GraphicsDevice gd = ResourceMgr.Instance.Game.GraphicsDevice;
            gd.VertexDeclaration = vertex_declaration;
            gd.Vertices[0].SetSource(vertex_buffer, 0, VertexData.SizeInBytes);
            gd.Indices = index_buffer;

            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(view);
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

        public override void DrawTransparent(Matrix view, Matrix projection)
        {
            if (reset_physics)
            {
                DoPhysicsPass("ResetPositions", positionRT);
                DoPhysicsPass("ResetVelocities", velocityRT);
                reset_physics = false;
            }
            ResourceMgr.Instance.Game.GraphicsDevice.VertexDeclaration = particle_vertex_declaration;
            ResourceMgr.Instance.Game.GraphicsDevice.Vertices[0].SetSource(particles_vertex_buffer, 0, ParticleData.SizeInBytes);

            point_sprite_effect.Parameters["World"].SetValue(Matrix.Identity);
            point_sprite_effect.Parameters["View"].SetValue(view);
            point_sprite_effect.Parameters["Projection"].SetValue(projection);
            point_sprite_effect.Parameters["positionMap"].SetValue(positionRT.GetTexture());

            point_sprite_effect.Begin(SaveStateMode.SaveState);


            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.PointSpriteEnable = true;
            
            foreach (EffectPass pass in point_sprite_effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                ResourceMgr.Instance.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.PointList, 0, particle_vertex_data.Length);

                pass.End();
            }
            point_sprite_effect.End();

            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.PointSpriteEnable = false;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;


            /*spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            spriteBatch.Draw(positionRT.GetTexture(), new Rectangle(0, 0, particle_count, particle_count), Color.White);
            spriteBatch.Draw(velocityRT.GetTexture(), new Rectangle(0, particle_count, particle_count, particle_count), Color.White);
            spriteBatch.End();*/
        }

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

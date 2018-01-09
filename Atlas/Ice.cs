using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using System;

namespace Atlas
{
    class Ice : Surface
    {
        public struct VertexData
        {
            public Vector3 Position;

            public static int SizeInBytes = (3) * 4;
            public static VertexElement[] VertexElements = new VertexElement[]
            {
                 new VertexElement(0, sizeof(float) * 3 * 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0)
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

        private Texture2D heightmap;
        private Effect effect;
        private Vector4 low_color;
        private Vector4 high_color;
        private float height_scale;

        private Effect snow_effect;

        private Texture2D random_texture;
        private RenderTarget2D temporaryRT;
        private RenderTarget2D positionRT;
        private RenderTarget2D velocityRT;
        private DepthStencilBuffer simulationDepthBuffer;
        private SpriteBatch spriteBatch;
        private Effect physics_effect;
        private bool reset_physics = false;
        private float last_update;


        private int particle_count;

        private float totalSeconds;

        public Ice(float initialHeight)
            : base(initialHeight)
        {
            position = new Vector3(0.0f, _height, 0.0f);
            length = 150.0f;
            geometry_size = 32;


            low_color = new Vector4(0.55f, 0.6f, 0.65f, 1.0f);
            high_color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            height_scale = 7.0f;

            particle_count = 32;

            reset_physics = true;
            last_update = 0.0f;

            totalSeconds = 0.0f;
        }

        /**
        * DrawableGameComponent
        */
        public override void LoadContent()
        {

            heightmap = ResourceMgr.Instance.Game.Content.Load<Texture2D>("Ice/height_map");

            buildGeometry();

            vertex_buffer = new VertexBuffer(ResourceMgr.Instance.Game.GraphicsDevice, VertexCount * VertexData.SizeInBytes, BufferUsage.WriteOnly);
            vertex_buffer.SetData(vertex_data);

            index_buffer = new IndexBuffer(ResourceMgr.Instance.Game.GraphicsDevice, typeof(short), IndexCount, BufferUsage.WriteOnly);
            index_buffer.SetData(vertex_indices);

            vertex_declaration = new VertexDeclaration(ResourceMgr.Instance.Game.GraphicsDevice, VertexData.VertexElements);

            particle_vertex_declaration = new VertexDeclaration(ResourceMgr.Instance.Game.GraphicsDevice, ParticleData.VertexElements);

            effect = ResourceMgr.Instance.Game.Content.Load<Effect>("Ice/Ice");
            effect.Parameters["low_color"].SetValue(low_color);
            effect.Parameters["high_color"].SetValue(high_color);
            effect.Parameters["height_scale"].SetValue(height_scale);

            temporaryRT = new RenderTarget2D(ResourceMgr.Instance.Game.GraphicsDevice, particle_count, particle_count, 1, SurfaceFormat.Vector4,
                ResourceMgr.Instance.Game.GraphicsDevice.PresentationParameters.MultiSampleType, 0);
            positionRT = new RenderTarget2D(ResourceMgr.Instance.Game.GraphicsDevice, particle_count, particle_count, 1, SurfaceFormat.Vector4,
                ResourceMgr.Instance.Game.GraphicsDevice.PresentationParameters.MultiSampleType, 0);
            velocityRT = new RenderTarget2D(ResourceMgr.Instance.Game.GraphicsDevice, particle_count, particle_count, 1, SurfaceFormat.Vector4,
                ResourceMgr.Instance.Game.GraphicsDevice.PresentationParameters.MultiSampleType, 0);
            simulationDepthBuffer = new DepthStencilBuffer(ResourceMgr.Instance.Game.GraphicsDevice, particle_count, particle_count, ResourceMgr.Instance.Game.GraphicsDevice.DepthStencilBuffer.Format,
                ResourceMgr.Instance.Game.GraphicsDevice.PresentationParameters.MultiSampleType, 0);
            spriteBatch = new SpriteBatch(ResourceMgr.Instance.Game.GraphicsDevice);

            physics_effect = ResourceMgr.Instance.Game.Content.Load<Effect>("Ice/SnowPhysics");
            physics_effect.Parameters["windStrength"].SetValue(1.0f * 5.0f);
            physics_effect.Parameters["windDirection"].SetValue(new Vector4(-0.5f, -2.0f, 0.1f, 0.0f));

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


            snow_effect = ResourceMgr.Instance.Game.Content.Load<Effect>("Ice/SnowPointSprite");
            snow_effect.Parameters["sprite_map"].SetValue(ResourceMgr.Instance.Game.Content.Load<Texture2D>("Ice/snow"));

            reset_physics = true;

            base.LoadContent();
        }

        public static float luminance(Vector4 color)
        {
    	    return 0.3f * color.X + 0.6f * color.Y + 0.1f * color.Z;
        }

        private void buildGeometry()
        {
            geometry_size = heightmap.Width;

            Color[] height_map = new Color[geometry_size * geometry_size];
            heightmap.GetData(height_map);

            vertex_data = new VertexData[VertexCount];

            float increment = length / (float)geometry_size;
            float halfsize = length / 2.0f;

            for (int x = 0; x != geometry_size; ++x)
            {
                for (int z = 0; z != geometry_size; ++z)
                {
                    float height = luminance( height_map[z * geometry_size + x].ToVector4() );
                    height = MathHelper.Clamp(height, 0.0f, 1.0f);

                    float position_x = position.X - halfsize + increment * (float)x;
                    float position_z = position.Z - halfsize + increment * (float)z;
                    float position_y = height * height_scale;

                    vertex_data[z * geometry_size + x].Position = new Vector3(position_x, position_y, position_z);
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

        public override BoundingBox GetBoundingBox()
        {
            return new BoundingBox(new Vector3(position.X - length, _height + _heightOffset - 1.0f, position.Z - length),
                new Vector3(position.X + length, _height + _heightOffset, position.Z + length));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            totalSeconds = (float)gameTime.TotalRealTime.TotalSeconds;

            float delta_to_update = 1.0f / 40.0f;
          
            if ( ((last_update + delta_to_update) < ((float)gameTime.TotalGameTime.TotalSeconds)) || last_update == 0.0f)
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
            reset_physics = true;
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

        public override void DrawOpaque(Matrix view, Matrix projection)
        {
            ResourceMgr.Instance.Game.GraphicsDevice.VertexDeclaration = vertex_declaration;
            ResourceMgr.Instance.Game.GraphicsDevice.Vertices[0].SetSource(vertex_buffer, 0, VertexData.SizeInBytes);
            ResourceMgr.Instance.Game.GraphicsDevice.Indices = index_buffer;

            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            effect.Parameters["time"].SetValue(totalSeconds);
            effect.Parameters["base_y"].SetValue(_height);
            

            effect.Begin();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
            
                ResourceMgr.Instance.Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexCount, 0, PrimitiveCount);
              
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

            snow_effect.Parameters["World"].SetValue(Matrix.Identity);
            snow_effect.Parameters["View"].SetValue(view);
            snow_effect.Parameters["Projection"].SetValue(projection);
            snow_effect.Parameters["positionMap"].SetValue(positionRT.GetTexture());

            snow_effect.Begin(SaveStateMode.SaveState);

            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.PointSpriteEnable = true;
            
            foreach (EffectPass pass in snow_effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                ResourceMgr.Instance.Game.GraphicsDevice.DrawPrimitives(PrimitiveType.PointList, 0, particle_vertex_data.Length);

                pass.End();
            }
            snow_effect.End();

            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.PointSpriteEnable = false;
            ResourceMgr.Instance.Game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

           
            /*spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            spriteBatch.Draw(positionRT.GetTexture(), new Rectangle(0, 0, particle_count, particle_count), Color.White);
            spriteBatch.Draw(velocityRT.GetTexture(), new Rectangle(0, particle_count, particle_count, particle_count), Color.White);
            spriteBatch.End();*/
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

        public float Length
        {
            get { return length; }
            set { length = value; }
        }

        public Vector3 Position
        {
            get { return position; } 
            set { position = value; }
        }

    }
}

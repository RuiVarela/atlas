using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Atlas
{
    //taken from
    //http://blogs.msdn.com/manders/archive/2006/11/28/helper-class-to-show-video-safe-areas.aspx
    class SafeArea
    {
        GraphicsDevice graphicsDevice;
        SpriteBatch spriteBatch;
        Texture2D tex; // Holds a 1x1 texture containing a single white texel
        int width; // Viewport width
        int height; // Viewport height
        int dx; // 5% of width
        int dy; // 5% of height
        Color notActionSafeColor = new Color(255, 0, 0, 127); // Red, 50% opacity
        Color notTitleSafeColor = new Color(255, 255, 0, 127); // Yellow, 50% opacity

        public void LoadGraphicsContent(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            spriteBatch = new SpriteBatch(graphicsDevice);
            tex = new Texture2D(graphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            Color[] texData = new Color[1];
            texData[0] = Color.White;
            tex.SetData<Color>(texData);
            width = graphicsDevice.Viewport.Width;
            height = graphicsDevice.Viewport.Height;
            dx = (int)(width * 0.05);
            dy = (int)(height * 0.05);
        }

        public void Draw()
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            // Tint the non-action-safe area red
            spriteBatch.Draw(tex, new Rectangle(0, 0, width, dy), notActionSafeColor);
            spriteBatch.Draw(tex, new Rectangle(0, height - dy, width, dy), notActionSafeColor);
            spriteBatch.Draw(tex, new Rectangle(0, dy, dx, height - 2 * dy), notActionSafeColor);
            spriteBatch.Draw(tex, new Rectangle(width - dx, dy, dx, height - 2 * dy), notActionSafeColor);

            // Tint the non-title-safe area yellow
            spriteBatch.Draw(tex, new Rectangle(dx, dy, width - 2 * dx, dy), notTitleSafeColor);
            spriteBatch.Draw(tex, new Rectangle(dx, height - 2 * dy, width - 2 * dx, dy), notTitleSafeColor);
            spriteBatch.Draw(tex, new Rectangle(dx, 2 * dy, dx, height - 4 * dy), notTitleSafeColor);
            spriteBatch.Draw(tex, new Rectangle(width - 2 * dx, 2 * dy, dx, height - 4 * dy), notTitleSafeColor);
            
            // Tint title-safe area green (de acordo com o que o XNA da)
            //spriteBatch.Draw(tex, graphicsDevice.Viewport.TitleSafeArea, new Color(0, 255, 0, 127));
            spriteBatch.End();
        }
    }
}

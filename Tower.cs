using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoffinGame
{
    public class Tower
    {
        public Rectangle Bounds;
        public int Health;
        public int MaxHealth = 100; // tower hp
        
        public int Level = 0; // default level 

        private float scale = 0.15f; 

        public Tower(int x, int y) // tower info
        {
            int calculatedWidth = (int)(1660 * scale);
            int calculatedHeight = (int)(2509 * scale);

            Bounds = new Rectangle(x, y - calculatedHeight, calculatedWidth, calculatedHeight);
            Health = MaxHealth;
        }
        
        public void Draw(SpriteBatch sb, Texture2D pixel, Texture2D defTex, Texture2D up1Tex, Texture2D up2Tex, SpriteEffects effect) // draw tower and health bar
        {
            Texture2D activeTexture = defTex;
            // tower upgrade texture
            if(Level==1) 
            {
                activeTexture = up1Tex;
            }
            else if(Level>= 2)
            {
                activeTexture = up2Tex;
            }

            sb.Draw(activeTexture, new Vector2(Bounds.X, Bounds.Y), null, Color.White, 0f, Vector2.Zero, scale, effect, 0f);
            
            Rectangle healthBar = new Rectangle(Bounds.X, Bounds.Y - 20, (int)(Bounds.Width * (Health / 100f)), 10);
            sb.Draw(pixel, healthBar, Color.Red);
        }
    }
}
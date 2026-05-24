using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace CoffinGame
{
    public class Bullet {
        public Vector2 Position;
        public Vector2 Velocity;
        public bool FromPlayer1; 
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, 20, 5);
        public void Update(float dt) { Position += Velocity * dt; }
        public void Draw(SpriteBatch sb, Texture2D pixel) { sb.Draw(pixel, Bounds, Color.Yellow); }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;
        private SpriteFont _font; 
        private Texture2D coffinTexture;
        private Texture2D towerDefault;
        private Texture2D towerUpgrade1;
        private Texture2D towerUpgrade2;
        private Texture2D backgroundGothic;

        private Texture2D skelWalk, skelAttack, skelDie;

        enum GameState { Menu, Combat, Shop, Invasion, GameOver } //states
        GameState _currentState = GameState.Menu;
        string winnerText = "";

        Tower player1, player2;
        Coffin mainCoffin;
        float roundTimer = 20f;
        int p1Gold = 0, p2Gold = 0;
        float p1PushPower = 1f, p2PushPower = 1f;
        float coffinVelocity = 0f;

        List<Monster> monsters = new List<Monster>();
        List<Bullet> bullets = new List<Bullet>();
        int waveCount = 0;
        KeyboardState previousState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // screen resolation
            _graphics.PreferredBackBufferWidth = 1920; 
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true; 
            _graphics.ApplyChanges();
        }

        protected override void Initialize() { ResetGame(); base.Initialize(); }

        private void ResetGame()
        {
            player1 = new Tower(10, 850); 
            player2 = new Tower(1660, 850); 
            
            mainCoffin = new Coffin(new Vector2(848, 850), coffinTexture);
            mainCoffin.Health = 150f; // coffin hp
            p1Gold = 0; p2Gold = 0; p1PushPower = 1f; p2PushPower = 1f;
            roundTimer = 20f; coffinVelocity = 0;
            monsters.Clear(); bullets.Clear(); waveCount = 0;
            winnerText = "";
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _font = Content.Load<SpriteFont>("Font"); 
            coffinTexture = Content.Load<Texture2D>("coffin_spritesheet");

            towerDefault = Content.Load<Texture2D>("default");
            towerUpgrade1 = Content.Load<Texture2D>("1");
            towerUpgrade2 = Content.Load<Texture2D>("2");
            backgroundGothic = Content.Load<Texture2D>("background");

            skelWalk = Content.Load<Texture2D>("Skeleton_01_White_Walk");
            skelAttack = Content.Load<Texture2D>("Skeleton_01_White_Attack1");
            skelDie = Content.Load<Texture2D>("Skeleton_01_White_Die");
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState ks = Keyboard.GetState();

            // menu keys
            if (_currentState == GameState.Menu)
            {
                if (ks.IsKeyDown(Keys.Escape)) Exit();
                if (ks.IsKeyDown(Keys.Space) && previousState.IsKeyUp(Keys.Space)) { ResetGame(); _currentState = GameState.Combat; }
            }
            
            // combat keys
            else if (_currentState == GameState.Combat)
            {
                roundTimer -= dt;
                if (ks.IsKeyDown(Keys.D) && previousState.IsKeyUp(Keys.D)) FireBullet(true);
                if (ks.IsKeyDown(Keys.Left) && previousState.IsKeyUp(Keys.Left)) FireBullet(false);

                // hit back bug fix
                if (mainCoffin.Bounds.Intersects(player1.Bounds)) coffinVelocity = 400f;
                if (mainCoffin.Bounds.Intersects(player2.Bounds)) coffinVelocity = -400f;

                coffinVelocity *= 0.94f; // velocity = hız
                mainCoffin.Update(coffinVelocity, dt, gameTime);
                UpdateBullets(dt);

                if (mainCoffin.Health <= 0 || roundTimer <= 0) 
                { 
                    bullets.Clear(); // bullet fix
                    _currentState = GameState.Shop; 
                }
            }

            // shop keys
            else if (_currentState == GameState.Shop)
            {
                if (ks.IsKeyDown(Keys.W) && previousState.IsKeyUp(Keys.W) && p1Gold >= 500) 
                { 
                    p1Gold -= 500; 
                    player1.Level++;
                    p1PushPower += 0.2f; 
                }
                
                if (ks.IsKeyDown(Keys.Up) && previousState.IsKeyUp(Keys.Up) && p2Gold >= 500) 
                { 
                    p2Gold -= 500; 
                    player2.Level++;
                    p2PushPower += 0.2f; 
                }
                
                if (ks.IsKeyDown(Keys.Space) && previousState.IsKeyUp(Keys.Space)) 
                { 
                    bullets.Clear(); // bullet fix
                    waveCount++; 
                    SpawnMonsters(); 
                    _currentState = GameState.Invasion; 
                }
            }

            // invasion keys
            else if (_currentState == GameState.Invasion)
            {
                for (int i = monsters.Count - 1; i >= 0; i--)
                {
                    Monster m = monsters[i];
                    Tower target = m.TargetSide == 1 ? player1 : player2;
                    m.Update(new Vector2(target.Bounds.X, target.Bounds.Y), dt, target.Bounds);

                    if (m.Bounds.Intersects(target.Bounds) && m.CanAttack()) target.Health -= m.Damage;

                    if (m.IsDeadAnimationFinished())
                    {
                        monsters.RemoveAt(i);
                        continue;
                    }

                    if (target == player1 && ks.IsKeyDown(Keys.D) && previousState.IsKeyUp(Keys.D)) FireBullet(true);
                    if (target == player2 && ks.IsKeyDown(Keys.Left) && previousState.IsKeyUp(Keys.Left)) FireBullet(false);
                }

                UpdateBullets(dt);

                // check win state
                if (player1.Health <= 0) { winnerText = "PLAYER 2 WINS!"; _currentState = GameState.GameOver; }
                else if (player2.Health <= 0) { winnerText = "PLAYER 1 WINS!"; _currentState = GameState.GameOver; }

                // round reset
                if (monsters.Count == 0 && bullets.Count == 0) 
                { 
                    bullets.Clear();
                    roundTimer = 20f; 
                    mainCoffin.Health = 150f;
                    mainCoffin.Position.X = 810; 
                    coffinVelocity = 0; 
                    _currentState = GameState.Combat; 
                }
            }

            // game over keys
            else if (_currentState == GameState.GameOver)
            {
                if (ks.IsKeyDown(Keys.Space) && previousState.IsKeyUp(Keys.Space)) _currentState = GameState.Menu;
                if (ks.IsKeyDown(Keys.Escape)) Exit();
            }

            previousState = ks;
            base.Update(gameTime);
        }

        // bullet system
        private void FireBullet(bool fromP1)
        {
            Vector2 start = fromP1 ? new Vector2(player1.Bounds.Right - 30, 810) : new Vector2(player2.Bounds.Left + 10, 810);
            Vector2 velocity = fromP1 ? new Vector2(1600f, 0) : new Vector2(-1600f, 0);
            bullets.Add(new Bullet { Position = start, Velocity = velocity, FromPlayer1 = fromP1 });
        }

        private void UpdateBullets(float dt)
        {
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i].Update(dt);
                bool hit = false;
                
                // coffin hits
                if (_currentState == GameState.Combat && bullets[i].Bounds.Intersects(mainCoffin.Bounds))
                {
                    mainCoffin.Health -= 1.2f; 
                    coffinVelocity += bullets[i].FromPlayer1 ? (175f * p1PushPower) : (-175f * p2PushPower);
                    if (bullets[i].FromPlayer1) p1Gold += 5; else p2Gold += 5;
                    hit = true;
                }
                // monsters hits
                else if (_currentState == GameState.Invasion)
                {
                    for (int j = monsters.Count - 1; j >= 0; j--)
                    {
                        if (monsters[j].Health > 0 && bullets[i].Bounds.Intersects(monsters[j].Bounds))
                        {
                            int damageApplied = bullets[i].FromPlayer1 ? (int)(1 * p1PushPower) : (int)(1 * p2PushPower);
                            monsters[j].TakeDamage(damageApplied);
                            
                            // monster hit = +5 gold
                            if (bullets[i].FromPlayer1) p1Gold += 5; else p2Gold += 5;
                            
                            hit = true;    
                            break;
                        }
                    }
                }

                if (hit || bullets[i].Position.X < 0 || bullets[i].Position.X > 1920) bullets.RemoveAt(i);
            }
        }

        private void SpawnMonsters() {
            Vector2 pos = mainCoffin.Position;
            
            float coffinCenterX = pos.X + 112f;
            int targetSide = (coffinCenterX < 960f) ? 1 : 2; // movement of monsters

            if (waveCount == 1) 
                for(int i=0; i<3; i++) monsters.Add(new Monster(new Vector2(pos.X + (i*70), pos.Y), 1, targetSide, skelWalk, skelAttack, skelDie));
            else if (waveCount == 2) { 
                for(int i=0; i<3; i++) monsters.Add(new Monster(new Vector2(pos.X + (i*70), pos.Y), 1, targetSide, skelWalk, skelAttack, skelDie)); 
                for(int i=0; i<2; i++) monsters.Add(new Monster(new Vector2(pos.X + (i*90), pos.Y), 2, targetSide, skelWalk, skelAttack, skelDie)); 
            }
            else monsters.Add(new Monster(pos, 3, targetSide, skelWalk, skelAttack, skelDie)); 
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DimGray); 
            _spriteBatch.Begin();
            _spriteBatch.Draw(backgroundGothic, new Rectangle(0, 0, 1920, 1080), Color.White);

            if (_currentState == GameState.Menu) // menu screen
            {
                _spriteBatch.DrawString(_font, "COFFIN GAME", new Vector2(860, 440), Color.White);
                _spriteBatch.DrawString(_font, "PRESS [SPACE] TO START", new Vector2(810, 510), Color.Yellow);
                _spriteBatch.DrawString(_font, "PRESS [ESC] TO QUIT", new Vector2(830, 580), Color.Red);
            }
            else if (_currentState == GameState.GameOver) // game over screen
            {
                _spriteBatch.DrawString(_font, winnerText, new Vector2(860, 500), Color.Red);
                _spriteBatch.DrawString(_font, "PRESS [SPACE] FOR MENU", new Vector2(800, 570), Color.Yellow);
            }
            else
            {
                player1.Draw(_spriteBatch, _pixel, towerDefault, towerUpgrade1, towerUpgrade2, SpriteEffects.FlipHorizontally);
                player2.Draw(_spriteBatch, _pixel, towerDefault, towerUpgrade1, towerUpgrade2, SpriteEffects.None);

                mainCoffin.Draw(_spriteBatch, _pixel);
                foreach (Monster m in monsters) m.Draw(_spriteBatch, _pixel);
                foreach (Bullet b in bullets) b.Draw(_spriteBatch, _pixel);

                _spriteBatch.DrawString(_font, "GOLD: " + p1Gold, new Vector2(50, 20), Color.Gold);
                _spriteBatch.DrawString(_font, "GOLD: " + p2Gold, new Vector2(1730, 20), Color.Gold);

                if (_currentState == GameState.Combat) // combat screen
                    _spriteBatch.Draw(_pixel, new Rectangle(0, 0, (int)(1920 * (roundTimer / 20f)), 15), Color.Yellow * 0.7f);

                if (_currentState == GameState.Shop) // shop screen
                {
                    _spriteBatch.Draw(_pixel, new Rectangle(660, 340, 600, 410), Color.Black * 0.85f);
                    _spriteBatch.DrawString(_font, "--- SHOP ---", new Vector2(880, 370), Color.White);
                    
                    string p1Text = "P1 Push Power [W] Lvl " + player1.Level + ": 500 Gold";
                    _spriteBatch.DrawString(_font, p1Text, new Vector2(740, 460), Color.Cyan);
                    
                    string p2Text = "P2 Push Power [UP] Lvl " + player2.Level + ": 500 Gold";
                    _spriteBatch.DrawString(_font, p2Text, new Vector2(740, 520), Color.Tomato);
                    
                    _spriteBatch.DrawString(_font, "PRESS [SPACE] TO SUMMON MONSTERS", new Vector2(740, 660), Color.Yellow);
                }
            }
            _spriteBatch.End();
            base.Update(gameTime);
        }
    }
}
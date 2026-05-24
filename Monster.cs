using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoffinGame
{
    public class Monster
    {
        public enum MonsterState { Idle, Walk, Attack, Die }
        public MonsterState CurrentState = MonsterState.Walk;

        public Vector2 Position;
        public int Health, MaxHealth, Damage;
        public int TargetSide;
        public Color MonsterColor;
        
        private float speed;
        private float attackTimer = 0f;

        private Texture2D _texWalk, _texAttack, _texDie;
        
        private int frameWidth = 96;
        private int frameHeight = 64;
        private int currentFrame;
        private float animationTimer;
        private float animationInterval = 80f;
        private float scale = 1.8f;

        public Rectangle Bounds;

        public Monster(Vector2 startPos, int level, int targetSide, Texture2D walk, Texture2D attack, Texture2D die)
        {
            this.TargetSide = targetSide;
            this._texWalk = walk;
            this._texAttack = attack;
            this._texDie = die;

            if (level == 1) //skeleton 1
            { 
                MaxHealth = 25; 
                Damage = 5; 
                MonsterColor = Color.White; 
                scale = 1.8f;
                speed = 110f;  
            }
            else if (level == 2) //skeleton 2
            { 
                MaxHealth = 50; 
                Damage = 10; 
                MonsterColor = Color.LightGreen; 
                scale = 1.8f; 
                speed = 85f; 
            }
            else //Boss
            { 
                MaxHealth = 100; 
                Damage = 20; 
                MonsterColor = Color.DarkRed; 
                scale = 3.0f; 
                speed = 65f; 
            }

            Position = new Vector2(startPos.X, 850 - (frameHeight * scale));
            Health = MaxHealth;
        }

        public void Update(Vector2 targetPos, float dt, Rectangle targetTowerBounds)
        {
            if (MaxHealth == 100) // Boss hitbox 
            {
                Bounds = new Rectangle((int)Position.X + 135, (int)Position.Y, (int)(frameWidth * scale) - 270, (int)(frameHeight * scale));
            }
            else // skeleton hitbox
            {
                Bounds = new Rectangle((int)Position.X + 50, (int)Position.Y, (int)(frameWidth * scale) - 100, (int)(frameHeight * scale));
            }
            
            
            if (Health <= 0)
            {
                if (CurrentState != MonsterState.Die)
                {
                    CurrentState = MonsterState.Die;
                    currentFrame = 0;
                    animationTimer = 0;
                }
            }
            else if (this.Bounds.Intersects(targetTowerBounds))
            {
                if (CurrentState != MonsterState.Attack)
                {
                    CurrentState = MonsterState.Attack;
                    currentFrame = 0;
                    animationTimer = 0;
                }
            }
            else
            {
                CurrentState = MonsterState.Walk;
            }

            if (CurrentState == MonsterState.Walk)
            {
                if (Position.X < targetPos.X) Position.X += speed * dt;
                else if (Position.X > targetPos.X) Position.X -= speed * dt;
            }

            if (attackTimer > 0) attackTimer -= dt;

            UpdateAnimation(dt);
        }

        private void UpdateAnimation(float dt)
        {
            int totalFrames = 10;
            if (CurrentState == MonsterState.Die) totalFrames = 13;

            animationTimer += dt * 1000;
            if (animationTimer > animationInterval)
            {
                currentFrame++;
                if (currentFrame >= totalFrames)
                {
                    if (CurrentState == MonsterState.Die)
                    {
                        currentFrame = totalFrames - 1;
                    }
                    else
                    {
                        currentFrame = 0;
                    }
                }
                animationTimer = 0;
            }
        }

        public bool CanAttack()
        {
            if (Health > 0 && attackTimer <= 0 && CurrentState == MonsterState.Attack)
            {
                attackTimer = 1.3f;
                return true;
            }
            return false;
        }

        public void TakeDamage(int amount)
        {
            if (CurrentState == MonsterState.Die) return;
            Health -= amount;
        }

        public bool IsDeadAnimationFinished()
        {
            return CurrentState == MonsterState.Die && currentFrame == 12;
        }

        public void Draw(SpriteBatch sb, Texture2D pixel)
        {
            Texture2D currentTex = _texWalk;
            if (CurrentState == MonsterState.Attack) currentTex = _texAttack;
            else if (CurrentState == MonsterState.Die) currentTex = _texDie;

            Rectangle sourceRect = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
            SpriteEffects effect = TargetSide == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            sb.Draw(currentTex, Position, sourceRect, MonsterColor, 0f, Vector2.Zero, scale, effect, 0f);

            if (CurrentState != MonsterState.Die)
            {
                int barWidth;
                int barX;

                if (MaxHealth == 100) // Boss health bar
                {
                    barWidth = (int)(frameWidth * scale) - 60; 
                    barX = (int)Position.X + 30;
                }
                else // skeleton health bar
                {
                    barWidth = (int)(frameWidth * scale) - 30;
                    barX = (int)Position.X + 15;
                }

                Rectangle bg = new Rectangle(barX, Bounds.Y - 15, barWidth, 7);
                Rectangle fill = new Rectangle(barX, Bounds.Y - 15, (int)(barWidth * ((float)Health / MaxHealth)), 7);
                sb.Draw(pixel, bg, Color.Black * 0.5f);
                sb.Draw(pixel, fill, Color.Red);
            }
        }
    }
}
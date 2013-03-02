using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace xna_game
{
    class Player
    {
        public Level Level
        {
            get { return level; }
        }
        Level level;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;
        Vector2 previousPosition;
        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        // Constants for controling horizontal movement
        private const float MoveAcceleration = 15000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;

        private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        private Rectangle localBounds;
        Rectangle screenRectangle = new Rectangle(0, 0, 800, 480);
        private Texture2D playerTexture;
        private int playerWidth;
        private int playerHeight;

        Texture2D levelTexture;
        Color[] playerTextureData;
        Color[] levelTextureData;

        Texture2D leftArmTex;
        Texture2D rightArmTex;
        Texture2D legTex;
        Texture2D headTex;
        Color[] leftArmData;
        Color[] rightArmData;
        Color[] legData;
        Color[] headData;

        private float collidedY;
        private float collidedX;
        private bool horizontalCollision;

        bool isOnGround;

        public Player(Level level, Vector2 position)
        {
            this.level = level;

            LoadContent();

            Reset(position);
        }

        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
        }


        /// <summary>
        /// gets us the bounding rectangle for the player
        /// used for physics purposes
        /// </summary>

        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - playerWidth/2) + localBounds.X;
                int top = (int)Math.Round(Position.Y - playerHeight/2) + localBounds.Y;
                //System.Console.WriteLine(Position.Y);
                return new Rectangle((int)Position.X, (int)Position.Y, playerWidth, playerHeight);
            }
        }
        

        /// <summary>
        /// this is where you load in the texture for the player character
        /// </summary>
        public void LoadContent()
        {
            //playerTexture = Level.Content.Load<Texture2D>("Sprites/Player/Idle");
            playerTexture = Level.Content.Load<Texture2D>("char");
            levelTexture = Level.Content.Load<Texture2D>("Levels/Level0");
            leftArmTex = Level.Content.Load<Texture2D>("left-col");
            rightArmTex = Level.Content.Load<Texture2D>("right-col");
            legTex = Level.Content.Load<Texture2D>("feet-coll");
            headTex = Level.Content.Load<Texture2D>("head-col");


            levelTextureData =
                new Color[levelTexture.Width * levelTexture.Height];
            levelTexture.GetData(levelTextureData);

            playerTextureData =
                new Color[playerTexture.Width * playerTexture.Height];
            playerTexture.GetData(playerTextureData);

            leftArmData =
                new Color[leftArmTex.Width * leftArmTex.Height];
            leftArmTex.GetData(leftArmData);

            rightArmData =
                new Color[rightArmTex.Width * rightArmTex.Height];
            rightArmTex.GetData(rightArmData);

            legData =
                new Color[legTex.Width * legTex.Height];
            legTex.GetData(legData);

            headData =
                new Color[headTex.Width * headTex.Height];
            headTex.GetData(headData);

            // Calculate bounds within texture size.
            //playerWidth = 25;
            //playerHeight = 50;
            playerWidth = playerTexture.Width;
            playerHeight = playerTexture.Height;
            localBounds = new Rectangle(0, 0, playerWidth, playerHeight);

        }

        public void Update(
            GameTime gameTime,
            KeyboardState keyboardState,
            GamePadState gamePadState,
            DisplayOrientation orientation,
            Texture2D background)
        {
            //System.Console.WriteLine("in Update");
            GetInput(keyboardState, gamePadState, orientation);

            ApplyPhysics(gameTime, background);

            // Clear input.
            movement = 0.0f;
            isJumping = false;
        }

        //process the input from any input devices for the player
        private void GetInput(
            KeyboardState keyboardState,
            GamePadState gamePadState,
            DisplayOrientation orientation)
        {
            //System.Console.WriteLine("in get input");
            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
            {
                movement = 0.0f;
                
            }
             

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                //System.Console.WriteLine("moving left");
                movement = -1.0f;
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
            }

            // Check if the player wants to jump.
            isJumping =
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W);
            
        }

        public void ApplyPhysics(GameTime gameTime, Texture2D background)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);
            //Console.WriteLine(velocity.X);
            // Apply pseudo-drag horizontally.
            if (isOnGround)
            {
                //Console.WriteLine("is on the ground");
                velocity.X *= GroundDragFactor;
            }
            else
            {
                velocity.X *= AirDragFactor;
            }

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions(background);

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && isOnGround) || jumpTime > 0.0f)
                {
                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
                
            }
            wasJumping = isJumping;

            return velocityY;
        }

        private void HandleCollisions(Texture2D Background)
        {
            Rectangle bounds = BoundingRectangle;
            //System.Console.WriteLine(bounds.Top + "" + bounds.Bottom);
            isOnGround = false;
            horizontalCollision = false;
            /*
            if ((IntersectPixels(bounds, playerTextureData, screenRectangle, levelTextureData)))
            //if ((IntersectPixels(screenRectangle, levelTextureData, bounds, playerTextureData)))
            {
                //Console.WriteLine("collided");
                //System.Console.WriteLine(bounds.Top + "" + bounds.Bottom);
                if (isOnGround)
                {
                    if (isJumping)
                    {
                        Position = new Vector2(Position.X, previousPosition.Y);
                    }
                    else
                    {
                        Position = new Vector2(Position.X, previousPosition.Y);
                    }
                }
                else
                {
                    Position = new Vector2(previousPosition.X, previousPosition.Y);
                }

                // Perform further collisions with the new bounds.
                bounds = BoundingRectangle;
            }
             */
            //if it intersects with the ground
            if ((IntersectPixels(bounds, legData, screenRectangle, levelTextureData, false)))
            {
                Console.WriteLine("leg collision");
                Position = new Vector2(Position.X, previousPosition.Y);
                isOnGround = true;
            }
            if ((IntersectPixels(bounds, rightArmData, screenRectangle, levelTextureData, true)))
            {
                Console.WriteLine("right collision");
                Position = new Vector2(previousPosition.X, Position.Y);
                isOnGround = true;
            }
            else if ((IntersectPixels(bounds, leftArmData, screenRectangle, levelTextureData, true)))
            {
                Console.WriteLine("left collision");
                Position = new Vector2(previousPosition.X, Position.Y);
                isOnGround = true;
            }
            if ((IntersectPixels(bounds, headData, screenRectangle, levelTextureData, false)))
            {
                //Console.WriteLine("head collision");
                if (horizontalCollision == true)
                {
                    Console.WriteLine("head collision");
                    position.Y += 3;
                }
                    Position = new Vector2(previousPosition.X, previousPosition.Y);
                isOnGround = true;
            }
            else
            {
                Console.WriteLine("falling");
            }
            previousBottom = bounds.Bottom;
        }

        public bool IntersectPixels(Rectangle rectangleA, Color[] dataA,
                                           Rectangle rectangleB, Color[] dataB, bool horizontal)
        {
            // Find the bounds of the rectangle intersection
            int top = Math.Max(rectangleA.Top, rectangleB.Top);
            int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
            int left = Math.Max(rectangleA.Left, rectangleB.Left);
            int right = Math.Min(rectangleA.Right, rectangleB.Right);

            // Check every point within the intersection bounds
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    // Get the color of both pixels at this point
                    int dataAindex = (x - rectangleA.Left) +
                                         (y - rectangleA.Top) * rectangleA.Width;
                    Color colorA = dataA[dataAindex];
                    int dataBindex = (x - rectangleB.Left) +
                                         (y - rectangleB.Top) * rectangleB.Width;
                    Color colorB = dataB[dataBindex];

                    // If both pixels are not completely transparent,
                    if (colorA.A != 0 && colorB.A != 0)
                    {
                        //System.Console.WriteLine("collided at: " + (x - rectangleA.Left) + " " + (y - rectangleA.Top) + "against " + (x - rectangleB.Left) + " " + (y - rectangleB.Top));
                        if (colorB.R < 200 && horizontal)
                        {
                            horizontalCollision = true;
                            position.Y -= 3;
                        }
                        else
                        {
                             //position.Y = previousPosition.Y;
                        }
                        // then an intersection has been found
                        return true;
                    }
                }
            }

            // No intersection found
            return false;
        }

            // For each potentially colliding tile

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            // Draw that sprite.
            //Vector2 origin = new Vector2(Position.X - playerTexture.Width / 2, Position.Y - playerTexture.Height);
            //spriteBatch.Draw(playerTexture, Position, localBounds, Color.White, 0.0f, origin,SpriteEffects.None, 0.0f);
            spriteBatch.Draw(levelTexture, screenRectangle, Color.White);
            spriteBatch.Draw(playerTexture, BoundingRectangle  , Color.White);
            
            
        }




    }


}

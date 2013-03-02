﻿using System;
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

        private Texture2D playerTexture;
        private int playerWidth;
        private int playerHeight;

        public bool IsOnGround
        {
            get { return isOnGround; }
        }
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
                int top = (int)Math.Round(Position.Y - playerHeight) + localBounds.Y;
                return new Rectangle(left, top, localBounds.Width * 2, localBounds.Height);
            }
        }
        

        /// <summary>
        /// this is where you load in the texture for the player character
        /// </summary>
        public void LoadContent()
        {
            //playerTexture = Level.Content.Load<Texture2D>("Sprites/Player/Idle");
            playerTexture = Level.Content.Load<Texture2D>("character");

            // Calculate bounds within texture size.            
            int width = 25;
            int x = 10;
            int height = 50;
            int y = 10;
            System.Console.WriteLine("current = " + width + " " + height + " " + x + " " + y);
            playerWidth = 25;
            playerHeight = 50;
            localBounds = new Rectangle(x, y, width, height);

        }

        public void Update(
            GameTime gameTime,
            KeyboardState keyboardState,
            GamePadState gamePadState,
            DisplayOrientation orientation)
        {
            //System.Console.WriteLine("in Update");
            GetInput(keyboardState, gamePadState, orientation);

            ApplyPhysics(gameTime);

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
                System.Console.WriteLine("moving left");
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

        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);
            //Console.WriteLine(velocity.X);
            // Apply pseudo-drag horizontally.
            if (IsOnGround)
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
            HandleCollisions();

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
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
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

        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            //System.Console.WriteLine("Tile Height = " + (float)bounds.Top);
            //System.Console.WriteLine(leftTile + " " + rightTile + " " + topTile + " " + bottomTile);

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                {
                                    isOnGround = true;
                                }

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    if (IsOnGround)
                                    {
                                        //System.Console.WriteLine("in this block");
                                    }
                                    
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X, Position.Y);
                                Console.WriteLine("Depth.X = " + depth.X);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            // Draw that sprite.
            Vector2 origin = new Vector2(Position.X - playerTexture.Width / 2, Position.Y - playerTexture.Height);
            //spriteBatch.Draw(playerTexture, Position, localBounds, Color.White, 0.0f, origin,SpriteEffects.None, 0.0f);
            spriteBatch.Draw(playerTexture, BoundingRectangle  , Color.White);
        }




    }


}

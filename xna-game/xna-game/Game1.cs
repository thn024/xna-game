using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Windows.Forms;

using XKeys = Microsoft.Xna.Framework.Input.Keys;
using System.Diagnostics;

namespace xna_game
{
    
    
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        //----------------------------------------------------------
        //GLOBAL VARS
        //----------------------------------------------------------
        int screenWidth = 800;
        int screenHeight = 480;
        bool ScreenBorder = true;

        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        Texture2D backgroundTexture;
        Texture2D characterTexture;
        Texture2D crosshair;
        KeyboardState keyState;
        KeyboardState prevKeyState;
        ParticleEngine2D.ParticleEngine particleEngine;
        Vector2 charPos = new Vector2(230, 380);
        float startY, jumpspeed = 0;
        bool jumping = false;//Init jumping to false

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            device = graphics.GraphicsDevice;
            //screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            //screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.ApplyChanges();
            startY = charPos.Y;//Starting position
            jumping = false;//Init jumping to false
            jumpspeed = 0;//Default no speed
            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            backgroundTexture = Content.Load<Texture2D>("background");
            characterTexture = Content.Load<Texture2D>("char2");
            crosshair = Content.Load<Texture2D>("crosshair");
            List<Texture2D> textures = new List<Texture2D>();
            textures.Add(Content.Load<Texture2D>("circle"));
            textures.Add(Content.Load<Texture2D>("star"));
            textures.Add(Content.Load<Texture2D>("diamond"));
            particleEngine = new ParticleEngine2D.ParticleEngine(textures, new Vector2(400, 240));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                this.Exit();
           
            // TODO: Add your update logic here
            ProcessKeyboard();
            if(Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                particleEngine.EmitterLocation = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                particleEngine.GenerateParticlesClick();
            }
            particleEngine.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            

            spriteBatch.Begin();
            drawBackground();
            spriteBatch.End();

            particleEngine.Draw(spriteBatch);

            spriteBatch.Begin();
            drawCrosshair();
            drawCharacter();
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void drawBackground()
        {
            Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            spriteBatch.Draw(backgroundTexture, screenRectangle, Color.White);
        }

        private void drawCharacter()
        {
            Rectangle screenRectangle = new Rectangle((int)charPos.X, (int)charPos.Y, 90, 90);
            spriteBatch.Draw(characterTexture, screenRectangle, Color.White);
        }

        private void drawCrosshair()
        {
            Rectangle screenRectangle = new Rectangle(Mouse.GetState().X-10, Mouse.GetState().Y-10, 20,20);
            spriteBatch.Draw(crosshair, screenRectangle, Color.White);
        }

        private void ProcessKeyboard()
        {
            prevKeyState = keyState;
            keyState = Keyboard.GetState();


            if (jumping)
            {
                charPos.Y -= jumpspeed;//Making it go up
                jumpspeed += 1;//Some math (explained later)
                System.Console.WriteLine(charPos.Y);
                if (charPos.Y <= 200)
                //If it's farther than ground
                {
                    
                    jumping = false;
                    jumpspeed = 0;
                }
            }
            else
            {
                if (charPos.Y <= 400)
                {
                    charPos.Y += jumpspeed;
                    jumpspeed++;
                }
            }

            if (keyState.IsKeyDown(XKeys.F) && prevKeyState.IsKeyUp(XKeys.F))
            {
                IntPtr hWnd = this.Window.Handle;
                var control = System.Windows.Forms.Control.FromHandle(hWnd);
                var form = control.FindForm();
                if (ScreenBorder)
                {
                    //default 1280 by 720 window
                    form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
                    form.WindowState = System.Windows.Forms.FormWindowState.Normal;
                    screenWidth = 800;
                    screenHeight = 480;
                }
                else
                {
                    //borderless fullscreen
                    form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    form.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                    screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                }

                ScreenBorder = !ScreenBorder;
            }
            else if (keyState.IsKeyDown(XKeys.W) && prevKeyState.IsKeyUp(XKeys.W))
            {
                if (jumping == false)
                {
                    jumping = true;
                }
                System.Console.WriteLine("jumping");
            }
            else if (keyState.IsKeyDown(XKeys.S) && prevKeyState.IsKeyUp(XKeys.S))
            {
                //Move Down
            }
            else if (keyState.IsKeyDown(XKeys.A))
            {
                charPos.X -= 5;
                System.Console.WriteLine("left");
            }
            else if (keyState.IsKeyDown(XKeys.D))
            {
                charPos.X += 5;
            }
            else if (keyState.IsKeyDown(XKeys.Escape))
            {
                Exit();
            }
        }

        private void jump()
        {
        }
    }
}

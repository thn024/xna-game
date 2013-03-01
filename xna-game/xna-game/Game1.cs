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
        int screenWidth = 1280;
        int screenHeight = 720;
        bool ScreenBorder = true;

        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        Texture2D backgroundTexture;
        Texture2D characterTexture;
        KeyboardState keyState;
        KeyboardState prevKeyState;
        ParticleEngine2D.ParticleEngine particleEngine;

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
            backgroundTexture = Content.Load<Texture2D>("cloudsBackground");
            characterTexture = Content.Load<Texture2D>("character");
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
            Rectangle screenRectangle = new Rectangle(Mouse.GetState().X-10, Mouse.GetState().Y-50, 60, 90);
            spriteBatch.Draw(characterTexture, screenRectangle, Color.White);
        }

        private void ProcessKeyboard()
        {
            prevKeyState = keyState;
            keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(XKeys.F) && prevKeyState.IsKeyUp(XKeys.F))
            {
                //toggle fullscreen on or off
                //graphics.ToggleFullScreen();
            }
            else if (keyState.IsKeyDown(XKeys.W) && prevKeyState.IsKeyUp(XKeys.W))
            {
                IntPtr hWnd = this.Window.Handle;
                var control = System.Windows.Forms.Control.FromHandle(hWnd);
                var form = control.FindForm();
                if (ScreenBorder)
                {
                    //default 1280 by 720 window
                    form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
                    form.WindowState = System.Windows.Forms.FormWindowState.Normal;
                    screenWidth = 1280;
                    screenHeight = 720;
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
            else if (keyState.IsKeyDown(XKeys.Escape))
            {
                Exit();
            }
        }
    }
}

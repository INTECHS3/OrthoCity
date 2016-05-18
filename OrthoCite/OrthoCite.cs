﻿using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OrthoCite.Entities;
using OrthoCite.Entities.MiniGames;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using System.Runtime.InteropServices;
using System;

namespace OrthoCite
{
    public enum GameContext
    {
        MENU,
        MAP,
        MINIGAME_PLATFORMER
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class OrthoCite : Game
    {
        BoxingViewportAdapter _viewportAdapter;
        Camera2D _camera;
        RuntimeData _runtimeData;
        readonly GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        List<IEntity> _entities;

        const int SCENE_WIDTH = 1366;
        const int SCENE_HEIGHT = 768;

        GameContext _gameContext;
        public bool _gameContextChanged;
        public int _gidLastForMap;

        public static void writeSpacerConsole() => System.Console.WriteLine("===========================================");
        

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public OrthoCite()
        {
            
            
            _runtimeData = new RuntimeData(this);
            _graphics = new GraphicsDeviceManager(this);

            _entities = new List<IEntity>();
            ChangeGameContext(GameContext.MAP);

#if DEBUG
            _graphics.PreferredBackBufferWidth = 928;
            _graphics.PreferredBackBufferHeight = 512;
            AllocConsole();
            System.Console.WriteLine("=== OrthoCite debug console ===");
#else
            _graphics.PreferredBackBufferWidth = SCENE_WIDTH;
            _graphics.PreferredBackBufferHeight = SCENE_HEIGHT;
            _graphics.IsFullScreen = true;
#endif

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, SCENE_WIDTH, SCENE_HEIGHT);
            _runtimeData.ViewAdapter = _viewportAdapter;
            _runtimeData.Scene = new Rectangle(0, 0, SCENE_WIDTH, SCENE_HEIGHT);
            _camera = new Camera2D(_viewportAdapter);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);

            foreach (var entity in _entities)
            {
                entity.LoadContent(this.Content, this.GraphicsDevice);
            }
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            foreach (var entity in _entities)
            {
                entity.UnloadContent();
            }
        }
       
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

#if DEBUG
            if(Keyboard.GetState().IsKeyDown(Keys.F11)) { recordConsole();  }
#endif
            foreach (var entity in _entities)
            {
                entity.Update(gameTime, Keyboard.GetState(), _camera);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.Black);
            
            foreach (var entity in _entities)
            {
                entity.Draw(_spriteBatch, _viewportAdapter.GetScaleMatrix(), _camera.GetViewMatrix());
            }

            if (_gameContextChanged) PopulateEntitiesFromGameContext();

            base.Draw(gameTime);
        }

        public void ChangeGameContext(GameContext context)
        {
            _gameContext = context;
            _gameContextChanged = true;
        }

        void PopulateEntitiesFromGameContext()
        {
            _entities.Clear();
            writeSpacerConsole();
            Console.Write("Context switched to ");

            switch (_gameContext)
            {
                case GameContext.MAP:
                    Console.WriteLine("map");
                    _entities.Add(new Map(_runtimeData, _gidLastForMap));
                    _entities.Add(new DialogBox(_runtimeData));
                    _gidLastForMap = 0;
                    break;
                case GameContext.MINIGAME_PLATFORMER:
                    Console.WriteLine("platformer minigame");
                    _entities.Add(new Platformer(_runtimeData));
                    _gidLastForMap = 0;
                    break;
            }

            _gameContextChanged = false;

#if DEBUG
            _entities.Add(new DebugLayer(_runtimeData));
#endif

            LoadContent();
        }

        private void recordConsole()
        {
            string cmdTmp = System.Console.ReadLine();
            string[] cmd = cmdTmp.Split(' ');
            foreach (var entity in _entities)
            {
                entity.Execute(cmd);
            }
        }
    }
}

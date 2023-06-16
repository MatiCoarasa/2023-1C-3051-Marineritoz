using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Content.Gizmos;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Entities;
using TGC.MonoGame.TP.Entities.Islands;
using TGC.MonoGame.TP.Menu;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Esta es la clase principal del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "Fonts/";
        public const string ContentFolderTextures = "Textures/";
        private Camera FollowCamera { get; set; }
        private ShipPlayer Ship { get; set; }
        private Effect TextureShader { get; set; }
        public Gizmos Gizmos { get; }
        private const bool GizmosEnabled = false;
        
        private Island[] Islands { get; set; }
        private IslandGenerator IslandGenerator { get; set; }
        private Water Water { get; set; }
        private float Time { get; }
        private SpriteFont Font { get; set; }
        
        public GraphicsDeviceManager Graphics { get; }
        public SpriteBatch SpriteBatch { get; set; }

        private BoundingBox[] _colliders;

        private Rain Rain { get; set; }

        private GlobalConfigurationSingleton GlobalConfig { get; }
        private MainMenu _menu;

        public GameStatus GameStatus = GameStatus.NormalGame;
        
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 200;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 200;
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;

            IsMouseVisible = true;

            Gizmos = new Gizmos();
            Gizmos.Enabled = GizmosEnabled;

            GlobalConfig = GlobalConfigurationSingleton.GetInstance();

            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            var rasterizerState = new RasterizerState();
            GraphicsDevice.RasterizerState = rasterizerState;
            
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            _menu = new MainMenu(this);

            FollowCamera = new ShipCamera(GraphicsDevice.Viewport.AspectRatio);
            Ship = new ShipPlayer(this);
            IslandGenerator = new IslandGenerator(this);


            Water = new Water(GraphicsDevice);
            Rain = new Rain(Content, GraphicsDevice);
            Rain.Initialize(
                GlobalConfig.RainSize, 
                GlobalConfig.RainMaxHeight, 
                GlobalConfig.RainMinHeight, 
                GlobalConfig.RainDropCount, 
                GlobalConfig.RainSpeedDrop,
                GlobalConfig.RainColor
                );

            _colliders = new BoundingBox[GlobalConfig.IslandsQuantity];
            
            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            _menu.LoadContent();
            Font = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "Arial16");
            Gizmos.LoadContent(GraphicsDevice, new ContentManager(Content.ServiceProvider, "Content"));

            // Load water
            Water.LoadContent(Content);
            
            // Load ship
            TextureShader = Content.Load<Effect>(ContentFolderEffects + "TextureShader");
            Ship.LoadContent(Content, TextureShader);

            // Load islands
            IslandGenerator.LoadContent(Content, TextureShader);
            Islands = IslandGenerator.CreateRandomIslands(GlobalConfig.IslandsQuantity, GlobalConfig.IslandsMaxXSpawn, GlobalConfig.IslandsMaxZSpawn, GlobalConfig.SpawnBoxSize);
            for (var i = 0; i < Islands.Length; i++)
            {
                _colliders[i] = Islands[i].BoundingBox;
            }
            
            Rain.Load();
            base.LoadContent();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) GameStatus = GameStatus.Exit;
            
            switch (GameStatus)
            {
                case GameStatus.NormalGame:
                    Gizmos.UpdateViewProjection(FollowCamera.View, FollowCamera.Projection);

                    Ship.Update(gameTime, FollowCamera);
                    foreach (var collider in _colliders)
                    {
                        Ship.CheckCollision(collider);
                    }
                    break;
                case GameStatus.Exit:
                    Exit();
                    break;
            }
            
            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            switch (GameStatus)
            {
                case GameStatus.MainMenu:
                    GraphicsDevice.Clear(Color.Khaki);
                    _menu.Draw(gameTime);
                    break;
                
                case GameStatus.NormalGame:
                    
                    GraphicsDevice.Clear(GlobalConfig.SkyColor);
                    Rain.Draw(gameTime, FollowCamera);
                    Water.Draw(FollowCamera.View, FollowCamera.Projection, Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds));

                    foreach (var island in Islands)
                    {
                        GraphicsDevice.BlendState = BlendState.Opaque;
                        island.Draw(FollowCamera.View, FollowCamera.Projection);
                    }

                    Ship.Draw(FollowCamera, SpriteBatch, Font);
                    Gizmos.Draw();
                    break;
            }


        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();
            Gizmos.Dispose();

            base.UnloadContent();
        }
    }
}
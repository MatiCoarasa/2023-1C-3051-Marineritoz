using System;
using BepuPhysics;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TGC.MonoGame.TP.Content.Gizmos;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Entities;
using TGC.MonoGame.TP.Entities.Islands;
using TGC.MonoGame.TP.Entities.Light;
using TGC.MonoGame.TP.Menu;
using TGC.MonoGame.TP.Menu.GodMode;
using TGC.MonoGame.TP.Utils.GUI.ImGuiNET;
using System.Collections.Generic;

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
        private Camera EnvironmentFollowCamera { get; set; }
        private Camera EnvironmentShipCamera { get; set; }
        private Camera ShipCamera { get; set; }
        private ShipPlayer Ship { get; set; }
        private Vector3 ShipPosition { get; set; }
        private Effect TextureShader { get; set; }
        private Effect BasicShader { get; set; }
        public Gizmos Gizmos { get; }
        private const bool GizmosEnabled = false;

        private bool _justChangedScreens;
        private float _timeSinceLastScreenChange;
        
        private Island[] Islands { get; set; }
        private Map Map { get; set; }
        private Water Water { get; set; }
        private float Time { get; }
        private SpriteFont Font { get; set; }
        
        public GraphicsDeviceManager Graphics { get; }
        public SpriteBatch SpriteBatch { get; set; }

        private List<BoundingBox> _colliders = new List<BoundingBox> { };

        private Rain Rain { get; set; }

        private GlobalConfigurationSingleton GlobalConfig { get; }
        private MainMenu _menu;

        public GameStatus GameStatus = GameStatus.MainMenu;
        
        private HealthBar HealthBar { get; set; }
        private ImGuiRenderer ImGuiRenderer { get; set; }

        private BoundingFrustum FrustumBounding { get; set; }
        
        private float TotalTime { get; set; }
        private RenderTarget2D EnvironmentMapRenderTarget { get; set; }
        private const int EnvironmentmapSize = 2048;

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

        private Song Song { get; set; }
        
        private SunLight SunLight { get; set; }
        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            var rasterizerState = new RasterizerState();
            GraphicsDevice.RasterizerState = rasterizerState;
            ShipCamera = new ShipCamera(GraphicsDevice.Viewport.AspectRatio);
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);
            EnvironmentFollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio, true);
            EnvironmentShipCamera = new ShipCamera(GraphicsDevice.Viewport.AspectRatio, true);

            ImGuiRenderer = new ImGuiRenderer(this);
            ImGuiRenderer.RebuildFontAtlas();

            SpriteBatch = new SpriteBatch(GraphicsDevice);

            _menu = new MainMenu(this);

            Ship = new ShipPlayer(this, true);
            Map = new Map(GraphicsDevice);
            TotalTime = 0;

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

            SunLight = new SunLight(GraphicsDevice);
            HealthBar = new HealthBar();
            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            EnvironmentMapRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, true, SurfaceFormat.Color, DepthFormat.Depth24);
            Font = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "Verdana16");
            Gizmos.LoadContent(GraphicsDevice, new ContentManager(Content.ServiceProvider, "Content"));
            Song = Content.Load<Song>(ContentFolderMusic + "piratas-del-caribe");
            MediaPlayer.IsRepeating = true;
            // Sunlight
            BasicShader = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            SunLight.LoadContent(BasicShader);
            // Load water
            Water.LoadContent(Content);
            // Load ship
            // var shipShader = Content.Load<Effect>(ContentFolderEffects + "ShipShader");
            TextureShader = Content.Load<Effect>(ContentFolderEffects + "TextureShader");
            Ship.LoadContent(TextureShader);
            HealthBar.LoadContent(Content);
            _menu.LoadContent(BasicShader, Ship, Font, new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24));
            // Load islands
            Map.Load(this, Content, TextureShader);
            _colliders = Map.IslandColliders();

            Rain.Load();
            
            Options.LoadModifiers();
            base.LoadContent();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TotalTime += deltaTime;
            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Volume = 0.01f;
                MediaPlayer.Play(Song);
            }
            switch (GameStatus)
            {
                case GameStatus.DeathMenu:
                case GameStatus.MainMenu:
                    _menu.Update(TotalTime, deltaTime);
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape) && !_justChangedScreens)
                    {
                        GameStatus = GameStatus.Exit;
                    }
                    break;
                case GameStatus.NormalGame:
                case GameStatus.GodModeGame:
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    {
                        GameStatus = GameStatus.MainMenu;
                        _timeSinceLastScreenChange = 0;
                        _justChangedScreens = true;
                    }
                    GameUpdates(gameTime);
                    break;
                case GameStatus.Exit:
                    Exit();
                    break;
            }
            
                _timeSinceLastScreenChange += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            if (_justChangedScreens && _timeSinceLastScreenChange > .5f)
            {
                _justChangedScreens = false;
            }
            base.Update(gameTime);
        }

        private void GameUpdates(GameTime gameTime)
        {
            var camera = GameStatus == GameStatus.NormalGame ? ShipCamera : FollowCamera;
            // var environmentCamera = GameStatus == GameStatus.NormalGame ? EnvironmentShipCamera : EnvironmentFollowCamera;
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Gizmos.UpdateViewProjection(camera.View, camera.Projection);
            ShipPosition = Ship.Update(TotalTime, deltaTime, camera, EnvironmentFollowCamera);
            foreach (var collider in _colliders)
            {
                Ship.CheckCollision(collider, HealthBar);
            }
            if (HealthBar.Life <= 0)
            {
                TotalTime = 0;
                HealthBar.Life = 100;
                GameStatus = GameStatus.DeathMenu;
            }
            SunLight.Update(TotalTime);
        }

        private void DrawSampleExplorer(GameTime gameTime)
        {
            ImGuiRenderer.BeforeLayout(gameTime);
            Options.DrawLayout();
            ImGuiRenderer.AfterLayout();
        }

        private void DrawEnvironment(Camera camera)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SetRenderTarget(EnvironmentMapRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            Ship.Draw(camera, SpriteBatch, SunLight.Light.Position, GraphicsDevice.Viewport.Height, false);
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(GlobalConfig.SkyColor);
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
                case GameStatus.DeathMenu:
                case GameStatus.MainMenu:
                    _menu.Draw(TotalTime, GameStatus);
                    break;
                case GameStatus.GodModeGame:
                    DrawEnvironment(EnvironmentFollowCamera);
                    Water.SetEnvironmentMappingDrawing();
                    GameDraw(FollowCamera);
                    DrawSampleExplorer(gameTime);
                    break;
                case GameStatus.NormalGame:
                    DrawEnvironment(EnvironmentFollowCamera);
                    Water.SetEnvironmentMappingDrawing();
                    GameDraw(ShipCamera);
                    break;
            }
        }

        private void GameDraw(Camera camera)
        {

            FrustumBounding = new BoundingFrustum(camera.View * camera.Projection);
            GraphicsDevice.Clear(GlobalConfig.SkyColor);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Rain.Draw(TotalTime, camera);
            Water.Draw(ShipPosition, SunLight.Light.Position, camera, TotalTime, EnvironmentMapRenderTarget);
            SunLight.Draw(camera, BasicShader);

            Map.Draw(this, camera, SunLight.Light.Position, FrustumBounding);

            Ship.Draw(camera, SpriteBatch, SunLight.Light.Position, GraphicsDevice.Viewport.Height, true);
            HealthBar.Draw(SpriteBatch, GraphicsDevice.Viewport);
            Gizmos.Draw();
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
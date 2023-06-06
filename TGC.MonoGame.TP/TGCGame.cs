using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using TGC.MonoGame.TP.Content.Gizmos;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Entities;
using TGC.MonoGame.TP.Entities.Islands;

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
        private Vector3 ShipPosition { get; set; }
        private Effect TextureShader { get; set; }
        public Gizmos Gizmos { get; }
        private const bool GizmosEnabled = true;
        
        private const int IslandsQuantity = 200;

        private Island[] Islands { get; set; }
        private IslandGenerator IslandGenerator { get; set; }
        private Water Water { get; set; }
        private float Time { get; }

        private BoundingBox[] _colliders;

        private Rain Rain { get; set; }
        private HealthBar HealthBar { get; set; }

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

            IsMouseVisible = false;

            Gizmos = new Gizmos();
            Gizmos.Enabled = GizmosEnabled;

            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
        }

        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            // Apago el backface culling.
            // Esto se hace por un problema en el diseno del modelo del logo de la materia.
            // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
            var rasterizerState = new RasterizerState();
            //rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);
            Ship = new ShipPlayer(this);
            IslandGenerator = new IslandGenerator(this);
            Water = new Water(GraphicsDevice);
            Rain = new Rain(Content, GraphicsDevice);
            Rain.Initialize(100f, 150f, -3f, 500, 1f);
            _colliders = new BoundingBox[IslandsQuantity];
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
            
            Gizmos.LoadContent(GraphicsDevice, new ContentManager(Content.ServiceProvider, "Content"));

            // Load water
            Water.LoadContent(Content);
            
            // Load ship
            TextureShader = Content.Load<Effect>(ContentFolderEffects + "TextureShader");
            Ship.LoadContent(GraphicsDevice, Content, TextureShader);
            HealthBar.LoadContent(Content);
            // Load islands
            IslandGenerator.LoadContent(Content, TextureShader);
            Islands = IslandGenerator.CreateRandomIslands(IslandsQuantity, 2000, 2000, 50);
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
            Gizmos.UpdateViewProjection(FollowCamera.View, FollowCamera.Projection);
            
            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //Salgo del juego.
                Exit();
            }

            ShipPosition = Ship.Update(gameTime, FollowCamera);
            foreach (var collider in _colliders)
            {
                Ship.CheckCollision(collider, HealthBar);
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(2, 5, 61));

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;


            Rain.Draw(gameTime, FollowCamera);

            Water.Draw(ShipPosition, FollowCamera.View, FollowCamera.Projection, Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds));

            foreach (var island in Islands)
            {
                GraphicsDevice.BlendState = BlendState.Opaque;
                island.Draw(FollowCamera.View, FollowCamera.Projection);
            }

            Ship.Draw(FollowCamera, SpriteBatch, GraphicsDevice.Viewport.Height);
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
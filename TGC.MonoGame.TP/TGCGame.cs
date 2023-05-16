using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using TGC.MonoGame.TP.Camera;
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
        private FollowCamera FollowCamera { get; set; }
        private ShipPlayer Ship { get; set; }
        private Effect TextureShader { get; set; }

        private int _islandsQuantity = 200;
        
        private Island[] Islands { get; set; }
        private IslandGenerator IslandGenerator { get; set; }
        private Water Water { get; set; }
        private float Time { get; }
        private SpriteFont Font { get; set; }

        private BoundingBox[] _colliders;
        private bool _hasShipCollisioned;
        
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
            IsFixedTimeStep = false;

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
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);
            Ship = new ShipPlayer();
            
            IslandGenerator = new IslandGenerator();
            Water = new Water(GraphicsDevice);
            
            _colliders = new BoundingBox[_islandsQuantity];
            
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
            
            Font = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "Arial16");
            
            // Load water
            Water.LoadContent(Content);
            
            // Load ship
            TextureShader = Content.Load<Effect>(ContentFolderEffects + "TextureShader");
            Ship.LoadContent(Content, TextureShader);

            // Load islands
            IslandGenerator.LoadContent(Content, TextureShader);
            Islands = IslandGenerator.CreateRandomIslands(200, 1500f, 1500f, .05f);
            for (int i = 0; i < Islands.Length; i++)
            {
                _colliders[i] = Islands[i].BoundingBox;
            }
            
            base.LoadContent();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //Salgo del juego.
                Exit();
            }
            Ship.Update(gameTime, FollowCamera);

            var collisionDetected = false;
            foreach (var collider in _colliders)
            {
                if (Ship.BoundingBox.Intersects(collider))
                {
                    collisionDetected = true;
                }
            }

            _hasShipCollisioned = collisionDetected;
            
            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Aqua);
            
            Ship.Draw(FollowCamera, SpriteBatch, Font);
            Water.Draw(FollowCamera.View, FollowCamera.Projection, Time);
            
            SpriteBatch.Begin();
            SpriteBatch.DrawString(Font, "Ship collision: " + _hasShipCollisioned, 
                new Vector2(0, 40), Color.Red);
            SpriteBatch.End();

            foreach (var island in Islands)
            {
                island.Draw(FollowCamera.View, FollowCamera.Projection);
            }
            Water.Draw(FollowCamera.View, FollowCamera.Projection, Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds));
        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }
    }
}
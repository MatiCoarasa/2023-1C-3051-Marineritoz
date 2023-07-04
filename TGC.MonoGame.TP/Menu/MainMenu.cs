using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Entities;
using TGC.MonoGame.TP.Entities.Light;

namespace TGC.MonoGame.TP.Menu;

public class MainMenu
{
    private TGCGame _game;
    private ButtonsGrid _buttons;
    private Texture2D _logo;
    private int _screenWidth;
    private int _screenHeight;
    private GlobalConfigurationSingleton GlobalConfig => GlobalConfigurationSingleton.GetInstance();

    // Background
    private Camera _camera;
    private Camera _environmentCamera;
    private Water _water;
    private Effect _basicShader;
    private SunLight _sunlight;
    private ShipPlayer _ship;
    private SpriteFont _font;
    private RenderTarget2D _environmentMapRenderTarget { get; set; }


    public MainMenu (TGCGame game)
        {
            _game = game;
            
            _screenWidth = _game.Graphics.GraphicsDevice.Viewport.Width;
            _screenHeight = _game.Graphics.GraphicsDevice.Viewport.Height;

            _sunlight = new SunLight(_game.GraphicsDevice);
            _water = new Water(_game.GraphicsDevice);

            var cameraPosition = new Vector3(1, 10, 1);
            var frontDirection = -cameraPosition;
            frontDirection.Normalize();

            // Obtengo el vector Derecha asumiendo que la camara tiene el vector Arriba apuntando hacia arriba
            // y no esta rotada en el eje X (Roll)
            var right = Vector3.Cross(frontDirection, Vector3.Up);

            // Una vez que tengo la correcta direccion Derecha, obtengo la correcta direccion Arriba usando
            // otro producto vectorial
            var cameraCorrectUp = Vector3.Cross(right, frontDirection);
            _camera = new StaticCamera(cameraPosition, frontDirection, cameraCorrectUp);
            _environmentCamera = new FollowCamera(_game.GraphicsDevice.Viewport.AspectRatio);

            var buttons = new List<Button>
            {
                new ("Start game", GameStatus.NormalGame),
                new ("God mode", GameStatus.GodModeGame),
                new ("Exit", GameStatus.Exit),
            };
            _buttons = new ButtonsGrid(game, _screenWidth/2, _screenHeight/2, buttons);
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        public void LoadContent(Effect basicShader, ShipPlayer ship, SpriteFont font, RenderTarget2D renderTarget2D)
        {
            _logo = _game.Content.Load<Texture2D>("Images/altamar");
            _buttons.LoadContent(_game.Content);
            _font = font;
            _water.LoadContent(_game.Content);
            _environmentMapRenderTarget = renderTarget2D;
            _basicShader = basicShader;
            _sunlight.LoadContent(_basicShader);

            _ship = ship;
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public void Update(float totalTime, float deltaTime)
        {
            _buttons.Update(Mouse.GetState());
            _sunlight.Update(totalTime);
            _ship.Update(totalTime, deltaTime, _camera, _environmentCamera);
        }

        private void DrawEnvironment(Camera camera)
        {
            _game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            _game.GraphicsDevice.SetRenderTarget(_environmentMapRenderTarget);
            _game.GraphicsDevice.Clear(Color.Transparent);
            _ship.Draw(camera, _game.SpriteBatch, _sunlight.Light.Position, _game.GraphicsDevice.Viewport.Height, false);
            _game.GraphicsDevice.SetRenderTarget(null);
            _game.GraphicsDevice.Clear(GlobalConfig.SkyColor);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        public void Draw(float totalTime, GameStatus gameStatus)
        {
            DrawEnvironment(_environmentCamera);
            _water.SetOceanDrawing();
            _water.Draw(_ship.World.Translation, _sunlight.Light.Position, _camera, totalTime, _environmentMapRenderTarget);
            _sunlight.Draw(_camera, _basicShader);
            _ship.Draw(_camera, _game.SpriteBatch, _sunlight.Light.Position, 0, true);
            var destRectangle = new Rectangle((_screenWidth - _logo.Width)/2,
                _screenHeight/100, _logo.Width, _logo.Height);
            _game.SpriteBatch.Begin();
            _game.SpriteBatch.Draw(_logo, destRectangle, Color.White);
            if (gameStatus == GameStatus.DeathMenu)
            {
                var text = "Perdiste!";
                var size = _font.MeasureString(text);
                _game.SpriteBatch.DrawString(_font, text, new Vector2((_screenWidth - size.Y)/2, 20), Color.Black);
            }
            _game.SpriteBatch.End();
            _buttons.Draw(_game.SpriteBatch);
        }

}

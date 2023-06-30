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
    
    // Background
    private Camera _camera;
    private Water _water;
    private Effect _basicShader;
    private SunLight _sunlight;
    private ShipPlayer _ship;


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
        public void LoadContent(Effect basicShader, ShipPlayer ship)
        {
            _logo = _game.Content.Load<Texture2D>("Images/altamar");
            _buttons.LoadContent(_game.Content);

            _water.LoadContent(_game.Content);

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
            _ship.Update(totalTime, deltaTime, _camera);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        public void Draw(float totalTime, RenderTargetCube renderTargetCube)
        {
            _game.GraphicsDevice.Clear(Color.LightGray);
            _water.Draw(_sunlight.Light.Position, _camera, totalTime, renderTargetCube);
            _sunlight.Draw(_camera, _basicShader);
            _ship.Draw(_camera, _game.SpriteBatch, 0, true);
            var destRectangle = new Rectangle((_screenWidth - _logo.Width)/2,
                _screenHeight/100, _logo.Width, _logo.Height);
            _game.SpriteBatch.Begin();
            _game.SpriteBatch.Draw(_logo, destRectangle, Color.White);
            _game.SpriteBatch.End();
            
            _buttons.Draw(_game.SpriteBatch);
        }

}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace TGC.MonoGame.TP.Menu;

public class MainMenu
{
    private TGCGame _game;
    private ButtonsGrid _buttons;
    private Texture2D _logo;
    private Background _background;

    private int _screenWidth;
    private int _screenHeight;
    
        public MainMenu (TGCGame game)
        {
            _game = game;
            
            _screenWidth = _game.Graphics.GraphicsDevice.Viewport.Width;
            _screenHeight = _game.Graphics.GraphicsDevice.Viewport.Height;

            var buttons = new List<Button>
            {
                new ("Start game", GameStatus.NormalGame),
                new ("Exit", GameStatus.Exit),
            };
            _buttons = new ButtonsGrid(game, _screenWidth/2, _screenHeight/2, buttons);
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        public void LoadContent()
        {
            _logo = _game.Content.Load<Texture2D>("Images/altamar");
            _buttons.LoadContent(_game.Content);
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            _buttons.Update(Mouse.GetState());
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        public void Draw(GameTime gameTime)
        
        
        {
            var destRectangle = new Rectangle((_screenWidth - _logo.Width)/2,
                _screenHeight/100, _logo.Width, _logo.Height);
            _game.SpriteBatch.Begin();
            _game.SpriteBatch.Draw(_logo, destRectangle, Color.White);
            _game.SpriteBatch.End();
            
            _buttons.Draw(_game.SpriteBatch);
        }

}

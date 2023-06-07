using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace TGC.MonoGame.TP.Menu;

public class Main
{
    private TGCGame _game;
    private List<Button> _buttons;
    private Texture2D _logo;
    private Background _background;
    
    protected void Initialize(TGCGame game)
    {
        _game = game;
    }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected void LoadContent()
        {
            _logo = _game.Content.Load<Texture2D>("Images/logo");
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected void Update(GameTime gameTime)
        {
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected void Draw(GameTime gameTime)
        {
            _game.SpriteBatch.Begin();
            _game.SpriteBatch.Draw(_logo, new Vector2(0, 0), Color.Aqua);
            _game.SpriteBatch.End();
        }

}

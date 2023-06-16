using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Menu;

public class Button
{
    public int Width;
    public int Height;
    
    private string _text;
    private GameStatus _gameStatus;

    private Texture2D _normalImg;
    private Texture2D _hoveredImg;

    public Button(string text, GameStatus gameStatus)
    {
        _text = text;
        _gameStatus = gameStatus;
    }

    public void LoadContent(ContentManager contentManager)
    {
        _normalImg = contentManager.Load<Texture2D>("Images/button");
        _hoveredImg = contentManager.Load<Texture2D>("Images/hoverbutton");
        Width = _normalImg.Width/2;
        Height = _normalImg.Height/2;
    }

    public void Draw(int x, int y, bool isHovered, SpriteBatch spriteBatch)
    {
        var rectangle = new Rectangle(x, y, Width, Height);
        spriteBatch.Begin();
        spriteBatch.Draw(isHovered ? _hoveredImg : _normalImg, rectangle, Color.White);
        spriteBatch.End();
    }

    public void Select(TGCGame game)
    {
        game.GameStatus = _gameStatus;
    }
}

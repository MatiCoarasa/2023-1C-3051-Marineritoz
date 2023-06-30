using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Menu;

public class Button
{
    public int Width;
    public int Height;

    private SpriteFont _font;
    
    private string _text;
    public bool IsHovered;
    private GameStatus _gameStatus;

    private Texture2D _normalImg;
    private Texture2D _hoveredImg;

    private Rectangle _rectangle; 

    public Button(string text, GameStatus gameStatus)
    {
        _text = text;
        _gameStatus = gameStatus;
    }

    public void LoadContent(ContentManager contentManager)
    {
        _font = contentManager.Load<SpriteFont>("Fonts/Arial16");
        _normalImg = contentManager.Load<Texture2D>("Images/button");
        _hoveredImg = contentManager.Load<Texture2D>("Images/hoverbutton");
        Width = _normalImg.Width/2;
        Height = _normalImg.Height/2;
    }

    public Rectangle SetDrawPosition(int x, int y)
    {
        _rectangle = new Rectangle(x-Width/2, y, Width, Height);
        return _rectangle;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();
        spriteBatch.Draw(IsHovered ? _hoveredImg : _normalImg, _rectangle, Color.White);
        
        var textSize = _font.MeasureString(_text);
        spriteBatch.DrawString(_font, _text, new Vector2(_rectangle.X + _rectangle.Width/2f - textSize.X/2, 
                                                                _rectangle.Y + _rectangle.Height/2f - textSize.Y/2), Color.Black);
        spriteBatch.End();
    }

    public void Select(TGCGame game)
    {
        game.GameStatus = _gameStatus;
    }
}

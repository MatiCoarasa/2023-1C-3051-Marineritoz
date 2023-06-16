using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Threading;

namespace TGC.MonoGame.TP.Menu;

public class ButtonsGrid
{
    private const int VerticalSpacing = 10;
    private Rectangle _gridSpace;
    private List<Button> _buttons;

    public ButtonsGrid(Point start, Point size, List<Button> buttons)
    {
        _gridSpace = new Rectangle(start, size);
        _buttons = buttons;
    }

    public void LoadContent(ContentManager contentManager)
    {
        foreach (var button in _buttons) button.LoadContent(contentManager);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var buttonRows = _buttons.Count;
        
        var cellHeight = (_gridSpace.Height - VerticalSpacing * (buttonRows - 1)) / buttonRows;
        for (var buttonIndex = 0; buttonIndex < _buttons.Count; buttonIndex++)
        {
            var button = _buttons[buttonIndex];
            var buttonX = _gridSpace.Center.X;
            var buttonY = _gridSpace.Top + (buttonIndex * (cellHeight + VerticalSpacing));

            button.Draw(buttonX, buttonY, false, spriteBatch);
        }
    }

}

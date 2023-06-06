using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Entities;

public class GearBox
{
    private const string ContentFolderPath = "Textures/Cambios/";
    public const string ContentFolderSpriteFonts = "Fonts/";
    private List<Texture2D> gearBoxOptions { get; set; } = new ();
    public int currentGearBoxOption { get; set; } = 1;
    public float currentVelocity  { get; set; } = 1f;
    
    private SpriteFont SpriteFont { get; set; }
    
    private Texture2D RectangleSelector { get; set; }

    public void LoadContent(GraphicsDevice graphicsDevice, ContentManager contentManager)
    {
        List<string> options = new List<string>()
        {
            "R", "N", "1", "2", "3", "4"
        };
        foreach (var option in options)
        {
            gearBoxOptions.Add(contentManager.Load<Texture2D>(ContentFolderPath + option));
        }

        SpriteFont = contentManager.Load<SpriteFont>(ContentFolderSpriteFonts + "Arial16");
        RectangleSelector = contentManager.Load<Texture2D>(ContentFolderPath + "selector");
    }

    public void Draw(SpriteBatch spriteBatch, float height)
    {
        spriteBatch.Begin();
        spriteBatch.Draw(
            gearBoxOptions[currentGearBoxOption], 
            new Vector2(0, height - 180),
            new Rectangle(0, 0, 270, 360),
            Color.White, 
            0f, 
            Vector2.One, 
            new Vector2(0.5f, 0.5f),
            SpriteEffects.None,
            0f
        );
        spriteBatch.Draw(
            RectangleSelector, 
            new Vector2(110, height - 30 * currentGearBoxOption - 25), 
            null,
            Color.LightBlue, 
            0f, 
            Vector2.Zero, 
            new Vector2(0.7f, 1.06f) ,
            SpriteEffects.None, 0f);
        spriteBatch.DrawString(
            SpriteFont, "< " + currentVelocity.ToString("0.00"), 
            new Vector2(115, height - 30 * currentGearBoxOption - 25), 
            Color.Black);
        spriteBatch.End();
    }
}
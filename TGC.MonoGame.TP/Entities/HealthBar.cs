using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Entities;

public class HealthBar
{
    private const string ContentFolderPath = "Textures/HealthBar/";
    public const string ContentFolderSpriteFonts = "Fonts/";
    private Texture2D Container { get; set; }
    private Texture2D Bar { get; set; }

    private SpriteFont SpriteFont { get; set; }
    public float Life { get; set; } = 100f;
    

    public void LoadContent(ContentManager contentManager)
    {
        SpriteFont = contentManager.Load<SpriteFont>(ContentFolderSpriteFonts + "Arial16");
        Container = contentManager.Load<Texture2D>(ContentFolderPath + "healthbar-container");
        Bar = contentManager.Load<Texture2D>(ContentFolderPath + "healthbar");
    }

    public void Draw(SpriteBatch spriteBatch, Viewport viewport)
    {
        var xPosition = viewport.Width - Container.Width / 2 - 5;
        var yContainer = (viewport.Height - Container.Height) / 2;
        var lifePercentage = Life / 100;
        spriteBatch.Begin();
        spriteBatch.Draw(
            Container, 
            new Vector2(xPosition, yContainer),
            null,
            Color.White, 
            0f, 
            Vector2.One, 
            new Vector2(0.5f, 1f),
            SpriteEffects.None,
            0f
        );
        spriteBatch.Draw(
            Bar, 
            new Vector2(xPosition, yContainer + Bar.Height * (1 - lifePercentage)), 
            null,
            Color.White, 
            0, 
            Vector2.Zero,
            new Vector2(0.5f, Life / 100),
            SpriteEffects.None, 0f);
        spriteBatch.End();
    }
}
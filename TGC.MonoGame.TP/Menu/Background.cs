using TGC.MonoGame.TP.Entities;

namespace TGC.MonoGame.TP.Menu;

public class Background
{
    private TGCGame _game;
    private ShipPlayer _ship;
    private Water _water;

    public Background(TGCGame game)
    {
        _game = game;
        _ship = new ShipPlayer(_game);
        _water = new Water(_game.GraphicsDevice);
    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Entities.Islands;

namespace TGC.MonoGame.TP.Entities;

public class EnemyShipsGenerator
{
    private List<EnemyShip> EnemyShips { get; set; } = new ();
    private int QuantityOfShips = 20;
    private Effect Effect { get; set; }
    private List<Texture2D> ColorTextures { get; } = new List<Texture2D>();

    public EnemyShipsGenerator(TGCGame game)
    {
        for (int i = 0; i < QuantityOfShips; i++)
        {
            EnemyShips.Add(new EnemyShip(game));
        }
    }

    public void LoadContent(Model model, Effect effect, Map map)
    {
        var enemyModel = model;
        Effect = effect;
        foreach (var mesh in enemyModel.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                ColorTextures.Add(((BasicEffect)meshPart.Effect).Texture);
                meshPart.Effect = Effect;
            }
        }
        foreach (var enemyShip in EnemyShips)
        {
            enemyShip.LoadContent(model, effect, map.getSafeSpawnPosition());
        }
    }

    public void Update(float totalTime, float deltaTime, Vector3 shipPosition, ShipPlayer shipPlayer, HealthBar healthBar, Map map)
    {
        foreach (var enemyShip in EnemyShips)
        {
            enemyShip.Update(totalTime, deltaTime, shipPosition);
            if (shipPlayer.CheckCollision(enemyShip.BoundingBox, healthBar))
            {
                enemyShip.RestartPosition(map.getSafeSpawnPosition());
            }
        }
    }

    public void Draw(Camera camera, Vector3 lightPosition)
    {
        foreach (var enemyShip in EnemyShips)
        {
            enemyShip.Draw(camera, lightPosition, ColorTextures);
        }
    }
    
}
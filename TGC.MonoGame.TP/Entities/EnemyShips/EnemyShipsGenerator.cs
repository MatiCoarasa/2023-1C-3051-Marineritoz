using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Entities.Islands;

namespace TGC.MonoGame.TP.Entities;

public class EnemyShipsGenerator
{
    private EnemyShip[] EnemyShips { get; set; }
    private Effect Effect { get; set; }
    private List<Texture2D> ColorTextures { get; } = new List<Texture2D>();

    public EnemyShipsGenerator(TGCGame game)
    {
        EnemyShips = new EnemyShip[4];
        for (int i = 0; i < EnemyShips.Length; i++)
        {
            EnemyShips[i] = new EnemyShip(game);
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
            enemyShip.LoadContent(model, effect, map.getSafeSpawnPosition(Vector3.Zero));
        }
    }

    public void Update(float totalTime, float deltaTime, Vector3 shipPosition, ShipPlayer shipPlayer, HealthBar healthBar, Map map)
    {
        for (int i = 0; i < EnemyShips.Length; i++)
        {
            EnemyShips[i].Update(totalTime, deltaTime, shipPosition, map.IslandColliders());
            if (shipPlayer.CheckCollision(EnemyShips[i].BoundingBox, healthBar))
            {
                EnemyShips[i].RestartPosition(map.getSafeSpawnPosition(shipPosition));
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
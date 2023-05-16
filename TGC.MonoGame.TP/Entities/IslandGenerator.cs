using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using TGC.MonoGame.TP.Entities.Islands;

namespace TGC.MonoGame.TP.Entities;

public class IslandGenerator
{
    private const string ContentFolder3D = "Models/";
    private IList<Model> IslandsModel { get; set; } = new List<Model>();
    private IList<IList<Texture2D>> IslandsTextures { get; set; } = new List<IList<Texture2D>>(); 
    private Effect Effect { get; set; }
    
    private readonly string[] _islandPaths = { "Island1/Island1", "Island2/Island2", "Island3/Island3" };

    public void LoadContent(ContentManager content, Effect effect)
    {
        Effect = effect;
        for (var i = 0; i < _islandPaths.Length; i++)
        {
            var model = content.Load<Model>(ContentFolder3D + _islandPaths[i]);
            IslandsTextures.Add(new List<Texture2D>());
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    IslandsTextures[i].Add(((BasicEffect)meshPart.Effect).Texture);
                    meshPart.Effect = Effect;
                }
            }
            IslandsModel.Add(model);
        }
    }

    private Island CreateIsland(int modelNumber, float scale, Vector3 translation)
    {
        return new Island(IslandsModel[modelNumber], Effect, IslandsTextures[modelNumber], scale, translation);
    }

    public Island[] CreateRandomIslands(int qty, float maxX, float maxZ, float noSpawnRadius)
    {
        Debug.WriteLine("[CreateRandomIslands] qty: " + qty + " maxX: " + maxX + " maxZ: " + maxZ);
        var islands = new Island[qty];

        var rnd = new Random();
        for (var i = 0; i < qty; i++)
        {
            // Resto .5f para "centralizar" el punto de origen de las islas.
            var rndX = rnd.NextSingle() - .5f;
            var rndZ = rnd.NextSingle() - .5f;

            rndX += Math.Sign(rndX) * noSpawnRadius;
            rndZ += Math.Sign(rndZ) * noSpawnRadius;
            
            var islandX = rndX * maxX;
            var islandZ = rndZ * maxZ;
            var islandVector = new Vector3(islandX, 0, islandZ);
            Debug.WriteLine("[Creating Island " + i + "] " + islandVector);

            var islandScale = rnd.NextSingle() / 100;
            var islandRotation = rnd.NextSingle() * Convert.ToSingle(Math.PI) * 2f;
            islands[i] = CreateIsland(rnd.Next(_islandPaths.Length), islandScale, new Vector3(islandX, 0, islandZ));
        }

        return islands;
    }
}
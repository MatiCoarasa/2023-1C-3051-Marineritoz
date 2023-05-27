﻿using System.Collections.Generic;
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
    private TGCGame Game { get; set; }
    private IList<Model> IslandsModel { get; set; } = new List<Model>();
    private IList<IList<Texture2D>> IslandsTextures { get; set; } = new List<IList<Texture2D>>(); 
    private Effect Effect { get; set; }
    private Random Rnd { get; set; }

    
    private readonly string[] _islandPaths = { "Island1/Island1", "Island2/Island2", "Island3/Island3" };

    public IslandGenerator(TGCGame game)
    {
        Game = game;
        Rnd = new Random();
    }

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
        return new Island(Game, IslandsModel[modelNumber], Effect, IslandsTextures[modelNumber], scale, translation);
    }

    public Island[] CreateRandomIslands(int qty, float maxX, float maxZ)
    {
        Debug.WriteLine("[CreateRandomIslands] qty: " + qty + " maxX: " + maxX + " maxZ: " + maxZ);
        var islands = new Island[qty];

        for (var i = 0; i < qty; i++)
        {
            islands[i] = CreateIslandInFreeSpace(islands, i, maxX, maxZ);
        }

        return islands;
    }

    private Island CreateIslandInFreeSpace(Island[] existingIslands, int currentIndex, float maxX, float maxZ)
    {
        var spawnBb = new BoundingBox(new Vector3(-20, -20, -20), new Vector3(20, 20, 20));
        while (true)
        {
            var islandVector = GetRandomPosition(maxX, maxZ);
            var islandScale = (Rnd.NextSingle() + .2f) / 100;
        
            var islandCandidate = CreateIsland(Rnd.Next(_islandPaths.Length), islandScale, islandVector);

            // Chequear colision con todas las islas existentes
            var islandCollisions = false;
            for (var i = 0; i < currentIndex; i++)
            {
                var existingIsland = existingIslands[i];
                if (islandCandidate.BoundingBox.Intersects(spawnBb) || islandCandidate.BoundingBox.Intersects(existingIsland.BoundingBox))
                {
                    islandCollisions = true;
                    break;
                }
            }
            
            if (!islandCollisions) return islandCandidate;
        }
    }

    private Vector3 GetRandomPosition(float maxX, float maxZ)
    {
        // Resto .5f para "centralizar" el punto de origen de las islas.
        var islandX = (Rnd.NextSingle() - .5f) * maxX;
        var islandZ = (Rnd.NextSingle() - .5f) * maxZ; 

        return new Vector3(islandX, 0, islandZ);
    }
}
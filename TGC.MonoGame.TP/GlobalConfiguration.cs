using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP;

public class GlobalConfigurationSingleton
{
    private GlobalConfigurationSingleton(){}

    private static GlobalConfigurationSingleton _instance;

    public static GlobalConfigurationSingleton GetInstance()
    {
        return _instance ??= new GlobalConfigurationSingleton();
    }
    
    // Environment config
    public Color SkyColor = new (0, 0, 15);

    // Rain config
    public float RainSize = 100;
    public float RainMaxHeight = 50;
    public float RainMinHeight = -3;
    public int RainDropCount = 500;
    public float RainSpeedDrop = 1;
    public Color RainColor = Color.White;

    // Island generation config
    //public int IslandsQuantity = 100; Inutilizada
    public int BlockSize = 600; // Calzado a 600 porque el tamaño maximo de la isla tipo 2 es 580.
    public float SquarePerSize = 20;  // Deben ser un numero PAR
    public float IslandsMinScale = .8f;  // Value between 0 and 1
    public float IslandsMaxScale = 1f;  // Value between 0 and 1
    public float IslandsMaxXSpawn = 3000;
    public float IslandsMaxZSpawn = 3000;
    public float SpawnBoxSize = 50;  // Area rectangular en la cual no spawnean islas

    // Player Ship config
    public float PlayerScale => .00025f;
    public float[] PlayerVelocities { get; } = {-8, 0, 2, 4, 6, 8};  // Lista de 5 cambios de velocidad
    public float PlayerSecsBetweenChanges = .5f;  // Tiempo de bloqueo entre cambios del barco
    public float PlayerMaxRotationVelocity = 1;
    public float PlayerAcceleration = 2;
    
    // Cameras Config
    public float FollowCameraDistanceInEnvironment { get; set; } = 3.200f;
    public float ShipCameraRadiusInEnvironment { get; set; } = 50f;
    public float MenuCamaraDistanceInEnvironment { get; set; } = 18f;

    // Water Config
    public Color WaterAmbientColor { get; set; } = new (124, 208, 255);
    public Color WaterDiffuseColor { get; set; } = new (7, 125, 244);
    public Color WaterSpecularColor { get; set; } = new (255, 255, 255);
    public float WaterKAmbient { get; set; } = 0.59f;
    public float WaterKDiffuse { get; set; } = 0.53f;
    public float WaterKSpecular { get; set; } = 0.27f;
    public float WaterShininess { get; set; } = 6.67f;
    public Color WaterColor { get; set; } = new (57, 180, 211);

    // Island Config
    public Color IslandAmbientColor { get; set; } = new (219, 244, 76);
    public Color IslandDiffuseColor { get; set; } = new (124, 125, 121);
    public Color IslandSpecularColor { get; set; } = new (71, 71, 65);
    public float IslandKAmbient { get; set; } = 0.480f;
    public float IslandKDiffuse { get; set; } = 0.400f;
    public float IslandKSpecular { get; set; } = 0.2f;
    public float IslandShininess { get; set; } = 32f;
    
    // Ship Config
    public float DistanceAboveWater { get; set; } = 0.5f;
    public Color ShipAmbientColor { get; set; } = new (164, 156, 156);
    public Color ShipDiffuseColor { get; set; } = new (126, 117, 110);
    public Color ShipSpecularColor { get; set; } = new (204, 204, 204);
    public float ShipKAmbient { get; set; } = 0.53f;
    public float ShipKDiffuse { get; set; } = 0.2f;
    public float ShipKSpecular { get; set; } = 0.2f;
    public float ShipShininess { get; set; } = 32f;

    public List<Vector4> Waves { get; set; } = new List<Vector4>()
    {
        new Vector4(0.1f, 20, 1, 1),
        new Vector4(0.1f, 10, 1, 0.6f),
        new Vector4(0.1f, 5, 1, 1.3f)
    };
}

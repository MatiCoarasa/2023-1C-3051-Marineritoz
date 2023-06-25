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
    public float RainMaxHeight = 150;
    public float RainMinHeight = -3;
    public int RainDropCount = 500;
    public float RainSpeedDrop = 1;
    public Color RainColor = Color.White;

    // Island generation config
    public int IslandsQuantity = 100;
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

    // Water Config
    public Color WaterAmbientColor { get; set; } = new (124, 208, 255);
    public Color WaterDiffuseColor { get; set; } = new (7, 125, 244);
    public Color WaterSpecularColor { get; set; } = new (255, 255, 255);
    public float WaterKAmbient { get; set; } = 0.59f;
    public float WaterKDiffuse { get; set; } = 0.53f;
    public float WaterKSpecular { get; set; } = 0.27f;
    public float WaterShininess { get; set; } = 6.67f;
    public Color WaterColor { get; set; } = new (57, 180, 211);
}

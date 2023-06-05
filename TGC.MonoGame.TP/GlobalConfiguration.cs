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
    public Color SkyColor = Color.Aqua;

    // Rain config
    public float RainSize = 100;
    public float RainMaxHeight = 150;
    public float RainMinHeight = -3;
    public float RainSpeedDrop = 500;
    public int RainDropCount = 1;
    
    // Island generation config
    public int IslandsQuantity = 100;
    public float IslandsMinScale = .8f;  // Value between 0 and 1
    public float IslandsMaxScale = 1f;  // Value between 0 and 1
    public float IslandsMaxXSpawn = 3000;
    public float IslandsMaxZSpawn = 3000;
    public float SpawnBoxSize = 50;  // Area rectangular en la cual no spawnean islas

    // Player Ship config
    public float PlayerScale => .00025f;
    public float[] PlayerVelocities { get; } = {-20, 0, 10, 20, 30, 40};  // Lista de 5 cambios de velocidad
    public float PlayerSecsBetweenChanges = .5f;  // Tiempo de bloqueo entre cambios del barco
    public float PlayerMaxRotationVelocity = 1;
    public float PlayerAcceleration = 2;
    
}

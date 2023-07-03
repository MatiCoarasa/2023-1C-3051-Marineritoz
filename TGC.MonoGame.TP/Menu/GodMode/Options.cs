using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Utils.GUI.Modifiers;

namespace TGC.MonoGame.TP.Menu.GodMode;

public static class Options
{
    private static GlobalConfigurationSingleton GlobalConfig => GlobalConfigurationSingleton.GetInstance();
    private static ModifierController WaterModifier = new ();
    private static ModifierController CameraModifier = new();
    private static ModifierController WavesModifier = new();
    private static ModifierController IslandModifier = new();
    private static ModifierController ShipModifier = new();

    public static void LoadModifiers()
    {
        WaterModifier.AddColor("Ambient Color", delegate(Color color)
        {
            GlobalConfig.WaterAmbientColor = color;
        }, GlobalConfig.WaterAmbientColor);
        WaterModifier.AddColor("Diffuse Color", delegate(Color color)
        {
            GlobalConfig.WaterDiffuseColor = color;
        }, GlobalConfig.WaterDiffuseColor);
        WaterModifier.AddColor("Specular Color", delegate(Color color)
        {
            GlobalConfig.WaterSpecularColor = color;
        }, GlobalConfig.WaterSpecularColor);
        WaterModifier.AddColor("Color", delegate(Color color)
        {
            GlobalConfig.WaterColor = color;
        }, GlobalConfig.WaterColor);
        WaterModifier.AddFloat("KAmbient", delegate(float value)
        {
            GlobalConfig.WaterKAmbient = value;
        }, GlobalConfig.WaterKAmbient, 0f, 1f);
        WaterModifier.AddFloat("KDiffuse", delegate(float value)
        {
            GlobalConfig.WaterKDiffuse = value;
        }, GlobalConfig.WaterKDiffuse, 0f, 1f);
        WaterModifier.AddFloat("KSpecular", delegate(float value)
        {
            GlobalConfig.WaterKSpecular = value;
        }, GlobalConfig.WaterKSpecular, 0f, 1f);
        WaterModifier.AddFloat("Shininess", delegate(float value)
        {
            GlobalConfig.WaterShininess = value;
        }, GlobalConfig.WaterShininess, 1f, 64f);
        CameraModifier.AddFloat("Ship Camera Radius In Environment", delegate(float value)
        {
            GlobalConfig.ShipCameraRadiusInEnvironment = value;
        }, GlobalConfig.ShipCameraRadiusInEnvironment);
        CameraModifier.AddFloat("Follow Camera Distance In Environment", delegate(float value)
        {
            GlobalConfig.FollowCameraDistanceInEnvironment = value;
        }, GlobalConfig.FollowCameraDistanceInEnvironment);
        CameraModifier.AddFloat("Menu Camera Distance In Environment", delegate(float value)
        {
            GlobalConfig.MenuCamaraDistanceInEnvironment = value;
        }, GlobalConfig.MenuCamaraDistanceInEnvironment);
        AddWave(0);
        AddWave(1);
        AddWave(2);
        IslandModifiers();
        ShipModifiers();
    }

    public static void AddWave(int i)
    {
        WavesModifier.AddFloat($"Wave {i + 1} steepness", delegate(float value)
        {
            var wave = GlobalConfig.Waves[i];
            wave.X = value;
            GlobalConfig.Waves[i] = wave;
        }, GlobalConfig.Waves[i].X);
        WavesModifier.AddFloat($"Wave {i + 1} wave length", delegate(float value)
        {
            var wave = GlobalConfig.Waves[i];
            wave.Y = value;
            GlobalConfig.Waves[i] = wave;
        }, GlobalConfig.Waves[i].Y);
        WavesModifier.AddFloat($"Wave {i + 1} X", delegate(float value)
        {
            var wave = GlobalConfig.Waves[i];
            wave.Z = value;
            GlobalConfig.Waves[i] = wave;
        }, GlobalConfig.Waves[i].Z);
        WavesModifier.AddFloat($"Wave {i + 1} Y", delegate(float value)
        {
            var wave = GlobalConfig.Waves[i];
            wave.W = value;
            GlobalConfig.Waves[i] = wave;
        }, GlobalConfig.Waves[i].W);
    }

    private static void IslandModifiers()
    {
        IslandModifier.AddColor("Ambient Color", delegate(Color color)
        {
            GlobalConfig.IslandAmbientColor = color;
        }, GlobalConfig.IslandAmbientColor);
        IslandModifier.AddColor("Diffuse Color", delegate(Color color)
        {
            GlobalConfig.IslandDiffuseColor = color;
        }, GlobalConfig.IslandDiffuseColor);
        IslandModifier.AddColor("Specular Color", delegate(Color color)
        {
            GlobalConfig.IslandSpecularColor = color;
        }, GlobalConfig.IslandSpecularColor);
        IslandModifier.AddFloat("KAmbient", delegate(float value)
        {
            GlobalConfig.IslandKAmbient = value;
        }, GlobalConfig.IslandKAmbient, 0f, 1f);
        IslandModifier.AddFloat("KDiffuse", delegate(float value)
        {
            GlobalConfig.IslandKDiffuse = value;
        }, GlobalConfig.IslandKDiffuse, 0f, 1f);
        IslandModifier.AddFloat("KSpecular", delegate(float value)
        {
            GlobalConfig.IslandKSpecular = value;
        }, GlobalConfig.IslandKSpecular, 0f, 1f);
        IslandModifier.AddFloat("Shininess", delegate(float value)
        {
            GlobalConfig.IslandShininess = value;
        }, GlobalConfig.IslandShininess, 1f, 64f);
    }
    
    private static void ShipModifiers()
    {
        ShipModifier.AddFloat("Up from Water", delegate(float value)
        {
            GlobalConfig.DistanceAboveWater = value;
        }, GlobalConfig.DistanceAboveWater);
        ShipModifier.AddColor("Ambient Color", delegate(Color color)
        {
            GlobalConfig.ShipAmbientColor = color;
        }, GlobalConfig.ShipAmbientColor);
        ShipModifier.AddColor("Diffuse Color", delegate(Color color)
        {
            GlobalConfig.ShipDiffuseColor = color;
        }, GlobalConfig.ShipDiffuseColor);
        ShipModifier.AddColor("Specular Color", delegate(Color color)
        {
            GlobalConfig.ShipSpecularColor = color;
        }, GlobalConfig.ShipSpecularColor);
        ShipModifier.AddFloat("KAmbient", delegate(float value)
        {
            GlobalConfig.ShipKAmbient = value;
        }, GlobalConfig.ShipKAmbient, 0f, 1f);
        ShipModifier.AddFloat("KDiffuse", delegate(float value)
        {
            GlobalConfig.ShipKDiffuse = value;
        }, GlobalConfig.ShipKDiffuse, 0f, 1f);
        ShipModifier.AddFloat("KSpecular", delegate(float value)
        {
            GlobalConfig.ShipKSpecular = value;
        }, GlobalConfig.ShipKSpecular, 0f, 1f);
        ShipModifier.AddFloat("Shininess", delegate(float value)
        {
            GlobalConfig.ShipShininess = value;
        }, GlobalConfig.ShipShininess, 1f, 64f);
    }

    public static void DrawLayout()
    {
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, 0));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 6f,
            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100));

        ImGui.Begin("TGC samples explorer", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoMove);

        if (ImGui.CollapsingHeader("Water"))
        {
            WaterModifier.Draw();
            ImGui.EndMenu();
        }
        if (ImGui.CollapsingHeader("Island"))
        {
            IslandModifier.Draw();
            ImGui.EndMenu();
        }
        if (ImGui.CollapsingHeader("Waves"))
        {
            WavesModifier.Draw();
            ImGui.EndMenu();
        }
        if (ImGui.CollapsingHeader("Cameras"))
        {
            CameraModifier.Draw();
            ImGui.EndMenu();
        }
        if (ImGui.CollapsingHeader("Ship"))
        {
            ShipModifier.Draw();
            ImGui.EndMenu();
        }

        ImGui.End();
    }
}
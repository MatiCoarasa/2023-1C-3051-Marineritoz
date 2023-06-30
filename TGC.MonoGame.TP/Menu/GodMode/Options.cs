using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Utils.GUI.Modifiers;

namespace TGC.MonoGame.TP.Menu.GodMode;

public static class Options
{
    private static GlobalConfigurationSingleton GlobalConfig => GlobalConfigurationSingleton.GetInstance();
    private static ModifierController WaterModifier = new ();
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
        ShipModifier.AddVector("Translation In Environment", delegate(Vector3 value)
        {
            GlobalConfig.PlayerTranslationInEnvironment = value;
        }, GlobalConfig.PlayerTranslationInEnvironment);
        ShipModifier.AddFloat("Scale In Environment", delegate(float value)
        {
            GlobalConfig.PlayerScaleInEnvironment = value;
        }, GlobalConfig.PlayerScaleInEnvironment);
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
        if (ImGui.CollapsingHeader("Ship"))
        {
            ShipModifier.Draw();
            ImGui.EndMenu();
        }

        ImGui.End();
    }
}
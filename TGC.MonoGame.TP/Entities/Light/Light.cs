﻿using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Entities.Light;

public struct Light
{
    public Vector3 Position;
    public Vector3 Color;
    public Vector3 ShowColor;

    private Light(Vector3 position, Vector3 color, Vector3 showColor) 
    {
        Position = position;
        Color = color;
        ShowColor = showColor;
    }

    internal Light GenerateShowColor()
    {
        return new Light(Position, Color, Vector3.Normalize(Color) * 2f);
    }
}
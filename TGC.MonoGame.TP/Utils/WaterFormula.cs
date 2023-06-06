﻿using System;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.DataClass;

namespace TGC.MonoGame.TP.Utils;

public static class WaterFormula
{
    private static Vector3 GerstnerWave(float time, ref Vector3 tangent, ref Vector3 binormal, Vector4 wave, Vector3 position)
    {
        var steepness = wave.X;
        var waveLength = wave.Y;
        var direction = new Vector2(wave.Z, wave.W);
        direction.Normalize();
        var k = 2 * Math.PI / waveLength;
        var speed = Math.Sqrt(9.8 / k);
        var waveAmplitude = steepness / k;
        var frequency = k * (Vector2.Dot(direction, new Vector2(position.X, position.Z)) - speed * time);
        var frequencyWithAmplitude = waveAmplitude * Math.Cos(frequency);

        var auxiliar = Convert.ToSingle(-direction.X * direction.Y * (steepness * Math.Sin(frequency)));
        tangent += new Vector3(
            // Convert.ToSingle(-direction.X * direction.X * (steepness * Math.Sin(frequency))),
            0,
            Convert.ToSingle(direction.X * (steepness * Math.Cos(frequency))),
            auxiliar
        );

        binormal += new Vector3(
            // auxiliar,
            0,
            Convert.ToSingle(direction.Y * (steepness * Math.Cos(frequency))),
            Convert.ToSingle(-direction.Y * direction.Y * (steepness * Math.Sin(frequency)))
        );
        return new Vector3(
            Convert.ToSingle(direction.X * frequencyWithAmplitude),
            Convert.ToSingle(waveAmplitude * Math.Sin(frequency)),
            Convert.ToSingle(direction.Y * frequencyWithAmplitude)
        );
    }
    
    private static Vector3 GerstnerWaveNvidia(float time, ref Vector3 tangent, ref Vector3 binormal, ref Vector3 normal, Vector4 wave, Vector3 position)
    {
        var steepness = wave.X;
        var waveLength = wave.Y;
        var direction = new Vector2(wave.Z, wave.W);
        direction.Normalize();
        var k = 2 * Math.PI / waveLength;
        var speed = Math.Sqrt(9.8 / k);
        var waveAmplitude = steepness / k;
        var frequency = k * (Vector2.Dot(direction, new Vector2(position.X, position.Z)) - speed * time);
        var frequencyWithAmplitude = waveAmplitude * Math.Cos(frequency);
        var WA = waveAmplitude * k;
        var S = Math.Sin(frequency);
        var C = Math.Cos(frequency);
        binormal += new Vector3(
            Convert.ToSingle(-steepness * direction.X * direction.Y * WA * S),
            Convert.ToSingle(direction.Y * WA * C),
            Convert.ToSingle(-steepness * direction.Y * direction.Y * WA * S)
        );
        tangent += new Vector3(
            Convert.ToSingle(-steepness * direction.X * direction.X * WA * S),
            Convert.ToSingle(direction.X * WA * S),
            Convert.ToSingle(-steepness * direction.X * direction.Y * WA * S)
        );
        normal += new Vector3(
            Convert.ToSingle(-direction.X * WA * C),
            Convert.ToSingle(-steepness * WA * S),
            Convert.ToSingle(-direction.Y * WA * C)
        );
        return new Vector3(
            Convert.ToSingle(direction.X * frequencyWithAmplitude),
            Convert.ToSingle(waveAmplitude * Math.Sin(frequency)),
            Convert.ToSingle(direction.Y * frequencyWithAmplitude)
        );
    }

    private static Vector4 Wave(float steepness, float waveLength, Vector2 direction)
    {
        return new Vector4(steepness, waveLength, direction.X, direction.Y);
    }

    public static WaterPosition GetPositionInWaveNvidia(this Vector3 anchorPosition, float timer)
    {
        var position = anchorPosition;
        var tangent = new Vector3(0, 0, 1);
        var binormal = new Vector3(1, 0, 0);
        var normal = new Vector3(0, 1, 0);
        var wave1 = Wave(0.25f, 15, new Vector2(1,1));
        var wave2 = Wave(0.25f, 10, new Vector2(1,0.6f));
        var wave3 = Wave(0.25f, 5, new Vector2(1,1.3f));
        position += GerstnerWaveNvidia(timer, ref tangent, ref binormal, ref normal, wave1, anchorPosition);
        position += GerstnerWaveNvidia(timer, ref tangent, ref binormal, ref normal, wave2, anchorPosition);
        position += GerstnerWaveNvidia(timer, ref tangent, ref binormal, ref normal, wave3, anchorPosition);
        return new WaterPosition(position, tangent, binormal);
    }

    public static WaterPosition GetPositionInWave(this Vector3 anchorPosition, float timer)
    {
        var position = anchorPosition;
        var tangent = Vector3.Zero;
        var binormal = Vector3.Zero;
        var wave1 = Wave(0.25f, 15, new Vector2(1,1));
        var wave2 = Wave(0.25f, 10, new Vector2(1,0.6f));
        var wave3 = Wave(0.25f, 5, new Vector2(1,1.3f));
        position += GerstnerWave(timer, ref tangent, ref binormal, wave1, anchorPosition);
        position += GerstnerWave(timer, ref tangent, ref binormal, wave2, anchorPosition);
        position += GerstnerWave(timer, ref tangent, ref binormal, wave3, anchorPosition);
        return new WaterPosition(position, tangent, binormal);
    }
}
using System;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.DataClass;

namespace TGC.MonoGame.TP.Utils;

public static class WaterFormula
{
    private static GlobalConfigurationSingleton GlobalConfig => GlobalConfigurationSingleton.GetInstance();
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
            Convert.ToSingle(-direction.X * direction.X * (steepness * Math.Sin(frequency))),
            Convert.ToSingle(direction.X * (steepness * Math.Cos(frequency))),
            auxiliar
        );

        binormal += new Vector3(
            auxiliar,
            Convert.ToSingle(direction.Y * (steepness * Math.Cos(frequency))),
            Convert.ToSingle(-direction.Y * direction.Y * (steepness * Math.Sin(frequency)))
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

    public static WaterPosition GetPositionInWave(this Vector3 anchorPosition, float timer)
    {
        var position = anchorPosition;
        var tangent = new Vector3(1, 0, 0);
        var binormal = new Vector3(0, 0, 1);
        position += GerstnerWave(timer, ref tangent, ref binormal, GlobalConfig.Waves[0], anchorPosition);
        position += GerstnerWave(timer, ref tangent, ref binormal, GlobalConfig.Waves[1], anchorPosition);
        position += GerstnerWave(timer, ref tangent, ref binormal, GlobalConfig.Waves[2], anchorPosition);
        return new WaterPosition(position, tangent, binormal);
    }
}
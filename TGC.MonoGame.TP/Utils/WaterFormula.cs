using System;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.DataClass;

namespace TGC.MonoGame.TP.Utils;

public static class WaterFormula
{
    private static Vector3 GerstnerWave(float time, ref Vector3 tangent, ref Vector3 binormal, float steepness, float waveLength, Vector2 direction, Vector3 position)
    {
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

    public static WaterPosition GetPositionInWave(this Vector3 anchorPosition, float timer)
    {
        anchorPosition.Y = 0;
        var position = anchorPosition;
        var tangent = Vector3.Zero;
        var binormal = Vector3.Zero;
        position += GerstnerWave(timer, ref tangent, ref binormal, 0.25f, 15, new Vector2(1,1), anchorPosition);
        position += GerstnerWave(timer, ref tangent, ref binormal, 0.25f, 10, new Vector2(1,0.6f), anchorPosition);
        position += GerstnerWave(timer, ref tangent, ref binormal, 0.25f, 5, new Vector2(1,1.3f), anchorPosition);
        return new WaterPosition(position, tangent, binormal);
    }
}
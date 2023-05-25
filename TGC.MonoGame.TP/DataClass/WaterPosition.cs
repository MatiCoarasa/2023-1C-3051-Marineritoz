using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.DataClass;

public class WaterPosition
{
    public WaterPosition(Vector3 _position, Vector3 _tangent, Vector3 _binormal)
    {
        position = _position;
        tangent = _tangent;
        binormal = _binormal;
    }

    public Vector3 position { get; set; }
    public Vector3 tangent { get; set; }
    public Vector3 binormal { get; set; }
}
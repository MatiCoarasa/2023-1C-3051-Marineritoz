using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.DataClass;

public class WaterPosition
{
    public WaterPosition()
    {
        position = Vector3.Zero;
        tangent = Vector3.Zero;
        binormal = Vector3.Zero;
        normal = Vector3.Zero;
    }
    
    public WaterPosition(Vector3 _position, Vector3 _tangent, Vector3 _binormal)
    {
        position = _position;
        tangent = _tangent;
        binormal = _binormal;
        normal = Vector3.Cross(_binormal, _tangent);
        normal.Normalize();
    }

    public Vector3 position { get; set; }
    public Vector3 tangent { get; set; }
    public Vector3 binormal { get; set; }
    public Vector3 normal { get; set; }
}
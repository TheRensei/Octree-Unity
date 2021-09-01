using UnityEngine;

public static class VectorExtensionsMethods
{
    public static Vector3 AddFloat(this Vector3 v, float f)
    {
        return new Vector3(v.x + f, v.y + f, v.z + f);
    }
    public static Vector3 SubtractFloat(this Vector3 v, float f)
    {
        return new Vector3(v.x - f, v.y - f, v.z - f);
    }
}

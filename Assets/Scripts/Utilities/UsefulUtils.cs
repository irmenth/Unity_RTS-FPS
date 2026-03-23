using UnityEngine;

public static class UsefulUtils
{
    public static int GetLayer(LayerMask mask)
    {
        int maskValue = mask.value;
        int layer = -1;

        while (maskValue > 0)
        {
            maskValue >>= 1;
            layer++;
        }

        return layer;
    }

    public static Vector3 V2ToV3(Vector2 v2, float y = 0f)
    {
        return new Vector3(v2.x, y, v2.y);
    }

    public static Vector2 V3ToV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }
}

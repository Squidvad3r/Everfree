using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float Get2DPerlin(Vector2 pos, float offset, float scale)
    {
        return Mathf.PerlinNoise((pos.x + 0.1f) / VoxelData.chunkWidth * scale + offset,
                                (pos.y + 0.1f) / VoxelData.chunkWidth * scale + offset);
    }

    public static bool Get3DPerlin(Vector3 pos, float offset, float scale, float threshold)
    {
        float x = (pos.x + offset + 0.1f) * scale;
        float y = (pos.y + offset + 0.1f) * scale;
        float z = (pos.z + offset + 0.1f) * scale;

        float XY = Mathf.PerlinNoise(x, y);
        float YZ = Mathf.PerlinNoise(y, z);
        float XZ = Mathf.PerlinNoise(x, z);
        float YX = Mathf.PerlinNoise(y, x);
        float ZY = Mathf.PerlinNoise(z, y);
        float ZX = Mathf.PerlinNoise(z, x);

        if((XY + YZ + XZ + YX + ZY + ZX) / 6f > threshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

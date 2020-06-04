using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord coord;

    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    World world;
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    int[,,] voxelMap = new int[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

    public bool isLoaded
    {
        get
        {
            return chunkObject.activeSelf;
        }
        set
        {
            chunkObject.SetActive(value);
        }
    }

    public Vector3 position
    {
        get
        {
            return chunkObject.transform.position;
        }
    }

    public Chunk(ChunkCoord _coord, World _world)
    {
        world = _world;
        coord = _coord;
        chunkObject = new GameObject();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshFilter = chunkObject.AddComponent<MeshFilter>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkWidth, 0, coord.z * VoxelData.chunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.z;

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }

    void PopulateVoxelMap()
    {
        for(int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for(int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for(int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + position);
                }
            }
        }
    }

    void CreateMeshData()
    {
        for(int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for(int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for(int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    if(world.blockTypes[voxelMap[x, y, z]].isSolid)
                    {
                        AddVoxelDataToChunk(new Vector3(x, y, z));
                    }
                }
            }
        }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if(x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if(!IsVoxelInChunk(x, y, z))
        {
            return world.blockTypes[world.GetVoxel(pos + position)].isSolid;
        }

        return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }

    void AddVoxelDataToChunk(Vector3 pos)
    {
        for(int h = 0; h < 6; h++)
        {
            if(!CheckVoxel(pos + VoxelData.faceChecks[h]))
            {
                int blockID = voxelMap[(int) pos.x, (int) pos.y, (int) pos.z];

                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[h, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[h, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[h, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[h, 3]]);

                AddTexture(world.blockTypes[blockID].GetTextureID(h));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);

                vertexIndex += 4;
            }
        }
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }
}

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public bool Equals(ChunkCoord otherChunk)
    {
        if(otherChunk == null)
        {
            return false;
        }
        else if(otherChunk.x == x && otherChunk.z == z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
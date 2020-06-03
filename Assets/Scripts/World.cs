﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Transform player;
    public Vector3 spawnPos;
    public Material material;
    public BlockType[] blockTypes;

    Chunk[,] chunks = new Chunk[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];
    List<ChunkCoord> loadedChunks = new List<ChunkCoord>();
    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    private void Start()
    {
        spawnPos = new Vector3((VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f,
                                VoxelData.chunkHeight + 2f,
                                (VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromPos(player.position);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromPos(player.position);

        if(!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }
    }

    void GenerateWorld()
    {
        for(int x = (VoxelData.worldSizeInChunks / 2) - VoxelData.viewDistanceInChunks;
            x < (VoxelData.worldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; x++)
        {
            for(int z = (VoxelData.worldSizeInChunks / 2) - VoxelData.viewDistanceInChunks;
                z < (VoxelData.worldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; z++)
            {
                CreateNewChunk(x, z);
            }
        }

        player.position = spawnPos;
    }

    ChunkCoord GetChunkCoordFromPos(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);

        return new ChunkCoord(x, z);
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromPos(player.position);
        List<ChunkCoord> prevLoadedChunks = new List<ChunkCoord>(loadedChunks);

        for(int x = coord.x - VoxelData.viewDistanceInChunks; x < coord.x + VoxelData.viewDistanceInChunks; x++)
        {
            for(int z = coord.z - VoxelData.viewDistanceInChunks; z < coord.z + VoxelData.viewDistanceInChunks; z++)
            {
                if(IsChunkInWorld(new ChunkCoord(x, z)))
                {
                    if(chunks[x, z] == null)
                    {
                        CreateNewChunk(x, z);
                    }
                    else if(!chunks[x, z].isLoaded)
                    {
                        chunks[x, z].isLoaded = true;
                            loadedChunks.Add(new ChunkCoord(x, z));
                    }
                }

                for(int i = 0; i < prevLoadedChunks.Count; i++)
                {
                    if(prevLoadedChunks[i].Equals(new ChunkCoord(x, z)))
                    {
                        prevLoadedChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach(ChunkCoord c in prevLoadedChunks)
        {
            chunks[c.x, c.z].isLoaded = false;
        }
    }

    void CreateNewChunk(int x, int z)
    {
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        loadedChunks.Add(new ChunkCoord(x, z));
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        if(coord.x > 0 && coord.x < VoxelData.worldSizeInChunks - 1 &&
            coord.z > 0 && coord.z < VoxelData.worldSizeInChunks - 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if(pos.x >= 0 && pos.x < VoxelData.worldSizeInVoxels &&
            pos.y >= 0 && pos.y < VoxelData.chunkHeight &&
            pos.z >= 0 && pos.z < VoxelData.worldSizeInVoxels)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetVoxel(Vector3 pos)
    {
        if(!IsVoxelInWorld(pos))
        {
            return 0;
        }

        if(pos.y < 1)
        {
            return 4;
        }
        else if(pos.y >= VoxelData.worldSizeInVoxels - 4 && pos.y != VoxelData.worldSizeInVoxels - 1)
        {
            return 3;
        }
        else if(pos.y == VoxelData.worldSizeInVoxels - 1)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

    [Header("Texture Values")]
    public int backFaceTex;
    public int frontFaceTex;
    public int topFaceTex;
    public int bottomFaceTex;
    public int leftFaceTex;
    public int rightFaceTex;

    public int GetTextureID(int faceIndex)
    {
        switch(faceIndex)
        {
            case 0:
                return backFaceTex;
            case 1:
                return frontFaceTex;
            case 2:
                return topFaceTex;
            case 3:
                return bottomFaceTex;
            case 4:
                return leftFaceTex;
            case 5:
                return rightFaceTex;
            default:
                Debug.Log("Error in GetTextureID; Invalid face index!");
                return 0;
        }
    }
}
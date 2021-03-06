﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int seed;
    private bool isCreatingChunks;
    public BiomeAttributes biome;
    public Transform player;
    public Vector3 spawnPos;
    public Material material;
    public BlockType[] blockTypes;
    public GameObject debugScreen;

    Chunk[,] chunks = new Chunk[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];
    List<ChunkCoord> loadedChunks = new List<ChunkCoord>();
    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();

    private void Start()
    {
        Random.InitState(seed);

        spawnPos = new Vector3((VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f,
                                VoxelData.chunkHeight - 50f,
                                (VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromPos(player.position);
        debugScreen.SetActive(false);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromPos(player.position);

        if(Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);

        if(!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if(chunksToCreate.Count > 0 && !isCreatingChunks)
            StartCoroutine("CreateChunks");
    }

    void GenerateWorld()
    {
        for(int x = (VoxelData.worldSizeInChunks / 2) - VoxelData.viewDistanceInChunks;
            x < (VoxelData.worldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; x++)
        {
            for(int z = (VoxelData.worldSizeInChunks / 2) - VoxelData.viewDistanceInChunks;
                z < (VoxelData.worldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; z++)
            {
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                loadedChunks.Add(new ChunkCoord(x, z));
            }
        }

        player.position = spawnPos;
    }

    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while(chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;
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

        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> prevLoadedChunks = new List<ChunkCoord>(loadedChunks);

        for(int x = coord.x - VoxelData.viewDistanceInChunks; x < coord.x + VoxelData.viewDistanceInChunks; x++)
        {
            for(int z = coord.z - VoxelData.viewDistanceInChunks; z < coord.z + VoxelData.viewDistanceInChunks; z++)
            {
                if(IsChunkInWorld(new ChunkCoord(x, z)))
                {
                    if(chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    else if(!chunks[x, z].isLoaded)
                    {
                        chunks[x, z].isLoaded = true;
                        
                    }

                    loadedChunks.Add(new ChunkCoord(x, z));
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

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if(!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.chunkHeight)
            return false;
        
        if(chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalPos(pos)].isSolid;
        
        return blockTypes[GetVoxel(pos)].isSolid;
    }

    public int GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        /* Immutable Pass */

        // If outside world, make it air.
        if(!IsVoxelInWorld(pos))
        {
            return 0;
        }

        // If bottom block, make it bedrock.
        if(yPos == 0)
        {
            return 1;
        }

        /* Basic Terrain Pass */

        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale))
                            + biome.solidGroundHeight;
        int voxelValue = 0;

        if(yPos == terrainHeight)
        {
            voxelValue = 2;
        }
        else if(yPos < terrainHeight && yPos >= terrainHeight - 4)
        {
            voxelValue = 3;
        }
        else if(yPos > terrainHeight)
        {
            return 0;
        }
        else
        {
            voxelValue = 1;
        }

        /* Second Pass */

        if(voxelValue == 1 || voxelValue == 2 || voxelValue == 3)
        {
            foreach(Lode lode in biome.lodes)
            {
                if(yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if(Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }
        
        return voxelValue;
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
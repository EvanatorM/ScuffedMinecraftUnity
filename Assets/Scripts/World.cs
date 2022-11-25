using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    North,
    East,
    South,
    West,
    Up,
    Down
}

public class ChunkData
{
    public Vector3Int chunkNum;
    public int[,,] blocks;
}

public class World : MonoBehaviour
{
    [SerializeField] Chunk chunkPrefab;
    [SerializeField] Vector3 worldSize;
    [SerializeField] Vector3Int chunkSize;

    [SerializeField] int seed;
    [SerializeField] NoiseSettings noiseSettings;

    Dictionary<Vector3Int, ChunkData> chunkData;

    public static World Instance;

    private void Awake()
    {
        Instance = this;

        Blocks.LoadResources();
    }

    void Start()
    {
        chunkData = new Dictionary<Vector3Int, ChunkData>();

        GenerateChunkData();
        GenerateWorld();
    }

    void GenerateChunkData()
    {
        for (int y = 0; y < worldSize.y; y++)
        {
            for (int z = 0; z < worldSize.z; z++)
            {
                for (int x = 0; x < worldSize.x; x++)
                {
                    GenerateChunk(new Vector3Int(x, y, z));
                }
            }
        }
    }

    void GenerateChunk(Vector3Int chunkPos)
    {
        ChunkData chunk = new ChunkData();
        chunk.chunkNum = chunkPos;

        int[,,] blocks = new int[chunkSize.x, chunkSize.y, chunkSize.z];

        for (int y = 0; y < chunkSize.y; y++)
        {
            for (int z = 0; z < chunkSize.z; z++)
            {
                for (int x = 0; x < chunkSize.x; x++)
                {
                    int blockX = x + (chunkPos.x * chunkSize.x);
                    int blockY = y + (chunkPos.y * chunkSize.y);
                    int blockZ = z + (chunkPos.z * chunkSize.z);

                    // Get height at position
                    int height = Mathf.RoundToInt(Noise.GetHeight(seed, noiseSettings, blockX, blockZ));

                    if (blockY == height) // Equal to height (Grass Layer)
                        blocks[x, y, z] = (int)Blocks.BLOCKS_BY_NAME.GRASS_BLOCK;
                    else if (blockY < height) // Less than height (Dirt Layer)
                        blocks[x, y, z] = (int)Blocks.BLOCKS_BY_NAME.DIRT_BLOCK;
                    else // Greater than height (Air)
                        blocks[x, y, z] = -1;
                }
            }
        }

        chunk.blocks = blocks;

        chunkData.Add(chunkPos, chunk);
    }

    void GenerateWorld()
    {
        for (int y = 0; y < worldSize.y; y++)
        {
            for (int z = 0; z < worldSize.z; z++)
            {
                for (int x = 0; x < worldSize.x; x++)
                {
                    Vector3Int chunkPos = new Vector3Int(x * chunkSize.x, y * chunkSize.y, z * chunkSize.z);
                    Chunk chunk = Instantiate(chunkPrefab, chunkPos, Quaternion.identity, transform);

                    chunk.InitChunk(chunkData[new Vector3Int(x, y, z)], chunkSize);
                }
            }
        }
    }

    public ChunkData GetNeighboringChunk(ChunkData chunk, Direction dir)
    {
        Vector3Int checkPos = chunk.chunkNum;
        switch (dir)
        {
            case Direction.North:
                checkPos.z++;
                break;
            case Direction.East:
                checkPos.x++;
                break;
            case Direction.South:
                checkPos.z--;
                break;
            case Direction.West:
                checkPos.x--;
                break;
            case Direction.Up:
                checkPos.y++;
                break;
            case Direction.Down:
                checkPos.y--;
                break;
        }

        if (chunkData.ContainsKey(checkPos))
            return chunkData[checkPos];

        return null;
    }
}

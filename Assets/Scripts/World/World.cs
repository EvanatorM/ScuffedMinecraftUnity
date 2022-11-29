using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
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
    [SerializeField] Player player;
    [SerializeField] int viewDistance;

    [SerializeField] Chunk chunkPrefab;
    [SerializeField] Vector3 worldSize;
    [SerializeField] Vector3Int chunkSize;
    [SerializeField] Vector3Int worldChunkOffset;

    [SerializeField] int seed;
    [SerializeField] NoiseSettings noiseSettings;
    [SerializeField] NoiseSettings treeSettings;
    [SerializeField] float treeChance;
    [SerializeField] NoiseSettings[] undergroundSettings;
    [SerializeField] int[] undergroundSettingsBlock;
    [SerializeField] int minTreeHeight;
    [SerializeField] int maxTreeHeight;

    [SerializeField] List<Chunk> loadedChunks = new List<Chunk>();
    [SerializeField] List<Vector3Int> loadedChunksPos = new List<Vector3Int>();

    Dictionary<Vector3Int, ChunkData> chunkData = new Dictionary<Vector3Int, ChunkData>();

    List<Vector3Int> chunkQueue = new List<Vector3Int>();
    bool generatingChunk = false;

    public static World Instance;

    private void Awake()
    {
        Instance = this;

        Blocks.LoadResources();
    }

    void Start()
    {
        GenerateChunkData();

        GenerateWorld();
    }

    /*void Update()
    {
        for (int y = -viewDistance; y <= viewDistance; y++)
        {
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int z = -viewDistance; z <= viewDistance; z++)
                {
                    Vector3Int playerChunkPos = new Vector3Int(Mathf.FloorToInt(player.transform.position.x / 16), Mathf.FloorToInt(player.transform.position.y / 16), Mathf.FloorToInt(player.transform.position.z / 16));
                    Vector3Int playerPos = new Vector3Int(x + playerChunkPos.x, y + playerChunkPos.y, z + playerChunkPos.z);

                    if (loadedChunksPos.Contains(playerPos))
                        continue;

                    if (!chunkData.ContainsKey(playerPos))
                        GenerateChunk(playerPos);

                    chunkQueue.Add(playerPos);

                    //CreateChunkObject(playerPos, playerPos * chunkSize);
                }
            }
        }

        for (int i = 0; i < loadedChunks.Count; i++)
        {
            Vector3Int playerChunkPos = new Vector3Int(Mathf.FloorToInt(player.transform.position.x / 16f), Mathf.FloorToInt(player.transform.position.y / 16f), Mathf.FloorToInt(player.transform.position.z / 16f));
            Vector3Int dist = new Vector3Int(Mathf.Abs(playerChunkPos.x - loadedChunksPos[i].x), Mathf.Abs(playerChunkPos.y - loadedChunksPos[i].y), Mathf.Abs(playerChunkPos.z - loadedChunksPos[i].z));

            if (dist.x > viewDistance ||
                dist.y > viewDistance ||
                dist.z > viewDistance)
            {
                Destroy(loadedChunks[i].gameObject);
                loadedChunks.RemoveAt(i);
                loadedChunksPos.RemoveAt(i);
            }
        }

        if (!generatingChunk && chunkQueue.Count > 0)
            StartCoroutine(RunChunkQueue());
    }

    IEnumerator RunChunkQueue()
    {
        generatingChunk = true;

        bool canGenerate = false;
        while (chunkQueue.Count > 0 && !canGenerate)
        {
            Vector3Int playerChunkPos = new Vector3Int(Mathf.FloorToInt(player.transform.position.x / 16f), Mathf.FloorToInt(player.transform.position.y / 16f), Mathf.FloorToInt(player.transform.position.z / 16f));
            Vector3Int dist = new Vector3Int(Mathf.Abs(playerChunkPos.x - chunkQueue[0].x), Mathf.Abs(playerChunkPos.y - chunkQueue[0].y), Mathf.Abs(playerChunkPos.z - chunkQueue[0].z));

            if (dist.x > viewDistance ||
                dist.y > viewDistance ||
                dist.z > viewDistance)
            {
                chunkQueue.RemoveAt(0);
            }
            else
                canGenerate = true;
        }
        CreateChunkObject(chunkQueue[0], chunkQueue[0] * chunkSize);
        chunkQueue.RemoveAt(0);

        yield return null;

        generatingChunk = false;
    }*/

    void GenerateChunkData()
    {
        for (int y = 0; y < worldSize.y; y++)
        {
            for (int z = 0; z < worldSize.z; z++)
            {
                for (int x = 0; x < worldSize.x; x++)
                {
                    GenerateChunk(new Vector3Int(x, y, z) + worldChunkOffset);
                }
            }
        }
    }

    ChunkData GenerateChunk(Vector3Int chunkPos)
    {
        ChunkData chunk = new ChunkData();
        chunk.chunkNum = chunkPos;

        int[,,] blocks = new int[chunkSize.x, chunkSize.y, chunkSize.z];

        for (int y = 0; y < chunkSize.y; y++)
        {
            int blockY = y + (chunkPos.y * chunkSize.y);

            for (int z = 0; z < chunkSize.z; z++)
            {
                int blockZ = z + (chunkPos.z * chunkSize.z);

                for (int x = 0; x < chunkSize.x; x++)
                {
                    int blockX = x + (chunkPos.x * chunkSize.x);
                    
                    // Get height at position
                    int height = Mathf.RoundToInt(Noise.GetHeight(seed, noiseSettings, blockX, blockZ));

                    if (blockY == height) // Equal to height (Grass Layer)
                        blocks[x, y, z] = (int)Blocks.BLOCKS_BY_NAME.GRASS_BLOCK;
                    else if (blockY < height - 4) // Less than 4 blocks below height (Stone Layer)
                        blocks[x, y, z] = (int)Blocks.BLOCKS_BY_NAME.STONE;
                    else if (blockY < height) // Less than height (Dirt Layer)
                        blocks[x, y, z] = (int)Blocks.BLOCKS_BY_NAME.DIRT_BLOCK;
                    else // Greater than height (Air)
                    {
                        float tree = Noise.GetHeight(seed, treeSettings, blockX, blockZ);
                        System.Random rand = new System.Random(seed + (int)(tree * 5) + blockX + blockZ);
                        int treeHeight = rand.Next(minTreeHeight, maxTreeHeight + 1);

                        if (tree <= treeChance && blockY - height <= treeHeight)
                            blocks[x, y, z] = (int)Blocks.BLOCKS_BY_NAME.LOG;
                        else if (tree <= treeChance && blockY - height <= treeHeight + 3)
                            blocks[x, y, z] = (int)Blocks.BLOCKS_BY_NAME.LEAVES;
                        else
                        {
                            bool leaves = false;
                            for (int dx = -2; dx <= 2; dx++)
                            {
                                for (int dz = -2; dz <= 2; dz++)
                                {
                                    int checkHeight = Mathf.RoundToInt(Noise.GetHeight(seed, noiseSettings, blockX + dx, blockZ + dz));

                                    tree = Noise.GetHeight(seed, treeSettings, blockX + dx, blockZ + dz);
                                    rand = new System.Random(seed + (int)(tree * 5) + blockX + dx + blockZ + dz);
                                    treeHeight = rand.Next(minTreeHeight, maxTreeHeight + 1);

                                    if (tree > treeChance)
                                        continue;

                                    int dist = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dz));
                                    int dist2 = Mathf.Abs(dx) + Mathf.Abs(dz);

                                    if (blockY - checkHeight > treeHeight - 2 && blockY - checkHeight <= treeHeight)
                                    {
                                        leaves = true;
                                        break;
                                    }
                                    else if (blockY - checkHeight == treeHeight + 1 && dist2 < 4)
                                    {
                                        leaves = true;
                                        break;
                                    }
                                    else if (blockY - checkHeight > treeHeight && blockY - checkHeight <= treeHeight + 2 && dist <= 1)
                                    {
                                        leaves = true;
                                        break;
                                    }
                                    else if (blockY - checkHeight == treeHeight + 3 && dist2 == 1)
                                    {
                                        leaves = true;
                                        break;
                                    }
                                }

                                if (leaves)
                                    break;
                            }

                            blocks[x, y, z] = leaves ? (int)Blocks.BLOCKS_BY_NAME.LEAVES : -1;
                        }
                    }

                    for (int i = 0; i < undergroundSettings.Length; i++)
                    {
                        if (Noise.GetNoise3D(seed, undergroundSettings[i], blockX, blockY, blockZ) <= undergroundSettings[i].chance)
                        {
                            if (blockY <= undergroundSettings[i].maxHeight)
                            {
                                blocks[x, y, z] = undergroundSettingsBlock[i];
                                break;
                            }
                        }
                    }
                }
            }
        }

        chunk.blocks = blocks;

        chunkData.Add(chunkPos, chunk);

        return chunk;
    }

    void GenerateWorld()
    {
        for (int y = 0; y < worldSize.y; y++)
        {
            for (int z = 0; z < worldSize.z; z++)
            {
                for (int x = 0; x < worldSize.x; x++)
                {
                    Vector3Int chunkPos = new Vector3Int((x + worldChunkOffset.x) * chunkSize.x, (y + worldChunkOffset.y) * chunkSize.y, (z + worldChunkOffset.z) * chunkSize.z);

                    CreateChunkObject(new Vector3Int(x, y, z) + worldChunkOffset, chunkPos);
                }
            }
        }
    }

    void CreateChunkObject(Vector3Int chunkNum, Vector3Int chunkPos)
    {
        Chunk chunk = Instantiate(chunkPrefab, chunkPos, Quaternion.identity, transform);
        chunk.name = $"Chunk ({chunkNum.x}, {chunkNum.y}, {chunkNum.z})";

        chunk.InitChunk(chunkData[chunkNum], chunkSize);

        loadedChunksPos.Add(chunkNum);
        loadedChunks.Add(chunk);
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

        return GenerateChunk(checkPos);
    }

    public void ReloadChunk(Vector3Int chunkPos)
    {
        Debug.Log($"Reloading Chunk: {chunkPos}");
        loadedChunks[loadedChunksPos.IndexOf(chunkPos)].GenerateChunk();
    }

    public void PlaceBlockInChunk(Vector3 blockPos)
    {
        blockPos = new Vector3(Mathf.Round(blockPos.x), Mathf.Round(blockPos.y), Mathf.Round(blockPos.z));

        Vector3Int chunkPos = new Vector3Int(Mathf.FloorToInt(blockPos.x / 16), Mathf.FloorToInt(blockPos.y / 16), Mathf.FloorToInt(blockPos.z / 16));
        Debug.Log($"Chunk Pos: {chunkPos}");

        int chunkIndex = loadedChunksPos.IndexOf(chunkPos);

        loadedChunks[chunkIndex].PlaceBlock(new Vector3Int(Mathf.RoundToInt(blockPos.x), Mathf.RoundToInt(blockPos.y), Mathf.RoundToInt(blockPos.z)));
    }
}

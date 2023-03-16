using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

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
    [SerializeField] int chunkSize;
    [SerializeField] Vector3Int worldChunkOffset;

    [SerializeField] int seed;
    [SerializeField, Tooltip("3x3, x = temperature, y = humidity")] NoiseSettings[] noiseSettings;
    [SerializeField] NoiseSettings temperatureNoise, humidityNoise;
    [SerializeField] NoiseSettings treeSettings;
    [SerializeField] float treeChance;
    [SerializeField] UndergroundNoiseSettings[] undergroundSettings;
    [SerializeField] int minTreeHeight;
    [SerializeField] int maxTreeHeight;

    [SerializeField] List<Chunk> loadedChunks = new List<Chunk>();
    [SerializeField] List<Vector3Int> loadedChunksPos = new List<Vector3Int>();

    Dictionary<Vector3Int, ChunkData> chunkData = new Dictionary<Vector3Int, ChunkData>();

    [SerializeField] List<Vector3Int> chunkDataQueue = new List<Vector3Int>();
    [SerializeField] List<Vector3Int> chunkDataGenerating = new List<Vector3Int>();
    [SerializeField] List<Vector3Int> chunkObjectQueue = new List<Vector3Int>();

    public static World Instance;

    bool initalGenerating = true;

    private void Awake()
    {
        Instance = this;

        Blocks.LoadResources();
    }

    void Start()
    {
        WorldGen.surfaceNoiseSettings = noiseSettings;
        WorldGen.undergroundNoiseSettings = undergroundSettings;
        WorldGen.temperatureSettings = temperatureNoise;
        WorldGen.humiditySettings = humidityNoise;

        for (int y = 0; y < worldSize.y; y++)
        {
            for (int z = 0; z < worldSize.z; z++)
            {
                for (int x = 0; x < worldSize.x; x++)
                {
                    Vector3Int chunkPos = new Vector3Int((x + worldChunkOffset.x), (y + worldChunkOffset.y), (z + worldChunkOffset.z));

                    chunkObjectQueue.Add(chunkPos);
                }
            }
        }

        //GenerateChunkData();

        //GenerateWorld();
    }

    void Update()
    {
        for (int i = 0; i < chunkDataQueue.Count; i++)
        {
            if (chunkData.ContainsKey(chunkDataQueue[i]))
            {
                Debug.LogWarning($"Trying to generate chunk data that already exists: {chunkDataQueue[i]}");
                chunkDataQueue.RemoveAt(i);
                i--;
                continue;
            }

            if (chunkDataGenerating.Contains(chunkDataQueue[i]))
                continue;

            chunkDataGenerating.Add(chunkDataQueue[i]);
            StartCoroutine(GenerateChunkFromQueue(chunkDataQueue[i]));
        }

        for (int i = 0; i < chunkObjectQueue.Count; i++)
        {
            if (chunkData.ContainsKey(chunkObjectQueue[0]))
            {
                Vector3Int chunkPos = new Vector3Int(chunkObjectQueue[0].x * chunkSize, 
                    chunkObjectQueue[0].y * chunkSize, chunkObjectQueue[0].z * chunkSize);
                CreateChunkObject(chunkObjectQueue[0], chunkPos);
                chunkObjectQueue.RemoveAt(0);
                i--;
            }
            else
            {
                if (!chunkDataQueue.Contains(chunkObjectQueue[0]))
                    chunkDataQueue.Add(chunkObjectQueue[0]);
            }
        }

        if (initalGenerating && chunkObjectQueue.Count == 0)
        {
            initalGenerating = false;
            Debug.Log("Generation Finished in " + Time.time + " seconds");

            player.enabled = true;
        }

        if (initalGenerating)
            return;

        // Render distance
        for (int y = -viewDistance; y <= viewDistance; y++)
        {
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int z = -viewDistance; z <= viewDistance; z++)
                {
                    Vector3Int playerChunkPos = new Vector3Int(Mathf.FloorToInt(player.transform.position.x / 16), Mathf.FloorToInt(player.transform.position.y / 16), Mathf.FloorToInt(player.transform.position.z / 16));
                    Vector3Int chunkRenderPos = new Vector3Int(x + playerChunkPos.x, y + playerChunkPos.y, z + playerChunkPos.z);

                    if (Vector3Int.Distance(playerChunkPos, chunkRenderPos) > viewDistance)
                        continue;

                    if (loadedChunksPos.Contains(chunkRenderPos))
                        continue;

                    if (chunkObjectQueue.Contains(chunkRenderPos))
                        continue;

                    chunkObjectQueue.Add(chunkRenderPos);
                }
            }
        }

        for (int i = 0; i < loadedChunks.Count; i++)
        {
            Vector3Int playerChunkPos = new Vector3Int(Mathf.FloorToInt(player.transform.position.x / 16f), Mathf.FloorToInt(player.transform.position.y / 16f), Mathf.FloorToInt(player.transform.position.z / 16f));
            Vector3Int dist = new Vector3Int(Mathf.Abs(playerChunkPos.x - loadedChunksPos[i].x), Mathf.Abs(playerChunkPos.y - loadedChunksPos[i].y), Mathf.Abs(playerChunkPos.z - loadedChunksPos[i].z));

            if (Vector3Int.Distance(playerChunkPos, loadedChunksPos[i]) > viewDistance)
            {
                Destroy(loadedChunks[i].gameObject);
                loadedChunks.RemoveAt(i);
                loadedChunksPos.RemoveAt(i);
                i--;
            }
        }
    }

    IEnumerator GenerateChunkFromQueue(Vector3Int chunkPos)
    {
        NativeArray<int> blocks = new NativeArray<int>(chunkSize * chunkSize * chunkSize, Allocator.TempJob);

        /*float[] undergroundChanceArray = new float[undergroundSettings.Length];
        for (int i = 0; i < undergroundSettings.Length; i++)
            undergroundChanceArray[i] = undergroundSettings[i].chance;
        int[] undergroundMaxHeightArray = new int[undergroundSettings.Length];
        for (int i = 0; i < undergroundSettings.Length; i++)
            undergroundChanceArray[i] = undergroundSettings[i].maxHeight;
        int[] undergroundSettingsBlock = new int[undergroundSettings.Length];
        for (int i = 0; i < undergroundSettings.Length; i++)
            undergroundSettingsBlock[i] = undergroundSettings[i].block;
        NativeArray<float> undergroundChance = new NativeArray<float>(undergroundChanceArray, Allocator.TempJob);
        NativeArray<int> undergroundMaxHeight = new NativeArray<int>(undergroundMaxHeightArray, Allocator.TempJob);
        NativeArray<int> undergroundBlock = new NativeArray<int>(undergroundSettingsBlock, Allocator.TempJob);*/

        GenerateChunkJob chunkJob = new GenerateChunkJob();
        chunkJob.seed = seed;
        chunkJob.chunkX = chunkPos.x;
        chunkJob.chunkY = chunkPos.y;
        chunkJob.chunkZ = chunkPos.z;
        chunkJob.chunkSize = chunkSize;

        chunkJob.blocks = blocks;

        JobHandle handle = chunkJob.Schedule();

        yield return new WaitUntil(() => handle.IsCompleted);

        handle.Complete();

        // Chunk done generating
        ChunkData chunk = new ChunkData();
        chunk.chunkNum = chunkPos;
        chunk.blocks = new int[chunkSize, chunkSize, chunkSize];

        for (int i = 0; i < blocks.Length; i++)
        {
            int blockX = i % 16;
            int blockY = i % 256 / 16;
            int blockZ = i / 256;
            chunk.blocks[blockX, blockY, blockZ] = blocks[i];
        }

        chunkData.Add(chunkPos, chunk);

        blocks.Dispose();
        //undergroundChance.Dispose();
        //undergroundMaxHeight.Dispose();
        //undergroundBlock.Dispose();
        chunkDataQueue.Remove(chunkPos);
        chunkDataGenerating.Remove(chunkPos);
    }

    public struct GenerateChunkJob : IJob
    {
        // Chunk Values
        public int seed;
        public int chunkX;
        public int chunkY;
        public int chunkZ;
        public int chunkSize;

        // Output
        public NativeArray<int> blocks;

        public void Execute()
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int blockY = y + (chunkY * chunkSize);

                for (int z = 0; z < chunkSize; z++)
                {
                    int blockZ = z + (chunkZ * chunkSize);

                    for (int x = 0; x < chunkSize; x++)
                    {
                        int blockX = x + (chunkX * chunkSize);
                        int blockIndex = z * chunkSize * chunkSize + y * chunkSize + x;

                        blocks[blockIndex] = WorldGen.GetBlockAtPos(blockX, blockY, blockZ, seed);
                    }
                }
            }
        }
    }

    /*ChunkData GenerateChunk(Vector3Int chunkPos)
    {
        ChunkData chunk = new ChunkData();
        chunk.chunkNum = chunkPos;

        int[,,] blocks = new int[chunkSize, chunkSize, chunkSize];

        for (int y = 0; y < chunkSize; y++)
        {
            int blockY = y + (chunkPos.y * chunkSize);

            for (int z = 0; z < chunkSize; z++)
            {
                int blockZ = z + (chunkPos.z * chunkSize);

                for (int x = 0; x < chunkSize; x++)
                {
                    int blockX = x + (chunkPos.x * chunkSize);
                    
                    // Get height at position
                    int height = Mathf.RoundToInt(Noise.GetHeight(seed, noiseSettings, blockX, blockZ));

                    if (blockY == height) // Equal to height (Grass Layer)
                        blocks[x, y, z] = (int)Blocks.BLOCKS_BY_NAME.GRASS_BLOCK;
                    else if (blockY < height - 4) // Less than 4 blocks below height (Stone Layer)
                        blocks[x, y, z] = (int)Blocks.BLOCKS_BY_NAME.STONE;
                    else if (blockY < height) // Less than height (Dirt Layer)
                        blocks[x, y, z] = (int)Blocks.BLOCKS_BY_NAME.DIRT_BLOCK;
                    else
                        blocks[x, y, z] = -1;
                    /*else // Greater than height (Air)
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
    }*/

    /*void GenerateWorld()
    {
        for (int y = 0; y < worldSize.y; y++)
        {
            for (int z = 0; z < worldSize.z; z++)
            {
                for (int x = 0; x < worldSize.x; x++)
                {
                    Vector3Int chunkPos = new Vector3Int((x + worldChunkOffset.x) * chunkSize, (y + worldChunkOffset.y) * chunkSize, (z + worldChunkOffset.z) * chunkSize);

                    CreateChunkObject(new Vector3Int(x, y, z) + worldChunkOffset, chunkPos);
                }
            }
        }
    }*/

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

        // Chunk doesn't exist
        if (!chunkDataQueue.Contains(checkPos))
            chunkDataQueue.Add(checkPos);
        return null;
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

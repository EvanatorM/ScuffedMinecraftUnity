using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    List<Vector3> vertices;
    List<Vector3> normals;
    List<Vector4> tangents;
    List<Vector2> uv;
    List<int> triangles;
    List<int> transparentTriangles;
    int vert = 0;

    ChunkData chunkData;
    Vector3Int chunkSize;
    MeshFilter meshFilter;

    public void InitChunk(ChunkData chunkData, Vector3Int chunkSize)
    {
        this.chunkData = chunkData;
        this.chunkSize = chunkSize;

        meshFilter = GetComponent<MeshFilter>();

        GenerateChunk();
    }

    public void GenerateChunk()
    {
        Mesh mesh = new Mesh
        {
            name = "Chunk Mesh"
        };

        vertices = new List<Vector3>();
        normals = new List<Vector3>();
        tangents = new List<Vector4>();
        uv = new List<Vector2>();
        triangles = new List<int>();
        transparentTriangles = new List<int>();
        vert = 0;

        for (int y = 0; y < chunkSize.y; y++)
        {
            for (int z = 0; z < chunkSize.z; z++)
            {
                for (int x = 0; x < chunkSize.x; x++)
                {
                    if (chunkData.blocks[x, y, z] == -1)
                        continue;

                    int blockID = chunkData.blocks[x, y, z];
                    ResourceBlock block = Blocks.BLOCKS[blockID];
                    GenerateCube(new Vector3Int(x, y, z), block);
                }
            }
        }

        mesh.subMeshCount = 2;
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.tangents = tangents.ToArray();
        mesh.uv = uv.ToArray();
        mesh.SetTriangles(triangles, 0);
        mesh.SetTriangles(transparentTriangles, 1);

        mesh.UploadMeshData(true);

        meshFilter.mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void GenerateCube(Vector3Int cubePos, ResourceBlock block)
    {
        if (cubePos.z < chunkSize.z - 1)
        {
            int checkBlock = chunkData.blocks[cubePos.x, cubePos.y, cubePos.z + 1];
            if (checkBlock == -1 || (Blocks.BLOCKS[checkBlock].transparent && !block.transparent))
                GenerateCubeFace(Direction.North, cubePos, block, block.sixSided);
        }
        else
        {
            ChunkData neighbor = World.Instance.GetNeighboringChunk(chunkData, Direction.North);
            int checkBlock = neighbor != null ? neighbor.blocks[cubePos.x, cubePos.y, 0] : -1;
            if (checkBlock == -1 || (Blocks.BLOCKS[checkBlock].transparent && !block.transparent))
                GenerateCubeFace(Direction.North, cubePos, block, block.sixSided);
        }
        if (cubePos.x < chunkSize.x - 1)
        {
            int checkBlock = chunkData.blocks[cubePos.x + 1, cubePos.y, cubePos.z];
            if (checkBlock == -1 || (Blocks.BLOCKS[checkBlock].transparent && !block.transparent))
                GenerateCubeFace(Direction.East, cubePos, block, block.sixSided);
        }
        else
        {
            ChunkData neighbor = World.Instance.GetNeighboringChunk(chunkData, Direction.East);
            int checkBlock = neighbor != null ? neighbor.blocks[0, cubePos.y, cubePos.z] : -1;
            if (checkBlock == -1 || (Blocks.BLOCKS[checkBlock].transparent && !block.transparent))
                GenerateCubeFace(Direction.East, cubePos, block, block.sixSided);
        }
        if (cubePos.z > 0)
        {
            int checkBlock = chunkData.blocks[cubePos.x, cubePos.y, cubePos.z - 1];
            if (checkBlock == -1 || (Blocks.BLOCKS[checkBlock].transparent && !block.transparent))
                GenerateCubeFace(Direction.South, cubePos, block, block.sixSided);
        }
        else
        {
            ChunkData neighbor = World.Instance.GetNeighboringChunk(chunkData, Direction.South);
            int checkBlock = neighbor != null ? neighbor.blocks[cubePos.x, cubePos.y, chunkSize.z - 1] : -1;
            if (checkBlock == -1 || (Blocks.BLOCKS[checkBlock].transparent && !block.transparent))
                GenerateCubeFace(Direction.South, cubePos, block, block.sixSided);
        }
        if (cubePos.x > 0)
        {
            int checkBlock = chunkData.blocks[cubePos.x - 1, cubePos.y, cubePos.z];
            if (checkBlock == -1 || (Blocks.BLOCKS[checkBlock].transparent && !block.transparent))
                GenerateCubeFace(Direction.West, cubePos, block, block.sixSided);
        }
        else
        {
            ChunkData neighbor = World.Instance.GetNeighboringChunk(chunkData, Direction.West);
            int checkBlock = neighbor != null ? neighbor.blocks[chunkSize.x - 1, cubePos.y, cubePos.z] : -1;
            if (checkBlock == -1 || (Blocks.BLOCKS[checkBlock].transparent && !block.transparent))
                GenerateCubeFace(Direction.West, cubePos, block, block.sixSided);
        }
        if (cubePos.y < chunkSize.y - 1)
        {
            int checkBlock = chunkData.blocks[cubePos.x, cubePos.y + 1, cubePos.z];
            if (checkBlock == -1 || (Blocks.BLOCKS[checkBlock].transparent && !block.transparent))
                GenerateCubeFace(Direction.Up, cubePos, block, block.sixSided);
        }
        else
        {
            ChunkData neighbor = World.Instance.GetNeighboringChunk(chunkData, Direction.Up);
            int checkBlock = neighbor != null ? neighbor.blocks[cubePos.x, 0, cubePos.z] : -1;
            if (checkBlock == -1 || (Blocks.BLOCKS[checkBlock].transparent && !block.transparent))
                GenerateCubeFace(Direction.Up, cubePos, block, block.sixSided);
        }
        if (cubePos.y > 0)
        {
            int checkBlock = chunkData.blocks[cubePos.x, cubePos.y - 1, cubePos.z];
            if (checkBlock == -1 || (Blocks.BLOCKS[checkBlock].transparent && !block.transparent))
                GenerateCubeFace(Direction.Down, cubePos, block, block.sixSided);
        }
        else
        {
            ChunkData neighbor = World.Instance.GetNeighboringChunk(chunkData, Direction.Down);
            int checkBlock = neighbor != null ? neighbor.blocks[cubePos.x, chunkSize.y - 1, cubePos.z] : -1;
            if (checkBlock == -1 || (Blocks.BLOCKS[checkBlock].transparent && !block.transparent))
                GenerateCubeFace(Direction.Down, cubePos, block, block.sixSided);
        }
    }

    void GenerateCubeFace(Direction dir, Vector3Int cubePos, ResourceBlock block, bool sixSided)
    {
        switch (dir)
        {
            case Direction.North:
                // Vertices
                vertices.Add(cubePos + new Vector3(.5f, -.5f, .5f));
                vertices.Add(cubePos + new Vector3(-.5f, -.5f, .5f));
                vertices.Add(cubePos + new Vector3(.5f, .5f, .5f));
                vertices.Add(cubePos + new Vector3(-.5f, .5f, .5f));
                // Normals
                for (int i = 0; i < 4; i++)
                    normals.Add(Vector3.forward);
                // Tangents
                for (int i = 0; i < 4; i++)
                    tangents.Add(new Vector4(-1f, 0f, 0f, -1f));
                break;
            case Direction.East:
                // Vertices
                vertices.Add(cubePos + new Vector3(.5f, -.5f, -.5f));
                vertices.Add(cubePos + new Vector3(.5f, -.5f, .5f));
                vertices.Add(cubePos + new Vector3(.5f, .5f, -.5f));
                vertices.Add(cubePos + new Vector3(.5f, .5f, .5f));
                // Normals
                for (int i = 0; i < 4; i++)
                    normals.Add(Vector3.right);
                // Tangents
                for (int i = 0; i < 4; i++)
                    tangents.Add(new Vector4(0f, 0f, 1f, -1f));
                break;
            case Direction.South:
                // Vertices
                vertices.Add(cubePos + new Vector3(-.5f, -.5f, -.5f));
                vertices.Add(cubePos + new Vector3(.5f, -.5f, -.5f));
                vertices.Add(cubePos + new Vector3(-.5f, .5f, -.5f));
                vertices.Add(cubePos + new Vector3(.5f, .5f, -.5f));
                // Normals
                for (int i = 0; i < 4; i++)
                    normals.Add(Vector3.back);
                // Tangents
                for (int i = 0; i < 4; i++)
                    tangents.Add(new Vector4(1f, 0f, 0f, -1f));
                break;
            case Direction.West:
                // Vertices
                vertices.Add(cubePos + new Vector3(-.5f, -.5f, .5f));
                vertices.Add(cubePos + new Vector3(-.5f, -.5f, -.5f));
                vertices.Add(cubePos + new Vector3(-.5f, .5f, .5f));
                vertices.Add(cubePos + new Vector3(-.5f, .5f, -.5f));
                // Normals
                for (int i = 0; i < 4; i++)
                    normals.Add(Vector3.left);
                // Tangents
                for (int i = 0; i < 4; i++)
                    tangents.Add(new Vector4(0f, 0f, -1f, -1f));
                break;
            case Direction.Up:
                // Vertices
                vertices.Add(cubePos + new Vector3(-.5f, .5f, -.5f));
                vertices.Add(cubePos + new Vector3(.5f, .5f, -.5f));
                vertices.Add(cubePos + new Vector3(-.5f, .5f, .5f));
                vertices.Add(cubePos + new Vector3(.5f, .5f, .5f));
                // Normals
                for (int i = 0; i < 4; i++)
                    normals.Add(Vector3.up);
                // Tangents
                for (int i = 0; i < 4; i++)
                    tangents.Add(new Vector4(1f, 0f, 0f, -1f));
                break;
            case Direction.Down:
                // Vertices
                vertices.Add(cubePos + new Vector3(-.5f, -.5f, .5f));
                vertices.Add(cubePos + new Vector3(.5f, -.5f, .5f));
                vertices.Add(cubePos + new Vector3(-.5f, -.5f, -.5f));
                vertices.Add(cubePos + new Vector3(.5f, -.5f, -.5f));
                // Normals
                for (int i = 0; i < 4; i++)
                    normals.Add(Vector3.down);
                // Tangents
                for (int i = 0; i < 4; i++)
                    tangents.Add(new Vector4(-1f, 0f, 0f, -1f));
                break;
        }

        // UV
        Vector2 sideTexPosMin = block.texPosMin[sixSided ? (int)dir : 0];
        Vector2 sideTexPosMax = block.texPosMax[sixSided ? (int)dir : 0];
        uv.Add(sideTexPosMin);
        uv.Add(new Vector2(sideTexPosMax.x, sideTexPosMin.y));
        uv.Add(new Vector2(sideTexPosMin.x, sideTexPosMax.y));
        uv.Add(sideTexPosMax);

        // Triangles
        if (block.transparent)
        {
            transparentTriangles.Add(vert + 0);
            transparentTriangles.Add(vert + 2);
            transparentTriangles.Add(vert + 1);
            transparentTriangles.Add(vert + 1);
            transparentTriangles.Add(vert + 2);
            transparentTriangles.Add(vert + 3);
        }
        else
        {
            triangles.Add(vert + 0);
            triangles.Add(vert + 2);
            triangles.Add(vert + 1);
            triangles.Add(vert + 1);
            triangles.Add(vert + 2);
            triangles.Add(vert + 3);
        }
        vert += 4;
    }

    public void DestroyBlockAtPos(Vector3 hitPos)
    {
        Vector3Int blockPos = new Vector3Int(Mathf.RoundToInt(hitPos.x), Mathf.RoundToInt(hitPos.y), Mathf.RoundToInt(hitPos.z));

        Vector3Int localBlockPos = blockPos - new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);

        Debug.Log($"BlockPos: {blockPos}, LocalBlockPos: {localBlockPos}");


        chunkData.blocks[localBlockPos.x, localBlockPos.y, localBlockPos.z] = -1;

        GenerateChunk();

        if (IsOnEdge(localBlockPos, Direction.North))
            World.Instance.ReloadChunk(chunkData.chunkNum + new Vector3Int(0, 0, 1));
        if (IsOnEdge(localBlockPos, Direction.South))
            World.Instance.ReloadChunk(chunkData.chunkNum + new Vector3Int(0, 0, -1));
        if (IsOnEdge(localBlockPos, Direction.East))
            World.Instance.ReloadChunk(chunkData.chunkNum + new Vector3Int(1, 0, 0));
        if (IsOnEdge(localBlockPos, Direction.West))
            World.Instance.ReloadChunk(chunkData.chunkNum + new Vector3Int(-1, 0, 0));
        if (IsOnEdge(localBlockPos, Direction.Up))
            World.Instance.ReloadChunk(chunkData.chunkNum + new Vector3Int(0, 1, 0));
        if (IsOnEdge(localBlockPos, Direction.Down))
            World.Instance.ReloadChunk(chunkData.chunkNum + new Vector3Int(0, -1, 0));
    }

    bool IsOnEdge(Vector3Int blockPos, Direction dir)
    {
        switch (dir)
        {
            case Direction.North:
                return blockPos.z == chunkSize.z - 1;
            case Direction.South:
                return blockPos.z == 0;
            case Direction.East:
                return blockPos.x == chunkSize.x - 1;
            case Direction.West:
                return blockPos.x == 0;
            case Direction.Up:
                return blockPos.y == chunkSize.y - 1;
            case Direction.Down:
                return blockPos.y == 0;
        }

        return false;
    }

    public void PlaceBlock(Vector3Int blockPos)
    {
        blockPos -= chunkData.chunkNum * 16;
        Debug.Log($"Chunk's Block Pos: {blockPos}");

        chunkData.blocks[blockPos.x, blockPos.y, blockPos.z] = 0;

        GenerateChunk();

        if (IsOnEdge(blockPos, Direction.North))
            World.Instance.ReloadChunk(chunkData.chunkNum + new Vector3Int(0, 0, 1));
        if (IsOnEdge(blockPos, Direction.South))
            World.Instance.ReloadChunk(chunkData.chunkNum + new Vector3Int(0, 0, -1));
        if (IsOnEdge(blockPos, Direction.East))
            World.Instance.ReloadChunk(chunkData.chunkNum + new Vector3Int(1, 0, 0));
        if (IsOnEdge(blockPos, Direction.West))
            World.Instance.ReloadChunk(chunkData.chunkNum + new Vector3Int(-1, 0, 0));
        if (IsOnEdge(blockPos, Direction.Up))
            World.Instance.ReloadChunk(chunkData.chunkNum + new Vector3Int(0, 1, 0));
        if (IsOnEdge(blockPos, Direction.Down))
            World.Instance.ReloadChunk(chunkData.chunkNum + new Vector3Int(0, -1, 0));
    }
}

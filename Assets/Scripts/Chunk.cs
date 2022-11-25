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
    int vert = 0;

    ChunkData chunkData;
    Vector3Int chunkSize;

    public void InitChunk(ChunkData chunkData, Vector3Int chunkSize)
    {
        this.chunkData = chunkData;
        this.chunkSize = chunkSize;

        GenerateChunk();
    }

    void GenerateChunk()
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

        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.tangents = tangents.ToArray();
        mesh.uv = uv.ToArray();
        mesh.triangles = triangles.ToArray();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    void GenerateCube(Vector3Int cubePos, ResourceBlock block)
    {
        if (cubePos.z < chunkSize.z - 1)
        {
            if (chunkData.blocks[cubePos.x, cubePos.y, cubePos.z + 1] == -1)
                GenerateCubeFace(Direction.North, cubePos, block, block.sixSided);
        }
        else
        {
            ChunkData neighbor = World.Instance.GetNeighboringChunk(chunkData, Direction.North);
            if (neighbor == null || neighbor.blocks[cubePos.x, cubePos.y, 0] == -1)
                GenerateCubeFace(Direction.North, cubePos, block, block.sixSided);
        }
        if (cubePos.x < chunkSize.x - 1)
        {
            if (chunkData.blocks[cubePos.x + 1, cubePos.y, cubePos.z] == -1)
                GenerateCubeFace(Direction.East, cubePos, block, block.sixSided);
        }
        else
        {
            ChunkData neighbor = World.Instance.GetNeighboringChunk(chunkData, Direction.East);
            if (neighbor == null || neighbor.blocks[0, cubePos.y, cubePos.z] == -1)
                GenerateCubeFace(Direction.East, cubePos, block, block.sixSided);
        }
        if (cubePos.z > 0)
        {
            if (chunkData.blocks[cubePos.x, cubePos.y, cubePos.z - 1] == -1)
                GenerateCubeFace(Direction.South, cubePos, block, block.sixSided);
        }
        else
        {
            ChunkData neighbor = World.Instance.GetNeighboringChunk(chunkData, Direction.South);
            if (neighbor == null || neighbor.blocks[cubePos.x, cubePos.y, chunkSize.z - 1] == -1)
                GenerateCubeFace(Direction.South, cubePos, block, block.sixSided);
        }
        if (cubePos.x > 0)
        {
            if (chunkData.blocks[cubePos.x - 1, cubePos.y, cubePos.z] == -1)
                GenerateCubeFace(Direction.West, cubePos, block, block.sixSided);
        }
        else
        {
            ChunkData neighbor = World.Instance.GetNeighboringChunk(chunkData, Direction.West);
            if (neighbor == null || neighbor.blocks[chunkSize.x - 1, cubePos.y, cubePos.z] == -1)
                GenerateCubeFace(Direction.West, cubePos, block, block.sixSided);
        }
        if (cubePos.y < chunkSize.y - 1)
        {
            if (chunkData.blocks[cubePos.x, cubePos.y + 1, cubePos.z] == -1)
                GenerateCubeFace(Direction.Up, cubePos, block, block.sixSided);
        }
        else
        {
            ChunkData neighbor = World.Instance.GetNeighboringChunk(chunkData, Direction.Up);
            if (neighbor == null || neighbor.blocks[cubePos.x, 0, cubePos.z] == -1)
                GenerateCubeFace(Direction.Up, cubePos, block, block.sixSided);
        }
        if (cubePos.y > 0)
        {
            if (chunkData.blocks[cubePos.x, cubePos.y - 1, cubePos.z] == -1)
                GenerateCubeFace(Direction.Down, cubePos, block, block.sixSided);
        }
        else
        {
            ChunkData neighbor = World.Instance.GetNeighboringChunk(chunkData, Direction.Down);
            if (neighbor == null || neighbor.blocks[cubePos.x, chunkSize.y - 1, cubePos.z] == -1)
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
        triangles.Add(vert + 0);
        triangles.Add(vert + 2);
        triangles.Add(vert + 1);
        triangles.Add(vert + 1);
        triangles.Add(vert + 2);
        triangles.Add(vert + 3);
        vert += 4;
    }
}

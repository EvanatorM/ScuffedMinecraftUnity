using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SimpleProceduralMesh : MonoBehaviour
{
    void OnEnable()
    {
        // Create a new mesh
        var mesh = new Mesh
        {
            name = "Procedural Mesh"
        };

        // Vertices
        // The positions of each vertex of the mesh
        mesh.vertices = new Vector3[]
        {
            Vector3.zero, Vector3.right, Vector3.up, // First Triangle: Zero, Right, Up
            new Vector3(1f, 1f) // Second Triangle: We can reuse 2 of the vertices from the first triangle
        };

        // Lighting
        // For flat lighting, this points in the down direction of the face for each vertex
        mesh.normals = new Vector3[]
        {
            Vector3.back, Vector3.back, Vector3.back, // First Triangle
            Vector3.back // Second Triangle
        };

        // Normal maps per vertex
        // First 3 numbers is the right direction of the face. 
        // Last is -1 for reasons I don't understand
        mesh.tangents = new Vector4[]
        {
            new Vector4(1f, 0f, 0f, -1f), // First Triangle
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f) // Second Triangle
        };

        // Texture positions
        // Position of the texture from 0-1 per vertex index
        mesh.uv = new Vector2[]
        {
            Vector2.zero, Vector2.right, Vector2.up, // First Triangle
            Vector2.one // Second Triangle
        };

        // Triangles
        // Create triangles using the vertices indices
        // Clockwise is the visible face
        mesh.triangles = new int[]
        {
            0, 2, 1, // First Triangle
            1, 2, 3 // Second Triangle
        };

        GetComponent<MeshFilter>().mesh = mesh;
    }
}

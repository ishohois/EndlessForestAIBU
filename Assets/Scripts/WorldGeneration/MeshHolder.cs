using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshHolder
{
    public Vector3[] Vertices { get; set; }
    public Vector2[] UVs { get; set; }
    public int[] Triangles { get; set; }

    private int triangleIndex;

    public MeshHolder(int width, int height)
    {
        Vertices = new Vector3[width * height];
        UVs = new Vector2[width * height];
        Triangles = new int[(width - 1) * (height) * 6];
    }

    public void AddTriangles(int a, int b, int c)
    {
        Triangles[triangleIndex] = a;
        Triangles[triangleIndex + 1] = b;
        Triangles[triangleIndex + 2] = c;

        triangleIndex += 3;
    }


    public Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Mesh";
        mesh.vertices = Vertices;
        mesh.uv = UVs;
        mesh.triangles = Triangles;

        mesh.RecalculateNormals();
        //mesh.RecalculateBounds();
        //mesh.RecalculateTangents();

        return mesh;
    }
}

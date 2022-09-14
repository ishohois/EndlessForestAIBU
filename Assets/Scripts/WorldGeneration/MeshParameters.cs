using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshParameters
{
    public Vector3[] Vertices { get; set; }
    public Vector2[] UVs { get; set; }
    public int[] Triangles { get; set; }

    public void InitializeMeshParameters(TerrainParameters terrainParameters)
    {
        int terrainSize = terrainParameters.Size;
        Vertices = new Vector3[terrainSize * terrainSize];
        UVs = new Vector2[terrainSize * terrainSize];
        //Triangles = new int[(terrainSize - 1) * (terrainSize - 1) * 6];


        Debug.Log("Vertice count " + Vertices.Length);
        Debug.Log("UVs count " + UVs.Length);
        //Debug.Log("Triangles count " + Triangles.Length);
    }
}

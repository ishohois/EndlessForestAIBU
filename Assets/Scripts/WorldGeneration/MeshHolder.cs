using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshHolder
{
    Vector3[] vertices { get; set; }
    Vector2[] uvs { get; set; }
    int[] triangles { get; set; }
    Vector3[] bakedNormals;

    Vector3[] borderVertices;
    int[] borderTriangles;

    int triangleIndex;
    int borderTriangleIndex;
    bool useFlatShading;

    public MeshHolder(int vertexCountPerLine, bool useFlatShading)
    {
        this.useFlatShading = useFlatShading;

        vertices = new Vector3[vertexCountPerLine * vertexCountPerLine];
        uvs = new Vector2[vertexCountPerLine * vertexCountPerLine];
        triangles = new int[(vertexCountPerLine - 1) * (vertexCountPerLine) * 6];

        borderVertices = new Vector3[vertexCountPerLine * 4 + 4];
        borderTriangles = new int[24 * vertexCountPerLine];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            borderVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {

        if (a < 0 || b < 0 || c < 0)
        {
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;
            borderTriangleIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;

            triangleIndex += 3;
        }

    }

    public void ProcessMesh()
    {
        if (useFlatShading == true)
        {
            FlatShading();
        }
        else
        {
            BakeNormals();
        }
    }



    private Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }


        return vertexNormals;
    }

    private Vector3 SurfaceNormalFromIndices(int IndexA, int IndexB, int IndexC)
    {
        Vector3 pointA = (IndexA < 0) ? borderVertices[-IndexA - 1] : vertices[IndexA];
        Vector3 pointB = (IndexB < 0) ? borderVertices[-IndexB - 1] : vertices[IndexB];
        Vector3 pointC = (IndexC < 0) ? borderVertices[-IndexC - 1] : vertices[IndexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    private void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }

    private void FlatShading()
    {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }


    public Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Mesh";
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        if (useFlatShading == true)
        {
            mesh.RecalculateNormals();
        }
        else
        {
            mesh.normals = bakedNormals;
        }
        return mesh;
    }
}

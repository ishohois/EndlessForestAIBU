using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMapVisualize : MonoBehaviour
{
    [SerializeField] private Renderer textureRenderer;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;

    public void RenderTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1f, texture.height);
    }

    public void RenderMesh(MeshHolder mesh, Texture2D texture)
    {
        meshFilter.sharedMesh = mesh.GenerateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

}

public enum RenderMode
{
    RenderNoiseMap,
    RenderColorMap,
    RenderMesh,
}

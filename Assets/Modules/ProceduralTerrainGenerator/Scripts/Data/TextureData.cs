using UnityEngine;

[CreateAssetMenu()]
public class TextureData : UpdatableData
{
    public Gradient colorMap;

    public void ApplyToMaterial(Material material)
    {
        //
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight) {
        material.SetVector("_elevationMinMax", new Vector2(minHeight, maxHeight));
    }
}

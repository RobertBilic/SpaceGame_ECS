using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

class BackgroundDustLayer : MonoBehaviour
{
    [SerializeField]
    private Renderer rendererToApply;

    [Range(0.0f, 1.0f)]
    public float ParallaxEffect;
    public float UVWrappingEffect;

    [Header("Material Property Overrides")]
    public float DustDensity = 0.3f;
    public float DustTileDensity = 50;
    public float DustSizeMin = 0.02f;
    public float DustSizeMax = 0.08f;
    public Vector4 ScrollSpeed = new Vector4(0.01f, 0.002f, 0, 0);
    public Vector4 UVScale = new Vector4(20, 20, 0, 0);
    public Color DustColorMin = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color DustColorMax = new Color(1f, 1f, 1f, 1f);
    public Color BackgroundColor = new Color(0, 0, 0, 1);

    MaterialPropertyBlock propertyBlock;

    public void ApplyChanges(Vector2 speed)
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        propertyBlock.SetFloat("_DustDensity", DustDensity);
        propertyBlock.SetFloat("_DustTileDensity", DustTileDensity);
        propertyBlock.SetFloat("_DustSizeMin", DustSizeMin);
        propertyBlock.SetFloat("_DustSizeMax", DustSizeMax);

        propertyBlock.SetColor("_DustColorMin", DustColorMin);
        propertyBlock.SetColor("_DustColorMax", DustColorMax);
        propertyBlock.SetColor("_BackgroundColor", BackgroundColor);

        var adjustedSpeed = speed * (1.0f - ParallaxEffect);

        propertyBlock.SetColor("_ScrollSpeed", new Color(adjustedSpeed.x, adjustedSpeed.y, 0.0f,0.0f));
        propertyBlock.SetColor("_UVScale", UVScale);

        rendererToApply.SetPropertyBlock(propertyBlock);
    }
}

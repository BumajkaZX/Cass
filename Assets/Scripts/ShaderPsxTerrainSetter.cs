using UnityEngine;

/// <summary>
/// Set resolution in shader
/// </summary>
public class ShaderPsxTerrainSetter : MonoBehaviour
{
    [SerializeField]
    private Material _psxMaterial = default;

    [SerializeField]
    private Vector2 _resolution = new Vector2(426, 240);

    [SerializeField]
    private float _ditherStrenght = default;

    [SerializeField]
    private float _ditherColor = default;

    private void OnValidate()
    {
        Vector4 resolution = new Vector4(_resolution.x, _resolution.y, 0, 0);
        _psxMaterial.SetVector("_resolution", resolution);
        _psxMaterial.SetFloat("_DitherStrenght", _ditherStrenght);
        _psxMaterial.SetFloat("_DitherStrenghtColor", _ditherColor);
    }
}

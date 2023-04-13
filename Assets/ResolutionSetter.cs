using UnityEngine;

/// <summary>
/// Set resolution on awake
/// </summary>
public class ResolutionSetter : MonoBehaviour
{
    [SerializeField]
    private int _height = 240;

    private void Awake()
    {
        float divider = Screen.currentResolution.height / _height;

        int width = (int)(Screen.currentResolution.width / divider);

        Screen.SetResolution(width, _height, Screen.fullScreenMode);
    }
}

using UnityEngine;

/// <summary>
/// Set resolution on awake
/// </summary>
public class ResolutionSetter
{
    private const int HEIGHT = 240;

    [RuntimeInitializeOnLoadMethod]
    private static void SetResolution()
    {
        float divider = Screen.currentResolution.height / HEIGHT;

        int width = (int)(Screen.currentResolution.width / divider);

        Screen.SetResolution(width, HEIGHT, Screen.fullScreenMode);

#if UNITY_EDITOR

        Debug.Log($"Resolution set  Width - {width}, Height - {HEIGHT}");

#endif

    }
}

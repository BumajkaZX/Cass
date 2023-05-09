using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

public class LoadManager : MonoBehaviour
{
    [Scene]
    public string LoadScene = default;

    private void Awake()
    {
        Scene loadScene = SceneManager.GetSceneByName(LoadScene);
    }

}

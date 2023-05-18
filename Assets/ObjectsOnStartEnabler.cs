using System.Collections.Generic;
using UnityEngine;
using Cass.LoadManager;
using System.Threading.Tasks;
using System;
using System.Threading;

public class ObjectsOnStartEnabler : MonoBehaviour, ILoadingCondition
{
    public int Order => 5;

    public string Name => typeof(ObjectsOnStartEnabler).Name;

    public bool IsInited => _isInit;

    [SerializeField]
    private List<Transform> _objectsToEnable = new List<Transform>();

    private bool _isInit = false;

    public Task<Action> Initialization(CancellationToken token)
    {
        _isInit = true;

        Action act = OnSceneStart;

        return Task.FromResult(act);
    }

    private void OnSceneStart()
    {
        foreach(Transform trans in _objectsToEnable)
        {
            trans.gameObject.SetActive(true);
        }
    }
}

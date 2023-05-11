using Cass.LoadManager;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Logger : MonoBehaviour, ILoadingCondition
{
    public static Logger Instance = default;
    public int Order => _order; 
    public string Name => typeof(Logger).Name;

    public bool IsInited => _isInited;

    public event Action<string> onMessageSended = delegate { };

    [SerializeField, Min(0)]
    private int _order = 0;

    private bool _isInited = false;

    public async Task<Action> Initialization(CancellationToken token)
    {
        Debug.LogError("LOGGGGGGGG");
        if(Instance != null)
        {
            Destroy(this);
            return null;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        _isInited = true;

        await Task.CompletedTask;

        return null;
    }


    public void Log(string message)
    {
        //localization needed
        Debug.Log(message);
        onMessageSended(message);
    }

}

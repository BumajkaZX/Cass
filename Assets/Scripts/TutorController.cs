using Cass.LoadManager;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Cass.FirstEntry;

public class TutorController : MonoBehaviour, ILoadingCondition
{
    public int Order => 5;

    public string Name => typeof(TutorController).Name;

    public bool IsInited => _isInited;

    private bool _isInited = false;

    public  Task<Action> Initialization(CancellationToken token)
    {
        _isInited = true;

        Action act = OnSceneStart;

        return Task.FromResult(act) ;
    }

    private void OnSceneStart()
    {
        Debug.LogWarning("Entry start");
        FirstEntry firstEntry = new FirstEntry();

        if (firstEntry.IsFirstEntry())
        {
            FirstEntry();
        }
        else
        {
            TutorialStart();
        }
    }

    private void FirstEntry()
    {

    }

    private void TutorialStart()
    {

    }
}

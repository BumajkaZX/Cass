namespace Cass.LoadManager
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using NaughtyAttributes;
    using UniRx;
    using System.Linq;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Load manager with conditions
    /// </summary>
    public class LoadManager : MonoBehaviour
    {
        /// <summary>
        /// Instance
        /// </summary>
        public static LoadManager Instance = default;

        [Scene]
        public string LoadScene = default;

        [Scene]
        public string MenuScene = default;

        /// <summary>
        /// On load complete
        /// </summary>
        [HideInInspector]
        public ReactiveProperty<bool> LoadComplete = new ReactiveProperty<bool>();

        [HideInInspector]
        public ReactiveProperty<bool> IsAvailableLoad = new ReactiveProperty<bool>(true);

        [SerializeField, Min(1)]
        private float _serviceTimeout = 5;

        [SerializeField]
        private bool _needWaitAction = false;

        private event Action _onSceneStart = delegate { };

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private async void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }

            await InitScripts();
        }

        private async void Start() => await LoadLevel(MenuScene, _needWaitAction);

        /// <summary>
        /// Set IsLoadAvailable true, if wait for action - true
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="waitForTouch"></param>
        /// <returns></returns>
        public async Task LoadLevel(string sceneName, bool waitForTouch)
        {
            _onSceneStart = delegate { };

            if (waitForTouch)
            {
                SceneManager.LoadSceneAsync(LoadScene, LoadSceneMode.Single);
            }

            AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(sceneName, waitForTouch ? LoadSceneMode.Additive : LoadSceneMode.Single);

            await WaitLoad(sceneLoad);

            Debug.Log("INITTTTT");

            if (!waitForTouch)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

                await InitScripts();

                LoadComplete.SetValueAndForceNotify(true);

                _onSceneStart();
            }

            if (waitForTouch)
            {
                IsAvailableLoad.Value = false;

                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

                await InitScripts();

                LoadComplete.SetValueAndForceNotify(true);

                await WaitAction();

                AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(LoadScene);

                await WaitLoad(sceneUnload);

                _onSceneStart();

            }

            Debug.Log("Scene Active");
        }

        private async Task WaitLoad(AsyncOperation sceneLoad)
        {
            while (!sceneLoad.isDone) 
            {
                if (_tokenSource.Token.IsCancellationRequested)
                {
                    return;
                }
                Debug.LogWarning(sceneLoad.progress);
                await Task.Yield();
            }
        }

        private async Task WaitAction()
        {
            while (!IsAvailableLoad.Value)
            {
                if (_tokenSource.Token.IsCancellationRequested)
                {
                    return;
                }
                await Task.Yield();
            }
        }

        /// <summary>
        /// Doesn't reinit blocked conditions 
        /// </summary>
        /// <returns></returns>
        private async Task InitScripts()
        {
            var conditions = FindObjectsOfType<MonoBehaviour>(true).OfType<ILoadingCondition>();

            var listconditions = conditions.OrderBy(condition => condition.Order).ToList();

            foreach (ILoadingCondition condition in listconditions)
            {
                if (_tokenSource.Token.IsCancellationRequested)
                {
                    return;
                }
                if (condition.IsInited)
                {
                    continue;
                }

                Debug.Log("Init load " + condition.Name);
                Task<Action> task = condition.Initialization(_tokenSource.Token);
                var completedTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(_serviceTimeout), _tokenSource.Token));
                if (completedTask != task)
                {
                    Debug.LogWarning("block " + condition.Name);
                }
                else
                {
                    Debug.Log($"Complete {condition.Name}");
                    if (task.Result != null)
                    {
                        _onSceneStart += task.Result;
                    }
                }
                
            }
        }

        private void OnDestroy()
        {
            _tokenSource.Cancel();
        }

    }
}

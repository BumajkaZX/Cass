namespace Cass.Services
{
    using UnityEngine;
    using Cass.LoadManager;
    using System.Threading.Tasks;
    using System;
    using System.Threading;
    using GooglePlayGames.BasicApi.SavedGame;
    using GooglePlayGames;
    using Cass.Character;
    using UniRx;
    using System.Text;

    /// <summary>
    /// Save/load playerinfo
    /// </summary>
    public class SaveManager : MonoBehaviour, ILoadingCondition
    {
        private const string SAVE_NAME = "PlayerInfoSaveName";

        public static SaveManager Instance = default;

        /// <summary>
        /// Use load to update info
        /// </summary>
        [HideInInspector]
        public ReactiveProperty<PlayerInfo> CloudPlayerInfo = new ReactiveProperty<PlayerInfo>();

        /// <summary>
        /// Use load to update info
        /// </summary>
        [HideInInspector]
        public ReactiveProperty<PlayerInfo> OfflinePlayerInfo = new ReactiveProperty<PlayerInfo>();

        public int Order => 3;

        public string Name => typeof(SaveManager).Name;

        public bool IsInited => _isInited;

        private ISavedGameClient _savedGameClient = default;

        private bool _isInited = false;

        private bool _isSaving = false;

        public async Task<Action> Initialization(CancellationToken token)
        { 
            Instance = this;
            DontDestroyOnLoad(gameObject);

#if !DEVELOPMENT_BUILD

            _savedGameClient = PlayGamesPlatform.Instance.SavedGame;

#endif

            await SaveLoad(false);

            _isInited = true;

            Action act = null;

            return act;
        }

        /// <summary>
        /// true - save, false - load
        /// </summary>
        /// <param name="isSave"></param>
        public async Task SaveLoad(bool isSave)
        {
            Debug.LogError(isSave + " save");

#if DEVELOPMENT_BUILD

                 _isSaving = isSave;
                await OfflineSaveLoad();
                return;

#endif
            if (!PlayGamesPlatform.Instance.IsAuthenticated())
            {
                _isSaving = isSave;
                await OfflineSaveLoad();
                return;
            }

            _isSaving = isSave;

            _savedGameClient.OpenWithAutomaticConflictResolution(typeof(PlayerInfo).Name, GooglePlayGames.BasicApi.DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, OpenSaveCallback);
        }

        private void OpenSaveCallback(SavedGameRequestStatus status, ISavedGameMetadata data)
        {
            if (status == SavedGameRequestStatus.Success)
            {
                if (_isSaving)
                {
                    SaveGame();
                }
                else
                {
                    LoadGame(data);
                }
            }
        }

        private void LoadGame(ISavedGameMetadata data) => _savedGameClient.ReadBinaryData(data, LoadCallback);

        private void LoadCallback(SavedGameRequestStatus status, byte[] data)
        {
            Debug.LogError(status);
            if(status == SavedGameRequestStatus.Success)
            {
                CloudPlayerInfo.SetValueAndForceNotify(JsonUtility.FromJson<PlayerInfo>(data.ToString()));
            }
        }

        private bool SaveGame()
        {

            var playerInfo = PlayersPool.Players.Find(_ => _.IsOwner).PlayerInfo;

            playerInfo.LastModified = DateTime.Now;

            byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(JsonUtility.ToJson(playerInfo));

            var updateForMetadata = new SavedGameMetadataUpdate.Builder().WithUpdatedDescription($"Update at: {DateTime.Now}").Build();

            _savedGameClient.CommitUpdate(playerInfo, updateForMetadata, data, SaveCallback);

            OfflineSave();

            return true;
        }

        private async Task OfflineSaveLoad()
        {
            if (_isSaving)
            {
                OfflineSave();
            }
            else
            {
                Debug.LogError("En");
                if (!PlayerPrefs.HasKey(SAVE_NAME))
                {
                    PlayerInfo info = new PlayerInfo();
                    OfflinePlayerInfo.SetValueAndForceNotify(info);
                    return;
                }
                Debug.LogError("Norm load");
                byte[] decodedBytes = Convert.FromBase64String(PlayerPrefs.GetString(SAVE_NAME));
                string decodedText = Encoding.UTF8.GetString(decodedBytes);

                OfflinePlayerInfo.SetValueAndForceNotify(JsonUtility.FromJson<PlayerInfo>(decodedText));
            }
        }

        private void OfflineSave()
        {
            byte[] bytesToEncode = Encoding.UTF8.GetBytes(JsonUtility.ToJson(PlayersPool.Players.Find(_ => _.IsOwner).PlayerInfo));
            string encodedText = Convert.ToBase64String(bytesToEncode);
            PlayerPrefs.SetString(SAVE_NAME, encodedText);
            PlayerPrefs.Save();
        }
        private void SaveCallback(SavedGameRequestStatus status, ISavedGameMetadata data)
        {
            if(status == SavedGameRequestStatus.Success)
            {
                Cass.Logger.Logger.Instance.Log("Succsesfull save");
            }
            else
            {
                Cass.Logger.Logger.Instance.Log("Save error");
            }
        }

        private async void OnApplicationQuit() => await SaveLoad(true);
    }
}

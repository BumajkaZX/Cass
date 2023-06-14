namespace Cass.Character
{
    using GooglePlayGames.BasicApi.SavedGame;
    using System;

    [Serializable]
    public class PlayerInfo : ISavedGameMetadata
    {
        public string HatId = default;

        public string GlassesId = default;

        public string BodyId = default;

        public string LandParticlesId = default;

        public string ActiveGunId = default;

        public TimeSpan TotalTime = default;

        public DateTime LastModified = default;

        public int Progress = default;

        public bool IsOpen => true;

        public string Filename => typeof(PlayerInfo).Name;

        public string Description => null;

        public string CoverImageURL => null;

        public TimeSpan TotalTimePlayed => TotalTime;

        public DateTime LastModifiedTimestamp => LastModified;
    }
}

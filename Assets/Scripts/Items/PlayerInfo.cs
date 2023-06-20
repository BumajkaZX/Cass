namespace Cass.Character
{
    using Cass.Items;
    using GooglePlayGames.BasicApi.SavedGame;
    using System;
    using Unity.Netcode;
    using UnityEngine;

    [Serializable]
    public class PlayerInfo : INetworkSerializable, ISavedGameMetadata
    {
        public string HatId = "defaultHat";

        public string GlassesId = "defaultGlasses";

        public string BodyId = "defaultBody";

        public string TailId = "defaultTail";

        public string ActiveGunId = "defaultShotgun";

        public string LastScene = "Tutor";

        public Vector3 LastPos = default;

        public TimeSpan TotalTime = default;

        public DateTime LastModified = default;

        public int Progress = default;

        public bool IsOpen => true;

        public string Filename => typeof(PlayerInfo).Name;

        public string Description => null;

        public string CoverImageURL => null;

        public TimeSpan TotalTimePlayed => TotalTime;

        public DateTime LastModifiedTimestamp => LastModified;

        public bool IsPutOn(string itemId) => itemId == HatId || itemId == GlassesId || itemId == BodyId || itemId == TailId || itemId == ActiveGunId;
        public void SetOutfit(Outfit item)
        {
            switch (item.OutfitType)
            {
                case OutfitType.Hat:
                    HatId = item.ItemId;
                    break;
                case OutfitType.Glasses:
                    GlassesId = item.ItemId;
                    break;
                case OutfitType.Body:
                    BodyId = item.ItemId;
                    break;
                case OutfitType.Tail:
                    TailId = item.ItemId;
                    break;
            }
        }
        public void SetGun(string id) => ActiveGunId = id;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref HatId);
            serializer.SerializeValue(ref GlassesId);
            serializer.SerializeValue(ref BodyId);
            serializer.SerializeValue(ref TailId);
            serializer.SerializeValue(ref ActiveGunId);
        }
    }
}

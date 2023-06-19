namespace Cass.Items
{
    using UnityEngine;

    public abstract class PlayerItem : ScriptableObject
    {
        public string ItemId => _id;

        public bool IsFree => _isFree;

        public bool IsBought = false;

        public Transform PrefabTransform => _prefabTransform;

        [SerializeField]
        protected  string _id = default;

        [SerializeField]
        protected bool _isFree = false;

        [SerializeField]
        protected Transform _prefabTransform = default;

    }
}

namespace Cass.Items
{
    using UnityEngine;

    public abstract class PlayerItem : ScriptableObject
    {
        public string ItemId => _id;

        public bool IsFree => _isFree;

        public bool IsBought = false;

        [SerializeField]
        protected  string _id = default;

        [SerializeField]
        protected bool _isFree = false;

    }
}

namespace Cass.Items
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "Outfit", menuName = "Outfit")]
    public class Outfit : PlayerItem
    {
        public OutfitType OutfitType => _outfitType;

        [SerializeField]
        private OutfitType _outfitType = default;

    }
}

namespace Cass.CanvasController
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "Canvas identificator", menuName = "Canvas identificator")]
    public class CanvasIdentificator : ScriptableObject
    {
        public string Id = default;

        public bool IsSolo = true;
    }
}

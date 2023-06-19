namespace Cass.Interactable
{
    public interface IInteractableObject
    {
        public void OnObjectInteract();

        public void StartInteraction(bool isStart);
    }
}

using UnityEngine;

public interface IInteractable
{
    float InteractRange { get; }
    bool IsInteractable { get; }
    string InteractMessage { get; }

    void Interact(GameObject interactor);
}
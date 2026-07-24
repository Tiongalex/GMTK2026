using UnityEngine;

public class CursedObject : MonoBehaviour
{
    public bool IsActive { get; set; } = false;
    public bool IsSealed { get; set; } = false;

    public void ActivateObject()
    {
        IsActive = true;
        Debug.Log($"[Stub] {gameObject.name} CursedObject Activated!");
    }

    public void DeactivateObject()
    {
        IsActive = false;
        Debug.Log($"[Stub] {gameObject.name} CursedObject Deactivated!");
    }

    public void OnExorcismFailed()
    {
        Debug.Log($"[Stub] {gameObject.name} CursedObject Exorcism Failed!");
    }
}

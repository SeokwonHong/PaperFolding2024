using UnityEngine;

public class DeleteButton : MonoBehaviour
{
    private PrefabPlacerManager manager;

    public void Setup(PrefabPlacerManager mgr)
    {
        manager = mgr;
    }

    public void OnDeleteClick()
    {
        manager.DeletePlacedObject();
        Destroy(gameObject); // É¾³ý°´Å¥×Ô¼º
    }
}
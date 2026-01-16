using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    void OnMouseDown()
    {
        FindObjectOfType<PrefabPlacerManager>().ShowDeleteButton(transform.position);
    }
}
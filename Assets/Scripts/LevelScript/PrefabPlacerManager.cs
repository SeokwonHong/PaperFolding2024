using UnityEngine;

public class PrefabPlacerManager : MonoBehaviour
{
    public GameObject[] prefabOptions; // 4 个可放置的预制体
    public LayerMask groundLayer;
    public Canvas worldUICanvas;
    public GameObject deleteButtonPrefab;

    private GameObject currentPlacing;
    private GameObject placedObject;

    private int currentPrefabIndex = -1;

    void Update()
    {
        if (currentPlacing != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                Vector3 placePos = hit.point;
                placePos.y = 0; // 锁定 Y 坐标
                currentPlacing.transform.position = placePos;
            }

            if (Input.GetMouseButtonDown(0)) // 左键放置
            {
                placedObject = currentPlacing;
                GlobalGameManager.Instance.gameState = GlobalGameManager.GameState.normal;
                currentPlacing = null;
            }

            if (Input.GetMouseButtonDown(1)) // 右键旋转
            {
                currentPlacing.transform.Rotate(0, 90, 0);
            }
        }
    }

    public void OnUIButtonClick(int index)
    {
        if (currentPlacing == null && placedObject == null)
        {
            currentPrefabIndex = index;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                Vector3 spawnPos = hit.point;
                spawnPos.y = 0; // 固定在地面高度
                currentPlacing = Instantiate(prefabOptions[index], spawnPos, Quaternion.identity);
                GlobalGameManager.Instance.gameState = GlobalGameManager.GameState.generatingBridge;
            }
        }
    }

    public void DeletePlacedObject()
    {
        if (placedObject != null)
        {
            Destroy(placedObject);
            placedObject = null;
        }
    }

    public void ShowDeleteButton(Vector3 worldPos)
    {
        GameObject btn = Instantiate(deleteButtonPrefab, worldUICanvas.transform);
        btn.transform.position = worldPos + new Vector3(0, 2, 0);
        btn.GetComponent<DeleteButton>().Setup(this);
    }
}
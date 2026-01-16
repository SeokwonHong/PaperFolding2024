using UnityEngine;

public class ShadowGridGeneratorSimple : MonoBehaviour
{
    public Camera shadowCamera;
    public RenderTexture shadowTexture;
    public GameObject gridPrefab;
    public Vector2 areaMin = new Vector2(-10, -10);
    public Vector2 areaMax = new Vector2(10, 10);
    public float cellSize = 1f;
    public float shadowThreshold = 0.5f;

    private Texture2D tex;

    void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateShadowGrid();
        }
      
    }
    void GenerateShadowGrid()
    {
        if (shadowCamera == null || shadowTexture == null || gridPrefab == null)
        {
            Debug.LogError("参数未设置完整！");
            return;
        }

        // 激活 RenderTexture，并读成 Texture2D
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = shadowTexture;

        tex = new Texture2D(shadowTexture.width, shadowTexture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, shadowTexture.width, shadowTexture.height), 0, 0);
        tex.Apply();

        RenderTexture.active = currentRT;

        // 遍历区域，每隔 cellSize 检测一次
        for (float x = areaMin.x; x <= areaMax.x; x += cellSize)
        {
            for (float z = areaMin.y; z <= areaMax.y; z += cellSize)
            {
                Vector3 worldPos = new Vector3(x, 0, z);

                if (IsInShadow(worldPos))
                {
                    Instantiate(gridPrefab, worldPos, Quaternion.identity, this.transform);
                }
                else
                {
                    print(IsInShadow(worldPos)+"worldPos:"+worldPos);
                }
            }
        }

        Destroy(tex); // 用完销毁
    }

    bool IsInShadow(Vector3 worldPos)
    {
        Vector3 viewPos = shadowCamera.WorldToViewportPoint(worldPos);

        // 超出摄像机视野范围直接认为不在阴影
        if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1)
            return false;

        int pixelX = Mathf.FloorToInt(viewPos.x * shadowTexture.width);
        int pixelY = Mathf.FloorToInt(viewPos.y * shadowTexture.height);

        Color pixel = tex.GetPixel(pixelX, pixelY);

        return pixel.grayscale < shadowThreshold;
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShadowMoveController : MonoBehaviour 
{
    [Header("“ı”∞ºÏ≤‚…Ë÷√")]
    public Camera shadowCamera;
    public RenderTexture shadowTexture; 
    public Shader shadowShader; 

    [Header("“∆∂Ø…Ë÷√")]
    public float moveDuration = 0.5f;
    public AnimationCurve moveCurve;

    private bool isMoving = false;
    private Queue<Vector3> moveQueue = new Queue<Vector3>();
    public float speed =0.05f;

    private GameObject refToRespawnPoint;
    Vector3 originalSize = new Vector3(0.5f, 0.5f, 0.5f);


    float horizInput;
    //SceneLoader
    public SceneLoader sceneLoader;


    public GameObject fracturedBoxPrefab;
    void Start()
    {

        shadowCamera.targetTexture = shadowTexture;

        
        refToRespawnPoint = GameObject.Find("RespawnPoint");
        
        
    }

    void Update()
    {

        if (!isMoving)
        {
            Vector2 input = GetInputDirection();
            if (input != Vector2.zero)
            {
                Vector3 dir = new Vector3(input.x, 0, input.y);
                Vector3 targetPos = transform.position + dir;

                if (!IsBlocked(targetPos))
                {
                    transform.position += dir;
                    //moveQueue.Enqueue(targetPos); 
                    //if (!isMoving)
                    //    StartCoroutine(ProcessMovementQueue());
                }
                if (!IsPositionInShadow(targetPos))
                {
                    StartCoroutine(Respawn());
                }
            }
        }
    }

    IEnumerator Respawn()
    {
        Vector3 targetSize = Vector3.zero;
        float duration = 0.2f;
        float timer = 0f;
        isMoving = true;


        GetComponent<Renderer>().enabled = false;

     
        GameObject fractured = Instantiate(fracturedBoxPrefab, this.transform.position, transform.rotation);

        foreach (Rigidbody rb in fractured.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        yield return new WaitForSeconds(1.6f);

       


        // 7. ø¯∑° π⁄Ω∫ ¥ŸΩ√ ∫∏¿Ã∞‘
        GetComponent<Renderer>().enabled = true;

        // 8. ∫Œº≠¡¯ π⁄Ω∫ ¡¶∞≈
        Destroy(fractured);

        // 9. π⁄Ω∫ √ ±‚»≠
        transform.localScale = targetSize;
        transform.position = refToRespawnPoint.transform.position;

        // 10. ≈©±‚ ∫πø¯ æ÷¥œ∏ﬁ¿Ãº«
        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            transform.localScale = Vector3.Lerp(targetSize, originalSize, t);
            yield return null;
        }

        isMoving = false;
    }


    bool IsBlocked(Vector3 targetPos)
    {
        Collider[] hits = Physics.OverlapSphere(targetPos, 0.2f); 
        foreach (var hit in hits)
        {
            if (!hit.isTrigger && hit.gameObject != gameObject) 
            {
                return true;
            }
        }
        return false;
    }

    Vector2 GetInputDirection()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) dir += new Vector2(0, speed);
        if (Input.GetKey(KeyCode.S)) dir += new Vector2(0, -speed);
        if (Input.GetKey(KeyCode.A)) dir += new Vector2(-speed, 0);
        if (Input.GetKey(KeyCode.D)) dir += new Vector2(speed, 0);
        return dir;
    }

    IEnumerator SmoothMove(Vector3 from, Vector3 to)
    {
        isMoving = true;
        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = timer / moveDuration;
            float easedT = moveCurve.Evaluate(t);
            transform.position = Vector3.Lerp(from, to, easedT);
            yield return null;
        }

        transform.position = to;
        isMoving = false;
    }

    bool IsPositionInShadow(Vector3 worldPos) 
    {
       
        Vector3 viewPos = shadowCamera.WorldToViewportPoint(worldPos);
        if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1) 
            
            return false;

        RenderTexture currentRT = RenderTexture.active; 
        RenderTexture.active = shadowTexture; 

        Texture2D tex = new Texture2D(shadowTexture.width, shadowTexture.height, TextureFormat.RGB24, false); 
        tex.ReadPixels(new Rect(0, 0, shadowTexture.width, shadowTexture.height), 0, 0);
        tex.Apply(); 

        int x = Mathf.FloorToInt(viewPos.x * shadowTexture.width); 
        int y = Mathf.FloorToInt(viewPos.y * shadowTexture.height);
        Color pixel = tex.GetPixel(x, y);


        Object.Destroy(tex); 
        RenderTexture.active = currentRT; 

        return pixel.grayscale < 0.3f; 
    }


    void OnTriggerEnter(Collider BlackHole)
    {
        if (BlackHole.CompareTag("BlackHole"))
        {
            StartCoroutine(NextLevel());
        }
    }
    IEnumerator NextLevel()
    {
        Vector3 targetSize = new Vector3(0f, 0f, 0f);
        float duration = 0.5f;
        float timer = 0f;
        isMoving = true;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            transform.localScale = Vector3.Lerp(originalSize, targetSize, t);
            yield return null;
        }
        timer = 0f;
        sceneLoader.LoadNextScene();
        yield return new WaitForSeconds(1f);

        transform.localScale = originalSize;

       

        
    }
}
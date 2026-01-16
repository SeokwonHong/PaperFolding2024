using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System;

[RequireComponent(typeof(Collider))]
public class PaperMovement : MonoBehaviour
{
    public Transform hinge,centerPaper,player;               
    public Collider mainCollider, holeCollider;
    public Vector3 foldAxis = Vector3.right; // 折叠轴（世界坐标）
    public float maxAngle = 180f;
    public float rotationSensitivity = 0.2f;
    public bool allowedToFold = true;

    public Vector3 initialPos;
    public bool isDragging = false, overHalf = false;
    public float currentAngle = 0f;

    private Vector3 dragStartPos;
    private Coroutine foldCoroutine;
    private Vector2 screenDragDirection; // 屏幕空间拖拽方向（用于 Dot 比较）

    public int visualLayer = 0;                // 折叠后所处视觉层
    private static PaperMovement currentlyDraggingPaper;
    private static readonly LayerMask paperLayerMask = 1 << 7;

    void Start()
    {
        initialPos = transform.position;

        UpdateTopVisual();

        #region dragging direction
        // 推导推荐拖拽方向（屏幕空间）
        Vector3 dir = (transform.position - centerPaper.position).normalized;
        Vector3 worldDragDir;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            worldDragDir = dir.x > 0 ? Vector3.left : Vector3.right;  // 左右纸 → 从中往左右拖
        else
            worldDragDir = dir.z > 0 ? Vector3.back : Vector3.forward; // 上下纸 → 从中往上或下拖

        // 投影到屏幕空间
        Vector3 worldStart = centerPaper.position;
        Vector3 worldEnd = centerPaper.position + worldDragDir;
        screenDragDirection = (Camera.main.WorldToScreenPoint(worldEnd) - Camera.main.WorldToScreenPoint(worldStart)).normalized;
        #endregion
    }

    void Update()
    {
        if (isDragging)
        {
            float normalized = Mathf.InverseLerp(0f, -maxAngle, currentAngle);
            if(normalized > 0.2f && normalized < 0.8f)
            {
                overHalf = true;
            }

            if(overHalf)
            {
                if (normalized > 0.95f || normalized < 0.05f)
                {
                    isDragging = false;
                    float target = normalized > 0.5f ? -maxAngle : 0f;

                    // 补差或携程
                    if (Mathf.Abs(currentAngle - target) < 0.1f)
                    {
                        float correction = target - currentAngle;
                        transform.RotateAround(hinge.position, foldAxis, correction);
                        currentAngle = target;
                        ApplyFinalVisualStates(target);
                    }
                    else
                    {
                        foldCoroutine = StartCoroutine(AutoCompleteFold(target));
                    }
                }
            }
            
        }

        if (isDragging && this == currentlyDraggingPaper)
        {
            UpdateDuringDrag();
            FoldFlipRootDuringDrag();  

            Vector3 dragCurrent = Input.mousePosition;
            Vector3 dragVector = dragCurrent - dragStartPos;

            float dragAmount = Vector3.Dot(dragVector.normalized, screenDragDirection) * dragVector.magnitude;
            
            float targetAngle = Mathf.Clamp(currentAngle - dragAmount * rotationSensitivity, -maxAngle, 0f);
            float delta = targetAngle - currentAngle;
            
            transform.RotateAround(hinge.position, foldAxis, delta);
            currentAngle = targetAngle;

            dragStartPos = dragCurrent;
        }
    }

    void OnMouseDown()
    {

        if (!IsInteractable()) return;

        if (foldCoroutine != null)
            StopCoroutine(foldCoroutine);

        SetChildrenVisible(true, true);
        SetSpriteGroup(1);

        isDragging = true;
        dragStartPos = Input.mousePosition;
        currentlyDraggingPaper = this;

    }

    void OnMouseUp()
    {
        isDragging = false;

        float target = (Mathf.Abs(currentAngle) > maxAngle * 0.5f) ? -maxAngle : 0f;

        // 判断是否差距非常小，直接跳过协程
        if (Mathf.Abs(currentAngle - target) <= 0.1f)
        {
            // 补上最终旋转
            float correction = target - currentAngle;
            transform.RotateAround(hinge.position, foldAxis, correction);
            currentAngle = target;

            // 执行最终状态设置
            ApplyFinalVisualStates(target);
        }
        else
        {
            foldCoroutine = StartCoroutine(AutoCompleteFold(target));
        }

    }


    IEnumerator AutoCompleteFold(float targetAngle)
    {
        while (Mathf.Abs(currentAngle - targetAngle) > 0.1f)
        {
            float delta = (targetAngle - currentAngle) * 0.2f;
            transform.RotateAround(hinge.position, foldAxis, delta);
            currentAngle += delta;

            yield return null;
        }

        float correction = targetAngle - currentAngle;
        transform.RotateAround(hinge.position, foldAxis, correction);
        currentAngle = targetAngle;

        // 重新赋予视觉层级（只在折叠时）
        if (Mathf.Approximately(targetAngle, -maxAngle))
        {

            AssignVisualLayer();
            transform.position = Vector3.zero;
        }
        else if (Mathf.Approximately(targetAngle, 0f))
        {
            visualLayer = 0; // 回到初始状态
            transform.position = initialPos;
        }

        PaperManager.Instance.RefreshAllTopStates(); // 所有纸判断自己是否在最上层
        SetSpriteGroup(0);
        currentlyDraggingPaper = null;
        overHalf = false;
        isDragging = false;

    }
    private void ApplyFinalVisualStates(float targetAngle)
    {
        if (Mathf.Approximately(targetAngle, -maxAngle))
        {
            AssignVisualLayer();
            transform.position = Vector3.zero; ;
        }
        else if (Mathf.Approximately(targetAngle, 0f))
        {
            visualLayer = 0;
            transform.position = initialPos;
        }

        PaperManager.Instance.RefreshAllTopStates();
        SetSpriteGroup(0);
        currentlyDraggingPaper = null;
        overHalf = false;
        isDragging = false;
    }
    
    void AssignVisualLayer()
    {
        // 查找当前方向已有的最大 visualLayer
        int max = 0;
        foreach (var paper in PaperManager.Instance.AllPapers)
        {
            if (paper != this && paper.visualLayer > 0)
            {
                if (paper.visualLayer > max) max = paper.visualLayer;
            }
        }

        //叠在下一个位置
        visualLayer = max + 1;
    }
    public bool IsInteractable()
    {
        if (!allowedToFold) return false;

        if (IsPlayerOnSolidPart()) return false;

        Bounds bounds = GetComponent<Collider>().bounds;
        Vector3 halfExtents = new Vector3(bounds.extents.x, 0.01f, bounds.extents.z);
        Collider[] hits = Physics.OverlapBox(bounds.center, halfExtents, Quaternion.identity, paperLayerMask);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            PaperMovement other = hit.GetComponent<PaperMovement>();
            if (other != null && other.visualLayer > this.visualLayer)
                return false;
        }

        
        return true;
    }

    public void UpdateTopVisual()
    {
        bool covered = IsCoveredByOtherPaper();

        if (!covered)
        {
            // 控制可见性
            if (currentAngle > -90f)
            {
                SetChildrenVisible(front: true, back: false);
                FlipRootFlip(true,true);
                FlipRootFlip(false, false);
            }
            else
            {
                SetChildrenVisible(front: false, back: true);
                FlipRootFlip(true, false);
                FlipRootFlip(false, true);
            }

            SetCollider(true);
        }
        else
        {
            SetChildrenVisible(false, false);
            FlipRootFlip(true, false);
            FlipRootFlip(false, false);
            SetCollider(false);
        }
       
    }

    bool IsCoveredByOtherPaper()
    {
        Bounds myBounds = GetComponentInChildren<Renderer>().bounds;

        foreach (var other in PaperManager.Instance.AllPapers)
        {
            if (other == this) continue;

            Renderer otherRenderer = other.GetComponentInChildren<Renderer>();
            if (otherRenderer == null) continue;

            Bounds otherBounds = otherRenderer.bounds;
            float shrinkMargin = 0.05f;

            Vector3 myMin = myBounds.min + new Vector3(shrinkMargin, 0, shrinkMargin);
            Vector3 myMax = myBounds.max - new Vector3(shrinkMargin, 0, shrinkMargin);

            bool overlapXZ =
                myMin.x < otherBounds.max.x && myMax.x > otherBounds.min.x &&
                myMin.z < otherBounds.max.z && myMax.z > otherBounds.min.z;

            if (overlapXZ && other.visualLayer > this.visualLayer)
                return true;
        }
        return false;
    }
    public List<PaperMovement> GetOverlappingPapersToHide()
    {
        List<PaperMovement> result = new List<PaperMovement>();
        Bounds myBounds = GetComponentInChildren<Renderer>().bounds;

        foreach (var other in PaperManager.Instance.AllPapers)
        {
            if (other == this) continue;

            Renderer otherRenderer = other.GetComponentInChildren<Renderer>();
            if (otherRenderer == null) continue;

            Bounds otherBounds = otherRenderer.bounds;
            float shrinkMargin = 0.05f;

            Vector3 myMin = myBounds.min + new Vector3(shrinkMargin, 0, shrinkMargin);
            Vector3 myMax = myBounds.max - new Vector3(shrinkMargin, 0, shrinkMargin);

            bool overlapXZ =
                myMin.x < otherBounds.max.x && myMax.x > otherBounds.min.x &&
                myMin.z < otherBounds.max.z && myMax.z > otherBounds.min.z;

            if (overlapXZ)
            {
                result.Add(other);
            }
        }

        return result;
    }

    void UpdateDuringDrag()
    {
        int max = PaperManager.Instance.AllPapers
         .Where(p => p != this)
         .Max(p => p.visualLayer);
        float normalized = Mathf.InverseLerp(0f, -180f, currentAngle);
        bool hidePapers = normalized > 0.99f;
        bool foldFlipRoots = normalized > 0.6f;

        #region hide overlapped paper and fliproot
        foreach (var paper in GetOverlappingPapersToHide())
        {
            #region  hide paper
            if (!hidePapers && isDragging && paper.visualLayer == max)
            {
                // 控制可见性
                if (paper.currentAngle > -90f)
                {
                    paper.SetChildrenVisible(front: true, back: false);
                }
                else
                {
                    paper.SetChildrenVisible(front: false, back: true);
                }
            }
            else if (hidePapers)
            {
                paper.SetChildrenVisible(false, false);
            }
            #endregion

            #region fold fliproot
            if (!foldFlipRoots && paper.visualLayer == max)
            {
                // 控制可见性
                if (paper.currentAngle > -90f)
                {
                    paper.FlipRootFlip(true, true);
                  
                }
                else
                {
                    paper.FlipRootFlip(false, true);
                }


            }
            else if (foldFlipRoots)
            {
                // 控制可见性
                if (paper.currentAngle > -90f)
                {
                    paper.FlipRootFlip(true, false);
                }
                else
                {
                    paper.FlipRootFlip(false, false);
                }
            }
            #endregion

        }
        #endregion

    }

    void FoldFlipRootDuringDrag()
    {
        float normalized = Mathf.InverseLerp(0f, -180f, currentAngle);
        bool foldFlipRoots = normalized > 0.5f;

        #region fold fliproot

        //back state
        if (foldFlipRoots)
        {
            FlipRootFlip(false, true);
            FlipRootFlip(true, false);
        }
        //front state
        else
        {
            FlipRootFlip(true, true);
            FlipRootFlip(false, false);
        }
        #endregion
    }

    void SetChildrenVisible(bool front, bool back)
    {
        Transform paperFront = null;
        Transform paperBack = null;

        // find paperFront
        foreach (Transform child in transform)
        {
            if (child.name.ToLower().Contains("front"))
            {
                paperFront = child;
                if (paperFront != null)
                    paperFront.gameObject.SetActive(front);
                break;
            }

        }

        // find paperBack
        foreach (Transform child in transform)
        {
            if (child.name.ToLower().Contains("back"))
            {
                paperBack = child;
                if (paperBack != null)
                    paperBack.gameObject.SetActive(back);
                break;
            }
        }
    }
    void SetSpriteGroup(int sortingOrder)
    {
        Transform paperFront = null;
        Transform paperBack = null;

        // find paperFront
        foreach (Transform child in transform)
        {
            if (child.name.ToLower().Contains("front"))
            {
                paperFront = child;
                if (paperFront.GetComponent<SortingGroup>() != null)
                    paperFront.GetComponent<SortingGroup>().sortingOrder = sortingOrder;
                break;
            }

        }

        // find paperBack
        foreach (Transform child in transform)
        {
            if (child.name.ToLower().Contains("back"))
            {
                paperBack = child;
                if (paperBack.GetComponent<SortingGroup>() != null)
                    paperBack.GetComponent<SortingGroup>().sortingOrder = sortingOrder;
                break;
            }
        }
    }
    void FlipRootFlip(bool isFront, bool isUp)
    {
        #region find front and back paper 
        Transform paperFront = null;
        Transform paperBack = null;

        // find paperFront
        foreach (Transform child in transform)
        {
            if (child.name.ToLower().Contains("front"))
            {
                paperFront = child;
                break;
            }

        }

        // find paperBack
        foreach (Transform child in transform)
        {
            if (child.name.ToLower().Contains("back"))
            {
                paperBack = child;
                break;
            }
        }

        #endregion

        //front paper situation
        if (paperFront != null && paperFront.gameObject.activeSelf && isFront)
        {
            foreach (Transform child in paperFront)
            {
                if (child.name.ToLower().Contains("fliproot"))
                {
                    FlipRoot flipRoot = child.gameObject.GetComponent<FlipRoot>();

                    if (isUp && flipRoot.flipRootState == FlipRoot.FlipRootStates.down)
                    {
                        //child.localEulerAngles = new Vector3(-90f, 0, 0);
                        StartCoroutine(flipRoot.FlipUp());
                        flipRoot.flipRootState = FlipRoot.FlipRootStates.up;
                    }
                    else if (!isUp && flipRoot.flipRootState == FlipRoot.FlipRootStates.up)
                    {
                        //child.localEulerAngles = Vector3.zero;
                        StartCoroutine(flipRoot.FlipBack());
                        flipRoot.flipRootState = FlipRoot.FlipRootStates.down;
                    }
                }
            }
        }

        //back paper situation
        if (paperBack != null && paperBack.gameObject.activeSelf && !isFront)
        {

            foreach (Transform child in paperBack)
            {
                if (child.name.ToLower().Contains("fliproot"))
                {
                    FlipRoot flipRoot = child.gameObject.GetComponent<FlipRoot>();

                    if (isUp && flipRoot.flipRootState == FlipRoot.FlipRootStates.down)
                    {
                        //child.localEulerAngles = new Vector3(-90f, 0, 0);
                        StartCoroutine(flipRoot.FlipUp());
                        flipRoot.flipRootState = FlipRoot.FlipRootStates.up;
                    }
                    else if (!isUp && flipRoot.flipRootState == FlipRoot.FlipRootStates.up)
                    {
                        //child.localEulerAngles = Vector3.zero;
                        StartCoroutine(flipRoot.FlipBack());
                        flipRoot.flipRootState = FlipRoot.FlipRootStates.down;
                    }
                }
            }
        }
    }
    void SetCollider(bool isColliderActive)
    {
        Collider collider = this.GetComponent<Collider>();
        collider.enabled = isColliderActive;
    }
    public bool IsPlayerOnSolidPart()
    {
        Vector3 playerPos = player.position;

        //on paper
        if (!mainCollider.bounds.Contains(playerPos))
            return false;

        if (holeCollider.bounds.Contains(playerPos))
        {
            return false; // on hole
        }

        return true; // on solid part
    }

    void OnDestroy()
    {
        PaperManager.Instance.AllPapers.Remove(this);
    }
}


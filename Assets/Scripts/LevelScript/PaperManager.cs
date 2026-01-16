using System.Collections.Generic;
using UnityEngine;

public class PaperManager : MonoBehaviour
{
    public static PaperManager Instance { get; private set; }

    public List<PaperMovement> AllPapers { get; private set; } = new List<PaperMovement>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RefreshPaperList();
    }

    // 重新查找所有纸张
    public void RefreshPaperList()
    {
        AllPapers.Clear();
        PaperMovement[] papers = FindObjectsOfType<PaperMovement>();
        foreach (var paper in papers)
        {
            if (!AllPapers.Contains(paper))
                AllPapers.Add(paper);
        }
    }

    // 刷新所有纸的显隐状态
    public void RefreshAllTopStates()
    {
        foreach (var paper in AllPapers)
        {
            paper.UpdateTopVisual(); // 判断是否被覆盖并决定是否显示子物体
        }
    }
}
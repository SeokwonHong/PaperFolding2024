using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperController : MonoBehaviour
{
    #region class PaperData
    protected class PaperData
    {
        public GameObject paper;
        public int page;
        public bool isFront;

        public PaperData(GameObject paper)
        {
            this.paper = paper;
            this.isFront = CheckPaperFront(paper.name);
            this.page = GetPageFromName(paper.name);
        }
    }

    //success 1; fail 0
    private static int GetPageFromName(string name)
    {
        var match = System.Text.RegularExpressions.Regex.Match(name, @"\d+");


        if (match.Success)
        {
            return int.Parse(match.Value);
        }
        else
        {
            Debug.LogError("Failed to extract page number from: " + name);
            return 0;
        }
    }

    //front true; back false
    private static bool CheckPaperFront(string name)
    {
        return name.ToLower().Contains("front"); //true
    }
    #endregion

    protected static List<PaperData> paperList = new List<PaperData>();
    protected int pageNum = 0, index = 0;
    protected bool isFront;

    //START
    protected void Start() 
    {
        pageNum = GetPageFromName(this.gameObject.name); 
        isFront = CheckPaperFront(this.gameObject.name);

        //no paper
        if (pageNum == 0)
        {
            Debug.LogError("Invalid page number (0) for: " + this.gameObject.name);
            return;
        }
        
        //front even; back singular
        if (isFront)
        {
            index = (pageNum * 2) - 2; // 0,2,4...
        }
        else
        {
            index = (pageNum * 2) - 1; // 1,3,5...
        }


        while (paperList.Count <= index)
        {
            paperList.Add(null);
        }

        paperList[index] = new PaperData(this.gameObject);

        if (isFront)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }


    void Update()
    {

    }

}

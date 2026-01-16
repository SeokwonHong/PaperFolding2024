using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperCell : MonoBehaviour
{
    public enum CellState { Normal, Folding, Folded }

    public Vector2Int originalPos;
    public Vector2Int currentPos;
    public int currentHeight = 0;

    //public bool isFrontFacing = true;
    public bool isVisible = true;
    public CellState state = CellState.Normal;


    public void SetVisible(bool visible)
    {
        isVisible = visible;
        //this.GetComponent<MeshRenderer>().enabled = visible;
    }

    public void Flip(bool isFrontFacing)
    {
        var renderer = GetComponent<Renderer>();
        renderer.material.color = isFrontFacing ? Color.white : Color.gray;
    }

    public IEnumerator AnimateMove(Vector3 targetPos, float duration = 0.3f)
    {
        Vector3 startPos = transform.position;
        float time = 0;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }
}
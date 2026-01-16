using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipRoot : MonoBehaviour
{
    public enum FlipRootStates { up,down }
    public FlipRootStates flipRootState;

    void Start()
    {
        flipRootState = FlipRootStates.down;

    }

    void Update()
    {
        
    }

    public IEnumerator FlipUp()
    {
        if (this.CompareTag("FRM"))
        {
            float duration = 0.1f;
            float t = 0f;

            while (t < duration)
            {
                float angle = Mathf.Lerp(90f, 0f, t / duration);
                this.transform.localEulerAngles = new Vector3(angle, 0, 0);
                t += Time.deltaTime;
                yield return null;
            }
            this.transform.localEulerAngles = Vector3.zero;
        }
        else if (this.CompareTag("FRP"))
        {
            float duration = 0.1f;
            float t = 0f;

            while (t < duration)
            {
                float angle = Mathf.Lerp(-90f, 0f, t / duration);
                this.transform.localEulerAngles = new Vector3(angle, 0, 0);
                t += Time.deltaTime;
                yield return null;
            }
            this.transform.localEulerAngles = Vector3.zero;
        }
    }

    public IEnumerator FlipBack()
    {
        if (this.CompareTag("FRM"))
        {
            float duration = 0.1f;
            float t = 0f;

            while (t < duration)
            {
                float angle = Mathf.Lerp(0f, 90f, t / duration);
                this.transform.localEulerAngles = new Vector3(angle, 0, 0);
                t += Time.deltaTime;
                yield return null;
            }
            this.transform.localEulerAngles = new Vector3(90f, 0f, 0);
        }
        else if (this.CompareTag("FRP"))
        {
            float duration = 0.1f;
            float t = 0f;

            while (t < duration)
            {
                float angle = Mathf.Lerp(0f, -90f, t / duration);
                this.transform.localEulerAngles = new Vector3(angle, 0, 0);
                t += Time.deltaTime;
                yield return null;
            }
            this.transform.localEulerAngles = new Vector3(-90f, 0f, 0);
        }
    }
}

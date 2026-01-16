using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WASD : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject W, A, S, D;
    private Renderer wRend, aRend, sRend, dRend;
    private static readonly Color pressedColor = new(0.5f, 0.5f, 0.5f);
    private static readonly Color defaultColor = new(1f, 1f, 1f);

    void Start()
    {
        wRend = W.GetComponent<Renderer>();
        aRend = A.GetComponent<Renderer>();
        sRend = S.GetComponent<Renderer>();
        dRend = D.GetComponent<Renderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) wRend.material.color = pressedColor;
        if (Input.GetKeyDown(KeyCode.A)) aRend.material.color = pressedColor;
        if (Input.GetKeyDown(KeyCode.S)) sRend.material.color = pressedColor;
        if (Input.GetKeyDown(KeyCode.D)) dRend.material.color = pressedColor;

        if (Input.GetKeyUp(KeyCode.W)) wRend.material.color = defaultColor;
        if (Input.GetKeyUp(KeyCode.A)) aRend.material.color = defaultColor;
        if (Input.GetKeyUp(KeyCode.S)) sRend.material.color = defaultColor;
        if (Input.GetKeyUp(KeyCode.D)) dRend.material.color = defaultColor;
    }

}

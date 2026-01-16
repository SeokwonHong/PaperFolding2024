using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CastShadow : MonoBehaviour
{
    void Start()
    {
        var renderer = GetComponent<SpriteRenderer>();
        renderer.shadowCastingMode = ShadowCastingMode.On;
        renderer.receiveShadows = true;


    }

    void Update()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalGameManager : MonoBehaviour
{
    public static GlobalGameManager Instance;

    public enum GameState { normal,generatingBridge}
    public GameState gameState = GameState.normal;

    //void Awake()
    //{
    //    // Singleton
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(gameObject); 
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //        return;
    //    }
    //}

    void Start()
    {
        //Frame default setting
        Application.targetFrameRate = 60;
    }
}

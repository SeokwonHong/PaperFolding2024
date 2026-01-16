using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeManager : MonoBehaviour
{
    public enum BridgeType { Bridge1, Bridge2, Bridge3, Bridge4 }
    public BridgeType bridgeType;

    public static Dictionary<BridgeType, bool> bridgeStates = new Dictionary<BridgeType, bool>()
    {
        { BridgeType.Bridge1, false },
        { BridgeType.Bridge2, false },
        { BridgeType.Bridge3, false },
        { BridgeType.Bridge4, false }
    };

    void Start()
    {
        // 오브젝트 이름에 따라 자동 설정
        foreach (BridgeType type in System.Enum.GetValues(typeof(BridgeType)))
        {
            if (gameObject.name == type.ToString())
            {
                bridgeType = type;
                break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            bridgeStates[bridgeType] = true;
            Debug.Log($"{bridgeType} was triggered by the player.");
        }
    }
}

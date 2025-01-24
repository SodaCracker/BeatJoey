using System;
using UnityEngine;

public enum PlayerActionType
{
    None,
    LightAttack,
    HeavyAttack,
    Block,
    Jump,
    Dodge,
    Dash,
}

public class PlayerController : MonoBehaviour
{
    public void DoAction(PlayerActionType actionType)
    {
        CurrentPlayerActionType = actionType;
        
        // OnBeatPlayerActionInfo playerActionInfo = new()
        // {
        //     m_playerActionType = actionType,
        //     m_triggerBeatTiming = MusicManager.Instance.BeatCountFromStart
        // };
        // LastPlayerActionInfo = playerActionInfo;

        switch (actionType)
        {
            case PlayerActionType.LightAttack:
                break;
            case PlayerActionType.HeavyAttack:
                break;
            case PlayerActionType.Block:
                break;
            case PlayerActionType.Jump:
                break;
            case PlayerActionType.Dodge:
                break;
            case PlayerActionType.Dash:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void Update()
    {
        CurrentPlayerActionType = m_currentPlayerActionType;
        m_currentPlayerActionType = PlayerActionType.None;
    }

    public PlayerActionType CurrentPlayerActionType { get; private set; }
    private PlayerActionType m_currentPlayerActionType;
    // public OnBeatPlayerActionInfo LastPlayerActionInfo { get; private set; } = new();
}
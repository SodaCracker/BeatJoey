using System;
using UnityEngine;

public enum EPlayerAction
{
    None,
    Wink,
    Jump,
}

public class PlayerActionManager : ManagerBase<PlayerActionManager>, ITickable
{
    public void Tick()
    {
        CurrentPlayerAction = m_newPlayerAction;
        m_newPlayerAction = EPlayerAction.None;
    }

    public void DoAction(EPlayerAction playerAction)
    {
        m_newPlayerAction = playerAction;

        OnBeatActionInfo actionInfo = new()
        {
            m_triggerBeatTiming = MusicManager.Instance.BeatCountFromStart,
            m_playerAction = m_newPlayerAction
        };
        LastActionInfo = actionInfo;

        EventOnPlayerDoAction?.Invoke(playerAction);
    }

    public static event Action<EPlayerAction> EventOnPlayerDoAction;
    public OnBeatActionInfo LastActionInfo { get; private set; } = new();

    public EPlayerAction CurrentPlayerAction { get; private set; }
    private EPlayerAction m_newPlayerAction;
    private Animator m_animator;

    private const string BadHitSound = nameof(BadHitSound);
    private const string GoodHitSound = nameof(GoodHitSound);
}
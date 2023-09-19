using UnityEngine;

public class InputManager : ManagerBase<InputManager>, ITickable
{
    public override bool Initialize()
    {
        Application.targetFrameRate = 60;
        return base.Initialize();
    }

    public void Tick()
    {
        if (Input.GetMouseButtonDown(0) && MusicManager.Instance.IsPlaying)
        {
            EPlayerAction playerActionType;
            if (ScoringManager.Instance.Temper < ScoringManager.TemperThreshold)
            {
                playerActionType = EPlayerAction.Wink;
            }
            else
            {
                playerActionType = MusicManager.Instance.CurrentSong
                    .m_onBeatActionSequence[ScoringManager.Instance.GetNearestPlayerActionInfoIdx()].m_playerActionType;
            }

            PlayerActionManager.Instance.DoAction(playerActionType);
        }
    }
}
using UnityEngine;

public class ScoringManager : ManagerBase<ScoringManager>, ITickable
{
    public void Tick()
    {
        m_additionalScore = 0;
        if (!MusicManager.Instance.IsPlaying) return;

        float deltaCount = MusicManager.Instance.BeatCount - MusicManager.Instance.PreviousBeatCount;
        m_scoringUnitSeeker.ProceedTime(deltaCount);

        // 如果玩家进行了输入，则判断成功与否
        if (m_playerActionManager.CurrentPlayerActionType != EPlayerAction.None)
        {
            int nearestIdx = GetNearestPlayerActionInfoIdx();
            SongInfo song = MusicManager.Instance.CurrentSong;

            OnBeatActionInfo markerAct = song.m_onBeatActionSequence[nearestIdx];
            OnBeatActionInfo playerAct = m_playerActionManager.LastActionInfo;

            // 计算玩家的输入和标记的时机的偏移值
            m_lastResult.m_timingError = playerAct.m_triggerBeatTiming - markerAct.m_triggerBeatTiming;
            m_lastResult.m_markerIdx = nearestIdx;

            // 比较「最近的标记」和「最后一次输入成功的标记」
            if (nearestIdx == m_previousHitIdx)
            {
                // 再次输入的是已确认过的标记
                m_additionalScore = 0;
            }
            else
            {
                // 初次被点击的标记，成功、失败的判断
                m_additionalScore = CheckScore(nearestIdx, m_lastResult.m_timingError);
            }

            if (m_additionalScore > 0)
            {
                m_previousHitIdx = nearestIdx;
            }
            else
            {
                m_additionalScore = MissScore;
            }
        }

        Score += m_additionalScore;
    }

    public int GetNearestPlayerActionInfoIdx()
    {
        SongInfo song = MusicManager.Instance.CurrentSong;
        int nearestIdx;

        if (m_scoringUnitSeeker.NextIdx == 0)
        {
            // 检索位置位于开头时，因为之前没有标记，所以不执行比较
            nearestIdx = 0;
        }
        else if (m_scoringUnitSeeker.NextIdx >= song.m_onBeatActionSequence.Count)
        {
            // 检索位置大于数组的尺寸时（超过最后一个标记时刻时）
            nearestIdx = song.m_onBeatActionSequence.Count - 1;
        }
        else
        {
            // 从前后两个标记中，选择距离输入时刻更近的一个
            OnBeatActionInfo currentAction = song.m_onBeatActionSequence[m_scoringUnitSeeker.NextIdx];
            OnBeatActionInfo previousAction = song.m_onBeatActionSequence[m_scoringUnitSeeker.NextIdx - 1];

            float actTiming = m_playerActionManager.LastActionInfo.m_triggerBeatTiming;

            // 选择时机偏移值较小的
            if (currentAction.m_triggerBeatTiming - actTiming < actTiming - previousAction.m_triggerBeatTiming)
            {
                // 检索位置更近
                nearestIdx = m_scoringUnitSeeker.NextIdx;
            }
            else
            {
                // 检索位置的前一个更近
                nearestIdx = m_scoringUnitSeeker.NextIdx - 1;
            }
        }

        return nearestIdx;
    }

    private float CheckScore(int actionInfoIdx, float timingError)
    {
        float score;
        // 对时机的偏移值取绝对值，以处理点击早于标记的情况
        timingError = Mathf.Abs(timingError);

        do
        {
            // 偏差大于 good 时为 miss
            if (timingError >= TimingErrorToleranceGood)
            {
                score = 0f;
                break;
            }

            // good 与 excellent 之间为 good
            if (timingError >= TimingErrorToleranceExcellent)
            {
                score = GoodScore;
                break;
            }

            score = ExcellentScore;
        } while (false);

        return score;
    }


    /// <summary>
    /// 玩家输入的结果
    /// </summary>
    public struct Result
    {
        public float m_timingError;
        public int m_markerIdx;
    }

    public float Score { get; private set; }
    public float Temper { get; set; }

    public Result m_lastResult;

    private PlayerActionManager m_playerActionManager;
    private readonly SequenceSeeker<OnBeatActionInfo> m_scoringUnitSeeker = new();

    private float m_additionalScore;

    private int m_previousHitIdx = -1;
    public const float TimingErrorToleranceGood = .22f;
    public const float TimingErrorToleranceExcellent = .12f;

    public const float MissScore = -1f;
    public const float GoodScore = 2f;
    public const float ExcellentScore = 4f;
    public const float TemperThreshold = .5f;
}
using System.Collections.Generic;
using System.Diagnostics;

public class EventManager : ManagerBase<EventManager>, ITickable
{
    public override bool Initialize()
    {
        PlayerActionManager.EventOnPlayerDoAction += OnPlayerDoAction;
        return base.Initialize();
    }

    public void Tick()
    {
        SongInfo song = MusicManager.Instance.CurrentSong;

        // 检查新的激活的事件
        if (MusicManager.Instance.IsPlaying)
        {
            // 时刻前进前先保存检索位置
            m_previousIdx = m_seekerUnit.NextIdx;
            m_seekerUnit.ProceedTime(MusicManager.Instance.BeatCount - MusicManager.Instance.PreviousBeatCount);

            // 更新后的检索位置位于前一个检索位置 m_previousIdx 和 m_seekerUnit.nextIdx 之间
            for (int idx = m_previousIdx, endIdx = m_seekerUnit.NextIdx; idx < endIdx; idx++)
            {
                // 复制事件数据
                StagingDirection clone = song.m_stagingDirectionSequence[idx].GetClone() as StagingDirection;
                Debug.Assert(clone != null, nameof(clone) + " != null");
                clone.OnBegin();
                m_activeEvents.AddLast(clone);
            }
        }

        for (LinkedListNode<StagingDirection> node = m_activeEvents.First; node != null; node = node.Next)
        {
            StagingDirection activeEvt = node.Value;
            activeEvt.Update();

            // 执行还没结束 
            if (!activeEvt.IsFinished) continue;

            activeEvt.OnEnd();
            // 从执行链表里删除
            m_activeEvents.Remove(node);
        }
    }

    public void EventListenerAdd(IEventListener listener)
    {
        m_eventListenerList.Add(listener);
    }

    public void EventListenerRemove(IEventListener listener)
    {
        m_eventListenerList.Remove(listener);
    }

    public void SetSeekerSequence()
    {
        m_seekerUnit.SetSequence(MusicManager.Instance.CurrentSong.m_stagingDirectionSequence);
    }

    private void OnPlayerDoAction(EPlayerAction playerAction)
    {
        foreach (IEventListener listener in m_eventListenerList)
        {
            listener.OnPlayerDoAction(playerAction);
        }
    }

    private readonly SequenceSeeker<StagingDirection> m_seekerUnit = new();

    /// <summary>
    /// 进行中的事件链表
    /// </summary>
    private readonly LinkedList<StagingDirection> m_activeEvents = new();

    private List<IEventListener> m_eventListenerList = new();

    private int m_previousIdx;
}
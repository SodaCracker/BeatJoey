using MarchingBytes;
using UnityEngine;

public class UIManager : ManagerBase<UIManager>, ITickable
{
    public override bool Initialize()
    {
        m_notePool.EventOnPoolObjectCreated += OnNoteCreated;
        m_notePool.CreatePools();
        return base.Initialize();
    }

    public void Tick()
    {
        if (MusicManager.Instance.IsPlaying)
        {
            var songInfo = MusicManager.Instance.CurrentSongInfo;

            int rightIdx = m_rightSeeker.NextIdx;
            int leftIdx = m_leftSeeker.NextIdx;

            float xOffset;
            for (int i = rightIdx; i < leftIdx; i++)
            {
                var note = songInfo.m_onBeatActionSequence[i];
                float size = ScoringManager.Instance.m_timingErrorToleranceGood * PixelsPerBeats;
                if (ScoringManager.Instance.Temper > ScoringManager.Instance.m_temperThreshold &&
                    note.m_playerActionType == PlayerActionType.Jump)
                {
                    size *= 1.5f;
                }

                xOffset = note.m_triggerBeatTime - MusicManager.Instance.BeatCountFromStart;
                xOffset *= PixelsPerBeats;

                var noteObj = m_notePool.GetNoteObj(i);
            }
        }
    }

    private void OnNoteCreated(string poolName, GameObject noteObj)
    {
        var note = noteObj.GetComponent<ObjectPool>();
        if (note != null)
        {
        }
    }

    public float PixelsPerBeats => Screen.width * 1f / m_markerEnterOffset;

    private int m_noteCount;
    private ObjectPool m_notePool;
    private const string NoteItemPoolName = "NoteItem";

    private float m_markerEnterOffset = 2.5f;
    private float m_markerExitOffset = -1f;

    private SequenceSeeker<OnBeatActionInfo> m_leftSeeker = new SequenceSeeker<OnBeatActionInfo>();
    private SequenceSeeker<OnBeatActionInfo> m_rightSeeker = new SequenceSeeker<OnBeatActionInfo>();
}

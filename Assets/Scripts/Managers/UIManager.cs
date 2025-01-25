using System.Linq;
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
        if (!MusicManager.Instance.IsPlaying) return;
        var songInfo = MusicManager.Instance.CurrentSongInfo;

        int rightIdx = m_rightSeeker.NextIdx;
        int leftIdx = m_leftSeeker.NextIdx;

        for (int i = rightIdx; i < leftIdx; i++)
        {
            var onBeatAction = songInfo.m_onBeatActionSequence[i];
            float size = ScoringManager.Instance.m_timingErrorToleranceGood * PixelsPerBeats;
            if (ScoringManager.Instance.Temper > ScoringManager.Instance.m_temperThreshold &&
                onBeatAction.m_playerActionType == PlayerActionType.Jump)
            {
                size *= 1.5f;
            }

            float xOffset = onBeatAction.m_triggerBeatTime - MusicManager.Instance.BeatCountFromStart;
            xOffset *= PixelsPerBeats;

            var note = m_notePool.GetNote(i);
            note.SetPosition(m_markerOrigin, xOffset, size);
        }
    }

    private void OnNoteCreated(string poolName, GameObject noteObj)
    {
        var note = noteObj.GetComponent<NotePool>();
        if (note != null)
        {
        }
    }

    public float PixelsPerBeats => Screen.width * 1f / m_markerEnterOffset;

    private Vector2 m_markerOrigin = new Vector2(20.0f, 300.0f);
    private NotePool m_notePool;

    private float m_markerEnterOffset = 2.5f;
    private float m_markerExitOffset = -1f;

    private SequenceSeeker<OnBeatActionInfo> m_leftSeeker = new SequenceSeeker<OnBeatActionInfo>();
    private SequenceSeeker<OnBeatActionInfo> m_rightSeeker = new SequenceSeeker<OnBeatActionInfo>();
}

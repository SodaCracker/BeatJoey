using UnityEngine;

public class MarkerGroupUIController
{
    public void DrawSeekerGroup()
    {
        if (!MusicManager.Instance.IsPlaying) return;
        
        SongInfo song = MusicManager.Instance.CurrentSong;
        int beginMarkerIdx = m_seekerBackward.NextIdx;
        int endMarkerIdx = m_seekerForward.NextIdx;

        float size = ScoringManager.TimingErrorToleranceGood * m_pixelsPerBeat;

        for (int drawIdx = beginMarkerIdx; drawIdx < endMarkerIdx; drawIdx++)
        {
            OnBeatActionInfo info = song.m_onBeatActionSequence[drawIdx];
            float xOffset = info.m_triggerBeatTiming - MusicManager.Instance.BeatCount;
            xOffset *= m_pixelsPerBeat;

            Rect drawRect = new(m_markerOrigin.x - size / 2f + xOffset, m_markerOrigin.y - size / 2f, size,
                size);
            Graphics.DrawTexture(drawRect, m_winkIcon);
        }
    }

    /// <summary>
    /// 从左边开始显示标记（滞后的标记）
    /// </summary>
    private readonly SequenceSeeker<OnBeatActionInfo> m_seekerBackward = new();

    /// <summary>
    /// 显示标记到右边结束（提前的标记）
    /// </summary>
    private readonly SequenceSeeker<OnBeatActionInfo> m_seekerForward = new();

    public Vector2 m_markerOrigin = new(20f, 300f);
    public Texture m_winkIcon;
    
    private readonly float m_pixelsPerBeat = Screen.width * 1f / MarkerEnterOffset;
    private const float MarkerEnterOffset = 2.5f;
    private const float MarkerLeaveOffset = -1f;
}
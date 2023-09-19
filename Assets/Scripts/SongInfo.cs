using System.Collections.Generic;

public class SongInfo
{
    public List<OnBeatActionInfo> m_onBeatActionSequence = new();
    public List<StagingDirection> m_stagingDirectionSequence = new();
    public float m_beatPerSecond = 120f / 60f;
    public float m_beatPerBar = 4f;
}
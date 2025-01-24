using UnityEngine;

public class MusicManager : ManagerBase<MusicManager>, ITickable
{
    public override bool Initialize()
    {
        Application.targetFrameRate = 60;
        m_audioSource = GameObject.Find("UIRoot").GetComponent<AudioSource>();
        IsMusicFinished = false;
        return base.Initialize();
    }

    public void Tick()
    {
        if (m_audioSource.isPlaying)
        {
            PreviousBeatCountFromStart = BeatCountFromStart;
            BeatCountFromStart = m_audioSource.time * CurrentSongInfo.m_beatPerSecond;
            FloatPreviousBeatCount = BeatCountFromStart - PreviousBeatCountFromStart;
        }
        else
        {
            if (m_isPlayingPreviousFrame &&
                (m_audioSource.timeSamples <= 0 ||
                 m_audioSource.timeSamples > m_audioSource.clip.samples))
            {
                IsMusicFinished = true;
            }

            m_isPlayingPreviousFrame = false;
        }
    }

    public bool IsPlaying => m_audioSource.isPlaying;

    public bool IsMusicFinished { get; private set; }
    public SongInfo CurrentSongInfo { get; private set; }

    public float SongLength => m_audioSource.clip.length * CurrentSongInfo.m_beatPerSecond;

    public float BeatCountFromStart { get; private set; }
    public float PreviousBeatCountFromStart { get; private set; }
    public float FloatPreviousBeatCount { get; private set; }

    private AudioSource m_audioSource;
    private bool m_isPlayingPreviousFrame;
}

using UnityEngine;

public class MusicManager : ManagerBase<MusicManager>, ITickable
{
    public override bool Initialize()
    {
        m_musicFinished = false;
        return base.Initialize();
    }

    public void Tick()
    {
        if (m_audioSource.isPlaying)
        {
            PreviousBeatCountFromStart = BeatCountFromStart;
            BeatCountFromStart = m_audioSource.time * CurrentSong.m_beatPerSecond;
            m_isPlayPreviousFrame = true;
        }
        else
        {
            if (m_isPlayPreviousFrame &&
                (m_audioSource.timeSamples <= 0 || m_audioSource.timeSamples >= m_audioSource.clip.samples))
            {
                m_musicFinished = true;
            }

            m_isPlayPreviousFrame = false;
        }
    }

    public void PlayMusicFromStart()
    {
        m_musicFinished = false;
        m_isPlayPreviousFrame = false;
        BeatCountFromStart = 0;
        PreviousBeatCountFromStart = 0;
        m_audioSource.Play();
    }
    public float BeatCountFromStart { get; private set; }
    public float BeatCount => BeatCountFromStart;

    public float PreviousBeatCountFromStart { get; private set; }
    public float PreviousBeatCount => PreviousBeatCount;

    public SongInfo CurrentSong { set; get; }
    public float Length => m_audioSource.clip.length * CurrentSong.m_beatPerSecond;
    public bool IsPlaying => m_audioSource.isPlaying;
    public bool IsFinished => m_musicFinished;

    private AudioSource m_audioSource;
    private bool m_isPlayPreviousFrame;
    private bool m_musicFinished;
}
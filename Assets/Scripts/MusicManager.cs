using UnityEngine;

public class MusicManager : ManagerBase<MusicManager>
{
    public float BeatCountFromStart { get; private set; }
    public float BeatCount => BeatCountFromStart;
    
    public float PreviousBeatCountFromStart { get; private set; }
    public float PreviousBeatCount => PreviousBeatCount;
    
    public SongInfo CurrentSong { set; get; }
    public bool IsPlaying => m_audioSource.isPlaying;

    private AudioSource m_audioSource;
}
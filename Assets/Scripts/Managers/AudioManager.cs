public class AudioManager : ManagerBase<AudioManager>
{
    public enum AudioType
    {
        SFX,
        BGM,
    }
    
    public void SetVolume(AudioType audioType, float volume)
    {
    }

    public override bool Initialize()
    {
        m_musicManager = new MusicManager();
        m_musicManager.Initialize();

        return true;
    }

    private MusicManager m_musicManager;
}

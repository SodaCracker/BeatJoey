using UnityEngine;
public class GameManager : ManagerBase<GameManager>, ITickable
{
    public override bool Initialize()
    {
        if (!InitializeGameSettings())
        {
            Debug.LogError("GameManager.Initialize InitializeGameSettings failed");
        }

        InitializeInputManager();
        InitializeEventManager();
        InitializeAudioManager();
        InitializePhaseManager();
        InitializeScoringManager();

        OnInitializeEnd();

        return true;
    }

    protected override bool Uninitialize()
    {
        return base.Uninitialize();
    }

    public void Tick()
    {
    }

    protected virtual bool InitializeGameSettings()
    {
        m_gameSettings = Resources.Load("GameSettings", typeof(GameSettings)) as GameSettings;
        if (m_gameSettings == null)
        {
            Debug.LogError("GameManager.InitializeGameSettings GameSettings is null");
            return false;
        }

        return true;
    }

    private void InitializeInputManager()
    {
        m_inputManager = InputManager.CreateManager();
        m_inputManager.Initialize();
    }

    private void InitializeEventManager()
    {
        m_eventManager = EventManager.CreateManager();
        m_eventManager.Initialize();
    }

    private void InitializeAudioManager()
    {
        m_audioManager = AudioManager.CreateManager();
        m_audioManager.Initialize();
    }

    private void InitializePhaseManager()
    {
        m_phaseManager = PhaseManager.CreateManager();
        m_phaseManager.Initialize();
    }

    private void InitializeScoringManager()
    {
        m_scoringManager = ScoringManager.CreateManager();
        m_scoringManager.Initialize();
    }

    private void OnInitializeEnd()
    {
        m_audioManager.SetVolume(AudioManager.AudioType.BGM, 1.0f);
        m_audioManager.SetVolume(AudioManager.AudioType.SFX, 1.0f);
        
        var playerPrefab = Resources.Load("Player") as GameObject;
        m_player = Object.Instantiate(playerPrefab).GetComponent<PlayerController>();
    }

    private PlayerController m_player;
    
    private GameSettings m_gameSettings;
    private InputManager m_inputManager;
    private EventManager m_eventManager;
    private AudioManager m_audioManager;
    private PhaseManager m_phaseManager;
    private ScoringManager m_scoringManager;
}

public class ManagerBase<T> where T : new()
{
    public static T Instance => s_instance;

    public static T CreateManager()
    {
        if (s_instance == null)
        {
            s_instance = new T();
        }

        return s_instance;
    }

    public virtual bool Initialize()
    {
        return true;
    }

    protected virtual bool Uninitialize()
    {
        return true;
    }

    private static T s_instance;
}

public interface ITickable
{
    void Tick();
}

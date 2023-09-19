using UnityEngine;

public class GameLauncher : MonoBehaviour
{
    private void Awake()
    {
        InputManager.CreateManager();
        MusicManager.CreateManager();
        EventManager.CreateManager();
        ScoringManager.CreateManager();
        PlayerActionManager.CreateManager();
        UIManager.CreateManager();
    }

    private void Start()
    {
        InputManager.Instance.Initialize();
        MusicManager.Instance.Initialize();
        EventManager.Instance.Initialize();
        
        EventManager.Instance.EventListenerAdd(UIManager.Instance);
    }

    private void Update()
    {
        InputManager.Instance.Tick();
        MusicManager.Instance.Tick();
        EventManager.Instance.Tick();
        UIManager.Instance.Tick();
    }
}
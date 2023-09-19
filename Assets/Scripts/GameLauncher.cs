using System;
using UnityEngine;

public class GameLauncher : MonoBehaviour
{
    private void Awake()
    {
        m_inputManager = InputManager.CreateManager();
        m_inputManager.Initialize();
    }

    private void Start()
    {
        m_musicManager = MusicManager.CreateManager();
        m_scoringManager = ScoringManager.CreateManager();
        m_eventManager = EventManager.CreateManager();
    }

    private InputManager m_inputManager;
    private MusicManager m_musicManager;
    private ScoringManager m_scoringManager;
    private EventManager m_eventManager;
}
using System;
using UnityEngine;

public class GameLauncher : MonoBehaviour
{
    private void Start()
    {
        m_gameManager = GameManager.CreateManager();
        m_gameManager.Initialize();
        
        
    }

    private void Update()
    {
        m_gameManager?.Tick();
    }

    private GameManager m_gameManager;
}

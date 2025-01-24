using System;
using UnityEngine;

public class ScoringManager : ManagerBase<ScoringManager>
{
    public float m_timingErrorToleranceGood = 0.22f;
    public float m_temperThreshold = .5f;
    
    public float Temper
    {
        get => m_temper;
        set => m_temper = Mathf.Clamp(value, 0.0f, 1.0f);
    }

    private float m_temper;
}

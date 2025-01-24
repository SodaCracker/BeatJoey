using System.Collections.Generic;
using UnityEngine;

public class SongInfo : ScriptableObject
{
    public List<OnBeatActionInfo> m_onBeatActionSequence = new List<OnBeatActionInfo>();

    public float m_beatPerSecond = 120.0f / 60.0f;
}

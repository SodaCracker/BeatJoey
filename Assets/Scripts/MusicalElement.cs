public abstract class MusicalElement
{
    public virtual MusicalElement GetClone()
    {
        var clone = MemberwiseClone() as MusicalElement;
        return clone;
    }
    public float m_triggerBeatTiming = 0f;
}

public class SequenceRegion : MusicalElement
{
    public float m_totalBeatCount;
    public string m_name;
    public float m_repeatPosition;
}

public class OnBeatActionInfo : MusicalElement
{
    public EPlayerAction m_playerActionType;
}
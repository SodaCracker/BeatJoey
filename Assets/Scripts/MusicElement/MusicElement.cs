public abstract class MusicElement
{
    public float m_triggerBeatTime;

    public virtual void ReadCustomParamsFromString(string[] parameters)
    {
    }

    public virtual MusicElement GetClone()
    {
        var clone = MemberwiseClone() as MusicElement;
        return clone;
    }
}

public class SequenceRegion : MusicElement
{
    public float m_totalBeatCount;
    public string m_name;
    public float m_repeatPosition;
}

public class OnBeatActionInfo : MusicElement
{
    public PlayerActionType m_playerActionType;
    public int m_lineNumber;
}

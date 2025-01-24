using System.Collections.Generic;

public class SequenceSeeker<ElementType> where ElementType : MusicElement
{
    public void ProceedTime(float deltaBeatCount)
    {
        m_currentBeatCount += deltaBeatCount;
        m_isJustPassElement = false;

        int nextIdx = FindNextElement(NextIdx);
        if (nextIdx == NextIdx) return;

        NextIdx = nextIdx;
        m_isJustPassElement = true;
    }

    /// <summary>
    /// 找到下一个元素
    /// </summary>
    /// <param name="startIdx"></param>
    /// <returns></returns>
    private int FindNextElement(int startIdx)
    {
        int nextIdx = startIdx;
        for (int i = startIdx; i < m_sequence.Count; i++)
        {
            ElementType musicElement = m_sequence[i];
            if (musicElement.m_triggerBeatTime <= m_currentBeatCount) continue;

            nextIdx = i;
            break;
        }

        return nextIdx;
    }

    public int NextIdx { get;private set; }
    
    private float m_currentBeatCount;
    private bool m_isJustPassElement;

    private List<ElementType> m_sequence;
}

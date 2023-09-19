using System.Collections.Generic;

public class SequenceSeeker<T> where T : MusicalElement
{
    public void ProceedTime(float deltaBeatCount)
    {
        m_currentBeatCount += deltaBeatCount; // 累加现在的时刻
        m_isJustPassElement = false; // 设置表示「检索位置刚好过」瞬间标记为 false

        int idx = FindNextElement(m_nextIdx); // 取得现在时候之后最近的那个标记时刻的索引
        if (idx == m_nextIdx) return;

        m_nextIdx = idx; // 之后那个标记是不是刚好要的检索位置？
        m_isJustPassElement = true; // 更新检索位置
    }

    /// <summary>
    /// 查找 m_currentBeatCount 之后的那个标记
    /// </summary>
    /// <param name="startIdx"></param>
    /// <returns></returns>
    private int FindNextElement(int startIdx)
    {
        int ret = m_sequence.Count; // 通过表示「超过了最后标记的时候」值进行初始化
        for (int i = startIdx; i < ret; i++)
        {
            if (m_sequence[i].m_triggerBeatTiming <= m_currentBeatCount) continue;
            return i; // 该标记位置在现在时刻之后
        }

        return ret;
    }

    public int NextIdx => m_nextIdx;
    public bool IsJustPassElement => m_isJustPassElement;

    /// <summary>
    /// 当前播放位置之后，最近的一个标记的索引
    /// </summary>
    private int m_nextIdx;

    private float m_currentBeatCount;
    private bool m_isJustPassElement; // debug 的便利

    private List<T> m_sequence;
}
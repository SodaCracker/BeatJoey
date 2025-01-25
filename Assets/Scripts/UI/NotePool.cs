using MarchingBytes;
using UnityEngine;

public class NotePool : EasyObjectPool
{
    public NoteUI GetNote(int idx)
    {
        return null;
    }

    public void Clear()
    {
        if (!Application.isPlaying) return;

        m_itemStart = 0;
        m_itemEnd = 0;
        m_totalItemCount = 0;
        for (int i = GetValidItemCount(); i >= 0; i--)
        {
            ReturnObjectToPool(m_content.GetChild(i).gameObject);
        }
    }

    public void Refresh()
    {
        if (!Application.isPlaying) return;
        Canvas.ForceUpdateCanvases();

        // recycle items if we can
        for (int i = 0; i < GetValidItemCount(); i++)
        {
            var child = m_content.GetChild(i);
            if (m_totalItemCount >= 0 && m_itemEnd >= m_totalItemCount)
            {
                child.SetAsLastSibling();
                ReturnObjectToPool(child.gameObject);
                i--;
            }
            else
            {
                var noteObj = child.GetComponent<NoteUI>();
                noteObj.ScrollCellIndex(m_itemEnd);
                m_itemEnd++;
            }
        }

        if (GetValidItemCount() > 0)
        {
            Canvas.ForceUpdateCanvases();
        }
    }

    private int GetValidItemCount()
    {
        int count = 0;
        for (int i = 0; i < m_content.childCount; i++)
        {
            var child = m_content.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                count++;
            }
        }

        return count;
    }

    public int m_totalItemCount;

    protected int m_itemStart;
    protected int m_itemEnd;

    protected RectTransform m_content;
}

public class NoteUI : PoolObject
{
    public void SetPosition(Vector2 markerOrigin, float xOffset, float size)
    {
        transform.position = new Vector3(markerOrigin.x + xOffset, markerOrigin.y, 0);
        transform.localScale = new Vector3(size, size, 1);
    }
    
    public int m_noteIdx;
    public bool m_isInUse;
}

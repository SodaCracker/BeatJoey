using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UnityEngine.UI
{
    [AddComponentMenu("ScrollRect/ChatLoopVerticalScrollRect")]
    public class ChatLoopVerticalScrollRect : LoopVerticalScrollRect
    {
        /// <summary>
        /// 从显示队列尾部起填充scrollView
        /// </summary>
        /// <param name="isScroll"></param>
        public void RefillCellsFromEnd(bool isScroll = true)
        {
            if (isScroll)
            {
                RefillCellsFromEndWithScroll();
            }
            else
            {
                RefillCellsFromEndWithOutScroll();
            }

        }


        protected void RefillCellsFromEndWithOutScroll()
        {
            // 清理之前的所以元素
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                prefabPool.ReturnObjectToPool(content.GetChild(i).gameObject);
            }

            // 当前无显示元素，直接返回
            if (totalCount <= 0)
            {
                itemTypeEnd = 0;
                itemTypeStart = 0;
                return;
            }

            // 设置头/尾索引
            itemTypeEnd = totalCount;
            itemTypeStart = itemTypeEnd - 1;

            // 开始往Content内插入元素，直到startIndex = 0 或者 除最后一个元素外的Content的大小超过ViewRect的大小
            float contextSize = 0f;
            while (contextSize <= viewRect.rect.height)
            {
                RectTransform nextItem = InstantiateNextItem(itemTypeStart);
                nextItem.SetAsFirstSibling();

                Canvas.ForceUpdateCanvases();
                contextSize += GetSize(nextItem);

                if (contextSize <= viewRect.rect.height)
                {
                    itemTypeStart--;
                }

                if (itemTypeStart < 0)
                {
                    itemTypeStart = 0;
                    break;
                }
            }

            if (contextSize >= viewRect.rect.height)
            {
                m_Content.anchoredPosition = new Vector2(m_Content.anchoredPosition.x, contextSize - viewRect.rect.height);
            }else
            {
                m_Content.anchoredPosition = new Vector2(m_Content.anchoredPosition.x, 0);
            }

            m_Velocity = Vector2.zero;

        }

        protected void RefillCellsFromEndWithScroll()
        {
            // 清理之前的所以元素
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                prefabPool.ReturnObjectToPool(content.GetChild(i).gameObject);
            }

            // 当前无显示元素，直接返回
            if (totalCount <= 0)
            {
                itemTypeEnd = 0;
                itemTypeStart = 0;
                return;
            }

            // 设置头/尾索引
            itemTypeEnd = totalCount;
            itemTypeStart = itemTypeEnd - 1;



            // 插入最后一个元素
            RectTransform lastItem = InstantiateNextItem(itemTypeStart);

            Canvas.ForceUpdateCanvases();

            float lastItemSize = GetSize(lastItem);

            // 开始往Content内插入元素，直到startIndex = 0 或者 除最后一个元素外的Content的大小超过ViewRect的大小
            float contextSizeWithoutLastItem = 0f;
            while (contextSizeWithoutLastItem <= viewRect.rect.height)
            {
                if (itemTypeStart <= 0)
                {
                    break;
                }
                else
                {
                    itemTypeStart--;

                    RectTransform nextItem = InstantiateNextItem(itemTypeStart);
                    nextItem.SetAsFirstSibling();

                    Canvas.ForceUpdateCanvases();
                    contextSizeWithoutLastItem += GetSize(nextItem);
                }

            }

            var LayoutGroup = m_Content.GetComponent<LayoutGroup>();
            if (LayoutGroup != null)
            {
                contextSizeWithoutLastItem += LayoutGroup.padding.top;
                contextSizeWithoutLastItem += LayoutGroup.padding.bottom;
            }

            // 如果除最后一个元素外的Content的大小超过ViewRect的大小，将Context的位置设置到正好看不见最后一个元素，再给一个初始速度让它滚动
            if (contextSizeWithoutLastItem > viewRect.rect.height)
            {
                m_Content.anchoredPosition = new Vector2(m_Content.anchoredPosition.x, contextSizeWithoutLastItem - viewRect.rect.height);
                m_Velocity = new Vector2(0, lastItemSize / m_velocityRatio);
            }
            else
            {
                // 如果整个Content的大小没超过ViewRect,则不移动
                float contextSize = contextSizeWithoutLastItem + lastItemSize;
                if (contextSize <= viewRect.rect.height)
                {
                    m_Content.anchoredPosition = new Vector2(m_Content.anchoredPosition.x, 0);
                    m_Velocity = new Vector2(0, 0);
                }
                // 否则，给个初始速度让其滚动
                else
                {
                    m_Content.anchoredPosition = new Vector2(m_Content.anchoredPosition.x, 0);
                    m_Velocity = new Vector2(0, lastItemSize / m_velocityRatio);
                }
            }
        }

        /// <summary>
        /// 速度计算因子
        /// </summary>
        private const float m_velocityRatio = 0.218f;
    }
}
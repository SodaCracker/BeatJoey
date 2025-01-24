using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UnityEngine.UI
{
    [AddComponentMenu("ScrollRect/DockedLoopVerticalScrollRect")]
    public class DockedLoopVerticalScrollRect : DockedLoopScrollRect
    {
        protected override void Awake()
        {
            base.Awake();
            directionSign = -1;

            GridLayoutGroup layout = content.GetComponent<GridLayoutGroup>();
            if (layout != null && layout.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
            {
                Debug.LogError("[LoopVerticalScrollRect] unsupported GridLayoutGroup constraint");
            }
        }


        protected override float GetSize(RectTransform item)
        {
            return LayoutUtility.GetPreferredHeight(item) + contentSpacing;
        }

        protected override float GetDimension(Vector2 vector)
        {
            return vector.y;
        }

        protected override Vector2 GetVector(float value)
        {
            return new Vector2(0, value);
        }


        /// <summary>
        /// 计算Docking模式中某个item边缘和ViewRect的偏移值
        /// </summary>
        /// <returns></returns>
        protected override Vector2 CalculateDockOffset()
        {
            var offset = Vector2.zero;

            // 获取item大小
            float itemSize = GetItemSize();

            // 计算viewBound和某个item便于的距离
            offset.y = (m_ViewBounds.max - m_ContentBounds.max).y % itemSize;


            // 距离加半个item大小的正负，来决定是向上还是向下滚动
            if ((offset.y + itemSize * 0.5f) <= 0)
            {
                offset.y += itemSize;
            }

            // 根据滚动方向，实时调整速度
            if (offset.y >= 0)
            {
                m_Velocity.y = Mathf.Abs(m_Velocity.y);
            }
            else
            {
                m_Velocity.y = -1f * Mathf.Abs(m_Velocity.y);
            }

            return offset;
        }




        /// <summary>
        /// 减速模式的tick
        /// </summary>
        /// <param name="deltaTime"></param>
        protected override void TickInertia(float deltaTime)
        {
            Vector2 position = m_Content.anchoredPosition;

            m_Velocity.y *= Mathf.Pow(m_DecelerationRate, deltaTime);


            float velocity = Mathf.Abs(m_Velocity.y);
            if (velocity < VelocitySplitBetweenInertiaAndDocking)
            {
                m_Velocity.y = VelocitySplitBetweenInertiaAndDocking;
                SetState(ScrollRectState.Docking);

            }

            position.y += m_Velocity.y * deltaTime;

            if (m_Velocity != Vector2.zero)
            {
                if (m_MovementType == MovementType.Clamped)
                {
                    var offset = CalculateOffset(position - m_Content.anchoredPosition);
                    position += offset;
                }

                SetContentAnchoredPosition(position);
            }
        }

        /// <summary>
        /// 停靠中的tick
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="deltaTime"></param>
        protected override void TickDocking(Vector2 offset, float deltaTime)
        {
            // 按照移动速度调整位置
            Vector2 position = m_Content.anchoredPosition;
            if (offset.y != 0)
            {
                position.y += m_Velocity.y * deltaTime;
            }
            
            if (m_Velocity.y > 0)
            {
                // 如果调整后超出范围，则设置成停止
                if (position.y > m_Content.anchoredPosition.y + offset.y)
                {
                    position.y = m_Content.anchoredPosition.y + offset.y;
                    m_Velocity = Vector2.zero;
                    SetContentAnchoredPosition(position);
                    OnMovementEnd();
                }
            }
            else if (m_Velocity.y < 0)
            {
                // 如果调整后超出范围，则设置成停止
                if (position.y < m_Content.anchoredPosition.y + offset.y)
                {
                    position.y = m_Content.anchoredPosition.y + offset.y;
                    m_Velocity = Vector2.zero;
                    SetContentAnchoredPosition(position);
                    OnMovementEnd();
                }
            }

            // 速度不为0时，设置Context的新位置
            if (m_Velocity != Vector2.zero)
            {
                if (m_MovementType == MovementType.Clamped)
                {
                    offset = CalculateOffset(position - m_Content.anchoredPosition);
                    position += offset;
                }

                SetContentAnchoredPosition(position);
            }
        }

        /// <summary>
        /// 移动结束时的处理
        /// </summary>
        protected override void OnMovementEnd()
        {
            
            SetState(ScrollRectState.Docked);

            var position = m_Content.anchoredPosition;
            float itemSize = GetItemSize();

            // 停止移动后，移除上方多余的item;
            var maxOffset = m_ContentBounds.max.y - m_ViewBounds.max.y;
            int rowToRemove = Mathf.RoundToInt(maxOffset / itemSize);
            int numberToRemove = rowToRemove * contentConstraintCount;

            for (int i = 0; i < numberToRemove; i++)
            {
                var rect = m_Content.GetChild(0);
                prefabPool.ReturnObjectToPool(rect.gameObject);
            }

            if (rowToRemove != 0)
            {
                position.y -= itemSize * rowToRemove;
            }


            //Debug.LogError("remove from start row: " + rowToRemove.ToString() + " count : " + numberToRemove);

            // 更新startIndex
            itemTypeStart += numberToRemove;

            // 移除下方多余的item
            var minOffset = m_ViewBounds.min.y - m_ContentBounds.min.y;
            rowToRemove = Mathf.RoundToInt(minOffset / itemSize);
            if (rowToRemove > 0)
            {
                
                numberToRemove = rowToRemove * contentConstraintCount;
                // 考虑最后一排不满的情况
                int remainder =  m_Content.childCount % contentConstraintCount;
                if (remainder != 0)
                {
                    numberToRemove -= contentConstraintCount - remainder;
                }


                for (int i = 0; i < numberToRemove; i++)
                {
                    var rect = m_Content.GetChild(m_Content.childCount - 1);
                    prefabPool.ReturnObjectToPool(rect.gameObject);
                }

                //Debug.LogError("remove from end row: " + rowToRemove.ToString() + " count : " + numberToRemove);

                // 更新endIndex
                itemTypeEnd -= numberToRemove;
            }


            m_Content.anchoredPosition = position;

            SetMaskEnable(false);
        }

        protected override bool UpdateItems(Bounds viewBounds, Bounds contentBounds)
        {
            if (m_currState == ScrollRectState.Docked)
            {
                return false;
            }

            bool changed = false;

            if (viewBounds.min.y < contentBounds.min.y + 1)
            {
                float size = NewItemAtEnd();
                if (size > 0)
                {
                    if (threshold < size)
                    {
                        threshold = size * 1.1f;
                    }
                    changed = true;
                }
            }
            else if (viewBounds.min.y > contentBounds.min.y + threshold)
            {
                float size = DeleteItemAtEnd();
                if (size > 0)
                {
                    changed = true;
                }
            }
            if (viewBounds.max.y > contentBounds.max.y - 1)
            {
                float size = NewItemAtStart();
                if (size > 0)
                {
                    if (threshold < size)
                    {
                        threshold = size * 1.1f;
                    }
                    changed = true;
                }
            }
            else if (viewBounds.max.y < contentBounds.max.y - threshold)
            {
                float size = DeleteItemAtStart();
                if (size > 0)
                {
                    changed = true;
                }
            }
            return changed;
        }
    }
}
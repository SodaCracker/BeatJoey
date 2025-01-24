using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UnityEngine.UI
{
    [AddComponentMenu("ScrollRect/LoopVerticalScrollRect")]
    public class LoopVerticalScrollRect : LoopScrollRect
    {
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

        protected override void Awake()
        {
            base.Awake();

            directionSign = -1;
            if (content != null)
            {
                GridLayoutGroup layout = content.GetComponent<GridLayoutGroup>();
                if (layout != null && layout.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
                {
                    Debug.LogError("[LoopHorizontalScrollRect] unsupported GridLayoutGroup constraint");
                }
            }
        }

        protected override bool UpdateItems(Bounds viewBounds, Bounds contentBounds)
        {
            bool changed = false;

            if (m_Dragging)
            {
                if (TryAddItemAtEnd(viewBounds, contentBounds))
                    changed = true;
                else if (TryDeleteItemAtEnd(viewBounds, contentBounds))
                    changed = true;

                if (TryAddItemAtStart(viewBounds, contentBounds))
                    changed = true;
                else if (TryDeleteItemAtStart(viewBounds, contentBounds))
                    changed = true;
            }
            else
            {
                if (0 < velocity[1])
                {
                    if(TryRecycleItemAtStart(viewBounds, contentBounds))
                        changed = true;
                }
                else if (velocity[1] < 0)
                {
                    if (TryRecycleItemAtEnd(viewBounds, contentBounds))
                        changed = true;
                }
                else
                {
                    if (TryDeleteItemAtEnd(viewBounds, contentBounds))
                        changed = true;

                    if (TryDeleteItemAtStart(viewBounds, contentBounds))
                        changed = true;
                }

                if (!changed)
                {
                    if (TryAddItemAtEnd(viewBounds, contentBounds))
                        changed = true;

                    if (TryAddItemAtStart(viewBounds, contentBounds))
                        changed = true;
                }
            }

            return changed;
        }

        bool TryAddItemAtEnd(Bounds viewBounds, Bounds contentBounds)
        {
            if (viewBounds.min.y < contentBounds.min.y + 1 ||
                (itemTypeEnd % contentConstraintCount != 0 && itemTypeEnd < totalCount))
            {
                float size = NewItemAtEnd();
                if (size > 0)
                {
                    if (threshold < size)
                    {
                        threshold = size * 1.1f;
                    }
                    return true;
                }
            }
            return false;
        }

        bool TryAddItemAtStart(Bounds viewBounds, Bounds contentBounds)
        {
            if (viewBounds.max.y > contentBounds.max.y - 1)
            {
                float size = NewItemAtStart();
                if (size > 0)
                {
                    if (threshold < size)
                    {
                        threshold = size * 1.1f;
                    }
                    return true;
                }
            }
            return false;
        }

        bool TryDeleteItemAtEnd(Bounds viewBounds, Bounds contentBounds)
        {
            if (viewBounds.min.y > contentBounds.min.y + threshold)
            {
                float size = DeleteItemAtEnd();
                if (size > 0)
                {
                    return true;
                }
            }
            return false;
        }

        bool TryDeleteItemAtStart(Bounds viewBounds, Bounds contentBounds)
        {
            if (viewBounds.max.y < contentBounds.max.y - threshold)
            {
                float size = DeleteItemAtStart();
                if (size > 0)
                {
                    return true;
                }
            }
            return false;
        }

        bool TryRecycleItemAtStart(Bounds viewBounds, Bounds contentBounds)
        {
            if (viewBounds.max.y < contentBounds.max.y - threshold)
            {
                float size = RecycleItemAtStart();
                if (size > 0)
                {
                    return true;
                }
            }
            return false;
        }

        bool TryRecycleItemAtEnd(Bounds viewBounds, Bounds contentBounds)
        {
            if (viewBounds.min.y > contentBounds.min.y + threshold)
            {
                float size = RecycleItemAtEnd();
                if (size > 0)
                {
                    return true;
                }
            }
            return false;
        }

    }

}
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    /// <summary>
    /// 自动停靠item边缘并去掉mask的LoopScroll
    /// </summary>
    [AddComponentMenu("ScrollRect/DockedLoopScrollRect", 16)]
    [SelectionBase]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class DockedLoopScrollRect : LoopScrollRect
    {

        public override void RefillCells(int startIdx = 0)
        {
            SetState(ScrollRectState.Init);
            base.RefillCells(startIdx);
        }


        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            // 开始拖动时，设置为拖动中
            SetState(ScrollRectState.Draging);

            base.OnBeginDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            // 结束拖拽时，计算ContextBounds和ViewBounds之间的偏移值
            // 如果偏移不为0，则表示Context超出View的范围，需要设置成弹簧模式，
            // 否则，直接用摩擦模式来减速
            var offset = CalculateElasticOffset();
            if (IsVector2Equal(offset, Vector2.zero))
            {
                SetState(ScrollRectState.Inertia);
            }
            else
            {
                SetState(ScrollRectState.Elastic);
            }
            base.OnEndDrag(eventData);
        }

        /// <summary>
        /// 计算弹簧模式中ContextBounds和ViewBounds之间的偏移值
        /// </summary>
        /// <returns></returns>
        protected Vector2 CalculateElasticOffset()
        {
            return base.CalculateOffset(Vector2.zero);

            //// 当计算viewBounds和contextBounds的offset为0时，如果处于docking状态，计算到dock位置的offset
            //if (!IsVector2Equal(offset, Vector2.zero))
            //{
            //    SetState(ScrollRectState.Elastic);
            //}

            //return offset;
        }

        /// <summary>
        /// 计算Docking模式中某个item边缘和ViewRect的偏移值
        /// </summary>
        /// <returns></returns>
        protected virtual Vector2 CalculateDockOffset()
        {
            return Vector2.zero;
        }

        protected override void LateUpdate()
        {
            if (!m_Content)
                return;

            EnsureLayoutHasRebuilt();
            UpdateScrollbarVisibility();
            UpdateBounds();


            float deltaTime = Time.unscaledDeltaTime;
            //Vector2 offset = CalculateOffset(Vector2.zero);
            //if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero))
            //if (!m_Dragging && (!IsVector2Equal(offset, Vector2.zero) || !IsVector2Equal(m_Velocity, Vector2.zero)))
            //{
            //    if (m_isInFadeStop)
            //    {
            //        TickFadeStopMovement(offset, deltaTime);
            //    }
            //    else
            //    {
            //        TickNormalMovement(offset, deltaTime);
            //    }
            //}

            Vector2 offset = Vector2.zero;

            // 根据当前状态，tick运动状态
            switch (m_currState)
            {
                case ScrollRectState.Elastic:
                    {
                        offset = CalculateElasticOffset();
                        TickElastic(offset, deltaTime);                  
                    }
                    break;
                case ScrollRectState.Inertia:
                    {
                        // 在减速过程中，如果触碰到边缘，进入弹簧模式
                        offset = CalculateElasticOffset();
                        if (!IsVector2Equal(offset, Vector2.zero))
                        {
                            SetState(ScrollRectState.Elastic);
                            return;
                        }

                        TickInertia(deltaTime);
                    }
                    break;
                case ScrollRectState.Docking:
                    {
                        // 在停靠过程中，如果触碰到边缘，进入弹簧模式
                        offset = CalculateElasticOffset();
                        if (!IsVector2Equal(offset, Vector2.zero))
                        {
                            SetState(ScrollRectState.Elastic);
                            return;
                        }

                        offset = CalculateDockOffset();
                        TickDocking(offset, deltaTime);
                    }
                    break;

            }


            // 拖动时，计算速度
            if (m_Dragging && m_Inertia)
            {
                Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
            }

            if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
            {
                UpdateScrollbars(offset);
                m_OnValueChanged.Invoke(normalizedPosition);
                UpdatePrevData();
            }
        }

        protected override void UpdateBounds()
        {
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();

            if (m_Content == null)
                return;

            // ============LoopScrollRect============
            // Don't do this in Rebuild
            // Canvas在RebuildingLayout时，UpdateItems会报错：Trying to remove {0} from rebuild list while we are already inside a rebuild loop. This is not supported
            if (Application.isPlaying && !CanvasUpdateRegistry.IsRebuildingLayout() )
            {
                bool change = UpdateItems(m_ViewBounds, m_ContentBounds);
                if (change)
                {
                    Canvas.ForceUpdateCanvases();
                    m_ContentBounds = GetBounds();
                }
                else
                {
                    // 初始状态直接进入Docked状态，来清除最开始的mask
                    if (m_currState == ScrollRectState.Init)
                    {
                        OnMovementEnd();
                    }
                }


            }
            // ============LoopScrollRect============

            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the content.
            Vector3 contentSize = m_ContentBounds.size;
            Vector3 contentPos = m_ContentBounds.center;
            Vector3 excess = m_ViewBounds.size - contentSize;
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (m_Content.pivot.x - 0.5f);
                contentSize.x = m_ViewBounds.size.x;
            }
            if (excess.y > 0)
            {
                contentPos.y -= excess.y * (m_Content.pivot.y - 0.5f);
                contentSize.y = m_ViewBounds.size.y;
            }

            m_ContentBounds.size = contentSize;
            m_ContentBounds.center = contentPos;
        }

        /// <summary>
        /// 弹簧模式的tick
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="deltaTime"></param>
        protected void TickElastic(Vector2 offset, float deltaTime)
        {
            Vector2 position = m_Content.anchoredPosition;

            // 弹簧模式如果运动到无offset,则进入移动结束状态
            if (IsVector2Equal(offset, Vector2.zero))
            {
                OnMovementEnd();
                return;
            }

            // 重新计算速度值
            for (int axis = 0; axis < 2; axis++)
            {
                // Apply spring physics if movement is elastic and content has an offset from the view.

                float speed = m_Velocity[axis];
                position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis], m_Content.anchoredPosition[axis] + offset[axis],
                    ref speed, m_Elasticity, Mathf.Infinity, deltaTime);
                m_Velocity[axis] = speed;
                //Debug.LogError("1 m_Velocity[1] = " + m_Velocity[1].ToString());
                
            }

            // 速度不为0，则移动Context
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
        /// 减速模式的tick
        /// </summary>
        /// <param name="deltaTime"></param>
        protected virtual void TickInertia(float deltaTime)
        {
            // donothing
        }

        protected virtual void TickDocking(Vector2 offset, float deltaTime)
        {
            // donothing
        }

        /// <summary>
        /// 设置当前拖动状态
        /// </summary>
        /// <param name="state"></param>
        protected void SetState(ScrollRectState state)
        {
            if (m_currState == state)
            {
                return;
            }

            // 拖动中时，恢复mask
            m_currState = state;
            switch (m_currState)
            {
                case ScrollRectState.Init:
                    SetMaskEnable(true);
                    break;
                case ScrollRectState.Draging:
                    SetMaskEnable(true);
                    break;
                case ScrollRectState.Docked:
                    SetMaskEnable(false);
                    break;
               
            }
        }

        /// <summary>
        /// 设置mask是否可用
        /// </summary>
        /// <param name="isenable"></param>
        protected void SetMaskEnable(bool isenable)
        {
            StartCoroutine(SetMaskEnableImp(isenable));
        }

        /// <summary>
        /// 设置mask是否可用(协程)
        /// </summary>
        /// <param name="isenable"></param>
        protected IEnumerator SetMaskEnableImp(bool isenable)
        {
            var mask = Mask;

            // 如果是设置为可用，则立刻起效
            if (isenable)
            {
                if (mask != null)
                {
                    mask.enabled = isenable;
                }

                yield break;
            }

            // 如果是设置为不可见，则延迟一帧起效
            yield return null;

            if (mask != null)
            {
                mask.enabled = isenable;
            }
        }

        /// <summary>
        /// 判断Vector2是否相等
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        protected bool IsVector2Equal(Vector2 src, Vector2 dest)
        {
            var xoffset = src.x - dest.x;
            var yoffset = src.y - dest.y;
            var res = (xoffset <= 0.01f && xoffset >= -0.01f) && (yoffset <= 0.01f && yoffset >= -0.01f);
            return res;
        }

        /// <summary>
        /// 移动结束时的处理
        /// </summary>
        protected virtual void OnMovementEnd()
        {
            // donothing
        }

        protected virtual float GetItemSize()
        {
            RectTransform rect = m_Content.GetChild(0) as RectTransform;
            if (rect != null)
            {
                return GetSize(rect);
            }

            return 0;
        }


        public enum ScrollRectState
        {
            Init,      // 初始化
            Draging,   // 拖动中
            Elastic,   // 弹簧运动中
            Inertia,   // 摩擦减速中
            Docking,   // 停靠中
            Docked,    // 停靠完毕
        }

        protected Mask m_mask;
        protected Mask Mask
        {
            get
            {
                if (m_mask == null)
                {
                    m_mask = GetComponentInChildren<Mask>(true);
                }

                return m_mask;
            }
        }

        protected ScrollRectState m_currState = ScrollRectState.Init;

        protected const float VelocitySplitBetweenInertiaAndDocking = 30f;
    }
}
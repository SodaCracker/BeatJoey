using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Loop Scroll Rect", 16)]
    [SelectionBase]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class LoopScrollRect : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
    {
        [HideInInspector]
        public Func<string, int> m_functionGetItemIndex = null;

        //==========LoopScrollRect==========
        [HideInInspector]
        public MarchingBytes.EasyObjectPool prefabPool;
        [HideInInspector]
        public string prefabPoolName;
        [HideInInspector]
        public int totalCount;  //negative means INFINITE mode
        [HideInInspector]
        public bool initInStart = false;
        [HideInInspector]
        public float threshold = 100;
        [HideInInspector]
        public bool reverseDirection = false;

        protected int itemTypeStart = 0;
        protected int itemTypeEnd = 0;

        protected abstract float GetSize(RectTransform item);
        protected abstract float GetDimension(Vector2 vector);
        protected abstract Vector2 GetVector(float value);
        protected int directionSign = 0;

        protected float m_ContentSpacing = -1;
        protected float contentSpacing
        {
            get
            {
                if (m_ContentSpacing >= 0)
                {
                    return m_ContentSpacing;
                }
                m_ContentSpacing = 0;
                if (content != null)
                {
                    HorizontalOrVerticalLayoutGroup layout1 = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
                    if (layout1 != null)
                    {
                        m_ContentSpacing = layout1.spacing;
                    }
                    else
                    {
                        GridLayoutGroup layout2 = content.GetComponent<GridLayoutGroup>();
                        if (layout2 != null)
                        {
                            m_ContentSpacing = GetDimension(layout2.spacing);
                        }
                    }
                }
                return m_ContentSpacing;
            }
        }

        protected int m_ContentConstraintCount = 0;
        protected int contentConstraintCount
        {
            get
            {
                if (m_ContentConstraintCount > 0)
                {
                    return m_ContentConstraintCount;
                }
                m_ContentConstraintCount = 1;
                if (content != null)
                {
                    GridLayoutGroup layout2 = content.GetComponent<GridLayoutGroup>();
                    if (layout2 != null)
                    {
                        if (layout2.constraint == GridLayoutGroup.Constraint.Flexible)
                        {
                            Debug.LogWarning("[LoopScrollRect] Flexible not supported yet");
                        }
                        m_ContentConstraintCount = layout2.constraintCount;
                    }
                }
                return m_ContentConstraintCount;
            }
        }

        protected virtual bool UpdateItems(Bounds viewBounds, Bounds contentBounds) { return false; }
        //==========LoopScrollRect==========

        public enum MovementType
        {
            Unrestricted, // Unrestricted movement -- can scroll forever
            Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
            Clamped, // Restricted movement where it's not possible to go past the edges
        }

        public enum ScrollbarVisibility
        {
            Permanent,
            AutoHide,
            AutoHideAndExpandViewport,
        }

        [Serializable]
        public class ScrollRectEvent : UnityEvent<Vector2> { }

        [SerializeField]
        protected RectTransform m_Content;
        public RectTransform content { get { return m_Content; }
            set {
                m_Content = value;
            } }

        [SerializeField]
        protected bool m_Horizontal = true;
        public bool horizontal { get { return m_Horizontal; } set { m_Horizontal = value; } }

        [SerializeField]
        protected bool m_Vertical = true;
        public bool vertical { get { return m_Vertical; } set { m_Vertical = value; } }

        [SerializeField]
        protected MovementType m_MovementType = MovementType.Elastic;
        public MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }

        [SerializeField]
        protected float m_Elasticity = 0.1f; // Only used for MovementType.Elastic
        public float elasticity { get { return m_Elasticity; } set { m_Elasticity = value; } }

        [SerializeField]
        protected bool m_Inertia = true;
        public bool inertia { get { return m_Inertia; } set { m_Inertia = value; } }

        [SerializeField]
        protected float m_DecelerationRate = 0.135f; // Only used when inertia is enabled
        public float decelerationRate { get { return m_DecelerationRate; } set { m_DecelerationRate = value; } }

        [SerializeField]
        protected float m_ScrollSensitivity = 1.0f;
        public float scrollSensitivity { get { return m_ScrollSensitivity; } set { m_ScrollSensitivity = value; } }

        [SerializeField]
        protected RectTransform m_Viewport;
        public RectTransform viewport { get { return m_Viewport; } set { m_Viewport = value; SetDirtyCaching(); } }

        [SerializeField]
        protected Scrollbar m_HorizontalScrollbar;
        public Scrollbar horizontalScrollbar
        {
            get
            {
                return m_HorizontalScrollbar;
            }
            set
            {
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                m_HorizontalScrollbar = value;
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                SetDirtyCaching();
            }
        }

        [SerializeField]
        protected Scrollbar m_VerticalScrollbar;
        public Scrollbar verticalScrollbar
        {
            get
            {
                return m_VerticalScrollbar;
            }
            set
            {
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                m_VerticalScrollbar = value;
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                SetDirtyCaching();
            }
        }

        [SerializeField]
        protected ScrollbarVisibility m_HorizontalScrollbarVisibility;
        public ScrollbarVisibility horizontalScrollbarVisibility { get { return m_HorizontalScrollbarVisibility; } set { m_HorizontalScrollbarVisibility = value; SetDirtyCaching(); } }

        [SerializeField]
        protected ScrollbarVisibility m_VerticalScrollbarVisibility;
        public ScrollbarVisibility verticalScrollbarVisibility { get { return m_VerticalScrollbarVisibility; } set { m_VerticalScrollbarVisibility = value; SetDirtyCaching(); } }

        [SerializeField]
        protected float m_HorizontalScrollbarSpacing;
        public float horizontalScrollbarSpacing { get { return m_HorizontalScrollbarSpacing; } set { m_HorizontalScrollbarSpacing = value; SetDirty(); } }

        [SerializeField]
        protected float m_VerticalScrollbarSpacing;
        public float verticalScrollbarSpacing { get { return m_VerticalScrollbarSpacing; } set { m_VerticalScrollbarSpacing = value; SetDirty(); } }

        [SerializeField]
        protected ScrollRectEvent m_OnValueChanged = new ScrollRectEvent();
        public ScrollRectEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        // The offset from handle position to mouse down position
        protected Vector2 m_PointerStartLocalCursor = Vector2.zero;
        protected Vector2 m_ContentStartPosition = Vector2.zero;

        protected RectTransform m_ViewRect;

        protected RectTransform viewRect
        {
            get
            {
                if (m_ViewRect == null)
                    m_ViewRect = m_Viewport;
                if (m_ViewRect == null)
                    m_ViewRect = (RectTransform)transform;
                return m_ViewRect;
            }
        }

        protected Bounds m_ContentBounds;
        protected Bounds m_ViewBounds;

        protected Vector2 m_Velocity;
        public Vector2 velocity { get { return m_Velocity; } set { m_Velocity = value; } }

        protected bool m_Dragging;

        protected Vector2 m_PrevPosition = Vector2.zero;
        protected Bounds m_PrevContentBounds;
        protected Bounds m_PrevViewBounds;
        [NonSerialized]
        protected bool m_HasRebuiltLayout = false;

        protected bool m_HSliderExpand;
        protected bool m_VSliderExpand;
        protected float m_HSliderHeight;
        protected float m_VSliderWidth;

        [System.NonSerialized]
        protected RectTransform m_Rect;
        protected RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        protected RectTransform m_HorizontalScrollbarRect;
        protected RectTransform m_VerticalScrollbarRect;

        protected DrivenRectTransformTracker m_Tracker;


        protected LoopScrollRect()
        {
            flexibleWidth = -1;
        }

        //==========LoopScrollRect==========
        protected override void Start()
        {
            base.Start();
            //if (initInStart)
            //{
            //    RefillCells();
            //}
        }

        public void ClearCells()
        {
            if (Application.isPlaying)
            {
                itemTypeStart = 0;
                itemTypeEnd = 0;
                totalCount = 0;
                for (int i = GetContentValidItemCount() - 1; i >= 0; i--)
                {
                    prefabPool.ReturnObjectToPool(content.GetChild(i).gameObject);
                }
            }
        }

        public bool RefillCells(string itemName)
        {
            if (m_functionGetItemIndex == null)
            {
                Debug.LogError("LoopScrollRect.RefillCells m_functionGetItemIndex is null.");
                return false;
            }
            int idx = m_functionGetItemIndex(itemName);
            if (idx < 0)
            {
                Debug.LogError(string.Format("LoopScrollRect.RefillCells itemName {0} dose not exist.", itemName));
                return false;
            }
            RefillCells(idx);
            return true;
        }

        public virtual void RefillCells(int startIdx = 0)
        {
            if (Application.isPlaying)
            {
                StopMovement();
                itemTypeStart = reverseDirection ? totalCount - startIdx : startIdx;
                itemTypeEnd = itemTypeStart;

                Canvas.ForceUpdateCanvases();

                // recycle items if we can
                for (int i = 0; i < GetContentValidItemCount(); i++)
                {
                    if (totalCount >= 0 && itemTypeEnd >= totalCount)
                    {
                        var child = content.GetChild(i);
                        child.SetAsLastSibling();
                        prefabPool.ReturnObjectToPool(child.gameObject);
                        i--;
                    }
                    else
                    {
                        var child = content.GetChild(i);
                        //child.SendMessage("ScrollCellIndex", itemTypeEnd, SendMessageOptions.DontRequireReceiver);
                        var poolObj = child.GetComponent<MarchingBytes.PoolObject>();
                        poolObj.ScrollCellIndex(itemTypeEnd);
                        itemTypeEnd++;
                    }
                }


                if (GetContentValidItemCount() > 0)
                {
                    Canvas.ForceUpdateCanvases();
                    Vector2 pos = content.anchoredPosition;
                    if (directionSign == -1)
                        pos.y = 0;
                    else if (directionSign == 1)
                        pos.x = 0;
                    content.anchoredPosition = pos;
                    UpdateBounds();
                }
            }
        }


        /// <summary>
        /// 返回Context下激活的go列表
        /// </summary>
        /// <returns></returns>
        public List<GameObject> GetActiveGameObjectListInContext()
        {
            List<GameObject> goList = new List<GameObject>();

            if (m_Content != null)
            {
                GameObject go;
                for (int i = 0; i < m_Content.childCount; ++i)
                {
                    go = m_Content.GetChild(i).gameObject;
                    if (go.activeSelf)
                    {
                        goList.Add(go);
                    }

                }
            }

            return goList;
        }

        /// <summary>
        /// 返回Context下所有的go列表
        /// </summary>
        /// <returns></returns>
        public List<GameObject> GetAllGameObjectListInContext()
        {
            List<GameObject> goList = new List<GameObject>();

            if (m_Content != null)
            {
                GameObject go;
                for (int i = 0; i < m_Content.childCount; ++i)
                {
                    go = m_Content.GetChild(i).gameObject;
                    goList.Add(go);
                }
            }

            return goList;
        }


        protected readonly Vector3[] m_ViewCorners = new Vector3[4];
        /// <summary>
        /// 获取在viewRect内的第一个物品
        /// </summary>
        /// <returns></returns>
        public GameObject GetFirstGameObjectInViewRect()
        {
            var items = GetActiveGameObjectListInContext();
            viewRect.GetWorldCorners(m_ViewCorners);
            var viewSize = new Vector2(m_ViewCorners[3].x - m_ViewCorners[0].x, m_ViewCorners[1].y - m_ViewCorners[0].y);
            Rect viewCheckRect = new Rect(m_ViewCorners[0], viewSize);
            foreach (var item in items)
            {
                // 得到Item下边缘点的坐标
                var checkConrners = new Vector3[4];
                item.GetComponent<RectTransform>().GetWorldCorners(checkConrners);
                var itemCheckPoint = new Vector2(item.transform.position.x, checkConrners[0].y);

                if (viewCheckRect.Contains(itemCheckPoint))
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取在viewRect内的第一个物品(计算物品中心点)
        /// </summary>
        /// <returns></returns>
        public GameObject GetFirstGameObjectCornerInViewRect()
        {
            var items = GetActiveGameObjectListInContext();
            viewRect.GetWorldCorners(m_ViewCorners);
            var viewSize = new Vector2(m_ViewCorners[3].x - m_ViewCorners[0].x, m_ViewCorners[1].y - m_ViewCorners[0].y);
            Rect viewCheckRect = new Rect(m_ViewCorners[0], viewSize);
            foreach (var item in items)
            {
                // 得到Item中心点的坐标
                var checkConrners = new Vector3[4];
                item.GetComponent<RectTransform>().GetWorldCorners(checkConrners);
                var itemCheckPoint = new Vector2(item.transform.position.x, item.transform.position.y);

                if (viewCheckRect.Contains(itemCheckPoint))
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取在viewRect内的最后一个可见的物体
        /// </summary>
        /// <returns></returns>
        public GameObject GetLastGameObjectInViewRect()
        {
            var currActiveItemList = GetActiveGameObjectListInContext();
            viewRect.GetWorldCorners(m_ViewCorners);
            var viewSize = new Vector2(m_ViewCorners[3].x - m_ViewCorners[0].x, m_ViewCorners[1].y - m_ViewCorners[0].y);
            Rect viewCheckRect = new Rect(m_ViewCorners[0], viewSize);

            // 遍历中上一个可见Item
            GameObject preInViewItem = null;
            // 遍历中当前的可见Item
            GameObject currInViewRectItem = null;

            // 是否找到了第一个可见的Item
            bool isFindFirstItemInViewRect = false;
            // 是否找到可见区域之后的第一个不可见Item
            bool isFindItemNotInViewRect = false;

            foreach (var activeItem in currActiveItemList)
            {
                // 得到Item上边缘点的坐标
                var checkConrners = new Vector3[4];
                activeItem.GetComponent<RectTransform>().GetWorldCorners(checkConrners);
                var itemCheckPoint = new Vector2(activeItem.transform.position.x, checkConrners[1].y);

                // 如果没有找到可见的item，则一直寻找
                if (!isFindFirstItemInViewRect)
                {
                    // 找到第一个可见item
                    if (viewCheckRect.Contains(itemCheckPoint))
                    {
                        currInViewRectItem = activeItem;
                        isFindFirstItemInViewRect = true;
                    }
                }
                else
                {
                    preInViewItem = currInViewRectItem;
                    currInViewRectItem = activeItem;

                    // 找到了第一个可见物体，找到之后的第一个不可见物体
                    if (!viewCheckRect.Contains(itemCheckPoint))
                    {
                        // 进入这里代表找到了之后第一个不可见的，此时preInViewItem就是最后一个可见的
                        isFindItemNotInViewRect = true;
                        break;
                    }
                }
            }

            if (!isFindFirstItemInViewRect)
            {
                // 没有找到一个可见物体，返回null
                return null;
            }

            if (isFindItemNotInViewRect)
            {
                // 如果找到可见区域之后的第一个不可见Item，那么上一个Item为不可见
                return preInViewItem;
            }

            // 如果没有找到可见区域之后的第一个不可见Item，那么当前项就是最后一个可见Item
            return currInViewRectItem;
        }

        protected float NewItemAtStart()
        {
            if (totalCount >= 0 && itemTypeStart - contentConstraintCount < 0)
            {
                return 0;
            }

            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                itemTypeStart--;
                RectTransform newItem = InstantiateNextItem(itemTypeStart);
                newItem.SetAsFirstSibling();
                size = Mathf.Max(GetSize(newItem), size);
                //Debug.LogError("NewItemAtStart index = " + itemTypeStart.ToString());
            }

            if (!reverseDirection)
            {
                Vector2 offset = GetVector(size);
                content.anchoredPosition += offset;
                m_PrevPosition += offset;
                m_ContentStartPosition += offset;
            }


            return size;
        }

        protected float DeleteItemAtStart()
        {
            if ((totalCount >= 0 && itemTypeEnd >= totalCount - 1) || GetContentValidItemCount() == 0)
            {
                return 0;
            }

            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldItem = content.GetChild(0) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);
                prefabPool.ReturnObjectToPool(oldItem.gameObject);
                oldItem.SetAsLastSibling();

                itemTypeStart++;

                //Debug.LogError("DeleteItemAtStart index = " + itemTypeStart.ToString());
                if (content.childCount == 0)
                {
                    break;
                }
            }

            if (!reverseDirection)
            {
                Vector2 offset = GetVector(size);
                content.anchoredPosition -= offset;
                m_PrevPosition -= offset;
                m_ContentStartPosition -= offset;
            }


            return size;
        }

        protected float RecycleItemAtStart()
        {
            if ((totalCount >= 0 && itemTypeEnd >= totalCount - 1) || GetContentValidItemCount() == 0)
            {
                return 0;
            }

            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldItem = content.GetChild(i) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);
                m_recycleList.Push(oldItem);

                itemTypeStart++;

                if (content.childCount == 0)
                {
                    break;
                }
            }

            if (!reverseDirection)
            {
                Vector2 offset = GetVector(size);
                content.anchoredPosition -= offset;
                m_PrevPosition -= offset;
                m_ContentStartPosition -= offset;
            }

            if (totalCount >= 0 && itemTypeEnd >= totalCount)
            {
                ClearRecycleList();
                return size;
            }

            float fillnumber = contentConstraintCount - itemTypeEnd % contentConstraintCount;
            if (fillnumber == contentConstraintCount)
            {
                fillnumber = 0;
            }
            float newCount = contentConstraintCount + fillnumber;

            for (int i = 0; i < newCount; i++)
            {
                RectTransform newItem = null;
                if (0 < m_recycleList.Count)
                {
                    newItem = m_recycleList.Pop();
                    var poolObj = newItem.GetComponent<MarchingBytes.PoolObject>();
                    poolObj.ScrollCellIndex(itemTypeEnd);
                }
                else
                    newItem = InstantiateNextItem(itemTypeEnd);

                newItem.SetSiblingIndex(GetContentValidItemCount() - 1);
                size = Mathf.Max(GetSize(newItem), size);
                itemTypeEnd++;

                if (totalCount >= 0 && itemTypeEnd >= totalCount)
                {
                    break;
                }
            }

            if (reverseDirection)
            {
                Vector2 offset = GetVector(size);
                content.anchoredPosition -= offset;
                m_PrevPosition -= offset;
                m_ContentStartPosition -= offset;
            }

            ClearRecycleList();
            return size;
        }


        protected float NewItemAtEnd()
        {
            if (totalCount >= 0 && itemTypeEnd >= totalCount)
            {
                return 0;
            }

            float size = 0;

            float fillnumber = contentConstraintCount - itemTypeEnd % contentConstraintCount;
            if (fillnumber == contentConstraintCount)
            {
                fillnumber = 0;
            }
            float newCount = contentConstraintCount + fillnumber;

            for (int i = 0; i < newCount; i++)
            {
                RectTransform newItem = InstantiateNextItem(itemTypeEnd);
                newItem.SetSiblingIndex(GetContentValidItemCount()-1);
                size = Mathf.Max(GetSize(newItem), size);
                itemTypeEnd++;

                //Debug.LogError("NewItemAtEnd index = " + itemTypeEnd.ToString());
                if (totalCount >= 0 && itemTypeEnd >= totalCount)
                {
                    break;
                }
            }

            if (reverseDirection)
            {
                Vector2 offset = GetVector(size);
                content.anchoredPosition -= offset;
                m_PrevPosition -= offset;
                m_ContentStartPosition -= offset;
            }


            return size;
        }

        int GetContentValidItemCount()
        {
            int count = 0;
            for(int i=0;i<content.childCount;i++)
            {
                var child = content.GetChild(i);
                if (child.gameObject.activeSelf)
                    count++;
            }

            return count;
        }

        protected float DeleteItemAtEnd()
        {                                                                       
            if ((totalCount >= 0 && itemTypeStart < contentConstraintCount) || GetContentValidItemCount() == 0)
            {
                return 0;
            }

            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldItem = content.GetChild(GetContentValidItemCount() - 1) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);
                oldItem.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
                prefabPool.ReturnObjectToPool(oldItem.gameObject);

                itemTypeEnd--;

                //Debug.LogError("DeleteItemAtEnd index = " + itemTypeEnd.ToString());
                if (itemTypeEnd % contentConstraintCount == 0 || GetContentValidItemCount() == 0)
                {
                    break;  //just delete the whole row
                }
            }

            if (reverseDirection)
            {
                Vector2 offset = GetVector(size);
                content.anchoredPosition += offset;
                m_PrevPosition += offset;
                m_ContentStartPosition += offset;
            }


            return size;
        }

        Stack<RectTransform> m_recycleList = new Stack<RectTransform>();
        void ClearRecycleList()
        {
            foreach (var item in m_recycleList)
            {
                prefabPool.ReturnObjectToPool(item.gameObject);
                item.SetAsLastSibling();
            }
            m_recycleList.Clear();
        }

        protected float RecycleItemAtEnd()
        {
            if ((totalCount >= 0 && itemTypeStart < contentConstraintCount) || GetContentValidItemCount() == 0)
            {
                return 0;
            }

            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldItem = content.GetChild(GetContentValidItemCount() - 1 -i) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);
                oldItem.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
                m_recycleList.Push(oldItem);

                itemTypeEnd--;

                //Debug.LogError("DeleteItemAtEnd index = " + itemTypeEnd.ToString());
                if (itemTypeEnd % contentConstraintCount == 0 || GetContentValidItemCount() == 0)
                {
                    break;  //just delete the whole row
                }
            }

            if (reverseDirection)
            {
                Vector2 offset = GetVector(size);
                content.anchoredPosition += offset;
                m_PrevPosition += offset;
                m_ContentStartPosition += offset;
            }


            if (totalCount >= 0 && itemTypeStart - contentConstraintCount < 0)
            {
                ClearRecycleList();
                return size;
            }
            //float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                itemTypeStart--;
                RectTransform newItem = null;
                if (0 < m_recycleList.Count)
                {
                    newItem = m_recycleList.Pop();
                    var poolObj = newItem.GetComponent<MarchingBytes.PoolObject>();
                    poolObj.ScrollCellIndex(itemTypeStart);
                }
                else
                    newItem = InstantiateNextItem(itemTypeStart);
                newItem.SetAsFirstSibling();
                size = Mathf.Max(GetSize(newItem), size);
                //Debug.LogError("NewItemAtStart index = " + itemTypeStart.ToString());
            }

            if (!reverseDirection)
            {
                Vector2 offset = GetVector(size);
                content.anchoredPosition += offset;
                m_PrevPosition += offset;
                m_ContentStartPosition += offset;
            }

            ClearRecycleList();
            return size;
        }

        protected RectTransform InstantiateNextItem(int itemIdx)
        {
            RectTransform nextItem = prefabPool.GetObjectFromPool(prefabPoolName).transform as RectTransform;
            nextItem.transform.SetParent(content, false);
            nextItem.gameObject.SetActive(true);

            var poolObj = nextItem.GetComponent<MarchingBytes.PoolObject>();
            poolObj.ScrollCellIndex(itemIdx);
            //nextItem.SendMessage("ScrollCellIndex", itemIdx);
            return nextItem;
        }
        //==========LoopScrollRect==========

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }

            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds();
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();

                m_HasRebuiltLayout = true;
            }
        }

        public virtual void LayoutComplete()
        { }

        public virtual void GraphicUpdateComplete()
        { }

        void UpdateCachedData()
        {
            Transform transform = this.transform;
            m_HorizontalScrollbarRect = m_HorizontalScrollbar == null ? null : m_HorizontalScrollbar.transform as RectTransform;
            m_VerticalScrollbarRect = m_VerticalScrollbar == null ? null : m_VerticalScrollbar.transform as RectTransform;

            // These are true if either the elements are children, or they don't exist at all.
            bool viewIsChild = (viewRect.parent == transform);
            bool hScrollbarIsChild = (!m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform);
            bool vScrollbarIsChild = (!m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform);
            bool allAreChildren = (viewIsChild && hScrollbarIsChild && vScrollbarIsChild);

            m_HSliderExpand = allAreChildren && m_HorizontalScrollbarRect && horizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_VSliderExpand = allAreChildren && m_VerticalScrollbarRect && verticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_HSliderHeight = (m_HorizontalScrollbarRect == null ? 0 : m_HorizontalScrollbarRect.rect.height);
            m_VSliderWidth = (m_VerticalScrollbarRect == null ? 0 : m_VerticalScrollbarRect.rect.width);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

            m_HasRebuiltLayout = false;
            m_Tracker.Clear();
            m_Velocity = Vector2.zero;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        public override bool IsActive()
        {
            return base.IsActive() && m_Content != null;
        }

        protected void EnsureLayoutHasRebuilt()
        {
            if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

        public virtual void StopMovement()
        {
            m_Velocity = Vector2.zero;
        }

        public virtual void OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (vertical && !horizontal)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    delta.y = delta.x;
                delta.x = 0;
            }
            if (horizontal && !vertical)
            {
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                    delta.x = delta.y;
                delta.y = 0;
            }

            Vector2 position = m_Content.anchoredPosition;
            position += delta * m_ScrollSensitivity;
            if (m_MovementType == MovementType.Clamped)
                position += CalculateOffset(position - m_Content.anchoredPosition);

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            m_Velocity = Vector2.zero;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            UpdateBounds();

            m_PointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
            m_ContentStartPosition = m_Content.anchoredPosition;
            m_Dragging = true;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            m_Dragging = false;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            UpdateBounds();

            var pointerDelta = localCursor - m_PointerStartLocalCursor;
            Vector2 position = m_ContentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
            position += offset;
            if (m_MovementType == MovementType.Elastic)
            {
                if (offset.x != 0)
                    position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x);
                if (offset.y != 0)
                    position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y);
            }

            SetContentAnchoredPosition(position);
        }

        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            if (!m_Horizontal)
                position.x = m_Content.anchoredPosition.x;
            if (!m_Vertical)
                position.y = m_Content.anchoredPosition.y;

            if (position != m_Content.anchoredPosition)
            {
                m_Content.anchoredPosition = position;
                UpdateBounds();
            }
        }


        /// <summary>
        /// 判断float是否相等
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        protected bool IsFloatEqual(float src, float dest)
        {
            var xoffset = src - dest;

            var res = (xoffset <= 0.01f && xoffset >= -0.01f);
            return res;
        }

        protected virtual void LateUpdate()
        {
            if (!m_Content)
                return;

            EnsureLayoutHasRebuilt();
            UpdateScrollbarVisibility();
            UpdateBounds();
            float deltaTime = Time.unscaledDeltaTime;
            Vector2 offset = CalculateOffset(Vector2.zero);
            if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero))
            {
                Vector2 position = m_Content.anchoredPosition;
                for (int axis = 0; axis < 2; axis++)
                {
                    // Apply spring physics if movement is elastic and content has an offset from the view.
                    //if (m_MovementType == MovementType.Elastic && (offset[axis] != 0))
                    if (m_MovementType == MovementType.Elastic && !IsFloatEqual(offset[axis], 0))
                    {
                        float speed = m_Velocity[axis];
                        position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis], m_Content.anchoredPosition[axis] + offset[axis], ref speed, m_Elasticity, Mathf.Infinity, deltaTime);
                        m_Velocity[axis] = speed;
                    }
                    // Else move content according to velocity with deceleration applied.
                    else if (m_Inertia)
                    {
                        m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);
                        if (Mathf.Abs(m_Velocity[axis]) < 1)
                            m_Velocity[axis] = 0;
                        position[axis] += m_Velocity[axis] * deltaTime;
                    }
                    // If we have neither elaticity or friction, there shouldn't be any velocity.
                    else
                    {
                        m_Velocity[axis] = 0;
                    }
                }

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

        protected void UpdatePrevData()
        {
            if (m_Content == null)
                m_PrevPosition = Vector2.zero;
            else
                m_PrevPosition = m_Content.anchoredPosition;
            m_PrevViewBounds = m_ViewBounds;
            m_PrevContentBounds = m_ContentBounds;
        }

        protected void UpdateScrollbars(Vector2 offset)
        {
            if (m_HorizontalScrollbar)
            {
                if (m_ContentBounds.size.x > 0)
                    m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / m_ContentBounds.size.x);
                else
                    m_HorizontalScrollbar.size = 1;

                m_HorizontalScrollbar.value = horizontalNormalizedPosition;
            }

            if (m_VerticalScrollbar)
            {
                if (m_ContentBounds.size.y > 0)
                    m_VerticalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / m_ContentBounds.size.y);
                else
                    m_VerticalScrollbar.size = 1;

                m_VerticalScrollbar.value = verticalNormalizedPosition;
            }
        }

        public Vector2 normalizedPosition
        {
            get
            {
                return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition);
            }
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        public int StartItemIndex { get { return itemTypeStart; } }
        public int EndItemIndex { get { return itemTypeEnd; } }

        public float horizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (m_ContentBounds.size.x <= m_ViewBounds.size.x)
                    return (m_ViewBounds.min.x > m_ContentBounds.min.x) ? 1 : 0;
                return (m_ViewBounds.min.x - m_ContentBounds.min.x) / (m_ContentBounds.size.x - m_ViewBounds.size.x);
            }
            set
            {
                SetNormalizedPosition(value, 0);
            }
        }

        public float verticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if (m_ContentBounds.size.y <= m_ViewBounds.size.y)
                    return (m_ViewBounds.min.y > m_ContentBounds.min.y) ? 1 : 0;
                ;
                return (m_ViewBounds.min.y - m_ContentBounds.min.y) / (m_ContentBounds.size.y - m_ViewBounds.size.y);
            }
            set
            {
                SetNormalizedPosition(value, 1);
            }
        }

        protected void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
        protected void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }

        protected void SetNormalizedPosition(float value, int axis)
        {
            EnsureLayoutHasRebuilt();
            UpdateBounds();
            // How much the content is larger than the view.
            float hiddenLength = m_ContentBounds.size[axis] - m_ViewBounds.size[axis];
            // Where the position of the lower left corner of the content bounds should be, in the space of the view.
            float contentBoundsMinPosition = m_ViewBounds.min[axis] - value * hiddenLength;
            // The new content localPosition, in the space of the view.
            float newLocalPosition = m_Content.localPosition[axis] + contentBoundsMinPosition - m_ContentBounds.min[axis];

            Vector3 localPosition = m_Content.localPosition;
            if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
            {
                localPosition[axis] = newLocalPosition;
                m_Content.localPosition = localPosition;
                m_Velocity[axis] = 0;
                UpdateBounds();
            }
        }

        protected static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        protected bool hScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.x > m_ViewBounds.size.x + 0.01f;
                return true;
            }
        }
        protected bool vScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.y > m_ViewBounds.size.y + 0.01f;
                return true;
            }
        }

        public virtual void CalculateLayoutInputHorizontal() { }
        public virtual void CalculateLayoutInputVertical() { }

        public virtual float minWidth { get { return -1; } }
        public virtual float preferredWidth { get { return -1; } }
        public virtual float flexibleWidth { get; private set; }

        public virtual float minHeight { get { return -1; } }
        public virtual float preferredHeight { get { return -1; } }
        public virtual float flexibleHeight { get { return -1; } }

        public virtual int layoutPriority { get { return -1; } }

        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();

            if (m_HSliderExpand || m_VSliderExpand)
            {
                m_Tracker.Add(this, viewRect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition);

                // Make view full size to see if content fits.
                viewRect.anchorMin = Vector2.zero;
                viewRect.anchorMax = Vector2.one;
                viewRect.sizeDelta = Vector2.zero;
                viewRect.anchoredPosition = Vector2.zero;

                // Recalculate content layout with this size to see if it fits when there are no scrollbars.
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);

                // Recalculate content layout with this size to see if it fits vertically
                // when there is a vertical scrollbar (which may reflowed the content to make it taller).
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
            if (m_HSliderExpand && hScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(viewRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If the vertical slider didn't kick in the first time, and the horizontal one did,
            // we need to check again if the vertical slider now needs to kick in.
            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded && viewRect.sizeDelta.x == 0 && viewRect.sizeDelta.y < 0)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);
            }
        }

        public virtual void SetLayoutVertical()
        {
            UpdateScrollbarLayout();
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();
        }

        protected void UpdateScrollbarVisibility()
        {
            if (m_VerticalScrollbar && m_VerticalScrollbarVisibility != ScrollbarVisibility.Permanent && m_VerticalScrollbar.gameObject.activeSelf != vScrollingNeeded)
                m_VerticalScrollbar.gameObject.SetActive(vScrollingNeeded);

            if (m_HorizontalScrollbar && m_HorizontalScrollbarVisibility != ScrollbarVisibility.Permanent && m_HorizontalScrollbar.gameObject.activeSelf != hScrollingNeeded)
                m_HorizontalScrollbar.gameObject.SetActive(hScrollingNeeded);
        }

        protected void UpdateScrollbarLayout()
        {
            if (m_VSliderExpand && m_HorizontalScrollbar)
            {
                m_Tracker.Add(this, m_HorizontalScrollbarRect,
                              DrivenTransformProperties.AnchorMinX |
                              DrivenTransformProperties.AnchorMaxX |
                              DrivenTransformProperties.SizeDeltaX |
                              DrivenTransformProperties.AnchoredPositionX);
                m_HorizontalScrollbarRect.anchorMin = new Vector2(0, m_HorizontalScrollbarRect.anchorMin.y);
                m_HorizontalScrollbarRect.anchorMax = new Vector2(1, m_HorizontalScrollbarRect.anchorMax.y);
                m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0, m_HorizontalScrollbarRect.anchoredPosition.y);
                if (vScrollingNeeded)
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), m_HorizontalScrollbarRect.sizeDelta.y);
                else
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(0, m_HorizontalScrollbarRect.sizeDelta.y);
            }

            if (m_HSliderExpand && m_VerticalScrollbar)
            {
                m_Tracker.Add(this, m_VerticalScrollbarRect,
                              DrivenTransformProperties.AnchorMinY |
                              DrivenTransformProperties.AnchorMaxY |
                              DrivenTransformProperties.SizeDeltaY |
                              DrivenTransformProperties.AnchoredPositionY);
                m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.x, 0);
                m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.x, 1);
                m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0);
                if (hScrollingNeeded)
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                else
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0);
            }
        }

        protected virtual void UpdateBounds()
        {
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();

            if (m_Content == null)
                return;

            // ============LoopScrollRect============
            // Don't do this in Rebuild
            // Canvas在RebuildingLayout时，UpdateItems会报错：Trying to remove {0} from rebuild list while we are already inside a rebuild loop. This is not supported
            if (Application.isPlaying && !CanvasUpdateRegistry.IsRebuildingLayout() && UpdateItems(m_ViewBounds, m_ContentBounds))
            {
                Canvas.ForceUpdateCanvases();
                m_ContentBounds = GetBounds();
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

        protected readonly Vector3[] m_Corners = new Vector3[4];
        protected Bounds GetBounds()
        {
            if (m_Content == null)
                return new Bounds();

            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var toLocal = viewRect.worldToLocalMatrix;
            m_Content.GetWorldCorners(m_Corners);
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(m_Corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        protected virtual Vector2 CalculateOffset(Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
            if (m_MovementType == MovementType.Unrestricted)
                return offset;

            Vector2 min = m_ContentBounds.min;
            Vector2 max = m_ContentBounds.max;

            if (m_Horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;
                if (min.x > m_ViewBounds.min.x)
                    offset.x = m_ViewBounds.min.x - min.x;
                else if (max.x < m_ViewBounds.max.x)
                    offset.x = m_ViewBounds.max.x - max.x;
            }

            if (m_Vertical)
            {
                min.y += delta.y;
                max.y += delta.y;
                if (max.y < m_ViewBounds.max.y)
                    offset.y = m_ViewBounds.max.y - max.y;
                else if (min.y > m_ViewBounds.min.y)
                    offset.y = m_ViewBounds.min.y - min.y;
            }

            return offset;
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected void SetDirtyCaching()
        {
            if (!IsActive())
                return;

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirtyCaching();
        }
#endif
    }
}
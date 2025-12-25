using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Connect.Core
{
    public class GamePlayManager : BaseBehavior
    {
        private WarehouseManager warehouseManager { get; set; }
        [Header("Swipe Settings")]
        [SerializeField] private float minSwipeDistance = 60f;
        [SerializeField] private float axisTolerance = 1.25f;

        private Vector2 _startPos;
        private bool _isSwiping;
        protected override void Start()
        {
            base.Start();
            this.warehouseManager = this.GetComponent<WarehouseManager>();

        }
        void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouse();
#else
        HandleTouch();
#endif
        }
        #region Input Player
        void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _startPos = Input.mousePosition;
                _isSwiping = true;
            }

            if (Input.GetMouseButtonUp(0) && _isSwiping)
            {
                var dir = DetectSwipe(Input.mousePosition);
                _isSwiping = false;

                if (dir.HasValue)
                    OnSwipeDetected(dir.Value);
            }
        }

        void HandleTouch()
        {
            if (Input.touchCount == 0)
                return;

            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                _startPos = touch.position;
                _isSwiping = true;
            }

            if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && _isSwiping)
            {
                var dir = DetectSwipe(touch.position);
                _isSwiping = false;

                if (dir.HasValue)
                    OnSwipeDetected(dir.Value);
            }
        }

        // =======================
        // CORE LOGIC
        // =======================

        MovementType? DetectSwipe(Vector2 endPos)
        {
            Vector2 delta = endPos - _startPos;

            if (delta.magnitude < minSwipeDistance)
                return null;

            float absX = Mathf.Abs(delta.x);
            float absY = Mathf.Abs(delta.y);

            if (absX > absY * axisTolerance)
            {
                return delta.x > 0
                    ? MovementType.Right
                    : MovementType.Left;
            }
            if (absY > absX * axisTolerance)
            {
                return delta.y > 0
                    ? MovementType.Up
                    : MovementType.Down;
            }
            return null;
        }

        void OnSwipeDetected(MovementType dir)
        {
            Debug.Log($"Swipe: {dir}");
            Resolve(
                this.warehouseManager.GetByOccupantTypes<BaseItem>(),
                dir);
        }
#endregion
        /// <summary>
        /// Resolve toàn bộ movement cho 1 swipe
        /// </summary>
        #region RESOLVE MOVEMENT
        public Dictionary<BaseItem, Ground> Resolve(
            List<BaseItem> items,
            MovementType dir)
        {
            var result = new Dictionary<BaseItem, Ground>();

            var ordered = OrderItems(items, dir);

            var occupied = new Dictionary<Vector2Int, BaseItem>();
            foreach (var item in items)
            {
                if (item.WarehouseIndex.HasValue)
                    occupied[item.WarehouseIndex.Value] = item;
            }

            foreach (var item in ordered)
            {
                if (!item.WarehouseIndex.HasValue)
                    continue;

                Vector2Int from = item.WarehouseIndex.Value;
                Vector2Int to = from + dir.GetAngle().AsVector2Int();

                if (!warehouseManager.HasGround(to))
                    continue;

                // gọi IsTraversable của bạn
                if (warehouseManager.IsTraversable(item, to, dir))
                {
                    RegisterMove(item, from, to, occupied, result);
                    continue;
                }

                // thử push chain
                if (TryResolvePush(item, to, dir, occupied, result))
                {
                    RegisterMove(item, from, to, occupied, result);
                }
            }

            return result;
        }

        #region PUSH LOGIC

        private bool TryResolvePush(
            BaseItem mover,
            Vector2Int target,
            MovementType dir,
            Dictionary<Vector2Int, BaseItem> occupied,
            Dictionary<BaseItem, Ground> result)
        {
            if (!occupied.ContainsKey(target))
                return false;

            BaseItem blocker = occupied[target];

            // không cho push chính mình
            if (blocker == mover)
                return false;

            Vector2Int next = target + dir.GetAngle().AsVector2Int();

            if (!this.warehouseManager.HasGround(next))
                return false;

            // nếu ô tiếp theo trống → đẩy
            if (!occupied.ContainsKey(next) &&
                this.warehouseManager.IsTraversable(blocker, next, dir))
            {
                RegisterMove(blocker, target, next, occupied, result);
                return true;
            }

            // nếu còn block → đệ quy push tiếp
            if (occupied.ContainsKey(next))
            {
                if (!TryResolvePush(blocker, next, dir, occupied, result))
                    return false;

                RegisterMove(blocker, target, next, occupied, result);
                return true;
            }

            return false;
        }

        #endregion

        #region APPLY MOVE

        private void RegisterMove(
            BaseItem item,
            Vector2Int from,
            Vector2Int to,
            Dictionary<Vector2Int, BaseItem> occupied,
            Dictionary<BaseItem, Ground> result)
        {
            occupied.Remove(from);
            occupied[to] = item;
            result[item] = warehouseManager.GetGround(to);
        }

        #endregion

        #region SORTING

        private List<BaseItem> OrderItems(
            List<BaseItem> items,
            MovementType dir)
        {
            return dir switch
            {
                MovementType.Right =>
                    items.OrderByDescending(i => i.WarehouseIndex.Value.x).ToList(),

                MovementType.Left =>
                    items.OrderBy(i => i.WarehouseIndex.Value.x).ToList(),

                MovementType.Up =>
                    items.OrderByDescending(i => i.WarehouseIndex.Value.y).ToList(),

                MovementType.Down =>
                    items.OrderBy(i => i.WarehouseIndex.Value.y).ToList(),

                _ => items
            };
        }

        #endregion
        #endregion
    }

}
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace Connect.Core
{

    public abstract class BaseItem : BaseBehavior, IPointerDownHandler, IPointerUpHandler
    {


        public interface IMovementValidation
        {
            bool IsTraversable(Vector2Int position, MovementType movement);
        }

        public interface ITimeProvider
        {
            uint TimeMs { get; }
        }

        public class Event : UnityEvent<BaseItem> { }

        public class MovementHistory : SimpleBaseData<MovementHistory>
        {
            public MovementType movementType { get; set; }
            public uint timeMs { get; set; }
        }

        public static readonly TimeSpan MinimumInputSpan = TimeSpan.FromMilliseconds(300);

        public Event OnMouseDownEvent = new Event();
        public Event OnMouseUpEvent = new Event();
        public Ground.MovementEvent OnStartDestinationEvent = new Ground.MovementEvent();
        public Ground.OccupantEvent OnArriveDestinationEvent = new Ground.OccupantEvent();

        public float movementVelocity = 3.5f;
        public ParticleSystem particles;
        protected ParticleSystemRenderer particlesRenderer;
        public SpriteRenderer spriteRenderer;

        public Ground warehouseDestination { get; set; }
        public Ground warehouseDestinationQueue { get; set; }
        public bool isMoving { get; private set; } = false;
        public MovementType? movementDirection { get; private set; }
        public bool isInputEnabled { get; private set; } = false;
        public BaseItem parent { get; private set; }
        public IMovementValidation validation { get; set; }
        public ITimeProvider timeProvider { get; set; }
        //public WarehouseBuildItemRequest origin { get; set; }

        #region properties
        protected virtual string SpriteName => this.GetType().GetCustomAttribute<SpriteAttribute>().Name;
        protected virtual string Spritesheet { get; } = "Images/spritesheet";
        public virtual ThumbnailAttribute Thumbnail => this.GetType().GetCustomAttribute<ThumbnailAttribute>();
        public virtual string ThumbnailSheet { get; } = "Images/thumbnails";
        protected virtual string SpriteKey { get; }
        protected virtual float SpriteOffsetY { get; } = 0.0f;
        protected virtual float InputHeightOverride { get; } = 0.0f;
        public virtual bool IsPushable { get; } = true;
        public virtual bool IsPassiveOccupant { get; } = false;
        public virtual int PushStrength { get; } = 0;
        public virtual int SortAdjustment { get; } = 0;
        protected virtual string MovementAudio => "";
        protected virtual string ParticleAudio => "";
        //sprite
        private SpriteRenderer _spriteRenderer;
        public SpriteRenderer SpriteRenderer
        {
            get
            {
                if (this._spriteRenderer == null)
                {
                    this._spriteRenderer = this.spriteRenderer;
                    //this.LazyGet(ref this._spriteRenderer);
                    //this.ResolveSprite();
                }
                return this._spriteRenderer;
            }
        }
        //collider
        protected BoxCollider2D inputCollider;// => this.LazyGet(ref this._inputCollider);

        private Vector2Int? _warehouseIndex;
        public virtual Vector2Int? WarehouseIndex
        {
            get
            {
                return this._warehouseIndex;
            }

            set
            {
                this._warehouseIndex = value;
                this.RootTransform.gameObject.name = $"{this.GetType()} {this.WarehouseIndex?.ToString()}";
            }
        }

        public Bounds WorldBounds
        {
            get
            {
                var bounds = this.SpriteRenderer.bounds;
                bounds.center = this.RootTransform.position;
                return bounds;
            }
        }

        public Transform RootTransform
        {
            get
            {
                return this.gameObject.transform;
            }
        }
        #endregion


        public void OnPointerDown(PointerEventData eventData)
        {

        }

        public void OnPointerUp(PointerEventData eventData)
        {

        }

    }

}
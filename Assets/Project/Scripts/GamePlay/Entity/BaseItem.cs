using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        //Animation
        protected Animation animationObject;
        protected AnimationEvents animationEvents;
        private UnityAction<AnimationEvent> animationEndCallback { get; set; }

        private bool isSpriteResolved { get; set; } = false;
        private Rect activeInputBounds { get; set; }

        public Ground warehouseDestination { get; set; }
        public Ground warehouseDestinationQueue { get; set; }
        public bool isMoving { get; private set; } = false;
        public MovementType? movementDirection { get; private set; }
        public bool isInputEnabled { get; private set; } = false;
        public BaseItem parent { get; private set; }
        public IMovementValidation validation { get; set; }
        public ITimeProvider timeProvider { get; set; }
        //public WarehouseBuildItemRequest origin { get; set; }

        private bool isPausedParticles { get; set; } = false;


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
        protected override void Start()
        {
            base.Start();
            this.animationObject = this.gameObject.GetComponentInChildren<Animation>();
            this.animationEvents = this.gameObject.GetComponentInChildren<AnimationEvents>();
            if (this.animationEvents != null)
            {
                animationEvents.OnAnimationStartEvent.AddListener(this.OnAnimationStart);
                animationEvents.OnAnimationEndEvent.AddListener(this.OnAnimationEnd);
            }
            this.inputCollider = this.gameObject.GetComponentInChildren<BoxCollider2D>();
            if (this.inputCollider != null && this.InputHeightOverride != 0.0f)
            {
                var diffY = (this.inputCollider.size.y - this.InputHeightOverride) * 0.5f;
                this.inputCollider.size = this.inputCollider.size.WithY(this.InputHeightOverride);
                this.inputCollider.offset = this.inputCollider.offset.WithY(diffY);
            }
            this.ResolveSprite();
            this.ToggleInput(this.isInputEnabled);

            if (this.particles != null)
            {
                this.particlesRenderer = this.particles.gameObject.GetComponent<ParticleSystemRenderer>();
            }
        }
        protected virtual void Update()
        {
            if (!this.Context.isRunning)
            {
                if (this.particles != null && this.particles.isPlaying)
                {
                    this.isPausedParticles = true;
                    this.particles.Stop();
                }
                return;
            }

            if (this.particles != null && this.isPausedParticles)
            {
                this.isPausedParticles = false;
                this.particles.Play();
            }

            if (this.warehouseDestination?.WarehouseIndex != null &&
                this.warehouseDestination.WarehouseIndex != this.WarehouseIndex &&
                !this.isMoving)
            {
                this.StartDestination();
            }

            if (this.isMoving)
            {
                if (!this.validation.IsTraversable(this.warehouseDestination.WarehouseIndex.Value, this.movementDirection.Value, this.PushStrength))
                {
                    this.CancelDestination();
                }
                else
                {
                    this.MoveToDestination(Time.deltaTime);
                }
            }
        }
        #region events

        public void OnPointerDown(PointerEventData eventData)
        {

        }

        public void OnPointerUp(PointerEventData eventData)
        {

        }

        protected virtual void OnAnimationStart(AnimationEvent animationEvent)
        {
        }

        protected virtual void OnAnimationEnd(AnimationEvent animationEvent)
        {
            this.animationEndCallback?.Invoke(animationEvent);
            this.animationEndCallback = null;
        }

        #endregion
        #region Method
        private void StartDestination()
        {
            var direction = (this.warehouseDestination.WarehouseIndex.Value - this.WarehouseIndex.Value).AsMovementType();
            if (direction.HasValue)
            {
                this.isMoving = true;
                this.movementDirection = direction;
                this.OnStartDestinationEvent.Invoke(this.warehouseDestination, this, direction.Value);
                if (this.MovementAudio.IsValid())
                {
                    this.PlaySfx(this.MovementAudio);
                }
            }
        }
        protected virtual void OnValidate()
        {
            this.ResolveSprite();
        }

        protected void ResolveSprite()
        {
            var spriteName = this.SpriteName;
            if (this.isSpriteResolved || spriteName.IsInvalid())
            {
                return;
            }
            var renderer = this.SpriteRenderer;
            if (renderer.sprite?.name != spriteName)
            {
                renderer.sprite = this.GetResources<Sprite>(this.Spritesheet)
                    .FirstOrDefault(x => x.name == spriteName);
            }
            //this.CenterOn(renderer.transform.position, this.WarehouseIndex);
            this.isSpriteResolved = true;
        }
        public virtual void ToggleInput(bool enableInput)
        {
            this.isInputEnabled = enableInput;
            if (this.inputCollider != null)
            {
                this.inputCollider.isTrigger = this.isInputEnabled;
                this.inputCollider.enabled = this.isInputEnabled;
            }

            var currentBounds = this.inputCollider != null ? this.inputCollider.bounds : this.SpriteRenderer.bounds;
            if (this.isInputEnabled)
            {
                var center = currentBounds.center;
                var size = currentBounds.size;
                this.activeInputBounds = new Rect(center, size);
            }
            else
            {
                this.activeInputBounds = new Rect(currentBounds.center, Vector3.zero);
            }
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
namespace Connect.Core
{

    public class WarehouseManager : BaseBehavior, BaseItem.IMovementValidation
    {
        public class Event : UnityEvent<WarehouseManager> { }

        /// <summary>
        /// Maximum width or height of any warehouse
        /// </summary>
        public const int MaxSize = 10;
        public Event OnInitializedEvent = new Event();
        public int defaultColumns = 6;
        public int defaultRows = 4;

        public BaseItem.ITimeProvider timeProvider { get; set; }

        private Ground[][] grounds => this.activeData?.grounds;
        private List<BaseItem> activeItems => this.activeData?.activeItems;
        private Texture2D renderCanvas { get; set; }
        private GameObject itemHolder { get; set; }
        private GameObject groundHolder { get; set; }
        private WarehouseData activeData { get; set; }
        private WarehouseMetaData.LevelMetaData activeLevelMeta { get; set; }

        #region properties
        public WarehouseData ActiveData
        {
            get
            {
                return this.activeData.MemberwiseClone();
            }
        }

        public WarehouseMetaData.LevelMetaData ActiveLevelMeta
        {
            get
            {
                return this.activeLevelMeta;
            }
        }

        public WarehouseDataBundle ActiveDataBundle
        {
            get
            {
                var bundle = new WarehouseDataBundle
                {
                    levelData = this.ActiveData,
                    levelMetaData = this.Context.data.activeWarehouseLevelMeta// this.ActiveLevelMeta
                };
                if (bundle?.levelMetaData?.completionMovementHistory != null)
                {
                    bundle.levelData.completionMovementHistory = bundle.levelMetaData.completionMovementHistory.ToList();
                }
                return bundle;
            }
        }

        public Bounds WorldBounds
        {
            get
            {
                var bound = new Bounds();
                var bounds = this.grounds.ForEach((ground, position) => ground?.WorldBounds)
                    .WithNonNull()
                    .ToList();
                bounds.ForEach(x => bound.Encapsulate(x));
                return bound;
            }
        }

        public bool HasMovingItems => this.activeItems.Any(x => x?.isMoving == true);
        public bool WinCondition
        {
            get
            {
                return true;
            }
        }

        #endregion

        // Start is called before the first frame update
        async protected override void Start()
        {
            base.Start();
            this.BuildWarehouse();

            var renderer = this.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                var thumb = new ThumbnailBuilder { scale = 0.5f };
                var sprite = thumb.GetSprite(this.grounds);
                renderer.sprite = sprite;
                renderer.sortingLayerName = "Ui";
            }

            this.OnInitializedEvent.Invoke(this);
        }

        public void BuildWarehouse()
        {
            this.activeData = this.Context.data.activeWarehouseData;

            if (this.itemHolder == null)
            {
                this.itemHolder = new GameObject("WarehouseHolder");
            }
            if (this.groundHolder == null)
            {
                this.groundHolder = new GameObject("GroundHolder");
                this.groundHolder.transform.parent = this.itemHolder.transform;
            }

            this.BuildGrounds();
            this.BuildItems();
           
        }

        public bool PlaceItem(Sprite sprite, Ground ground) =>
            this.PlaceItem(sprite, ground.WarehouseIndex.Value.x, ground.WarehouseIndex.Value.y);
        public bool PlaceItem(Sprite sprite, int column, int row) =>
            this.BuildItem(WarehouseBuildItemRequest.FromSprite(sprite, column, row), true);
        public bool PlaceItem(BaseItem item, Ground ground) =>
            this.PlaceItem(item, ground.WarehouseIndex.Value.x, ground.WarehouseIndex.Value.y);
        public bool PlaceItem(BaseItem item, int column, int row)
        {
            if (!this.HasGround(column, row))
            {
                return false;
            }
            var ground = this.grounds[column][row];
            ground.SetOccupant(item);
            return true;
        }

        private bool HasGround(Vector2Int? position) => position.HasValue && this.HasGround(position.Value.x, position.Value.y);
        private bool HasGround(int column, int row)
        {
            return this.activeData.HasGround(column, row);
            // return this.grounds != null &&
            //     column >= 0 && this.grounds.Length > column &&
            //     row >= 0 && this.grounds[column].Length > row &&
            //     this.grounds[column][row] != null;
        }

        private bool HasOccupant(Vector2Int? position) => position.HasValue && this.HasOccupant(position.Value.x, position.Value.y);
        private bool HasOccupant(int column, int row)
        {
            if (!this.HasGround(column, row))
            {
                throw new ArgumentException($"Cant determine occupant, ground doesn't exist at ({column},{row})");
            }
            return this.grounds[column][row]?.occupant != null;
        }

        private BaseItem GetOccupant(Vector2Int? position) => !position.HasValue ? null : this.GetOccupant(position.Value.x, position.Value.y);
        private BaseItem GetOccupant(int column, int row)
        {
            if (!this.HasOccupant(column, row))
            {
                return null;
            }
            return this.grounds[column][row].occupant;
        }

        public BaseItem GetInboundOccupant(Vector2Int? position)
        {
            return !position.HasValue ? null : this.activeItems.FirstOrDefault(x => x.warehouseDestination?.WarehouseIndex == position);
        }

        private void BuildGrounds()
        {
            if (this.activeData.columns == 0 && this.activeData.rows == 0)
            {
                if (this.activeData.missingGround.HasItems() || this.activeData.buildItems.HasItems())
                {
                    this.activeData.columns = Math.Max(
                        this.activeData.missingGround.HasItems() ? this.activeData.missingGround.Max(x => x.x) + 1 : 0,
                        this.activeData.buildItems.HasItems() ? this.activeData.buildItems.Max(x => x.column) + 1 : 0);
                    this.activeData.rows = Math.Max(
                        this.activeData.missingGround.HasItems() ? this.activeData.missingGround.Max(x => x.y) + 1 : 0,
                        this.activeData.buildItems.HasItems() ? this.activeData.buildItems.Max(x => x.row) + 1 : 0);
                }
                else
                {
                    this.activeData.columns = this.defaultColumns;
                    this.activeData.rows = this.defaultRows;
                }
            }

            this.activeData.grounds = new Ground[this.activeData.columns][];
            for (var column = 0; column < this.activeData.columns; ++column)
            {
                this.grounds[column] = new Ground[this.activeData.rows];
                for (var row = 0; row < this.activeData.rows; ++row)
                {
                    if (this.activeData.missingGround == null || !this.activeData.missingGround.Contains(new WarehouseMissingGround(column, row)))
                    {
                        this.AddGround(column, row);
                    }
                }
            }
            this.CalculateMissingGround();
        }

        private void CalculateMissingGround()
        {
            this.activeData?.CalculateMissingGround();
        }

        public Ground AddGround(Vector2Int relativePosition, bool allowResize = false) =>
            this.AddGround(relativePosition.x, relativePosition.y, allowResize);
        public Ground AddGround(int column, int row, bool allowResize = false)
        {
            if (allowResize && (column < 0 || row < 0 || column >= this.grounds.Length || row >= this.grounds[0].Length))
            {
                this.activeData.GrowGrounds(column, row);
                this.activeData.Validate();
                column = column < 0 ? 0 : column;
                row = row < 0 ? 0 : row;
            }

            var prefabReference = this.File.GetPrefab<Ground>();
            var groundPosition = new Vector3(Ground.groundWidthUnits * column, Ground.groundHeightUnits * row);
            var ground = Instantiate(prefabReference, groundPosition, Quaternion.identity, this.groundHolder.transform).GetComponent<Ground>();
            ground.WarehouseIndex = new Vector2Int(column, row);
            
            this.grounds[column][row] = ground;
            if (allowResize)
            {
                this.CalculateMissingGround();
            }
            return ground;
        }

        public void BuildItems()
        {
            if (this.activeData?.buildItems == null)
            {
                return;
            }
            foreach (var item in this.activeData.buildItems)
            {
                this.BuildItem(item);
            }
        }

        public bool BuildItem(WarehouseBuildItemRequest item, bool removeExisting = false)
        {
            if (!this.HasGround(item.column, item.row) || this.HasOccupant(item.column, item.row) && !removeExisting)
            {
                return false;
            }

            BaseItem newItem = null;
            if (item.itemType != null)
            {
                var prefabReference = this.File.GetPrefab(Type.GetType(item.itemType));
                var newObject = Instantiate(prefabReference, this.itemHolder.transform);
                newItem = newObject.GetComponent<BaseItem>();
            }

            if (newItem != null)
            {
                newItem.validation = this;
                newItem.timeProvider = this.timeProvider;
                newItem.origin = item;
                return this.PlaceItem(newItem, item.column, item.row);
            }

            return false;
        }

        public T GetByOccupantType<T>() where T : BaseItem => this.GetByOccupantTypes<T>().FirstOrDefault();
        public List<T> GetByOccupantTypes<T>()
            where T : BaseItem
        {
            return this.GetGroundByOccupantTypes<T>()
                .Select(x => x.occupant)
                .Cast<T>()
                .ToList();
        }

        public Ground GetGround(Vector2Int? position) => position.HasValue ? this.GetGround(position.Value.x, position.Value.y) : null;
        public Ground GetGround(int column, int row)
        {
            return this.HasGround(column, row) ? this.grounds[column][row] : null;
        }

        public Ground GetGroundByOccupantType<T>() where T : BaseItem => this.GetGroundByOccupantTypes<T>().FirstOrDefault();
        public List<Ground> GetGroundByOccupantTypes<T>()
            where T : BaseItem
        {
            return this.grounds.AsNotNull()
                .SelectMany(x => x.AsNotNull().Where(y => y != null && y.HasOccupant && y.occupant.GetType() == typeof(T)))
                .ToList();
        }

        public bool IsTraversable(Vector2Int destination, MovementType movementType)
        {
            if (!this.HasGround(destination))
            {
                return false;
            }
            var direction = movementType.GetAngle();

            if (!this.HasOccupant(destination) ||
                this.GetOccupant(destination).IsPassiveOccupant ||
                this.GetOccupant(destination).isMoving ||
                this.GetOccupant(destination).GetType() == typeof(Block))
            {
                return true;
            }
            return false;
        }

        public List<T> GetItems<T>() where T : BaseItem
        {
            return this.activeItems.Where(x => x.GetType() == typeof(T))
                .Cast<T>()
                .ToList();
        }

    }
}
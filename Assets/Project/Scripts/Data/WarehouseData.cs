using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
namespace Connect.Core
{
    [RelativePath(Directory = "Data/Levels/", FileName = "level", IsResource = true)]
    public class WarehouseData : BaseData<WarehouseData>
    {
        public class MetaEvent : UnityEvent<WarehouseData, WarehouseMetaData.LevelMetaData> { }

        public List<WarehouseBuildItemRequest> buildItems { get; set; }
        public HashSet<WarehouseMissingGround> missingGround { get; set; }
        public int columns { get; set; }
        public int rows { get; set; }
        public string author { get; set; }
        public string authorDeviceIdentifier { get; set; }
        public List<BaseItem.MovementHistory> completionMovementHistory { get; set; }

        [JsonIgnore]
        public int runtimeIndex { get; set; } = -1;
        [JsonIgnore]
        public CategoryType? runtimeCategory { get; set; }
        [JsonIgnore]
        public Ground[][] grounds { get; set; }
        [JsonIgnore]
        public List<BaseItem> activeItems { get; set; } = new List<BaseItem>();

        #region properties
        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                if (this.runtimeIndex >= 0)
                {
                    var categoryName = !this.runtimeCategory.HasValue ? "" : GameContext.Instance.Locale.Get(this.runtimeCategory.Value.ToString());
                    return $"{categoryName}: {this.runtimeIndex + 1}";
                }
                else
                {
                    return GameContext.Instance.Locale.Get(LocaleTextType.CustomLevel);
                }
            }
        }
        [JsonIgnore]
        public string NormalizedName
        {
            get
            {
                if (this.runtimeIndex >= 0)
                {
                    var categoryName = !this.runtimeCategory.HasValue ? "" : this.runtimeCategory.Value.ToString();
                    return $"{categoryName}: {this.runtimeIndex + 1}";
                }
                else
                {
                    return GameContext.Instance.Locale.Get(LocaleTextType.CustomLevel);
                }
            }
        }
        #endregion

        public WarehouseData() : base()
        {
            this.authorDeviceIdentifier = this.GetIdentifier();
        }

        public void CalculateMissingGround()
        {
            this.missingGround = this.grounds.ForEach((ground, position) =>
            {
                return this.HasGround(position) ? null : new WarehouseMissingGround(position.x, position.y);
            })
            .WithNonNull()
            .ToHashSet();
        }

        public bool HasGround(Vector2Int? position) => position.HasValue && this.HasGround(position.Value.x, position.Value.y);
        public bool HasGround(int column, int row)
        {
            return this.grounds != null &&
                column >= 0 && this.grounds.Length > column &&
                row >= 0 && this.grounds[column].Length > row &&
                this.grounds[column][row] != null;
        }

        public void GrowGrounds(int columns, int rows)
        {
            var frontColumnPadding = -Math.Min(0, columns);
            var frontRowPadding = -Math.Min(0, rows);
            var backColumnPadding = Math.Max(this.grounds.Length - 1, columns) - (this.grounds.Length - 1);
            var backRowPadding = Math.Max(this.grounds[0].Length - 1, rows) - (this.grounds[0].Length - 1);
            this.ResizeGrounds(frontColumnPadding, backColumnPadding, frontRowPadding, backRowPadding);
        }

        public void ResizeGrounds(int frontColumnPadding, int backColumnPadding, int frontRowPadding, int backRowPadding)
        {
            var newGrounds = new Ground[this.grounds.Length + frontColumnPadding + backColumnPadding][];
            Enumerable.Range(0, newGrounds.Length).ForEach(x => newGrounds[x] = new Ground[this.grounds[0].Length + frontRowPadding + backRowPadding]);
            Debug.Log($"Modifying grounds array (Front: ({frontColumnPadding}, {frontRowPadding}), Back: ({backColumnPadding}, {backRowPadding})");

            this.grounds.ForEach((ground, position) =>
            {
                var newX = position.x + frontColumnPadding;
                var newY = position.y + frontRowPadding;
                if (newX < 0 || newY < 0 || newX >= newGrounds.Length || newY >= newGrounds[0].Length)
                {
                    if (ground != null)
                    {
                        throw new ArgumentException("Ground needs to be disposed before truncated out of grid.");
                    }
                    return;
                }
                newGrounds[position.x + frontColumnPadding][position.y + frontRowPadding] = this.grounds[position.x][position.y];
            });

            this.grounds = newGrounds;
            this.columns = this.grounds.Length;
            this.rows = this.grounds[0].Length;
        }

        public void Validate()
        {
            this.ValidateIndicies();
            this.ValidateBuildItems();
        }

        public void ValidateIndicies()
        {
            this.grounds.ForEach((ground, position) => ground != null ? ground.WarehouseIndex = position : null);
        }

        public void ValidateBuildItems(Ground individualGround = null, BaseItem occupant = null)
        {
            if (individualGround != null)
            {
                var buildItems = this.buildItems
                    .Where(x => x.Position != individualGround.WarehouseIndex)
                    .ToList();

                var buildItem = individualGround.AsBuildItem;
                if (buildItem != null)
                {
                    buildItems.Add(buildItem);
                }

                this.buildItems = buildItems.ToList();
            }
            else
            {
                this.buildItems = this.grounds.ForEach((ground, position) => ground?.AsBuildItem)
                    .WithNonNull()
                    .ToList();
            }

            this.ValidateActiveItems(individualGround, occupant);
        }

        public void ValidateActiveItems(Ground individualGround = null, BaseItem occupant = null)
        {
            if (occupant != null)
            {
                // invokee was likely from a removed occupant
                if (individualGround.occupant != occupant)
                {
                    this.activeItems.Remove(occupant);
                    var occupantBuildItem = occupant.AsBuildItem;
                    this.buildItems.Remove(occupantBuildItem);
                }
                else if (!this.activeItems.Contains(occupant))
                {
                    this.activeItems.Add(occupant);
                }
            }
            else
            {
                this.activeItems = this.grounds.ForEach((ground, position) => ground?.occupant)
                    .WithNonNull()
                    .ToList();
            }
        }

        public static WarehouseData FromJson(string exportData)
        {
            return JsonConvert.DeserializeObject<WarehouseData>(exportData);
        }

        public new WarehouseData MemberwiseClone()
        {
            var data = base.MemberwiseClone();
            data.buildItems = data.buildItems.AsNotNull()
                .Select(x => x.MemberwiseClone())
                .ToList();
            data.missingGround = data.missingGround.AsNotNull()
                .Select(x => x.MemberwiseClone())
                .ToHashSet();
            return data;
        }

        public class Comparer : IEqualityComparer<WarehouseData>
        {
            public bool Equals(WarehouseData dataOne, WarehouseData dataTwo)
            {
                if (dataOne == null && dataTwo == null)
                {
                    return true;
                }
                else if (dataOne == null || dataTwo == null)
                {
                    return false;
                }
                var buildItemEquality = Enumerable.SequenceEqual(
                    dataOne.buildItems.AsNotNull().OrderBy(x => x.row).ThenBy(x => x.column),
                    dataTwo.buildItems.AsNotNull().OrderBy(x => x.row).ThenBy(x => x.column),
                    new WarehouseBuildItemRequest.Comparer());
                var missingGroundEquality = Enumerable.SequenceEqual(
                    dataOne.missingGround.AsNotNull().OrderBy(x => x.x).ThenBy(x => x.y),
                    dataTwo.missingGround.AsNotNull());
                return buildItemEquality && missingGroundEquality;
            }

            public int GetHashCode(WarehouseData obj)
            {
                return obj?.buildItems?.GetHashCode() ?? -1;
            }
        }
    }

    public class WarehouseMissingGround : SimpleBaseData<WarehouseMissingGround>
    {
        public int x { get; set; }
        public int y { get; set; }

        public WarehouseMissingGround(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            var other = (WarehouseMissingGround)obj;
            return other != null && other.x == this.x && other.y == this.y;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() + this.y.GetHashCode();
        }
    }

    public class WarehouseBuildItemRequest : SimpleBaseData<WarehouseBuildItemRequest>
    {
        public int column { get; set; }
        public int row { get; set; }
        public BoxType? boxType { get; set; }
        public BoxType? markerType { get; set; }
        public string itemType { get; set; }

        #region properties
        [JsonIgnore]
        public Vector2Int Position => new Vector2Int(this.column, this.row);

        [JsonIgnore]
        public string Name => $"{(this.boxType?.ToString() ?? this.markerType?.ToString()).OnNullOrEmpty(this.itemType)} ({this.column}, {this.row})";
        #endregion

        public static WarehouseBuildItemRequest FromSprite(Sprite sprite, Vector2Int position) =>
            FromSprite(sprite, position.x, position.y);
        public static WarehouseBuildItemRequest FromSprite(Sprite sprite, int column, int row)
        {
            var item = new WarehouseBuildItemRequest
            {
                column = column,
                row = row
            };
            var behavior = AttributeExtensions.GetAssemblyTypes<SpriteAttribute>()
                .FirstOrDefault(x =>
                    x.attribute.Name == sprite.name ||
                    x.attribute.NamePattern.IsValid() && sprite.name.Contains(x.attribute.NamePattern))
                .type;
            if (behavior != null)
            {
                item.itemType = behavior.ToString();
                return item;
            }
            else
            {
                // let's try to match on enum-types
                var enumSprites = AttributeExtensions.GetAssemblyEnumMembers<SpriteAttribute>();
                var enumMatch = enumSprites.FirstOrDefault(x => x.matches.AsNotNull().Any(y => y.attributes.AsNotNull().Any(z => (z as SpriteAttribute)?.Name == sprite.name)));
                if (enumMatch.matches.HasItems())
                {
                    var enumType = enumMatch.enumType;
                    var (enumName, attributes) = enumMatch.matches.First(x => x.attributes.Any(y => (y as SpriteAttribute)?.Name == sprite.name));
                    var spriteAttr = (SpriteAttribute)attributes.First(x => (x as SpriteAttribute).Name == sprite.name);
                    switch (spriteAttr.Key)
                    {
                        default:
                            throw new NotImplementedException($"Can't resolve PlaceItem from '{spriteAttr.Key}' (sprite: {sprite?.name}).");
                    }
                    return item;
                }
            }

            throw new ArgumentException($"Couldn't PlaceItem for sprite '{sprite?.name}'.");
        }

        public class Comparer : IEqualityComparer<WarehouseBuildItemRequest>
        {
            public bool Equals(WarehouseBuildItemRequest x, WarehouseBuildItemRequest y)
            {
                if (x == null && y == null)
                {
                    return true;
                }
                else if (x == null || y == null)
                {
                    return false;
                }
                return x.column == y.column &&
                    x.row == y.row &&
                    x.boxType == y.boxType &&
                    x.markerType == y.markerType &&
                    x.itemType == y.itemType;
            }

            public int GetHashCode(WarehouseBuildItemRequest obj)
            {
                return obj?.GetHashCode() ?? -1;
            }
        }
    }
}
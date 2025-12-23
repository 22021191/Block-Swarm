using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
namespace Connect.Core
{
[RelativePath(Directory = "Data/Bundles/", FileName = "level")]
public class WarehouseDataBundle
{
    public WarehouseData levelData { get; set; }

    public WarehouseMetaData.LevelMetaData levelMetaData { get; set; }

    [JsonIgnore]
    public string PostfixString
    {
        get
        {
            var moveCount = this.levelMetaData?.completionMovementHistory?.Count ?? 0;
            var buildCount = this.levelData?.buildItems?.Count ?? 0;
            var columnCount = this.levelData?.columns ?? 0;
            var rowCount = this.levelData?.rows ?? 0;
            var groundCount = (rowCount) * (columnCount) - this.levelData.missingGround.AsNotNull().Count();
            return $"ground-{groundCount}" +
                $"_moves-{moveCount}" +
                $"_items-{buildCount}" +
                $"_cols-{columnCount}" +
                $"_rows-{rowCount}";
        }
    }
}
}
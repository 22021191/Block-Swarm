using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
namespace Connect.Core
{
    [RelativePath(Directory = "Data/Meta/", FileName = "warehouse")]
    public class WarehouseMetaData : BaseData<WarehouseMetaData>
    {
        public List<LevelMetaData> levelData { get; set; }
        public UserSettings userSettings { get; set; }
        public UserData userData { get; set; }

        public WarehouseMetaData()
        {
            this.levelData = new List<LevelMetaData>();
            this.userData = new UserData();
            this.userSettings = new UserSettings();
        }

        public class UserSettings : BaseData<UserSettings>
        {
            public List<String> shownNotices { get; set; }
        }

        public class UserData : BaseData<UserData>
        {
            public string authToken { get; set; }
            public string id { get; set; }
            public string generatedName { get; set; }
        }

        public class LevelMetaData : BaseData<LevelMetaData>
        {
            public DateTime? createdAtDate { get; set; }
            public DateTime? lastPlayedDate { get; set; }
            public uint? bestCompletionMs { get; set; }
            public DateTime? bestCompletionDate { get; set; }
            public DateTime? firstCompletionDate { get; set; }
            public string relativePath { get; set; }
            public List<BaseItem.MovementHistory> completionMovementHistory { get; set; }

            #region properties
            [JsonIgnore]
            public bool IsCompleted => this.firstCompletionDate.HasValue;

            [JsonIgnore]
            public string FileName => this.relativePath.AsFileName();
            #endregion

            public LevelMetaData() : this(null) { }
            public LevelMetaData(WarehouseData levelData) : base()
            {
                if (levelData != null)
                {
                    this.SetDataReference(levelData);
                    this.createdAtDate = DateTime.Now;
                }
            }

            public void Update(GameContext.Data contextData, BaseItem.ITimeProvider time)
            {
                this.lastPlayedDate = DateTime.Now;
                if (!this.firstCompletionDate.HasValue)
                {
                    this.firstCompletionDate = this.lastPlayedDate;
                }
                if (!this.bestCompletionMs.HasValue || this.bestCompletionMs.Value > time.TimeMs)
                {
                    this.bestCompletionDate = this.lastPlayedDate;
                    this.bestCompletionMs = time.TimeMs;
                    this.completionMovementHistory = contextData.RecentMovementHistory;
                }
            }
        }
    }
}

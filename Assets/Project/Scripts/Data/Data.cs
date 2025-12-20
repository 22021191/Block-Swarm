using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace Connect.Core
{
   public class Data : BaseData<Data>
    {
        public class SettingEvent : UnityEvent<string, bool> { }

        public SettingEvent OnSettingChanged = new SettingEvent();
       
        private bool _isSoundEnabled;
        public bool IsSoundEnabled
        {
            get
            {
                return this._isSoundEnabled;
            }
            set
            {
                this._isSoundEnabled = value;
                this.OnSettingChanged?.Invoke(nameof(this.IsSoundEnabled), value);
            }
        }

        private List<BaseItem.MovementHistory> _recentMovementHistory = new List<BaseItem.MovementHistory>();
        public List<BaseItem.MovementHistory> RecentMovementHistory
        {
            set
            {
                this._recentMovementHistory = value;
            }
            get
            {
                return this._recentMovementHistory.AsNotNull()
                    .Select(x => x.MemberwiseClone())
                    .ToList();
            }
        }

        public void LogMovement(BaseItem item, MovementType movementType, uint timeMs)
        {
            // this.LogMovement((BaseItem.MovementHistory)new BaseItem.MovementHistory
            // {
            //     movementType = movementType,
            //     timeMs = timeMs
            // }
            // .SetDataReference(item.origin));
        }

        public void LogMovement(BaseItem.MovementHistory movement)
        {
            this._recentMovementHistory.Add(movement);
        }

    }

}
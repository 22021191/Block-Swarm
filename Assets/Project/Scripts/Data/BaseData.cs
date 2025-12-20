using UnityEngine;
using Newtonsoft.Json;
namespace Connect.Core
{
   public abstract class BaseData<T>
    {
        public string localIdentifier { get; set; }
        public string globalIdentifier { get; set; }

        #region properties
        [JsonIgnore]
        public string Identifier => this.localIdentifier.OnNullOrEmpty(this.globalIdentifier);
        #endregion

        public BaseData()
        {
            this.localIdentifier = this.GetGuid();
        }

        public new T MemberwiseClone() => (T)base.MemberwiseClone();

        public string GetGuid() => System.Guid.NewGuid().ToString();

        public string GetIdentifier() => SystemInfo.deviceUniqueIdentifier;

        public virtual bool ShouldSerializelocalIdentifier() => true;

        public virtual bool ShouldSerializeglobalIdentifier() => true;

        public void RotateGuid() => this.localIdentifier = this.GetGuid();

        public BaseData<T> SetDataReference<TData>(TData otherData) where TData : BaseData<TData>
        {
            this.localIdentifier = otherData?.localIdentifier;
            this.globalIdentifier = otherData?.globalIdentifier;
            return this;
        }
    }

    public abstract class SimpleBaseData<T> : BaseData<T>
    {
        public override bool ShouldSerializeglobalIdentifier() => false;
        public override bool ShouldSerializelocalIdentifier() => false;
    }
}
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace Connect.Core
{

    public abstract class BaseBehavior : MonoBehaviour
    {
        public class VisibilityEvent : UnityEvent<bool> { }

        protected AudioSource audioSource;

        #region properties
        protected GameContext Context
        {
            get
            {
                return GameContext.Instance;
            }
        }
        private EventSystem _eventSystem;
        protected EventSystem EventSystem
        {
            get
            {
                return (this._eventSystem = this._eventSystem ?? GameObject.FindObjectOfType<EventSystem>());
            }
        }
        #endregion

        protected virtual void Start()
        {
            this.audioSource = this.GetComponent<AudioSource>();
        }

        public virtual void PlaySfx()
        {
            if (this.audioSource == null || !this.Context.data.IsSoundEnabled)
            {
                return;
            }
            this.audioSource.Play();
        }

        public void PlaySfx(string fileName, bool loop = false)
        {
            if (this.audioSource == null || !this.Context.data.IsSoundEnabled || !fileName.IsValid())
            {
                return;
            }
        }

        public void StopAudio()
        {
            if (this.audioSource == null || !this.audioSource.isPlaying)
            {
                return;
            }

            this.audioSource.Stop();
        }

    }

    
}
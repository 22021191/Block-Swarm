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

        protected FileManager File
        {
            get
            {
                return GameContext.Instance.file;
            }
        }

        protected LocaleManager Locale
        {
            get
            {
                return GameContext.Instance.Locale;
            }
        }

        //protected NetworkManager Network
        //{
        //    get
        //    {
        //        return GameContext.Instance.network;
        //    }
        //}

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
            var audio = this.GetResource<AudioClip>($"Audio/SFX/{fileName}");
            if (audio != null)
            {
                this.audioSource.loop = loop;
                if (loop)
                {
                    if (!this.audioSource.isPlaying || !this.audioSource.clip.Equals(audio))
                    {
                        this.audioSource.clip = audio;
                        this.audioSource.Play();
                    }
                }
                else
                {
                    this.audioSource.PlayOneShot(audio);
                }
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

        protected T GetResource<T>(string resourcePath) where T : UnityEngine.Object
        {
            return this.Context != null ? this.File.GetResource<T>(resourcePath) : Resources.Load<T>(resourcePath);
        }

        protected List<T> GetResources<T>(string resourcePath) where T : UnityEngine.Object
        {
            return this.Context != null ? this.File.GetResources<T>(resourcePath) : Resources.LoadAll<T>(resourcePath).ToList();
        }

    }


}
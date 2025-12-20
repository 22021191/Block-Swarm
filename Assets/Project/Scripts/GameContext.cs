using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
namespace Connect.Core
{
    public class GameContext
    {
        private static GameContext _instance;
        public static GameContext Instance => _instance = _instance ?? new GameContext();
        public Data data { get; private set; } = new Data();
        public const int PixelPerUnit = 128;

        public bool isRunning { get; private set; } = true;

        #region properties
        public bool IsPaused
        {
            get
            {
                return !this.isRunning;
            }
        }
        #endregion

        public GameContext()
        {

        }

        public void SetRunning(bool isRunning)
        {
            if (this.isRunning == isRunning)
            {
                return;
            }

            this.isRunning = isRunning;
        }

        public void ResetData()
        {
            Debug.Log("Reset data requested");
        }


    }
}
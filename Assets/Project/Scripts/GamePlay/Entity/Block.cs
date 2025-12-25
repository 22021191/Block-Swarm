using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Connect.Core
{
    //[Prefab(Path = "Prefabs/Items/Gameplay/PlayerPrefab3")]
    [Sprite(Name = "player_d0", NamePattern = "player_")]
    [Thumbnail(Name = "thumbnail_player")]
    public class Block : BaseItem
    {
        public List<MovementType> directions = new List<MovementType>
        {
            MovementType.Up,
            MovementType.Down,
            MovementType.Left,
            MovementType.Right
        };
        public SpriteRenderer shadow;
        private List<GameObject> _DirectonArrow = new List<GameObject>();
        protected override string MovementAudio => "step2";
        private MovementType? lastMovementDirection;
        private void Awake()
        {
            _DirectonArrow.Clear();

            foreach (Transform t in transform.GetComponentsInChildren<Transform>())
            {
                if (t != transform) // loại bỏ object cha
                    _DirectonArrow.Add(t.gameObject);
            }
        }
        protected void Start()
        {
            base.Start();
            for (int i = 0; i < _DirectonArrow.Count; i++)
            {
                _DirectonArrow[i].SetActive(directions.Contains((MovementType)i));
            }

        }
        protected override void Update()
        {
            base.Update();

            if (this.Context.IsPaused)
            {
                return;
            }

        }

        public override void ArriveDestination()
        {
            this.lastMovementDirection = this.movementDirection;
            if (this.movementDirection.HasValue)
            {
                //this.Context.data.LogMovement(this, this.movementDirection.Value);
            }

            base.ArriveDestination();

            if (this.warehouseDestination == null)
            {
            }
        }
        public bool HasDirection(MovementType type)
        {
            return directions.Contains(type);
        }

    }
}
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
        private static int animateFps = 10;
        private static float animateDelta = 1.0f / animateFps;
        private static Dictionary<MovementType, string[]> animateSeries = new Dictionary<MovementType, string[]>
    {
        { MovementType.Down, new string[]{ "player_d1", "player_d0", "player_d2" } },
        { MovementType.Right, new string[]{ "player_r1", "player_r0", "player_r2" } },
        { MovementType.Up, new string[]{ "player_u1", "player_u0", "player_u2" } },
        { MovementType.Left, new string[]{ "player_l1", "player_l0", "player_l2" } }
    };

        public SpriteRenderer shadow;
        protected override string MovementAudio => "step2";

        private MovementType? lastMovementDirection;

        public override void ArriveDestination()
        {
            this.lastMovementDirection = this.movementDirection;
            if (this.movementDirection.HasValue)
            {
                this.Context.data.LogMovement(this, this.movementDirection.Value, this.timeProvider.TimeMs);
            }

            base.ArriveDestination();

            if (this.warehouseDestination == null)
            {
                this.frameResetStep = animateDelta;
            }
        }

    }
}
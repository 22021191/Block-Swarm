using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Connect.Core
{
    public class SwarmBlock : Block
    {
        private HashSet<SwarmBlock> connectedSwarmBlocks = new HashSet<SwarmBlock>();
        private bool isCheckingCollision = false;
        private bool hasCheckedInitialConnections = false;

        private void Awake()
        {
            connectedSwarmBlocks.Add(this); // Tự kết nối với chính nó
        }

        protected override void Update()
        {
            base.Update();
            
            // Kiểm tra kết nối ban đầu một lần sau khi warehouse được khởi tạo
            if (!hasCheckedInitialConnections && this.WarehouseIndex.HasValue)
            {
                hasCheckedInitialConnections = true;
                CheckAndConnectAdjacentSwarmBlocks();
            }
        }

        public override void ArriveDestination()
        {
            base.ArriveDestination();
            // Kiểm tra và kết nối với các swarm blocks kề nhau sau khi đến đích
            CheckAndConnectAdjacentSwarmBlocks();
        }

        /// <summary>
        /// Kiểm tra các swarm blocks kề nhau và kết nối chúng
        /// </summary>
        private void CheckAndConnectAdjacentSwarmBlocks()
        {
            if (!this.WarehouseIndex.HasValue)
                return;

            var warehouseManager = this.GetComponentInParent<WarehouseManager>();
            if (warehouseManager == null)
                warehouseManager = FindFirstObjectByType<WarehouseManager>();

            if (warehouseManager == null)
                return;

            // Kiểm tra 4 hướng xung quanh
            var directions = new[] { MovementType.Up, MovementType.Down, MovementType.Left, MovementType.Right };
            foreach (var dir in directions)
            {
                var adjacentPos = this.WarehouseIndex.Value + dir.GetAngle().AsVector2Int();
                var adjacentGround = warehouseManager.GetGround(adjacentPos);
                
                if (adjacentGround != null && adjacentGround.HasOccupant && adjacentGround.occupant is SwarmBlock adjacentSwarmBlock && adjacentSwarmBlock != this)
                {
                    ConnectTo(adjacentSwarmBlock);
                }
            }
        }

        /// <summary>
        /// Kết nối với một swarm block khác
        /// </summary>
        public void ConnectTo(SwarmBlock other)
        {
            if (other == null || other == this)
                return;

            // Kết nối tất cả các block trong nhóm của this với nhóm của other
            var thisGroup = GetConnectedGroup();
            var otherGroup = other.GetConnectedGroup();

            foreach (var block in otherGroup)
            {
                thisGroup.Add(block);
                block.connectedSwarmBlocks = thisGroup;
            }
        }

        /// <summary>
        /// Lấy tất cả các swarm blocks được kết nối (bao gồm chính nó)
        /// </summary>
        public HashSet<SwarmBlock> GetConnectedGroup()
        {
            return connectedSwarmBlocks;
        }

        /// <summary>
        /// Lấy tất cả các hướng có thể di chuyển từ nhóm swarm blocks
        /// </summary>
        public List<MovementType> GetCombinedDirections()
        {
            var group = GetConnectedGroup();
            var combinedDirections = new HashSet<MovementType>();

            foreach (var block in group)
            {
                if (block != null && block.directions != null)
                {
                    foreach (var dir in block.directions)
                    {
                        combinedDirections.Add(dir);
                    }
                }
            }

            return combinedDirections.ToList();
        }

        /// <summary>
        /// Kiểm tra xem nhóm swarm có thể di chuyển theo hướng này không
        /// </summary>
        public bool CanGroupMove(MovementType direction)
        {
            var combinedDirections = GetCombinedDirections();
            return combinedDirections.Contains(direction);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCheckingCollision)
                return;

            var otherSwarmBlock = other.GetComponent<SwarmBlock>();
            if (otherSwarmBlock != null && otherSwarmBlock != this)
            {
                isCheckingCollision = true;
                ConnectTo(otherSwarmBlock);
                otherSwarmBlock.ConnectTo(this);
                isCheckingCollision = false;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isCheckingCollision)
                return;

            var otherSwarmBlock = collision.gameObject.GetComponent<SwarmBlock>();
            if (otherSwarmBlock != null && otherSwarmBlock != this)
            {
                isCheckingCollision = true;
                ConnectTo(otherSwarmBlock);
                otherSwarmBlock.ConnectTo(this);
                isCheckingCollision = false;
            }
        }
    }
}

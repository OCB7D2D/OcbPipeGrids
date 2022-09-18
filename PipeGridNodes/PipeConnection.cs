using System;
using System.Collections.Generic;
using System.IO;

namespace PipeManager
{

    public class PipeConnection : PipeBlock<IBlockConnection>
    {
        public override uint StorageID => 1;

        public byte ConnectMask => BLK?.ConnectMask ?? 63;
        public int MaxConnections => BLK?.MaxConnections ?? 6;
        public bool BreakDistance => BLK?.BreakDistance ?? false;

        // public virtual byte ConnectMask { get; set; } = 63;
        // public virtual int MaxConnections { get; set; } = 6;
        // public virtual bool BreakDistance => false;

        class Creator : IfaceGridNodeFactory
        {
            public uint StorageID => 1;
            public PipeNode Create(BinaryReader br)
                => new PipeConnection(br);
        }

        // Default constructor
        public PipeConnection(
            Vector3i position,
            BlockValue bv)
        : base(position, bv)
        {
        }

        // Constructor from save
        public PipeConnection(
            BinaryReader br)
        : base(br)
        {
        }

        // Gather neighbours and register connection
        protected override void OnManagerAttached(PipeGridManager manager)
        {
            // First get all our neighbours
            for (int side = 0; side < 6; side++)
            {
                if (manager.TryGetNode( // TryGetConnection
                    WorldPos + FullRotation.Vector[side],
                    out PipeConnection neighbour))
                {
                    Neighbours[side] = neighbour;
                }
            }
            // Now register us with manager
            base.OnManagerAttached(manager);
        }

        private PipeGrid _grid;

        public PipeGrid Grid
        {
            get => _grid;
            set
            {
                // Do nothing if not changing
                if (_grid == value) return;
                // Call virtual
                UpdateGrid(value);
                // Update state
                _grid = value;
            }
        }

        // Overload to add more behavior on grid change
        protected virtual void UpdateGrid(PipeGrid grid)
        {
            Grid?.RemoveConnection(this);
            grid?.AddConnection(this);
        }

        public override void Cleanup()
        {
            Grid = null;
        }

        // Use public API below to keep Count in sync
        public readonly PipeConnection[] Neighbours
            = new PipeConnection[6];

        // Amount of valid neighbours
        public int Count { get; private set; }

        public void CollectNeighbours(ref List<PipeConnection> neighbours)
        {
            foreach (var neighbour in Neighbours)
            {
                if (neighbour == null) continue;
                neighbours.Add(neighbour);
            }
        }

        public List<PipeConnection> GetNeighbours()
        {
            List<PipeConnection> neighbours
                = new List<PipeConnection>();
            CollectNeighbours(ref neighbours);
            return neighbours;
        }

        public PipeConnection this[int index]
        {
            get => Neighbours[index];
            set
            {
                if (Neighbours[index] == null && value != null) Count++;
                else if (Neighbours[index] != null && value == null) Count--;
                Neighbours[index] = value;
            }
        }

        public bool CanConnect(byte side, byte rotation)
        {
            side = FullRotation.InvSide(side, rotation);
            return (ConnectMask & (byte)(1 << (byte)side)) != 0;
        }

        public bool CanConnect(byte side)
        {
            // Rotates question back into local frame
            return CanConnect(side, Rotation);
        }

        public override string ToString()
        {
             return string.Format(
                 "Connection (Grid {0})",
                 Grid != null ? Grid.ID : -1);
        }
        private struct Walker
        {
            public byte dist;
            public PipeConnection cur;
            public PipeConnection prv;
            public Walker(PipeConnection cur, PipeConnection prv = null, byte dist = 1)
            {
                this.cur = cur;
                this.prv = prv;
                this.dist = dist;
            }
        }

        internal ConnectorFlag GetFlags()
        {
            ConnectorFlag flags = 0;
            if (BreakDistance)
                flags |= ConnectorFlag.Breaker;
            if (Grid != null && Grid.IsCyclic)
                flags |= ConnectorFlag.Cyclic;
            return flags;
        }

        private static Queue<Walker> todo
            = new Queue<Walker>();

        public byte CountLongestDistance()
        {
            byte dist = 0;
            if (Grid == null) return 0;
            else if (Grid.IsCyclic)
            {
                return byte.MaxValue;
            }
            if (BreakDistance) return dist;
            todo.Enqueue(new Walker(this));
            while (todo.Count > 0)
            {
                Walker walk = todo.Dequeue();
                if (dist < walk.dist)
                    dist = walk.dist;
                // Utils.FastMax(dist, walk.dist);
                for (var side = 0; side < 6; side++)
                {
                    var neighbour = walk.cur.Neighbours[side];
                    if (neighbour == null) continue;
                    if (neighbour == walk.prv) continue;
                    if (neighbour.BreakDistance) continue;
                    todo.Enqueue(new Walker(neighbour,
                        walk.cur, (byte)(walk.dist + 1)));
                }
            }
            todo.Clear();
            return dist;
        }

        static uint WalkerIDs = 0;

        private struct Propagator
        {
            public PipeConnection Cur;
            public PipeConnection Prev;
            public uint WalkID;
            public Propagator(PipeConnection cur, PipeConnection prev = null, uint walkID = 0)
            {

                Cur = cur;
                Prev = prev;
                if (walkID != 0) WalkID = walkID;
                else WalkID = ++WalkerIDs;
            }
        }

        private static readonly Queue<Propagator>
            propagate = new Queue<Propagator>();

        private uint LastWalker { get; set; }

        private void PropagateGridChange(PipeConnection prv, PipeGrid grid = null)
        {
            if (grid == null) grid = prv?.Grid;
            PropagateThroughGrid(prv, (cur, lst) =>
                { cur.Grid = grid; return false; });
        }

        // Generic propagator function to walk the grid
        public void PropagateThroughGrid(PipeConnection prv,
            Func<PipeConnection, PipeConnection, bool> Processor)
        {
            // Enqueue ourself as the starting point
            propagate.Enqueue(new Propagator(this, prv));
            // Process until no more tree nodes
            while (propagate.Count > 0)
            {
                // Get first item from the queue to process
                Propagator propagator = propagate.Dequeue();
                // Call function to do the processing
                // Abort chain when it returns true
                if (Processor(propagator.Cur, propagator.Prev)) continue;
                // Process all potential neighbours
                for (var side = 0; side < 6; side++)
                {
                    // Get optional neighbour of given side
                    var neighbour = propagator.Cur.Neighbours[side];
                    // Skip non existing neighbour
                    if (neighbour == null) continue;
                    // Don't walk backwards in the tree
                    if (neighbour == propagator.Prev) continue;
                    // Detect cyclic case by checking our walk id
                    if (propagator.Cur.LastWalker == propagator.WalkID)
                    {
                        // Mark the grid cyclic and abort here
                        propagator.Cur.Grid.IsCyclic = true;
                    }
                    else
                    {
                        // Propagate to neighbour tree node
                        propagate.Enqueue(new Propagator(
                            neighbour, propagator.Cur,
                            propagator.WalkID));
                    }
                }
                // Store our walk id to detect cyclic cases
                propagator.Cur.LastWalker = propagator.WalkID;
            }
            // Make sure to clean up
            propagate.Clear();
        }

        public void AddConnection(PipeGridManager manager)
        {

            Log.Out("  add grid item");

            PipeConnection neighbour;

            // Collect neighbours from existing blocks/connections
            for (byte side = 0; side < 6; side++)
            {
                Neighbours[side] = null;
                // Check if we can connect to side
                if (!CanConnect(side)) continue;
                // Try to fetch the node at the given side
                var offset = FullRotation.Vector[side];
                if (manager.TryGetNode(WorldPos + offset, out neighbour)) // TryGetConnection
                {
                    // Check if other one can connect to use
                    byte mirrored = FullRotation.Mirror(side);
                    if (!neighbour.CanConnect(mirrored)) continue;
                    // Update the node connectors
                    Neighbours[side] = neighbour;
                    neighbour[mirrored] = this;
                }
            }

            int count = 0; int source = -1; int first = -1;

            for (int side = 0; side < 6; side++)
            {
                if (Neighbours[side] == null) continue;
                if (Neighbours[side].Grid == null)
                {
                    Log.Error("Neighbour without grid found");
                }
                //else if (connection[side].Grid.HasSource)
                //{
                //    if (source != -1)
                //    {
                //        Log.Error("Too many sources, can't join!!");
                //    }
                //    source = side;
                //}
                else if (first == -1)
                {
                    first = side;
                }
                count++;
            }

            if (source == -1)
            {
                source = first;
            }

            // manager.AddPipeGridNode(this);

            if (count == 0)
            {
                Log.Out("Creating Grid");
                Grid = new PipeGrid(manager);
                Log.Out("Created Grid");
            }
            else
            {
                Log.Out("Joining grid");
                PropagateGridChange(Neighbours[source]);
                Log.Out("Joined grid");
            }

        }

        public override string GetCustomDescription()
        {
            return string.Format("Part has {0} pipes\nGrid {1}",
                CountLongestDistance(), Grid);
        }
    }

}
using System;
using System.Collections.Generic;
using System.IO;

namespace NodeFacilitator
{

    public class PipeConnection : NodeBlock<IBlockConnection>
    {

        public static TYPES NodeType = TYPES.PipeConnection;
        public override uint StorageID => (uint)NodeType;

        public byte ConnectMask => BLOCK?.ConnectMask ?? 63;
        public uint SideMask => BLOCK?.SideMask ?? 0;
        public int MaxConnections => BLOCK?.MaxConnections ?? 6;
        public bool BreakSegment => BLOCK?.BreakSegment ?? false;

        int PipeCount => Grid == null ? 0 : Grid.Count;

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
        protected override void OnManagerAttached(NodeManager manager)
        {
            if (Manager == manager) return;
            Manager?.RemoveConnection(this);
            base.OnManagerAttached(manager);
            //Manager?.RemoveConnection(this);
            manager?.AddConnection(this);
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
        // private int Count;

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
                // if (Neighbours[index] == null && value != null) Count++;
                // else if (Neighbours[index] != null && value == null) Count--;
                Neighbours[index] = value;
            }
        }

        public bool CanConnect(byte side, byte rotation)
        {
            side = FullRotation.InvFace(side, rotation);
            return (ConnectMask & (byte)(1 << (byte)side)) != 0;
        }

        public bool CanConnect(byte side)
        {
            // Rotates question back into local frame
            return CanConnect(side, Rotation);
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
            if (BreakSegment)
                flags |= ConnectorFlag.Breaker;
            //if (Grid != null && Grid.IsCyclic)
            //    flags |= ConnectorFlag.Cyclic;
            return flags;
        }

        private static Queue<Walker> todo
            = new Queue<Walker>();

        private static HashSet<PipeConnection> Seen
            = new HashSet<PipeConnection>();

        // Count number of pipes up to end or breaker
        // Not the cheapest to call due to lookup set
        public int CountSegmentPipes()
        {
            int dist = 1;
            Seen.Clear();
            //Log.Out("Count Segments {0}", BLOCK);
            if (Grid == null) return 0;
            if (BreakSegment) return 0;
            todo.Enqueue(new Walker(this));
            while (todo.Count > 0)
            {
                Walker walk = todo.Dequeue();
                for (var side = 0; side < 6; side++)
                {
                    var neighbour = walk.cur.Neighbours[side];
                    if (Seen.Contains(neighbour)) continue;
                    Seen.Add(neighbour);
                    if (neighbour == null) continue;
                    if (neighbour == walk.prv) continue;
                    if (neighbour.BreakSegment) continue;
                    todo.Enqueue(new Walker(neighbour,
                        walk.cur, (byte)(walk.dist + 1)));
                    dist += 1;
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
            public Propagator(PipeConnection cur,
                PipeConnection prev = null,
                uint walkID = 0)
            {

                Cur = cur;
                Prev = prev;
                // Use custom or unique static id
                if (walkID != 0) WalkID = walkID;
                else WalkID = ++WalkerIDs;
            }
        }

        private static readonly Queue<Propagator>
            propagate = new Queue<Propagator>();

        private uint LastWalker { get; set; }

        public void PropagateGridChange(PipeConnection prv, PipeGrid grid = null)
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

            for (byte side = 0; side < 6; side++)
            {
                if (Neighbours[side] == null) continue;
            }

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
                    if (propagator.Cur.LastWalker != propagator.WalkID)
                    {
                        // Propagate to neighbour tree node
                        propagate.Enqueue(new Propagator(
                            neighbour, propagator.Cur,
                            propagator.WalkID));
                    }
                    //else
                    //{
                    //    // Mark the grid cyclic and abort here
                    //    if (propagator.Cur.Grid != null)
                    //        propagator.Cur.Grid.IsCyclic = true;
                    //}
                }
                // Store our walk id to detect cyclic cases
                propagator.Cur.LastWalker = propagator.WalkID;
            }
            // Make sure to clean up
            propagate.Clear();
        }

        public override string GetCustomDescription()
        {
            return string.Format("Part has {0} pipes\nBreak {2} (has {3})\nGrid {1}", PipeCount, Grid, BreakSegment, CountSegmentPipes());
        }

        public override string ToString()
        {
            return string.Format(
                "Connection (Grid {0})",
                Grid != null ? Grid.ID : -1);
        }
    
    }

}
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NodeFacilitator
{
    public class PipeFluidConverter : PipeReservoir, IPoweredNode, IExchangeItems
    {

        public static new TYPES NodeType = TYPES.PipeFluidConverter;
        public override uint StorageID => (uint)TYPES.PipeFluidConverter;

        public float FertilizerStock { get; set; } = 0;
        public float PesticideStock { get; set; } = 0;

        public float PesticideStockMax { get; set; } = 2000;
        public float FertilizerStockMax { get; set; } = 2000;

        public float MaxMaterialLevel = 500;

        // public bool IsPowered { get; set; } = true;

        public override ulong NextTick => 30;

        // PipeReservoir Input;
        PipeReservoir Output1;
        PipeReservoir Output2;

        PipeConnection Connection0;
        PipeConnection Connection1;
        PipeConnection Connection2;

        // Vector3i InPos(byte rotation) => WorldPos + FullRotation.Rotate(rotation, BLOCK.InputPosition);
        Vector3i OutPos1(byte rotation) => WorldPos + FullRotation.Rotate(rotation, new Vector3i(1, 0, 0));
        Vector3i OutPos2(byte rotation) => WorldPos + FullRotation.Rotate(rotation, new Vector3i(-1, 0, 0));
        Vector3i ConnectPos0(byte rotation) => WorldPos + FullRotation.Rotate(rotation, new Vector3i(0, 0, -1));
        Vector3i ConnectPos1(byte rotation) => WorldPos + FullRotation.Rotate(rotation, new Vector3i(1, 0, -1));
        Vector3i ConnectPos2(byte rotation) => WorldPos + FullRotation.Rotate(rotation, new Vector3i(-1, 0, -1));

        public int AskExchangeCount(int type, int count)
        {
            if (type >= Block.ItemsStartHere)
            {
                switch (ItemClass.nameIdMapping.GetNameForId(type))
                {
                    case "resourceCompost": return (int)Mathf.Min(FertilizerStockMax - FertilizerStock, count);
                    case "resourcePesticide": return (int)Mathf.Min(PesticideStockMax - PesticideStock, count);
                }
            }
            return 0;
        }

        public int ExecuteExchange(int type, int count)
        {
            Log.Warning("Exchange item request {0} x {1}", type, count);
            if (type >= Block.ItemsStartHere)
            {
                switch (ItemClass.nameIdMapping.GetNameForId(type))
                {
                    case "resourcePesticide":
                        PesticideStock += count;
                        break;
                    case "resourceCompost":
                        FertilizerStock += count;
                        break;
                }
            }
            else
            {
                Log.Warning("Block name {0}", ItemClass.nameIdMapping.GetNameForId(type));
            }
            return count;
        }

        public bool ConsumeWater(float amount)
        {
            if (FertilizerStock < amount) return false;
            FertilizerStock -= amount;
            return true;
        }

        public void FillWater(float amount)
        {
            if (amount <= 0) return;
            if (FertilizerStock > MaxMaterialLevel)
            {
                FertilizerStock = MaxMaterialLevel;
                // UpdateWaterLevel();
            }
            else if (FertilizerStock < MaxMaterialLevel)
            {
                FertilizerStock += amount;
                if (FertilizerStock > MaxMaterialLevel)
                    FertilizerStock = MaxMaterialLevel;
                // UpdateWaterLevel();
            }
        }

        public int ExchangeItem(int count, int factor)
        {
            if (factor < 0)
            {
                float req = FertilizerStock - MaxMaterialLevel;
                int buckets = (int)Mathf.Ceil(req / factor);
                buckets = MathUtils.Min(count, buckets);
                FillWater(buckets * -factor);
                return buckets;
            }
            else if (factor > 0)
            {
                int buckets = (int)(FertilizerStock / factor);
                buckets = MathUtils.Min(count, buckets);
                ConsumeWater(buckets * factor);
                return buckets;
            }
            return 0;
        }


        public PipeFluidConverter(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
            Log.Out("Create from Block {0}", BLOCK);
            string name = BLOCK.GetBlockName();

            // BlockValue BlkIn = Block.GetBlockValue(name + "Input");
            BlockValue BlkOut1 = Block.GetBlockValue(name + "Output1");
            BlockValue BlkOut2 = Block.GetBlockValue(name + "Output2");
            // if (BlkIn.isair) throw new Exception(name + "Input not found");
            if (BlkOut1.isair) throw new Exception(name + "Output1 not found");
            if (BlkOut2.isair) throw new Exception(name + "Output2 not found");
            // Set rotation so it can be picked up again
            BlkOut1.rotation = Rotation;
            BlkOut2.rotation = Rotation;

            // Input = new PipeReservoir(InPos(Rotation), BlkIn);
            Output1 = new PipeReservoir(OutPos1(Rotation), BlkOut1);
            Output2 = new PipeReservoir(OutPos2(Rotation), BlkOut2);
            Output1.IsPowered = Output2.IsPowered = true;

            // BlockValue BlkIn = Block.GetBlockValue(name + "Input");
            BlockValue BlkConnect0 = Block.GetBlockValue(name + "Connect0");
            BlockValue BlkConnect1 = Block.GetBlockValue(name + "Connect1");
            BlockValue BlkConnect2 = Block.GetBlockValue(name + "Connect2");
            // if (BlkIn.isair) throw new Exception(name + "Input not found");
            if (BlkConnect0.isair) throw new Exception(name + "Connect0 not found");
            if (BlkConnect1.isair) throw new Exception(name + "Connect1 not found");
            if (BlkConnect2.isair) throw new Exception(name + "Connect2 not found");
            // Set rotation so it can be picked up again
            BlkConnect0.rotation = Rotation;
            BlkConnect1.rotation = Rotation;
            BlkConnect2.rotation = Rotation;

            // Input = new PipeReservoir(InPos(Rotation), BlkIn);
            Connection0 = new PipeConnection(ConnectPos0(Rotation), BlkConnect0);
            Connection1 = new PipeConnection(ConnectPos1(Rotation), BlkConnect1);
            Connection2 = new PipeConnection(ConnectPos2(Rotation), BlkConnect2);

        }

        public override void OnAfterLoad()
        {
            base.OnAfterLoad();
            //Log.Warning("===== AfterLoad {0} {1}",
            //    InPos(Rotation), OutPos(Rotation));
            if (Manager == null) return;
            // Manager.TryGetNode(InPos(Rotation), out Input);
            Manager.TryGetNode(OutPos1(Rotation), out Output1);
            Manager.TryGetNode(OutPos2(Rotation), out Output2);

            Manager.TryGetNode(ConnectPos1(Rotation), out Connection1);
            Manager.TryGetNode(ConnectPos2(Rotation), out Connection2);
            // Log.Warning("Read after load {0} {1}", Input, Output);
        }

        public PipeFluidConverter(
            BinaryReader br)
        : base(br)
        {
            Log.Out("Loading Fluid Converter");
            FertilizerStock = br.ReadSingle();
            PesticideStock = br.ReadSingle();
            // Clean Water
            // SetFluidType(2);
        }

        public override void Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(FertilizerStock);
            bw.Write(PesticideStock);
        }

        // Keep a list of plants that get water from us.
        internal readonly HashSet<FarmWell> Wells
            = new HashSet<FarmWell>();

        public override string GetCustomDescription()
        {
            return string.Format("Converter In: {0}\nFertilizer: {1} (stock: {3})\nPesticide: {2} (stock: {4})",
                base.GetCustomDescription(),
                Output1?.GetCustomDescription(),
                Output2?.GetCustomDescription(),
                FertilizerStock, PesticideStock);
                
        }

        // Note: there is a chance for endless recursion if you
        // don't add the correct nodes instead of myself.
        protected override void OnManagerAttached(NodeManager manager)
        {
            Log.Out("OnManagerAttached");
            if (Manager == manager) return;
            if (manager == null)
            {
                // Manager?.RemoveManagedNode(InPos(Rotation));
                Manager?.RemoveManagedNode(OutPos1(Rotation));
                Manager?.RemoveManagedNode(OutPos2(Rotation));
                Manager?.RemoveManagedNode(ConnectPos0(Rotation));
                Manager?.RemoveManagedNode(ConnectPos1(Rotation));
                Manager?.RemoveManagedNode(ConnectPos2(Rotation));
            }
            else
            {
                // Input?.AttachToManager(manager);
                Connection0?.AttachToManager(manager);
                Connection1?.AttachToManager(manager);
                Connection2?.AttachToManager(manager);
                Output1?.AttachToManager(manager);
                Output2?.AttachToManager(manager);
            }
            base.OnManagerAttached(manager);
            // Manager?.RemoveFluidConverer(this);
            // manager?.AddFluidConverter(this);
        }

        /*
        protected override void UpdateGrid(PipeGrid grid)
        {
            base.UpdateGrid(grid);
            Log.Out("Update irrigation grid");
        }

        protected override void TickTower(float capacity, List<PipeReservoir> reservoirs)
        {
            // Skip over ourself, and take from others
            for (int i = 1; i < reservoirs.Count; i++)
            {
                PipeReservoir reservoir = reservoirs[i];
                // Can only take from reservoirs if they are filled 50%
                if (reservoir.FillState / reservoir.MaxFillState < 0.5f) continue;
                float taking = Math.Min(capacity * 2, MaxFillState - FillState);
                reservoir.FillState -= taking;
                FillState += taking;
            }
        }
        */

        public override bool Tick(ulong delta)
        {
            if (Output1 != null)
            {
                FertilizerStock -= NodeBlockHelper.ConvertReservoir(
                    Mathf.Min(delta / 50f, FertilizerStock / 0.01f),
                    0.25f, this, Output1) * 0.01f;
            }
            if (Output2 != null)
            {
                PesticideStock -= NodeBlockHelper.ConvertReservoir(
                    Mathf.Min(delta / 50f, PesticideStock / 0.01f),
                    0.25f, this, Output2) * 0.01f;
            }
            return base.Tick(delta);
        }

    }
}

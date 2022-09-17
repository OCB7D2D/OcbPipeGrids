using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeManager
{
    class PipeWell : PipeNode
    {
        public static int StorageType => 9;

        public uint MaxWaterLevel = 150;
        public float WaterAvailable = 0;
        public byte SunLight = 0;

        private readonly BlockPipeWell BlockWell = null;

        public float FromGround => BlockWell != null ? BlockWell.FromGround : 0.08f / 1000f;
        public float FromFreeSky => BlockWell != null ? BlockWell.FromFreeSky : 0.25f / 1000f;
        public float FromWetSurface => BlockWell != null ? BlockWell.FromWetSurface : 0.15f / 1000f;
        public float FromSnowfall => BlockWell != null ? BlockWell.FromSnowfall : 0.4f / 1000f;
        public float FromRainfall => BlockWell != null ? BlockWell.FromRainfall : 0.8f / 1000f;
        public float FromIrrigation => BlockWell != null ? BlockWell.FromIrrigation : 5f / 1000f;

        public PipeWell(Vector3i position, BlockValue bv)
            : base(position, bv)
        {
            GetBlock(out BlockWell);
        }

        public PipeWell(BinaryReader br) : base(br)
        {
            WaterAvailable = br.ReadSingle();
            MaxWaterLevel = br.ReadUInt32();
            SunLight = br.ReadByte();
            GetBlock(out BlockWell);
        }

        public override void Write(BinaryWriter bw)
        {
            // Write base data first
            base.Write(bw);
            // Store additional data
            bw.Write(WaterAvailable);
            bw.Write(MaxWaterLevel);
            bw.Write(SunLight);
        }

        public override string GetCustomDescription()
        {
            throw new NotImplementedException();
        }
    }
}
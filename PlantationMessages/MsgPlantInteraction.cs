namespace NodeManager
{

    public class MsgPlantInteraction : RemoteQuery<NetPkgRemotePlantInteraction>
    {

        public enum PlantInteraction
        {
            Heal
        }

        private PlantInteraction Interaction;
        private float Value;

        public void Setup(Vector3i position,
            PlantInteraction interaction, float value)
        {
            base.Setup(position);
            Interaction = interaction;
            Value = value;
        }

        public override void ProcessOnWorker(PipeGridWorker worker)
        {
            if (worker.Manager.PlantsDict.TryGetValue(Position, out IPlant plant))
            {
                if (Interaction == PlantInteraction.Heal)
                    plant.ChangeHealth((int)Value);
                Log.Warning("Plant must be healed at {0} {1} {2}", Position,
                    Interaction, Value);
            }
        }

        public override void Read(PooledBinaryReader br)
        {
            base.Read(br);
            Interaction = (PlantInteraction)br.ReadByte();
            Value = br.ReadSingle();
        }

        public override void Write(PooledBinaryWriter bw)
        {
            base.Write(bw);
            bw.Write((byte)Interaction);
            bw.Write(Value);
        }

        protected override void SetupNetPkg(NetPkgRemotePlantInteraction pkg) => pkg.Setup(this);

    }

}

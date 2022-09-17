namespace NodeFacilitator
{

    //########################################################
    // A remote package for a specific block position.
    //########################################################

    public abstract class RemoteNodePackage<T> : RemoteBasePackage<T> where T : NetPackage
    {

        public Vector3i Position { get; protected set; } = Vector3i.zero;

        public virtual void Setup(Vector3i position)
        {
            Position = position;
        }

        // Used to setup incoming from network streams
        public override void Read(PooledBinaryReader br)
        {
            Position = new Vector3i(
                br.ReadInt32(),
                br.ReadInt32(),
                br.ReadInt32());
        }

        // Used to pass query through network streams
        public override void Write(PooledBinaryWriter bw)
        {
            bw.Write(Position.x);
            bw.Write(Position.y);
            bw.Write(Position.z);
        }

    }

    //########################################################
    //########################################################

}

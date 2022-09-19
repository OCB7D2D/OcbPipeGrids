namespace NodeManager
{
    public abstract class RemotePackage<T> where T : NetPackage
    {
        public Vector3i Position { get; private set; } = Vector3i.zero;

        // Used to setup incoming from interface
        public virtual void Setup(Vector3i position)
        {
            Position = position;
        }

        // Used to setup incoming from network streams
        public virtual void Read(PooledBinaryReader br)
        {
            Position = new Vector3i(
                br.ReadInt32(),
                br.ReadInt32(),
                br.ReadInt32());
        }

        // Used to pass query through network streams
        public virtual void Write(PooledBinaryWriter bw)
        {
            bw.Write(Position.x);
            bw.Write(Position.y);
            bw.Write(Position.z);
        }

        // Get package to send query over wire
        public NetPackage CreateNetPackage()
        {
            T pkg = NetPackageManager.GetPackage<T>();
            SetupNetPkg(pkg);
            return pkg;
        }

        // Used to setup package from message
        protected abstract void SetupNetPkg(T pkg);

        // Implemented by all specializations
        public abstract int GetLength();

    }
}

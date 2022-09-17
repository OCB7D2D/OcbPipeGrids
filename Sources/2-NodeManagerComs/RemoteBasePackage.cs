namespace NodeFacilitator
{

    //########################################################
    // A network package for NodeManager means that it can either
    // originate from a remote clients or from the local client.
    // It doesn't yet say if the message should be evaluated at
    // NodeManager worker thread or at the main thread directly.
    //########################################################

    public abstract class RemoteBasePackage<T> where T : NetPackage
    {

        // Get package to send query over wire
        public NetPackage CreateNetPackage()
        {
            T pkg = NetPackageManager.GetPackage<T>();
            SetupNetPkg(pkg);
            return pkg;
        }

        // Used to pass query through network streams
        public abstract void Read(PooledBinaryReader br);

        // Used to pass query through network streams
        public abstract void Write(PooledBinaryWriter bw);

        // Used to setup package from message
        protected abstract void SetupNetPkg(T pkg);

        // Implemented by all specializations
        public abstract int GetLength();

    }

    //########################################################
    //########################################################

}

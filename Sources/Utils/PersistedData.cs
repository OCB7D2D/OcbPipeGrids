using System;
using System.IO;
using UnityEngine;

public class PersistedData
{
    bool WasConfigLoaded = true;
    readonly IPersistable Persitable;

    // Temporary thread object when running
    private ThreadManager.ThreadInfo ThreadInfo;

    public PersistedData(IPersistable persitable)
    {
        Persitable = persitable;
    }

    public bool IsSaveThreadRunning()
        => ThreadInfo != null &&
           !ThreadInfo.HasTerminated();

    public void SaveThreaded()
    {
        if (!ConnectionManager.Instance.IsServer) return;
        if (!GameManager.Instance.gameStateManager.IsGameStarted()) return;
        if (!IsSaveThreadRunning()) StartSaveThreaded();
    }

    public void SaveSynchronous() => ExecuteSaveAction(stream => PersistToDisk(stream));

    private void StartSaveThreaded()
    {
        ExecuteSaveAction(stream =>
        {
            var manager = new ThreadManager.ThreadFunctionLoopDelegate(RunSaveThread);
            ThreadInfo = ThreadManager.StartThread(Persitable.GetThreadKey(),
                null, manager, null, _parameter: stream);
        });
    }

    private int RunSaveThread(ThreadManager.ThreadInfo _threadInfo) =>
        PersistToDisk((PooledExpandableMemoryStream)_threadInfo.parameter);

    private void ExecuteSaveAction(Action<PooledExpandableMemoryStream> storer)
    {
        // if (!ConnectionManager.Instance.IsServer) return;
        if (ThreadInfo != null && ThreadManager.ActiveThreads.ContainsKey(Persitable.GetThreadKey())) return;
        PooledExpandableMemoryStream expandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
        using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
        {
            pooledBinaryWriter.SetBaseStream(expandableMemoryStream);
            Persitable.Write(pooledBinaryWriter);
        }
        storer.Invoke(expandableMemoryStream);
    }

    private int PersistToDisk(PooledExpandableMemoryStream parameter)
    {
        // VehicleManager saving
        string fname = Persitable.GetStoragePath();
        if (File.Exists(fname))
        {
            // Abort if config exists but was not loaded
            if (WasConfigLoaded == false) return -1;
            File.Copy(fname, Persitable.GetBackupPath(), true);
        }
        parameter.Position = 0L;
        StreamUtils.WriteStreamToFile(parameter, fname);
        Log.Out("{0} saved {1} bytes",
            Persitable.GetPersistName(),
            parameter.Length);
        MemoryPools.poolMemoryStream.FreeSync(parameter);
        return -1;
    }

    public virtual void Cleanup()
    {
        if (ConnectionManager.Instance.IsServer)
        {
            Log.Out("Saving on close {0}", Persitable.GetStoragePath());
            StartSaveThreaded();
            // ToDo: Error if we can't safe!?
            // if (ThreadInfo == null) return;
            WasConfigLoaded = false;
            ThreadInfo.WaitForEnd();
            ThreadInfo = null;
        }
    }

    public void LoadPersistedData()
    {
        string storage = Persitable.GetStoragePath();
        // if (!File.Exists(storage)) return;
        try
        {
            using (FileStream fileStream = File.OpenRead(storage))
            {
                using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
                {
                    pooledBinaryReader.SetBaseStream(fileStream);
                    Persitable.Read(pooledBinaryReader);
                    WasConfigLoaded = true;
                }
            }
            Log.Out("{0} {2}, loaded {1}",
                Persitable.GetPersistName(),
                Persitable.GetLoadInfo(),
                storage);
        }
        catch (Exception)
        {
            Log.Warning("Loading {0} failed, trying backup ...",
                Persitable.GetPersistName());
            // ToDo: should reset partially loaded stuff?
            string backup = Persitable.GetBackupPath();
            if (!File.Exists(backup)) return;
            using (FileStream fileStream = File.OpenRead(backup))
            {
                using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
                {
                    pooledBinaryReader.SetBaseStream(fileStream);
                    Persitable.Read(pooledBinaryReader);
                    WasConfigLoaded = true;
                }
            }
            Log.Out("{0} {2}, loaded {1}",
                Persitable.GetPersistName(),
                Persitable.GetLoadInfo(),
                backup);
        }
    }

    private float SaveTime = 0;

    public void Update()
    {
        SaveTime -= Time.deltaTime;
        if (SaveTime > 0.0) return;
        if (IsSaveThreadRunning()) return;
        SaveTime = 120f;
        SaveThreaded();
    }

}

using System.IO;

public interface IPersistable
{

    string GetLoadInfo();
    string GetPersistName();
    string GetBackupPath();
    string GetStoragePath();
    string GetThreadKey();

    // Implement your write functionality
    void Write(BinaryWriter bw);

    // Implement your read functionality
    void Read(BinaryReader br);


}

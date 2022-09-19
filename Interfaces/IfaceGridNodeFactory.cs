using NodeManager;
using System.IO;

public interface IfaceGridNodeFactory
{
    uint StorageID { get; }
    PipeNode Create(BinaryReader br);
}
using NodeFacilitator;

public abstract class ImpBlockPipeReservoirPowered : ImpBlockGridNodePowered, IBlockReservoir
{
	public virtual float MaxFillState { get; set; } = 150f;
	public override bool BreakSegment => true;

    public ushort FluidType { get; set; }

    public override void Init()
	{
		base.Init();
		// Parse optional block XML setting properties
		// BlockConfig.InitConnection(this);
	}

}

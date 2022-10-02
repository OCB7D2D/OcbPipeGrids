public abstract class ImpBlockPipeReservoirUnpowered : ImpBlockGridNodeUnpowered, IBlockReservoir
{
	public virtual float MaxFillState { get; set; } = 150f;
	public override bool BreakDistance => true;
	public ushort FluidType { get; set; }

	public override void Init()
	{
		base.Init();
		// Parse optional block XML setting properties
		PipeBlockHelper.InitBlock(this);
	}

}

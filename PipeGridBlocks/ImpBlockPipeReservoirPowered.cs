using NodeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

abstract class ImpBlockPipeReservoirPowered : ImpBlockGridNodePowered, IBlockReservoir
{
	public virtual float MaxFillState { get; set; } = 150f;
	public override bool BreakDistance => true;

    public ushort FluidType { get; set; }

    public override void Init()
	{
		base.Init();
		// Parse optional block XML setting properties
		BlockConfig.InitConnection(this);
	}

}

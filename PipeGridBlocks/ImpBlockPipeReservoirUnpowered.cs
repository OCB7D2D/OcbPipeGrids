using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

abstract class ImpBlockPipeReservoirUnpowered : ImpBlockGridNodeUnpowered, IBlockReservoir
{
	public virtual float MaxFillState { get; set; } = 150f;
	public override bool BreakDistance => true;

	public override void Init()
	{
		base.Init();
		// Parse optional block XML setting properties
		PipeBlockHelper.InitBlock(this);
	}

}

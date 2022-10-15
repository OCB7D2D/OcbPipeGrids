using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NodeManager
{

    public interface IWorldPos
    {
        Vector3i WorldPos { get; }
    }

    public interface IWorldRotated : IWorldPos
    {
        byte Rotation { get; }
    }

    public interface IWorldLink<T> : IWorldPos
    {
        void AddLink(T item);
    }

    // Item that has reach (potentially rotated)
    public interface IReacher : IReacherBlock, IWorldRotated
    {
        bool IsInReach(Vector3i target);
        Vector3i Dimensions { get; }
        Vector3i RotatedReach { get; }
        Vector3i RotatedOffset { get; }
    }

    public interface IReacherBlock// : IBlockNode
    {
        IBlockNode IBLK { get; }
        Vector3i BlockReach { get; set; }
        Vector3i ReachOffset { get; set; }
        Color BoundHelperColor { get; set; }
        Color ReachHelperColor { get; set; }
    }


    public interface IFilled
    {
        float FillState { get; set; }
    }

    public interface IHasPlant { IPlant Plant { get; set; } }
    public interface IHasPower { bool IsPowered { get; set; } }
    public interface IHasWells : IWorldLink<IWell> { HashSet<IWell> Wells { get; } }
    public interface IHasSoils : IWorldLink<ISoil> { HashSet<ISoil> Soils { get; } }
    public interface IHasPlants : IWorldLink<IPlant> { HashSet<IPlant> Plants { get; } }
    public interface IHasIrrigator : IWorldLink<IIrrigator> { HashSet<IIrrigator> Irrigators { get; } }
    public interface IHasComposters : IWorldLink<IComposter> { HashSet<IComposter> Composters { get; } }

    // The composter has soils linked that will use compost from it

    public interface IComposter : IHasSoils, IFilled, IReacher, IEqualityComparer<NodeBase> {}


    // So far a well is pretty much the same interface as the composter, so use it
    // It additionally has the SunLight and the linked irrigators we consume from
    public interface IWell : IHasIrrigator, IComposter, ISunLight, IEqualityComparer<NodeBase>
    {
    }

    public interface ISoil : IHasPlant, IHasWells, IHasComposters,
        ISunLight, ITickable, IfaceGridNodeManaged, IEqualityComparer<NodeBase>
    {
        float WaterState { get; }
        float SoilState { get; }
    }



    public interface IPlant : IHasPlants, ISunLight, ITickable, IfaceGridNodeManaged, IEqualityComparer<NodeBase>
    {
        int Illness { get; }
    }

    public interface IIrrigator : IHasWells, IFilled, IReacher, IHasPower, ITickable, IfaceGridNodeManaged, IEqualityComparer<NodeBase>
    {

    }


}
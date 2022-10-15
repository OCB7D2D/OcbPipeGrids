using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NodeManager
{
    class PlantHelper
    {
        public static float ConsumeMurkyWater(ulong delta,
            HashSet<IWell> wells, float factor)
        {

            var tscale = NodeManager.TimeScale(delta);

            // This gives a variation from 0.4 to 0.8
            float want = Mathf.Pow(0.5f, 1f / wells.Count) * 0.8f;

            Log.Out("Wanting {0}", want);

            // Consume more 
            if (factor > 0.5) want *= factor;

            var taken = WellHelper.ConsumeFluids(wells, want * tscale);

            Log.Out("Taken {0} , inv {1}", taken, taken / tscale);
            var score = taken / factor / tscale * 3;

            // Consume some rain (or maximal all of it)
            //float rain = Mathf.Min(scale * 10f, 1f);
            //taken += CurrentRain * scale * rain;
            //CurrentRain -= CurrentRain * scale * rain;

            factor = WellHelper.MixFluids(
                factor, score, delta, 12000);

            return factor;
        }

        public static float ConsumeCompost(ulong delta,
            HashSet<IComposter> composters, float factor)
        {
            return factor;
        }


        private static float ConsumeFrom<T> (HashSet<T> providers, float wanted, float single) where T : IFilled
        {
            float having = 0; float taken = 0;
            // debugger;
            foreach (var provider in providers)
            {
                having += Mathf.Min(provider.FillState, single);
            }
            if (having == 0) return 0;
            float taking = Mathf.Min(having, wanted);
            foreach (var provider in providers)
            {
                float took = Mathf.Min(provider.FillState, single) * taking / having;
                provider.FillState -= took;
                //if (isNaN(took)) debugger;
                taken += took;
            }
            //if (taken - 0.0001 > wanted) debugger;
            return taken;
        }


        internal static float TickFactor<T>(ulong delta, HashSet<T> providers, float WaterImprovementFactor,
            float WaterMaintenanceFactor, float WaterMaintenanceExponent, float min, float max, float WaterState)
                where T : IFilled
        {

            // We don't want to have this factor linear
            // Reaching perfect soil needs exponential care
            float cost = Mathf.Pow(WaterState,
                WaterMaintenanceExponent);
            // Multiply cost with time factors
            cost *= WaterMaintenanceFactor * delta;

            // Take maintenance
            WaterState -= cost;

            Log.Out("Costing {0}", cost);

            // Note: might be always true?
            if (WaterState < max)
            {
                // Get the soil required to make us perfect
                float required = max - WaterState;
                // Apply time time factors to prolonge improvements
                float wanted = required * delta * WaterImprovementFactor;
                // Add maintenance costs
                wanted += cost;

                // Give a little bonus if we have multiple wells?
                float amount = wanted * Mathf.Pow(providers.Count, 0.3f);
                // Don't take more than needed
                wanted = Mathf.Min(wanted, required);
                amount = Mathf.Min(amount, required);
                // Try to consume `amount` from wells
                WaterState += ConsumeFrom(providers, amount, wanted);
            }

            // consumedWater += FarmLand.TickWater(delta,
            //     MaintenanceCost, ref WaterState);
            if (WaterState < min) return min;
            else if (WaterState > max) return max;
            else return WaterState;

        }

    }
}

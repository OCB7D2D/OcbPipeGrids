using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NodeFacilitator
{
    class PlantHelper
    {
        /*
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

        */

        private static float ConsumeFrom<T> (IEnumerable<T> providers, float wanted, float single) where T : IFilled
        {
            float having = 0; float taken = 0;
            // debugger;
            foreach (var provider in providers)
            {
                having += Mathf.Min(provider.FillLevel, single);
            }
            if (having == 0) return 0;
            float taking = Mathf.Min(having, wanted);
            foreach (var provider in providers)
            {
                float took = Mathf.Min(provider.FillLevel, single) * taking / having;
                provider.FillLevel -= took;
                //if (isNaN(took)) debugger;
                taken += took;
            }
            //if (taken - 0.0001 > wanted) debugger;
            return taken;
        }


        internal static float TickFactor<T>(ulong delta, ICollection<T> providers,
            MaintenanceOptions options, float state,
            RangeOptions range, float speed = 1f)
                where T : IFilled
        {

            // We don't want to have this factor linear
            // Reaching perfect soil needs exponential care
            float cost = Mathf.Pow(state,
                options.MaintenanceExponent);
            // Multiply cost with time factors
            cost *= options.MaintenanceFactor * delta;

            // Take maintenance
            state -= cost;

            // Log.Out("Costing {0}", cost);

            // Note: might be always true?
            if (state < range.Max)
            {
                // Get the soil required to make us perfect
                float required = range.Max - state;
                // Apply time time factors to prolonge improvements
                float wanted = required * delta * options.ImprovementFactor * speed;
                // Add maintenance costs
                wanted += cost;

                // Give a little bonus if we have multiple wells?
                float amount = wanted * Mathf.Pow(providers.Count, 0.3f);
                // Don't take more than needed
                wanted = Mathf.Min(wanted, required);
                amount = Mathf.Min(amount, required);
                // Try to consume `amount` from wells
                state += ConsumeFrom(providers, amount, wanted);
            }

            // consumedWater += FarmLand.TickWater(delta,
            //     MaintenanceCost, ref WaterState);
            if (state < range.Min) return range.Min;
            else if (state > range.Max) return range.Max;
            else return state;
        }

        internal static float ConsumeFactor(ulong delta, IFilled source,
            MaintenanceOptions options, float state,
            RangeOptions range, float speed = 1f)
        {
            // We don't want to have this factor linear
            // Reaching perfect soil needs exponential care
            float cost = Mathf.Pow(state,
                options.MaintenanceExponent);
            // Multiply cost with time factors
            cost *= options.MaintenanceFactor * delta;

            // Take maintenance
            state -= cost;

            // Note: might be always true?
            if (state < range.Max)
            {
                // Get the soil required to make us perfect
                float required = range.Max - state;
                // Apply time time factors to prolonge improvements
                float wanted = required * delta * options.ImprovementFactor * speed;
                // Add maintenance costs
                wanted += cost;
                // Try to consume `wanted` amount
                var amount = Math.Min(source.FillLevel,
                    Mathf.Min(wanted, required));
                source.FillLevel -= amount;
                state += amount;
            }

            if (state < range.Min) return range.Min;
            else if (state > range.Max) return range.Max;
            else return state;
        }
    }
}

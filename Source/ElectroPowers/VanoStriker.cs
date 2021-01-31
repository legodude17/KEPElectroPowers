using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ElectroPowers
{
    public class VanoStriker : Thing
    {
        private List<int> lightningTicks;

        public override void Tick()
        {
            var num = lightningTicks.RemoveAll(i => i <= Find.TickManager.TicksGame);
            if (num <= 0) return;
            for (var i = 0; i < num; i++)
                Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Map, Position));
            Rotation = Rot4.FromAngleFlat(Rotation.AsAngle + 0.2f);
            if (lightningTicks.Count == 0) Destroy();
        }

        public override void PostMake()
        {
            base.PostMake();
            lightningTicks = new List<int>();
            var num = Rand.Range(4, 10);
            for (var i = 0; i < num; i++) lightningTicks.Add(Find.TickManager.TicksGame + Rand.Range(i * 10, i * 40));
        }
    }
}
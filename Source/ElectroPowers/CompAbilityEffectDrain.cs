using RimWorld;
using Verse;

namespace ElectroPowers
{
    public class CompAbilityEffectDrain : CompAbilityEffect
    {
        public new DrainProps Props => props as DrainProps;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Log.Message("target: " + target);
            base.Apply(target, dest);
            var health = parent.pawn.health;
            Log.Message("health: " + health);
            var hediff = health.hediffSet.GetFirstHediffOfDef(EPDefOf.ElectroDrain);
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(EPDefOf.ElectroDrain, parent.pawn, health.hediffSet.GetBrain());
                hediff.Severity = Props.instantComa ? 4 : 1;
                health.AddHediff(hediff, health.hediffSet.GetBrain());
            }
            else
            {
                if (Props.instantComa) hediff.Severity = 4;
                else hediff.Severity += 1;
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Log.Message("target: " + target);
            return base.CanApplyOn(target, dest);
        }
    }

    public class DrainProps : AbilityCompProperties
    {
        public bool instantComa = false;

        public DrainProps()
        {
            compClass = typeof(CompAbilityEffectDrain);
        }
    }
}
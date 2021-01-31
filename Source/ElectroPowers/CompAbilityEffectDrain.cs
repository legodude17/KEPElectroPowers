using RimWorld;
using Verse;

namespace ElectroPowers
{
    public class CompAbilityEffectDrain : CompAbilityEffect
    {
        public new DrainProps Props => props as DrainProps;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var health = parent.pawn.health;
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
    }

    public class DrainProps : CompProperties_AbilityEffect
    {
        // ReSharper disable once InconsistentNaming
        public bool instantComa = false;

        public DrainProps()
        {
            compClass = typeof(CompAbilityEffectDrain);
        }
    }
}
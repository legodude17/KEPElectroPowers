using RimWorld;
using Verse;

namespace ElectroPowers
{
    public class CompAbilityEffect_BreakShield : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Log.Message("target: " + target);
            base.Apply(target, dest);
            var pawn = target.Pawn;
            Log.Message("Pawn: " + pawn);
            foreach (var apparel in pawn.apparel.WornApparel)
                if (apparel is ShieldBelt belt)
                {
                    belt.CheckPreAbsorbDamage(new DamageInfo(DamageDefOf.EMP, 1f, instigator: parent.pawn));
                    MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, EPDefOf.Mote_NerveAbility);
                }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Log.Message("target: " + target);
            return base.CanApplyOn(target, dest);
        }
    }
}
using RimWorld;
using Verse;

namespace ElectroPowers
{
    public class HediffComp_Ability : HediffComp
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            parent.pawn.abilities.GainAbility((props as HediffCompProperties_Ability)?.Ability);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            parent.pawn.abilities.RemoveAbility((props as HediffCompProperties_Ability)?.Ability);
        }
    }

    public class HediffCompProperties_Ability : HediffCompProperties
    {
        public AbilityDef Ability;

        public HediffCompProperties_Ability()
        {
            compClass = typeof(HediffComp_Ability);
        }
    }
}
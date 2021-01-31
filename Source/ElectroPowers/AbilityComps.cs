using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ElectroPowers
{
    public class CompAbilityEffect_BreakShield : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var pawn = target.Pawn;
            foreach (var apparel in pawn.apparel.WornApparel)
                if (apparel is ShieldBelt belt)
                    belt.CheckPreAbsorbDamage(new DamageInfo(DamageDefOf.EMP, 1f, instigator: parent.pawn));
        }
    }

    public class CompAbilityEffect_EMP : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var thing = target.Thing;
            thing.TakeDamage(new DamageInfo(DamageDefOf.EMP, 20f, instigator: parent.pawn));
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return base.CanApplyOn(target, dest) && target.HasThing &&
                   (target.Pawn != null && target.Pawn.RaceProps.IsMechanoid ||
                    target.Thing is Building_Turret turret);
        }
    }

    public class CompProperties_ApplyHediffToSelfAndTarget : CompProperties_AbilityEffect
    {
        public StatDef durationMultiplier;
        public bool onlyBrain = true;
        public int selfDurationMultiplyer = 1;
        public HediffDef toSelf;
        public HediffDef toTarget;
    }

    public class CompAbilityEffect_ApplyHeddifToSelfAndTarget : CompAbilityEffect
    {
        public CompProperties_ApplyHediffToSelfAndTarget Props =>
            props as CompProperties_ApplyHediffToSelfAndTarget;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Apply(parent.pawn, Props.toSelf);
            if (target.Pawn != null && !(target.Pawn == parent.pawn && Props.toTarget == Props.toSelf))
                Apply(target.Pawn, Props.toTarget);
        }

        private void Apply(Pawn pawn, HediffDef hd)
        {
            var hediff = HediffMaker.MakeHediff(hd, pawn,
                Props.onlyBrain ? pawn.health.hediffSet.GetBrain() : null);
            var hcd = hediff.TryGetComp<HediffComp_Disappears>();
            if (hcd != null)
                hcd.ticksToDisappear = GetDurationSeconds(pawn).SecondsToTicks();

            pawn.health.AddHediff(hediff, null, null);
        }

        public float GetDurationSeconds(Pawn target)
        {
            var num = parent.def.statBases.GetStatValueFromList(StatDefOf.Ability_Duration, 10f);
            if (Props.durationMultiplier != null) num *= target.GetStatValue(Props.durationMultiplier);

            if (target == parent.pawn && Props.selfDurationMultiplyer != 0) num *= Props.selfDurationMultiplyer;

            return num;
        }
    }

    public class CompProperties_GameCondition : CompProperties_AbilityEffectWithDuration
    {
        // ReSharper disable once InconsistentNaming
        public GameConditionDef condition;

        public CompProperties_GameCondition()
        {
            compClass = typeof(CompAbilityEffect_GameCondition);
        }
    }

    public class CompAbilityEffect_GameCondition : CompAbilityEffect_WithDuration
    {
        public new CompProperties_GameCondition Props =>
            props as CompProperties_GameCondition;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            parent.pawn.Map.gameConditionManager.RegisterCondition(
                GameConditionMaker.MakeCondition(Props.condition, GetDurationSeconds(target.Pawn).SecondsToTicks()));
        }
    }

    public class GameCondition_VanoRain : GameCondition
    {
        public override WeatherDef ForcedWeather()
        {
            return WeatherDef.Named("Rain");
        }
    }

    public class CompAbilityEffect_Explosions : CompAbilityEffect
    {
        public new CompProperties_Explosions Props => props as CompProperties_Explosions;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            foreach (var explosion in Props.explosions)
            {
                Log.Message(explosion.type + " " + explosion.radius + " " + explosion.damage);
                GenExplosion.DoExplosion(target.Cell, parent.pawn.Map, explosion.radius, explosion.type, parent.pawn,
                    explosion.damage);
            }
        }
    }

    public class CompProperties_Explosions : CompProperties_AbilityEffect
    {
        public List<ExplosionDef> explosions;

        public CompProperties_Explosions()
        {
            compClass = typeof(CompAbilityEffect_Explosions);
        }

        public override IEnumerable<string> ConfigErrors(AbilityDef parentDef)
        {
            foreach (var configError in base.ConfigErrors(parentDef)) yield return configError;

            foreach (var explosion in explosions)
            {
                if (explosion.type == null) yield return "Invalid DamageDef in ExplosionDef";

                if (explosion.radius <= 0f) yield return "Invalid radius " + explosion.radius + " in ExplosionDef";
            }
        }
    }

    public class ExplosionDef
    {
        public int damage = 1;
        public float radius;
        public DamageDef type;
    }
}
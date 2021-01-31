// HediffComp_Ability.cs by Joshua Bennett
// 
// Created 2021-01-25

using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

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

    [StaticConstructorOnStartup]
    public class HediffComp_Shield : HediffComp, IHediffDraw, IHediffDamage, IHediffGizmo
    {
        private static readonly Material BubbleMat =
            MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

        private float energy;
        private Vector3 impactAngleVect;
        private int lastAbsorbDamageTick;
        private ShieldState state;
        private int ticksToReset;
        public HediffCompProperties_Shield Props => props as HediffCompProperties_Shield;

        public bool PreAbsorbDamage(DamageInfo dinfo)
        {
            if (state != ShieldState.Active) return false;

            if (dinfo.Def == DamageDefOf.EMP)
            {
                energy = 0f;
                Break();
                return false;
            }

            if (!dinfo.Def.isRanged && !dinfo.Def.isExplosive) return false;
            energy -= dinfo.Amount * Props.dischargePerDamage;
            if (energy < 0f)
            {
                Break();
            }
            else
            {
                SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(parent.pawn.Position,
                    parent.pawn.Map));
                impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
                var loc = parent.pawn.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
                var num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
                MoteMaker.MakeStaticMote(loc, parent.pawn.Map, ThingDefOf.Mote_ExplosionFlash, num);
                var num2 = (int) num;
                for (var i = 0; i < num2; i++)
                    MoteMaker.ThrowDustPuff(loc, parent.pawn.Map, Rand.Range(0.8f, 1.2f));

                lastAbsorbDamageTick = Find.TickManager.TicksGame;
            }

            return true;
        }

        public void DrawExtras()
        {
            if (state == ShieldState.Active && parent.pawn.Spawned && !parent.pawn.Downed && !parent.pawn.Dead &&
                (parent.pawn.InAggroMentalState || parent.pawn.Drafted ||
                 parent.pawn.Faction.HostileTo(Faction.OfPlayer) && !parent.pawn.IsPrisoner ||
                 lastAbsorbDamageTick < Find.TickManager.TicksGame + 60))
            {
                var num = Mathf.Lerp(1.2f, 1.55f, energy);
                var vector = parent.pawn.Drawer.DrawPos;
                vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                var num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    var num3 = (8 - num2) / 8f * 0.05f;
                    vector += impactAngleVect * num3;
                    num -= num3;
                }

                float angle = Rand.Range(0, 360);
                var s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default;
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
            }
        }

        public IEnumerable<Gizmo> GetGizmos()
        {
            if (Find.Selector.SingleSelectedThing == parent.pawn)
                yield return new Gizmo_LevelReadout
                {
                    Label = parent.def.LabelCap,
                    Value = energy,
                    MaxValue = Props.maxEnergy
                };
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            energy = Props.maxEnergy;
            state = ShieldState.Active;
            ticksToReset = -1;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref energy, "energy");
            Scribe_Values.Look(ref state, "state");
            Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (state == ShieldState.Resetting)
            {
                ticksToReset--;
                if (ticksToReset <= 0)
                {
                    ticksToReset = -1;
                    state = ShieldState.Active;
                    energy = Props.energyOnReset;
                    if (parent.pawn.Spawned)
                    {
                        SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(parent.pawn.Position,
                            parent.pawn.Map));
                        MoteMaker.ThrowLightningGlow(parent.pawn.TrueCenter(), parent.pawn.Map, 3f);
                    }
                }
            }

            if (state == ShieldState.Active && energy < Props.maxEnergy && Props.canRecharge)
                energy += Props.rechargeRate / 60f;
            if (energy > Props.maxEnergy) energy = Props.maxEnergy;
        }

        private void Break()
        {
            SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(parent.pawn.Position, parent.pawn.Map));
            MoteMaker.MakeStaticMote(parent.pawn.TrueCenter(), parent.pawn.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
            for (var i = 0; i < 6; i++)
                MoteMaker.ThrowDustPuff(
                    parent.pawn.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) *
                    Rand.Range(0.3f, 0.6f), parent.pawn.Map, Rand.Range(0.8f, 1.2f));

            energy = 0f;
            ticksToReset = 3200;
            state = ShieldState.Resetting;

            if (!Props.canRecharge) parent.pawn.health.RemoveHediff(parent);
        }
    }

    public class HediffCompProperties_Shield : HediffCompProperties
    {
        public HediffCompProperties_Shield()
        {
            compClass = typeof(HediffComp_Shield);
        }

        // ReSharper disable InconsistentNaming
        public bool canRecharge = true;
        public float dischargePerDamage = 1;
        public float energyOnReset = 0;
        public float maxEnergy = 100;

        public float rechargeRate = 1;
        // ReSharper restore InconsistentNaming
    }
}
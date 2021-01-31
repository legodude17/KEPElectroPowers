using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ElectroPowers
{
    [HarmonyPatch]
    public class MoreHediffOptions
    {
        [HarmonyPatch(typeof(Pawn_HealthTracker), "PreApplyDamage")]
        [HarmonyPostfix]
        public static void HediffDamage(DamageInfo dinfo, ref bool absorbed, Pawn_HealthTracker __instance)
        {
            absorbed = absorbed || __instance.hediffSet.hediffs.OfType<HediffWithComps>()
                .SelectMany(hediff => hediff.comps).Any(comp => comp is IHediffDamage hd && hd.PreAbsorbDamage(dinfo));
        }

        [HarmonyPatch(typeof(Pawn), "GetGizmos")]
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> HediffGizmo(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            return __result.Concat(__instance.health.hediffSet.hediffs.OfType<HediffWithComps>()
                .SelectMany(hediff => hediff.comps).OfType<IHediffGizmo>().SelectMany(hg => hg.GetGizmos()));
        }

        [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", typeof(Vector3), typeof(float), typeof(bool),
            typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool))]
        [HarmonyPostfix]
        public static void IHediffDraw(bool portrait, Pawn ___pawn)
        {
            if (portrait) return;
            foreach (var hd in ___pawn.health.hediffSet.hediffs.OfType<HediffWithComps>()
                .SelectMany(hediff => hediff.comps).OfType<IHediffDraw>()) hd.DrawExtras();
        }
    }

    public interface IHediffDraw
    {
        void DrawExtras();
    }

    public interface IHediffDamage
    {
        bool PreAbsorbDamage(DamageInfo dinfo);
    }

    public interface IHediffGizmo
    {
        IEnumerable<Gizmo> GetGizmos();
    }
}
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ElectroPowers
{
    public class CompUseEffect_RandomHediff : CompUseEffect
    {
        public RandomHediffProps Props => props as RandomHediffProps;

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            if (!base.CanBeUsedBy(p, out failReason)) return false;
            var part = p.RaceProps.body.GetPartsWithDef(Props.bodyPart).FirstOrDefault();
            if (part == null)
            {
                failReason = p + " does not have a " + Props.bodyPart.LabelCap;
                return false;
            }

            if (Props.hediffs.TrueForAll(hd => p.health.hediffSet.HasHediff(hd, part)))
            {
                failReason = p + " already has all abilities";
                return false;
            }

            return true;
        }

        public override void DoEffect(Pawn usedBy)
        {
            var part = usedBy.RaceProps.body.GetPartsWithDef(Props.bodyPart).FirstOrDefault();
            var def = Props.hediffs.Where(hd => !usedBy.health.hediffSet.HasHediff(hd, part)).RandomElement();
            usedBy.health.AddHediff(def, part);
        }
    }

    public class RandomHediffProps : CompProperties
    {
        public BodyPartDef bodyPart;
        public List<HediffDef> hediffs;

        public RandomHediffProps()
        {
            compClass = typeof(CompUseEffect_RandomHediff);
        }
    }
}
using RimWorld;
using UnityEngine;
using Verse;

namespace ElectroPowers
{
    public class VanoExplosion : Thing
    {
        private float angleX;
        private float angleY;
        private float rotAX = 0.1f;
        private float rotAY = 0.1f;
        private float rotVX = 0.1f;
        private float rotVY = 0.1f;
        private float scale = 5f;

        public override void Tick()
        {
            rotAX += 0.1f;
            rotVX += rotAX;
            angleX += rotVX;
            if (angleX >= 2 * Mathf.PI) angleX = 0;
            rotAX += 0.1f;
            rotVX += rotAX;
            angleX += rotVX;
            if (angleX >= 2 * Mathf.PI) angleX = 0;
            scale -= 0.1f;
            if (scale <= 0.1f)
            {
                GenExplosion.DoExplosion(Position, Map, 39.9f, DamageDefOf.Bomb, this);
                Destroy();
            }

            Rotation = Rot4.FromAngleFlat(Rotation.AsAngle + rotVX);
            Graphic.drawSize = new Vector2(scale, scale);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref angleX, "angle");
            Scribe_Values.Look(ref rotAX, "rotA");
            Scribe_Values.Look(ref rotVX, "rotV");
            Scribe_Values.Look(ref angleY, "angle");
            Scribe_Values.Look(ref rotAY, "rotA");
            Scribe_Values.Look(ref rotVY, "rotV");
            Scribe_Values.Look(ref scale, "scale");
        }
    }
}
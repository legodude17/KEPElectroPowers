using HarmonyLib;
using Verse;

namespace ElectroPowers
{
    internal class ElectroPowersMod : Mod
    {
        public ElectroPowersMod(ModContentPack content) : base(content)
        {
            var harm = new Harmony("kep.electropowers");
            harm.PatchAll();
            Log.Message("Applied patches for " + harm.Id);
        }
    }
}
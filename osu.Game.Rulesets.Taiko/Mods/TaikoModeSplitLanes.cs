// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModeSplitLanes : Mod, IPlayfieldTypeMod
    {
        public override string Name => @"Slit Lanes";
        public override string Acronym => @"SL";
        public override LocalisableString Description => @"One lane for dons, one lane for kats";
        public override double ScoreMultiplier => 1.0;
        public override bool Ranked => false;
        public override ModType Type => ModType.Conversion;
        public PlayfieldType PlayfieldType => PlayfieldType.Split;
    }
}

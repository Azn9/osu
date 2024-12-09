// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class SplitTaikoPlayfield : ScrollingPlayfield
    {
        private TaikoPlayfield? donsPlayfield;
        private TaikoPlayfield? katsPlayfield;

        [BackgroundDependencyLoader]
        private void load()
        {
            donsPlayfield = new TaikoPlayfield()
            {
                Depth = 1,
            };
            katsPlayfield = new TaikoPlayfield()
            {
                Position = new Vector2(0, TaikoPlayfield.BASE_HEIGHT), // Just below the dons playfield
                Depth = 1, // To avoid swells being rendered behind the second playfield
            };

            AddNested(donsPlayfield);
            AddNested(katsPlayfield);

            InternalChildren = new[]
            {
                donsPlayfield,
                katsPlayfield
            };
        }

        #region Pooling support

        public override void Add(HitObject h)
        {
            switch (h)
            {
                case BarLine barLine: // Add bar lines to both playfields
                    donsPlayfield?.Add(barLine);
                    katsPlayfield?.Add(barLine);
                    break;

                case Hit hit:
                    if (hit.Type == HitType.Centre)
                    {
                        donsPlayfield?.Add(hit);
                    }
                    else // if (hit.Type == HitType.Rim)
                    {
                        katsPlayfield?.Add(hit);
                    }

                    break;

                default:
                    donsPlayfield?.Add(h);
                    break;
            }
        }

        public override bool Remove(HitObject h)
        {
            switch (h)
            {
                case BarLine barLine:
                    return (donsPlayfield?.Remove(barLine) ?? false) && (katsPlayfield?.Remove(barLine) ?? false);

                case Hit hit:
                    if (hit.Type == HitType.Centre)
                    {
                        return donsPlayfield?.Remove(hit) ?? false;
                    }
                    else // if (hit.Type == HitType.Rim)
                    {
                        return katsPlayfield?.Remove(hit) ?? false;
                    }

                default:
                    return donsPlayfield?.Remove(h) ?? false;
            }
        }

        #endregion

        #region Non-pooling support

        public override void Add(DrawableHitObject h)
        {
            switch (h)
            {
                case DrawableBarLine barLine: // Add bar lines to both playfields
                    donsPlayfield?.Add(barLine);
                    katsPlayfield?.Add(barLine);
                    break;

                case DrawableTaikoHitObject drawableHitObject:
                    if (drawableHitObject.HitObject is Hit hit)
                    {
                        if (hit.Type == HitType.Centre)
                        {
                            donsPlayfield?.Add(h);
                        }
                        else // if (hit.Type == HitType.Rim)
                        {
                            katsPlayfield?.Add(h);
                        }

                        break;
                    }
                    else // For everything else, add to the dons playfield
                    {
                        donsPlayfield?.Add(h);
                    }

                    break;

                default:
                    donsPlayfield?.Add(h);
                    break;
            }
        }

        public override bool Remove(DrawableHitObject h)
        {
            switch (h)
            {
                case DrawableBarLine barLine:
                    return (donsPlayfield?.Remove(h) ?? false) && (katsPlayfield?.Remove(barLine) ?? false);

                case DrawableTaikoHitObject drawableHitObject:
                    if (drawableHitObject.HitObject is not TaikoHitObject taikoHitObject)
                        return donsPlayfield?.Remove(h) ?? false;

                    if (taikoHitObject is not Hit hit)
                        return donsPlayfield?.Remove(h) ?? false;

                    if (hit.Type == HitType.Centre)
                    {
                        return donsPlayfield?.Remove(h) ?? false;
                    }
                    else // if (hit.Type == HitType.Rim)
                    {
                        return katsPlayfield?.Remove(h) ?? false;
                    }

                default:
                    return donsPlayfield?.Remove(h) ?? false;
            }
        }

        #endregion
    }
}

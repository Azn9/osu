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
    public partial class DualTaikoPlayfield : ScrollingPlayfield
    {
        private TaikoPlayfield? donsPlayfield;
        private TaikoPlayfield? katsPlayfield;

        [BackgroundDependencyLoader]
        private void load()
        {
            donsPlayfield = new TaikoPlayfield();
            katsPlayfield = new TaikoPlayfield()
            {
                Position = new Vector2(0, TaikoPlayfield.BASE_HEIGHT)
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
                case BarLine barLine:
                    donsPlayfield?.Add(barLine);
                    katsPlayfield?.Add(barLine);
                    break;

                case Hit hit:
                    if (hit.Type == HitType.Centre)
                    {
                        donsPlayfield?.Add(hit);
                    }
                    else
                    {
                        katsPlayfield?.Add(hit);
                    }

                    break;

                default:
                    base.Add(h);
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

                    return katsPlayfield?.Remove(hit) ?? false;

                default:
                    return base.Remove(h);
            }
        }

        #endregion

        #region Non-pooling support

        public override void Add(DrawableHitObject h)
        {
            switch (h)
            {
                case DrawableBarLine barLine:
                    donsPlayfield?.Add(barLine);
                    katsPlayfield?.Add(barLine);
                    break;

                case DrawableTaikoHitObject drawableHitObject:
                    if (drawableHitObject.HitObject is TaikoHitObject taikoHitObject)
                    {
                        if (taikoHitObject is Hit hit)
                        {
                            if (hit.Type == HitType.Centre)
                            {
                                donsPlayfield?.Add(h);
                            }
                            else
                            {
                                katsPlayfield?.Add(h);
                            }
                        }
                        else
                        {
                            base.Add(h);
                        }
                    }
                    else
                    {
                        base.Add(h);
                    }

                    break;

                default:
                    base.Add(h);
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
                    if (drawableHitObject.HitObject is TaikoHitObject taikoHitObject)
                    {
                        if (taikoHitObject is Hit hit)
                        {
                            if (hit.Type == HitType.Centre)
                            {
                                return donsPlayfield?.Remove(h) ?? false;
                            }

                            return katsPlayfield?.Remove(h) ?? false;
                        }

                        return base.Remove(h);
                    }

                    return base.Remove(h);

                default:
                    return base.Remove(h);
            }
        }

        #endregion
    }
}

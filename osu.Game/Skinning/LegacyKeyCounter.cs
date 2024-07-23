﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Skinning
{
    public partial class LegacyKeyCounter : KeyCounter
    {
        public bool UsesFixedAnchor { get; set; }

        public float TransitionDuration { get; set; } = 150f;

        public Colour4 KeyTextColour { get; set; } = Colour4.White;

        public Colour4 KeyDownBackgroundColour { get; set; } = Colour4.Yellow;

        public Colour4 KeyUpBackgroundColour { get; set; } = Colour4.White;

        private float keyTextRotation = 0f;

        public float KeyTextRotation
        {
            get => keyTextRotation;
            set
            {
                keyTextRotation = value;
                overlayKeyText.Rotation = value;
            }
        }

        private Container keyContainer = null!;

        private OsuSpriteText overlayKeyText = null!;

        private Sprite keySprite = null!;

        public LegacyKeyCounter(InputTrigger trigger)
            : base(trigger)
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Child = keyContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Children = new Drawable[]
                {
                    keySprite = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        BypassAutoSizeAxes = Axes.Both,
                        Rotation = -90,
                    },
                    overlayKeyText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = trigger.Name,
                        Colour = KeyTextColour,
                        Font = OsuFont.Default.With(fixedWidth: true),
                        Rotation = KeyTextRotation
                    },
                }
            };

            // Legacy key counter size
            Height = Width = 48 * 0.95f;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource source)
        {
            Texture? keyTexture = source.GetTexture($"inputoverlay-key");

            if (keyTexture != null)
                keySprite.Texture = keyTexture;
        }

        protected override void Activate(bool forwardPlayback = true)
        {
            base.Activate(forwardPlayback);
            keyContainer.ScaleTo(0.75f, TransitionDuration);
            keyContainer.FadeColour(KeyDownBackgroundColour, TransitionDuration);
            overlayKeyText.Text = CountPresses.Value.ToString();
        }

        protected override void Deactivate(bool forwardPlayback = true)
        {
            base.Deactivate(forwardPlayback);
            keyContainer.ScaleTo(1f, TransitionDuration);
            keyContainer.FadeColour(KeyUpBackgroundColour, TransitionDuration);
        }
    }
}

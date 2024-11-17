// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class PerformancePointsCounter : RollingCounter<int>
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; }

        [Resolved]
        private GameplayState gameplayState { get; set; }

        [CanBeNull]
        private List<TimedDifficultyAttributes> timedAttributes;

        private readonly CancellationTokenSource loadCancellationSource = new CancellationTokenSource();

        private JudgementResult lastJudgement;
        private PerformanceCalculator performanceCalculator;
        private ScoreInfo scoreInfo;

        private Mod[] clonedMods;

        [BackgroundDependencyLoader]
        private void load(BeatmapDifficultyCache difficultyCache)
        {
            if (gameplayState != null)
            {
                performanceCalculator = gameplayState.Ruleset.CreatePerformanceCalculator(IsIncrementing);
                clonedMods = gameplayState.Mods.Select(m => m.DeepClone()).ToArray();

                scoreInfo = new ScoreInfo(gameplayState.Score.ScoreInfo.BeatmapInfo, gameplayState.Score.ScoreInfo.Ruleset) { Mods = clonedMods };

                var gameplayWorkingBeatmap = new GameplayWorkingBeatmap(gameplayState.Beatmap);
                difficultyCache.GetTimedDifficultyAttributesAsync(gameplayWorkingBeatmap, gameplayState.Ruleset, clonedMods, loadCancellationSource.Token)
                               .ContinueWith(task => Schedule(() =>
                               {
                                   timedAttributes = task.GetResultSafely();

                                   IsValid = true;

                                   if (lastJudgement != null)
                                       onJudgementChanged(lastJudgement);
                               }), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (scoreProcessor != null)
            {
                scoreProcessor.NewJudgement += onJudgementChanged;
                scoreProcessor.JudgementReverted += onJudgementChanged;
            }

            if (gameplayState?.LastJudgementResult.Value != null)
                onJudgementChanged(gameplayState.LastJudgementResult.Value);
        }

        private double lastPpValue = 0;
        private double currentPpValue = 0;

        public virtual bool IsValid { get; set; }

        protected virtual bool IsIncrementing => false;
        protected virtual bool IsPerfect => false;

        private void onJudgementChanged(JudgementResult judgement)
        {
            lastJudgement = judgement;

            var attrib = getAttributeAtTime(judgement);

            if (gameplayState == null || attrib == null || scoreProcessor == null)
            {
                IsValid = false;
                return;
            }

            scoreProcessor.PopulateScore(scoreInfo);

            if (IsIncrementing)
            {
                double newDoubleValue = performanceCalculator?.Calculate(scoreInfo, attrib).Total ?? 0;
                double diff = newDoubleValue - lastPpValue;
                lastPpValue = newDoubleValue;

                if (diff > 0)
                {
                    currentPpValue += diff;
                    Current.Value = (int)Math.Round(currentPpValue, MidpointRounding.AwayFromZero);
                }
            }
            else if (IsPerfect)
            {
                scoreInfo.Accuracy = 1.0;
                int great = scoreInfo.Statistics.GetValueOrDefault(HitResult.Great, 0);
                int ok = scoreInfo.Statistics.GetValueOrDefault(HitResult.Ok, 0);
                int meh = scoreInfo.Statistics.GetValueOrDefault(HitResult.Meh, 0);
                int miss = scoreInfo.Statistics.GetValueOrDefault(HitResult.Miss, 0);
                int lth = scoreInfo.Statistics.GetValueOrDefault(HitResult.LargeTickHit, 0);
                int ltm = scoreInfo.Statistics.GetValueOrDefault(HitResult.LargeTickMiss, 0);
                int sth = scoreInfo.Statistics.GetValueOrDefault(HitResult.SmallTickHit, 0);
                int stm = scoreInfo.Statistics.GetValueOrDefault(HitResult.SmallTickMiss, 0);

                int totalHits = great + ok + meh + miss + lth + ltm + sth + stm;

                scoreInfo.Statistics[HitResult.Great] = totalHits;
                scoreInfo.Statistics[HitResult.Ok] = 0;
                scoreInfo.Statistics[HitResult.Meh] = 0;
                scoreInfo.Statistics[HitResult.Miss] = 0;
                scoreInfo.Statistics[HitResult.LargeTickHit] = 0;
                scoreInfo.Statistics[HitResult.LargeTickMiss] = 0;
                scoreInfo.Statistics[HitResult.SmallTickHit] = 0;
                scoreInfo.Statistics[HitResult.SmallTickMiss] = 0;

                scoreInfo.Combo = totalHits;
                scoreInfo.MaxCombo = totalHits;

                Current.Value = (int)Math.Round(performanceCalculator?.Calculate(scoreInfo, attrib).Total ?? 0, MidpointRounding.AwayFromZero);
            }
            else
            {
                Current.Value = (int)Math.Round(performanceCalculator?.Calculate(scoreInfo, attrib).Total ?? 0, MidpointRounding.AwayFromZero);
            }

            IsValid = true;
        }

        [CanBeNull]
        private DifficultyAttributes getAttributeAtTime(JudgementResult judgement)
        {
            if (timedAttributes == null || timedAttributes.Count == 0)
                return null;

            int attribIndex = timedAttributes.BinarySearch(new TimedDifficultyAttributes(judgement.HitObject.GetEndTime(), null));
            if (attribIndex < 0)
                attribIndex = ~attribIndex - 1;

            return timedAttributes[Math.Clamp(attribIndex, 0, timedAttributes.Count - 1)].Attributes;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (scoreProcessor != null)
            {
                scoreProcessor.NewJudgement -= onJudgementChanged;
                scoreProcessor.JudgementReverted -= onJudgementChanged;
            }

            loadCancellationSource?.Cancel();
        }

        // TODO: This class shouldn't exist, but requires breaking changes to allow DifficultyCalculator to receive an IBeatmap.
        private class GameplayWorkingBeatmap : WorkingBeatmap
        {
            private readonly IBeatmap gameplayBeatmap;

            public GameplayWorkingBeatmap(IBeatmap gameplayBeatmap)
                : base(gameplayBeatmap.BeatmapInfo, null)
            {
                this.gameplayBeatmap = gameplayBeatmap;
            }

            public override IBeatmap GetPlayableBeatmap(IRulesetInfo ruleset, IReadOnlyList<Mod> mods, CancellationToken cancellationToken)
                => gameplayBeatmap;

            protected override IBeatmap GetBeatmap() => gameplayBeatmap;

            public override Texture GetBackground() => throw new NotImplementedException();

            protected override Track GetBeatmapTrack() => throw new NotImplementedException();

            protected internal override ISkin GetSkin() => throw new NotImplementedException();

            public override Stream GetStream(string storagePath) => throw new NotImplementedException();
        }
    }
}

using System;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace VSEWW
{
    internal class Window_GameOver : Window
    {
        private readonly string stats;
        private readonly float score;

        private Texture2D winston;

        public override Vector2 InitialSize => new Vector2(850f, 500f);

        public Texture2D WinstonIcon
        {
            get
            {
                if (winston is null)
                {
                    winston = ContentFinder<Texture2D>.Get("UI/HeroArt/Storytellers/WaveSurvivalStoryteller", false);
                    if (winston is null)
                    {
                        winston = BaseContent.BadTex;
                    }
                }
                return winston;
            }
        }

        public Window_GameOver(string msg, bool allowKeepPlaying)
        {
            StringBuilder sB = new StringBuilder();
            TimeSpan timeSpan = new TimeSpan(0, 0, (int)Find.GameInfo.RealPlayTimeInteracting);
            sB.AppendLine(msg);
            sB.AppendLine();

            var winston = Find.CurrentMap.GetComponent<MapComponent_Winston>();
            var counter = Find.World.GetComponent<WorldComponent_KillCounter>();
            sB.AppendLine("VESWW.SurvivedX".Translate(winston.currentWave - 1));
            score = ((winston.currentWave * 100) + (counter.totalKill * 5) - (Find.StoryWatcher.statsRecord.colonistsKilled * 50)) * Find.Storyteller.difficultyDef.threatScale;
            sB.AppendLine("VESWW.SurvivedDaysX".Translate(Find.TickManager.TicksGame.TicksToDays()));
            sB.AppendLine("VESWW.TotalKill".Translate(counter.totalKill));
            sB.AppendLine();

            sB.AppendLine("Playtime".Translate() + ": " + timeSpan.Days + "LetterDay".Translate() + " " + timeSpan.Hours + "LetterHour".Translate() + " " + timeSpan.Minutes + "LetterMinute".Translate() + " " + timeSpan.Seconds + "LetterSecond".Translate());
            sB.AppendLine("Difficulty".Translate() + ": " + Find.Storyteller.difficultyDef.LabelCap);

            sB.AppendLine();
            sB.AppendLine("NumThreatBigs".Translate() + ": " + Find.StoryWatcher.statsRecord.numThreatBigs);
            sB.AppendLine("NumEnemyRaids".Translate() + ": " + Find.StoryWatcher.statsRecord.numRaidsEnemy);
            sB.AppendLine();
            sB.AppendLine("ThisMapDamageTaken".Translate() + ": " + Find.CurrentMap.damageWatcher.DamageTakenEver);
            sB.AppendLine("ColonistsKilled".Translate() + ": " + Find.StoryWatcher.statsRecord.colonistsKilled);
            sB.AppendLine("VESWW.MaxPopEver".Translate(Find.StoryWatcher.statsRecord.greatestPopulation));
            sB.AppendLine();
            sB.AppendLine("ColonistsLaunched".Translate() + ": " + Find.StoryWatcher.statsRecord.colonistsLaunched);

            stats = sB.ToString().TrimEndNewlines();

            forcePause = true;
            doCloseX = false;
            doCloseButton = true;
            closeOnClickedOutside = false;
            closeOnCancel = false;
            absorbInputAroundWindow = true;
            doWindowBackground = true;
            drawShadow = true;

            Find.WindowStack.TryRemove(typeof(Window_WaveCounter));
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Storyteller art
            Rect texRect = inRect.LeftHalf();
            Widgets.DrawTextureFitted(texRect, WinstonIcon, 1f);
            // Text
            Rect textRect = inRect.RightHalf();
            // Title
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect gameOverRect = new Rect(textRect)
            {
                height = 50
            };
            Widgets.Label(gameOverRect, "VESWW.GameOver".Translate());
            Rect scoreOverRect = new Rect(textRect)
            {
                height = 50,
                y = gameOverRect.yMax
            };
            Widgets.Label(scoreOverRect, "VESWW.Score".Translate(score));
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            // Stats
            Rect statsRect = new Rect(textRect)
            {
                height = textRect.height - scoreOverRect.height - 10,
                y = scoreOverRect.yMax + 10
            };
            Widgets.Label(statsRect, stats);
        }
    }
}
using System;
using System.Text;
using UnityEngine;
using Verse;

namespace VSEWW
{
    internal class Window_GameOver : Window
    {
        readonly bool canKeepPlaying;
        readonly string message;
        readonly string stats;

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
            sB.AppendLine("Playtime".Translate() + ": " + timeSpan.Days + "LetterDay".Translate() + " " + timeSpan.Hours + "LetterHour".Translate() + " " + timeSpan.Minutes + "LetterMinute".Translate() + " " + timeSpan.Seconds + "LetterSecond".Translate());
            sB.AppendLine("Storyteller".Translate() + ": " + Find.Storyteller.def.LabelCap);
            sB.AppendLine("Difficulty".Translate() + ": " + Find.Storyteller.difficultyDef.LabelCap);

            sB.AppendLine();
            sB.AppendLine("NumThreatBigs".Translate() + ": " + Find.StoryWatcher.statsRecord.numThreatBigs);
            sB.AppendLine("NumEnemyRaids".Translate() + ": " + Find.StoryWatcher.statsRecord.numRaidsEnemy);
            sB.AppendLine();
            if (Find.CurrentMap != null)
                sB.AppendLine("ThisMapDamageTaken".Translate() + ": " + Find.CurrentMap.damageWatcher.DamageTakenEver);
            sB.AppendLine("ColonistsKilled".Translate() + ": " + Find.StoryWatcher.statsRecord.colonistsKilled);
            sB.AppendLine();
            sB.AppendLine("ColonistsLaunched".Translate() + ": " + Find.StoryWatcher.statsRecord.colonistsLaunched);

            stats = sB.ToString().TrimEndNewlines();
            message = msg;
            canKeepPlaying = allowKeepPlaying;

            forcePause = true;
            doCloseX = true;
            doCloseButton = false;
            closeOnClickedOutside = false;
            closeOnCancel = false;
            absorbInputAroundWindow = true;
            doWindowBackground = true;
            drawShadow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect texRect = inRect.LeftHalf();
            Widgets.DrawTextureFitted(texRect, WinstonIcon, 1f);

            Rect textRect = inRect.RightHalf();
            Widgets.Label(textRect, stats);
        }
    }
}

using System.Collections.Generic;

namespace VSEWW
{
    public enum RewardCategory
    {
        Poor,
        Normal,
        Good,
        Excellent,
        Legendary
    }

    public static class RewardCategoryExtension
    {
        public static Dictionary<RewardCategory, int> GetCommonality(int waveN)
        {
            var com = new Dictionary<RewardCategory, int>()
            {
                {RewardCategory.Poor, 0},
                {RewardCategory.Normal, 0},
                {RewardCategory.Good, 0},
                {RewardCategory.Excellent, 0},
                {RewardCategory.Legendary, 0},
            };

            if (waveN <= 5)
            {
                com[RewardCategory.Poor] = 90;
                com[RewardCategory.Normal] = 8;
                com[RewardCategory.Good] = 2;
                return com;
            }
            if (waveN <= 10)
            {
                com[RewardCategory.Poor] = 80;
                com[RewardCategory.Normal] = 12;
                com[RewardCategory.Good] = 4;
                com[RewardCategory.Excellent] = 3;
                com[RewardCategory.Legendary] = 1;
                return com;
            }
            if (waveN <= 15)
            {
                com[RewardCategory.Poor] = 50;
                com[RewardCategory.Normal] = 35;
                com[RewardCategory.Good] = 10;
                com[RewardCategory.Excellent] = 3;
                com[RewardCategory.Legendary] = 2;
                return com;
            }
            if (waveN <= 20)
            {
                com[RewardCategory.Poor] = 20;
                com[RewardCategory.Normal] = 50;
                com[RewardCategory.Good] = 20;
                com[RewardCategory.Excellent] = 7;
                com[RewardCategory.Legendary] = 3;
                return com;
            }
            if (waveN <= 25)
            {
                com[RewardCategory.Poor] = 10;
                com[RewardCategory.Normal] = 20;
                com[RewardCategory.Good] = 30;
                com[RewardCategory.Excellent] = 6;
                com[RewardCategory.Legendary] = 4;
                return com;
            }
            if (waveN <= 30)
            {
                com[RewardCategory.Poor] = 5;
                com[RewardCategory.Normal] = 30;
                com[RewardCategory.Good] = 50;
                com[RewardCategory.Excellent] = 10;
                com[RewardCategory.Legendary] = 5;
                return com;
            }
            if (waveN <= 40)
            {
                com[RewardCategory.Poor] = 5;
                com[RewardCategory.Normal] = 20;
                com[RewardCategory.Good] = 50;
                com[RewardCategory.Excellent] = 20;
                com[RewardCategory.Legendary] = 5;
                return com;
            }
            if (waveN <= 50)
            {
                com[RewardCategory.Normal] = 20;
                com[RewardCategory.Good] = 30;
                com[RewardCategory.Excellent] = 40;
                com[RewardCategory.Legendary] = 10;
                return com;
            }
            if (waveN <= 60)
            {
                com[RewardCategory.Normal] = 15;
                com[RewardCategory.Good] = 25;
                com[RewardCategory.Excellent] = 45;
                com[RewardCategory.Legendary] = 15;
                return com;
            }

            com[RewardCategory.Good] = 25;
            com[RewardCategory.Excellent] = 50;
            com[RewardCategory.Legendary] = 25;
            return com;
        }
    }
}
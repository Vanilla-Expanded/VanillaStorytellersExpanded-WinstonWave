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
                com[RewardCategory.Poor] = 100;
                com[RewardCategory.Normal] = 3;
                com[RewardCategory.Good] = 2;
                com[RewardCategory.Excellent] = 1;
                return com;
            }
            if (waveN <= 10)
            {
                com[RewardCategory.Poor] = 100;
                com[RewardCategory.Normal] = 6;
                com[RewardCategory.Good] = 4;
                com[RewardCategory.Excellent] = 2;
                com[RewardCategory.Legendary] = 1;
                return com;
            }
            if (waveN <= 15)
            {
                com[RewardCategory.Poor] = 100;
                com[RewardCategory.Normal] = 16;
                com[RewardCategory.Good] = 10;
                com[RewardCategory.Excellent] = 5;
                com[RewardCategory.Legendary] = 2;
                return com;
            }
            if (waveN <= 20)
            {
                com[RewardCategory.Poor] = 100;
                com[RewardCategory.Normal] = 38;
                com[RewardCategory.Good] = 25;
                com[RewardCategory.Excellent] = 10;
                com[RewardCategory.Legendary] = 4;
                return com;
            }
            if (waveN <= 25)
            {
                com[RewardCategory.Poor] = 50;
                com[RewardCategory.Normal] = 100;
                com[RewardCategory.Good] = 38;
                com[RewardCategory.Excellent] = 25;
                com[RewardCategory.Legendary] = 10;
                return com;
            }
            if (waveN <= 30)
            {
                com[RewardCategory.Poor] = 38;
                com[RewardCategory.Normal] = 100;
                com[RewardCategory.Good] = 56;
                com[RewardCategory.Excellent] = 22;
                com[RewardCategory.Legendary] = 14;
                return com;
            }
            if (waveN <= 40)
            {
                com[RewardCategory.Poor] = 12;
                com[RewardCategory.Normal] = 100;
                com[RewardCategory.Good] = 100;
                com[RewardCategory.Excellent] = 30;
                com[RewardCategory.Legendary] = 20;
                return com;
            }
            if (waveN <= 50)
            {
                com[RewardCategory.Normal] = 50;
                com[RewardCategory.Good] = 100;
                com[RewardCategory.Excellent] = 50;
                com[RewardCategory.Legendary] = 20;
                return com;
            }
            if (waveN <= 60)
            {
                com[RewardCategory.Normal] = 25;
                com[RewardCategory.Good] = 100;
                com[RewardCategory.Excellent] = 100;
                com[RewardCategory.Legendary] = 20;
                return com;
            }

            com[RewardCategory.Good] = 100;
            com[RewardCategory.Excellent] = 100;
            com[RewardCategory.Legendary] = 50;
            return com;
        }
    }
}

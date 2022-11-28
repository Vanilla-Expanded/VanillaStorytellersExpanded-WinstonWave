using System.Collections.Generic;
using Verse;

namespace VSEWW
{
    public static class RewardCommonalities
    {
        public static SimpleCurve Poor = new SimpleCurve(new List<CurvePoint>
        {
            new CurvePoint(0, 90),
            new CurvePoint(6, 80),
            new CurvePoint(16, 50),
            new CurvePoint(21, 20),
            new CurvePoint(26, 10),
            new CurvePoint(31, 5),
            new CurvePoint(41, 5),
            new CurvePoint(51, 0),
        });
        public static SimpleCurve Normal = new SimpleCurve(new List<CurvePoint>
        {
            new CurvePoint(0, 8),
            new CurvePoint(6, 12),
            new CurvePoint(16, 35),
            new CurvePoint(21, 50),
            new CurvePoint(26, 20),
            new CurvePoint(31, 30),
            new CurvePoint(41, 20),
            new CurvePoint(51, 20),
            new CurvePoint(61, 15),
            new CurvePoint(66, 0),
        });
        public static SimpleCurve Good = new SimpleCurve(new List<CurvePoint>
        {
            new CurvePoint(0, 2),
            new CurvePoint(6, 4),
            new CurvePoint(16, 10),
            new CurvePoint(21, 20),
            new CurvePoint(26, 30),
            new CurvePoint(31, 50),
            new CurvePoint(41, 50),
            new CurvePoint(51, 40),
            new CurvePoint(61, 25),
            new CurvePoint(66, 25),
        });
        public static SimpleCurve Excellent = new SimpleCurve(new List<CurvePoint>
        {
            new CurvePoint(0, 0),
            new CurvePoint(6, 3),
            new CurvePoint(16, 3),
            new CurvePoint(21, 7),
            new CurvePoint(26, 6),
            new CurvePoint(31, 10),
            new CurvePoint(41, 20),
            new CurvePoint(51, 30),
            new CurvePoint(61, 45),
            new CurvePoint(66, 50),
        });
        public static SimpleCurve Legendary = new SimpleCurve(new List<CurvePoint>
        {
            new CurvePoint(0, 0),
            new CurvePoint(6, 1),
            new CurvePoint(16, 2),
            new CurvePoint(21, 3),
            new CurvePoint(26, 4),
            new CurvePoint(31, 5),
            new CurvePoint(41, 5),
            new CurvePoint(51, 10),
            new CurvePoint(61, 15),
            new CurvePoint(66, 20),
        });

        public static Dictionary<RewardCategory, float> GetCommonalities(int waveN)
        {
            return new Dictionary<RewardCategory, float>()
            {
                {RewardCategory.Poor, Poor.Evaluate(waveN)},
                {RewardCategory.Normal, Normal.Evaluate(waveN)},
                {RewardCategory.Good, Good.Evaluate(waveN)},
                {RewardCategory.Excellent, Excellent.Evaluate(waveN)},
                {RewardCategory.Legendary, Legendary.Evaluate(waveN)},
            };
        }
    }
}

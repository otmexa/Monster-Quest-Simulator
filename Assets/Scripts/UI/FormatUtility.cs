using UnityEngine;

namespace MonsterQuest.UI
{
    public static class FormatUtility
    {
        public static string FormatNumber(int number)
        {
            if (number >= 1000000000)
                return (number / 1000000000f).ToString("0.##") + "B";
            if (number >= 1000000)
                return (number / 1000000f).ToString("0.##") + "M";
            if (number >= 1000)
                return (number / 1000f).ToString("0.##") + "K";
            
            return number.ToString();
        }

        public static string GetQiRank(int level)
        {
            string[] ranks = {
                "Mortal",            // 0
                "Qi Novato I",       // 1
                "Qi Novato II",      // 2
                "Qi Novato III",     // 3
                "Qi Aprendiz I",     // 4
                "Qi Aprendiz II",    // 5
                "Qi Aprendiz III",   // 6
                "Guerrero Marcial I", // 7
                "Guerrero Marcial II", // 8
                "Guerrero Marcial III", // 9
                "Maestro Marcial I", // 10
                "Maestro Marcial II", // 11
                "Maestro Marcial III", // 12
                "Gran Maestro I",    // 13
                "Gran Maestro II",   // 14
                "Gran Maestro III"   // 15
            };

            if (level <= 0) return ranks[0];
            if (level < ranks.Length) return ranks[level];

            int stage = (level - 16) / 3;
            int subStage = ((level - 16) % 3) + 1;
            string numRoman = subStage == 1 ? "I" : (subStage == 2 ? "II" : "III");
            
            string[] higherRanks = { "Rey Marcial", "Emperador Marcial", "Santo Marcial", "Soberano Marcial", "Dios Divino" };
            if (stage < higherRanks.Length)
            {
                return $"{higherRanks[stage]} {numRoman}";
            }

            return $"Dios Supremo {level}";
        }
    }
}

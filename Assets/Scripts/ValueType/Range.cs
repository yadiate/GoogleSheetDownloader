using System.Linq;
using UnityEngine;

namespace RIPinc.GoogleSheet
{
    public struct Range
    {
        // [ShowInInspector]
        public int MinScore { get; set; }
        // [ShowInInspector] 
        public int MaxScore { get; set; }

        public float MinScoreF { get; set; }
        public float MaxScoreF { get; set; }

        public Range(int minScore, int maxScore)
        {
            MinScore = minScore;
            MaxScore = maxScore;
            MinScoreF = minScore;
            MaxScoreF = maxScore;
        }

        public Range(float minScore, float maxScore)
        {
            MinScoreF = minScore;
            MaxScoreF = maxScore;
            MinScore = (int)minScore;
            MaxScore = (int)maxScore;
        }

        public Range(string minScore, string maxScore)
        {
            MinScoreF = float.Parse(minScore);
            MaxScoreF = float.Parse(maxScore);
            MinScore = (int)MinScoreF;
            MaxScore = (int)MaxScoreF;
        }


        public static implicit operator int(Range range)
        {
            return Random.Range(range.MinScore, range.MaxScore + 1);
        }

        public static implicit operator float(Range range)
        {
            return Random.Range(range.MinScoreF, range.MaxScoreF);
        }

        public bool CheckRandom(float start, float end)
        {
            var value = Random.Range(start, end);
            if (value < (int)this)
                return true;
            return false;
        }

        public static Range Parse(string value)
        {
            var values = value.Split(',').ToList();
            if (values.Count == 1)
                values = value.Split('~').ToList();
            if (values.Count == 1)
                values = value.Split('-').ToList();
            if (values.Count == 1)
                values.Add(values[0]);

            if (int.TryParse(values[0], out var _0) && int.TryParse(values[1], out var _1))
            {
                return new Range(_0, _1);
            }
            else if (float.TryParse(values[0], out var _00) && float.TryParse(values[1], out var _11))
            {
                return new Range(_00, _11);
            }

            return new Range(0, 0);
        }

        public bool IsInRange(float per)
        {
            return per >= MinScoreF && per <= MaxScoreF;
        }

        public bool IsInRange(int per)
        {
            return per >= MinScore && per <= MaxScore;
        }

        public int Average()
        {
            return (MinScore + MaxScore) / 2;
        }
        public float AverageF()
        {
            return (MinScoreF + MaxScoreF) / 2f;
        }
    }
}
using System;

namespace NEA_ai_pathfinding
{
    // session-wide settings/state (shared by forms)
    public static class GameSession
    {
        // simple mode flags so Home screen can pick Lab vs Challenge
        public static bool IsLabMode = false;

        // lab settings to pass maze sizes without touching the playground UI controls
        public static int LabMazeWidth = 0;
        public static int LabMazeHeight = 0;
        public static double LabBraidProbability = 0.20;
        public static double LabRoomDensity = 0.10;
        public static int LabStepsPerTick = 12;
        public static bool LabUseAgent = true;
        public static double LabAgentLearning = 0.08;
        public static double LabAgentDiscount = 0.75;
        public static double LabAgentExploration = 0.35;
        public static double LabAgentGreedy = 0.25;

        // who is currently logged in and playing
        public static string CurrentUsername = "";
        public static string CurrentRole = "";
        public static int CurrentLevel = 1;
        public static string CurrentMazeColour = "Green";



        // stats for each level and role
        public static string[] RunnerLevelRatings = new string[8];
        public static string[] BlockerLevelRatings = new string[8];
        public static bool[] RunnerLevelCompleted = new bool[8];
        public static bool[] BlockerLevelCompleted = new bool[8];
        public static double[] RunnerLevelTimes = new double[8];
        public static double[] BlockerLevelTimes = new double[8];
        public static int[] RunnerLevelScores = new int[8];
        public static int[] BlockerLevelScores = new int[8];
        public static bool[] RunnerTimedOutFound = new bool[8]; // runner ran out of time, shows "Found!"
        public static bool[] BlockerTimeoutWins = new bool[8]; // blocker held off the runner until time ran out

        // coin images are loaded from Assets; fall back to null if missing
        private static Image? _golden_token;
        public static Image? golden_token
        {
            get
            {
                if (_golden_token == null)
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "golden_coin.png");
                    if (File.Exists(path))
                    {
                        _golden_token = Image.FromFile(path);
                    }
                    else
                    {
                        // Fallback to embedded resource if available:
                        _golden_token = null;
                    }
                }
                return _golden_token;
            }
        }

        private static Image? _silver_token;
        public static Image? silver_token
        {
            get
            {
                if (_silver_token == null)
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "silver_coin.png");
                    if (File.Exists(path))
                    {
                        _silver_token = Image.FromFile(path);
                    }
                    else
                    {
                        // Fallback to embedded resource if available:
                        _silver_token = null;
                    }
                }
                return _silver_token;
            }
        }

        private static Image? _bronze_token;
        public static Image? bronze_token
        {
            get
            {
                if (_bronze_token == null)
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "bronze_coin.png");
                    if (File.Exists(path))
                    {
                        _bronze_token = Image.FromFile(path);
                    }
                    else
                    {
                        // Fallback to embedded resource if available:
                        _bronze_token = null;
                    }
                }
                return _bronze_token;
            }
        }

        

        public static int GetTimeLimitSeconds(string role, int level)
        {
            // keep level inside 1..8
            if (level < 1)
            {
                level = 1;
            }
            if (level > 8)
            {
                level = 8;
            }

            // runner starts with the most time on level 1, then loses time each level
            int[] runnerLimits = new int[] { 110, 100, 90, 80, 75, 70, 65, 60 };
            // blocker starts with the least time on level 1, then gains time each level
            int[] blockerLimits = new int[] { 60, 70, 85, 95, 105, 120, 135, 150 };

            if (role != null && role.ToLower() == "blocker")
            {
                return blockerLimits[level - 1];
            }

            return runnerLimits[level - 1];
        }

        private static int GetRatingRank(string rating)
        {
            if (string.IsNullOrWhiteSpace(rating))
            {
                return 0;
            }

            switch (rating.Trim().ToUpperInvariant())
            {
                case "A":
                    return 4;
                case "B":
                    return 3;
                case "C":
                    return 2;
                case "D":
                    return 1;
                default:
                    return 0;
            }
        }

        public static string GetRatingLetter(string role, double seconds, int score, int level)
        {
            if (role != null && role.ToLower() == "blocker")
            {
                int limit = GetTimeLimitSeconds("blocker", level);
                if (limit <= 0)
                {
                    limit = 1;
                }

                double half = limit * 0.5;
                double threeQuarter = limit * 0.75;
                double almostFull = limit * 0.95;

                if (seconds <= half)
                {
                    return "A";
                }
                if (seconds <= threeQuarter)
                {
                    return "B";
                }
                if (seconds <= almostFull)
                {
                    return "C";
                }
                return "D";
            }

            // treat "no tokens" as the worst grade straight away
            if (score <= 0)
            {
                return "D";
            }
            double safeSeconds = seconds;
            if (safeSeconds < 1.0)
            {
                safeSeconds = 1.0;
            }

            // roughly token points you earn per minute
            double pointsPerMinute = score * 60.0 / safeSeconds;

            if (pointsPerMinute >= 20.0)
            {
                return "A";
            }

            if (pointsPerMinute >= 10.0)
            {
                return "B";
            }

            if (pointsPerMinute >= 5.0)
            {
                return "C";
            }

            return "D";
        }

        public static void StoreLevelResult(string role, int level, double seconds, int score)
        {
            if (level < 1)
            {
                level = 1;
            }
            if (level > 8)
            {
                level = 8;
            }

            string rating = GetRatingLetter(role, seconds, score, level);
            int index = level - 1;

            if (role != null && role.ToLower() == "runner")
            {
                int existingRank = GetRatingRank(RunnerLevelRatings[index]);
                int newRank = GetRatingRank(rating);

                if (!RunnerLevelCompleted[index] || newRank >= existingRank)
                {
                    RunnerLevelCompleted[index] = true;
                    RunnerLevelRatings[index] = rating;
                    RunnerLevelTimes[index] = seconds;
                    RunnerLevelScores[index] = score;
                    RunnerTimedOutFound[index] = false; // cleared it properly, so remove the "found" tag
                }
            }
            else if (role != null && role.ToLower() == "blocker")
            {
                int existingRank = GetRatingRank(BlockerLevelRatings[index]);
                int newRank = GetRatingRank(rating);

                if (!BlockerLevelCompleted[index] || newRank >= existingRank)
                {
                    BlockerLevelCompleted[index] = true;
                    BlockerLevelRatings[index] = rating;
                    BlockerLevelTimes[index] = seconds;
                    BlockerLevelScores[index] = score;
                }
            }
        }

        public static void ResetLevelProgress()
        {
            for (int i = 0; i < 8; i++)
            {
                RunnerLevelCompleted[i] = false;
                RunnerLevelRatings[i] = "";
                RunnerLevelTimes[i] = 0.0;
                RunnerLevelScores[i] = 0;
                RunnerTimedOutFound[i] = false;

                BlockerLevelCompleted[i] = false;
                BlockerLevelRatings[i] = "";
                BlockerLevelTimes[i] = 0.0;
                BlockerLevelScores[i] = 0;
                BlockerTimeoutWins[i] = false;
            }
        }

        public static void ApplyScoreRow(string role, int level, double seconds, int score)
        {
            // simple replay of scores from the database back into memory
            if (level < 1 || level > 8)
            {
                return;
            }

            int index = level - 1;

            if (role != null && role.ToLower() == "blocker")
            {
                BlockerTimeoutWins[index] = true; // saved blocker scores mean they held on
            }
            else if (role != null && role.ToLower() == "runner")
            {
                RunnerTimedOutFound[index] = false; // saved runner scores are proper clears
            }

            StoreLevelResult(role, level, seconds, score);
        }

        public static void SetModeLab()
        {
            IsLabMode = true;
        }

        public static void SetModeChallenge()
        {
            IsLabMode = false;
        }
    }
}

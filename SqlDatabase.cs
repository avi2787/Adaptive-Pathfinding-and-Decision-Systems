using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;

namespace NEA_ai_pathfinding
{

    // ip when in school- 10.20.170.205
    // else- 62.171.193.18
    internal class SqlDatabase
    {
        private MySqlConnection myConnection;
        private const string myConnectionString = "server=62.171.193.18;uid=AditS;pwd=Computer99!;database=adits";
        private const int HashIterations = 20000;
        public bool LastCallFailed { get; private set; }
        public string LastErrorMessage { get; private set; }

        public SqlDatabase()
        {
            myConnection = new MySqlConnection(myConnectionString);
            LastCallFailed = false;
            LastErrorMessage = "";
        }

        // users


        public bool RegisterUser(string username, string password)
        {
            try
            {
                LastCallFailed = false;
                LastErrorMessage = "";
                if (myConnection.State != System.Data.ConnectionState.Open) // open if not already
                {
                    myConnection.Open();
                }

                string stored = PasswordHasher.Create(password, HashIterations); // PBKDF2 + salt for stored passwords

                MySqlCommand myCommand = new MySqlCommand();
                myCommand.Connection = myConnection;
                // password_hash column now holds "iter:salt:hash"
                myCommand.CommandText = "INSERT INTO tbl_user(username,password_hash) VALUES(@username,@password)";
                myCommand.Parameters.AddWithValue("@username", username);
                myCommand.Parameters.AddWithValue("@password", stored);
                myCommand.ExecuteNonQuery();

                return true;
            }
            catch (MySqlException)
            {
                // username may already exist or db is down
                LastCallFailed = true;
                LastErrorMessage = "Username already exists or database unavailable";
                return false;
            }
            finally
            {
                myConnection.Close();
            }
        }

        public bool LoginUser(string username, string password) // verify password
        {
            try
            {
                LastCallFailed = false;
                LastErrorMessage = "";
                if (myConnection.State != System.Data.ConnectionState.Open) // open if not already
                {
                    myConnection.Open();
                }

                string storedHash = "";
                using (MySqlCommand cmd = new MySqlCommand("SELECT password_hash FROM tbl_user WHERE username=@u", myConnection))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    object result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        LastErrorMessage = "Invalid login details";
                        return false;
                    }
                    storedHash = Convert.ToString(result);
                }

                // expect salt:hash format only (old plain rows should be removed)
                if (string.IsNullOrWhiteSpace(storedHash)) // no password stored
                {
                    LastErrorMessage = "Invalid login details";
                    return false;
                }

                bool ok = PasswordHasher.Verify(password, storedHash, out _);
                if (!ok)
                {
                    LastErrorMessage = "Invalid login details";
                }
                return ok;
            }
            catch (MySqlException)
            {
                // database could not be reached
                LastCallFailed = true;
                LastErrorMessage = "Database unavailable";
                return false;
            }
            finally
            {
                myConnection.Close();
            }
        }



        public int GetUserId(string username) 
        {
            try
            {
                LastCallFailed = false;
                LastErrorMessage = "";
                if (myConnection.State != System.Data.ConnectionState.Open)
                    myConnection.Open();

                MySqlCommand myCommand = new MySqlCommand();
                myCommand.Connection = myConnection;
                myCommand.CommandText = "SELECT user_id FROM tbl_user WHERE username = @username";
                myCommand.Parameters.AddWithValue("@username", username.Trim());

                object result = myCommand.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    // user missing, return 0 like before
                    return 0;
                }
                int id = Convert.ToInt32(result);

                return id;
            }
            catch (MySqlException)
            {
                // database call failed
                LastCallFailed = true;
                LastErrorMessage = "Database unavailable";
                return 0;
            }
            finally
            {
                myConnection.Close();
            }
        }





        //scores
        // saw this format on google/stack overflow i think
        public List<(string Username, double Seconds, int Score, string Role)> GetFastestTimes(int limit)
        {
            var rows = new List<(string Username, double Seconds, int Score, string Role)>();

            try
            {
                LastCallFailed = false;
                LastErrorMessage = "";
                if (myConnection.State != System.Data.ConnectionState.Open)
                    myConnection.Open();

                using var c = new MySqlCommand(@"
            SELECT u.username, s.`time`, s.`score`, s.`role`
            FROM tbl_scores s
            JOIN tbl_user u ON u.user_id = s.user_id
            ORDER BY
              CASE WHEN LOWER(s.`role`) = 'blocker' THEN -s.`time` ELSE s.`time` END ASC,
              s.`score` DESC
            LIMIT @n;", myConnection);

                c.Parameters.AddWithValue("@n", limit);

                using var r = c.ExecuteReader();
                while (r.Read())
                {
                    string user = r.IsDBNull(0) ? "" : r.GetString(0); 
                    double secs = r.IsDBNull(1) ? 0.0 : Convert.ToDouble(r.GetValue(1));
                    int score = r.IsDBNull(2) ? 0 : Convert.ToInt32(r.GetValue(2));
                    string role = r.IsDBNull(3) ? "" : r.GetString(3);
                    rows.Add((user, secs, score, role));
                }
            }
            catch (MySqlException)
            {
                LastCallFailed = true;
                LastErrorMessage = "Database unavailable";
            }
            finally
            {
                if (myConnection.State == System.Data.ConnectionState.Open)
                    myConnection.Close();
            }

            return rows;
        }

        // Global leaderboard shows best runner progress per user (levels desc, time asc, coins desc).
        public List<(string Username, double TotalTime, int TotalCoins, int LevelsCleared)> GetGlobalLeaderboard(int limit)
        {
            var rows = new List<(string Username, double TotalTime, int TotalCoins, int LevelsCleared)>();

            try
            {
                LastCallFailed = false;
                LastErrorMessage = "";
                if (myConnection.State != System.Data.ConnectionState.Open)
                    myConnection.Open();

                using var c = new MySqlCommand(@"
            SELECT u.username,
                   SUM(s.`time`) AS total_time,
                   SUM(s.`score`) AS total_coins,
                   COUNT(DISTINCT s.level) AS levels_cleared
            FROM tbl_scores s
            JOIN tbl_user u ON u.user_id = s.user_id
            WHERE LOWER(s.`role`) = 'runner'
            GROUP BY u.username
            ORDER BY levels_cleared DESC,
                     total_time ASC,
                     total_coins DESC
            LIMIT @n;", myConnection);

                c.Parameters.AddWithValue("@n", limit);

                using var r = c.ExecuteReader();
                while (r.Read())
                {
                    string user = r.IsDBNull(0) ? "" : r.GetString(0);
                    double time = r.IsDBNull(1) ? 0.0 : Convert.ToDouble(r.GetValue(1));
                    int coins = r.IsDBNull(2) ? 0 : Convert.ToInt32(r.GetValue(2));
                    int cleared = r.IsDBNull(3) ? 0 : r.GetInt32(3);
                    rows.Add((user, time, coins, cleared));
                }
            }
            catch (MySqlException)
            {
                LastCallFailed = true;
                LastErrorMessage = "Database unavailable";
            }
            finally
            {
                if (myConnection.State == System.Data.ConnectionState.Open)
                    myConnection.Close();
            }

            return rows;
        }

        public List<(string Username, double Seconds, int Score, string Role)> GetFastestTimesForLevel(int level, int limit)
        {
            var rows = new List<(string Username, double Seconds, int Score, string Role)>();

            try
            {
                LastCallFailed = false;
                LastErrorMessage = "";
                if (myConnection.State != System.Data.ConnectionState.Open)
                    myConnection.Open();

                using var c = new MySqlCommand(@"
            SELECT u.username, s.`time`, s.`score`, s.`role`
            FROM tbl_scores s
            JOIN tbl_user u ON u.user_id = s.user_id
            WHERE s.level = @lvl
            ORDER BY
              CASE WHEN LOWER(s.`role`) = 'blocker' THEN -s.`time` ELSE s.`time` END ASC,
              s.`score` DESC
            LIMIT @n;", myConnection);

                c.Parameters.AddWithValue("@n", limit);
                c.Parameters.AddWithValue("@lvl", level);

                using var r = c.ExecuteReader();
                while (r.Read())
                {
                    string user = r.IsDBNull(0) ? "" : r.GetString(0);
                    double secs = r.IsDBNull(1) ? 0.0 : Convert.ToDouble(r.GetValue(1));
                    int score = r.IsDBNull(2) ? 0 : Convert.ToInt32(r.GetValue(2));
                    string role = r.IsDBNull(3) ? "" : r.GetString(3);
                    rows.Add((user, secs, score, role));
                }
            }
            catch (MySqlException)
            {
                LastCallFailed = true;
                LastErrorMessage = "Database unavailable";
            }
            finally
            {
                if (myConnection.State == System.Data.ConnectionState.Open)
                    myConnection.Close();
            }

            return rows;
        }

        public List<(string Username, int LevelsCleared)> GetLevelsCleared(int limit)
        {
            var rows = new List<(string Username, int LevelsCleared)>();

            try
            {
                LastCallFailed = false;
                LastErrorMessage = "";
                if (myConnection.State != System.Data.ConnectionState.Open)
                    myConnection.Open();

                using var c = new MySqlCommand(@"
            SELECT u.username, COUNT(DISTINCT s.level) AS levels_cleared
            FROM tbl_scores s
            JOIN tbl_user u ON u.user_id = s.user_id
            GROUP BY u.username
            ORDER BY levels_cleared DESC, u.username ASC
            LIMIT @n;", myConnection);

                c.Parameters.AddWithValue("@n", limit);

                using var r = c.ExecuteReader();
                while (r.Read())
                {
                    string user = r.IsDBNull(0) ? "" : r.GetString(0);
                    int cleared = r.IsDBNull(1) ? 0 : r.GetInt32(1);
                    rows.Add((user, cleared));
                }
            }
            catch (MySqlException)
            {
                LastCallFailed = true;
                LastErrorMessage = "Database unavailable";
            }
            finally
            {
                if (myConnection.State == System.Data.ConnectionState.Open)
                    myConnection.Close();
            }

            return rows;
        }

        public List<(string Role, int Level, double Seconds, int Score)> GetScoresForUser(string username)
        {
            var rows = new List<(string Role, int Level, double Seconds, int Score)>();

            try
            {
                LastCallFailed = false;
                LastErrorMessage = "";
                if (myConnection.State != System.Data.ConnectionState.Open)
                    myConnection.Open();

                // fetch every recorded run for this user, so we can rebuild per level progress in memory
                using var c = new MySqlCommand(@"
            SELECT s.role, s.level, s.`time`, s.`score`
            FROM tbl_scores s
            JOIN tbl_user u ON u.user_id = s.user_id
            WHERE u.username = @u;", myConnection);

                c.Parameters.AddWithValue("@u", username.Trim());

                using var r = c.ExecuteReader();
                while (r.Read())
                {
                    string role = r.IsDBNull(0) ? "" : r.GetString(0);
                    int level = r.IsDBNull(1) ? 0 : r.GetInt32(1);
                    double secs = r.IsDBNull(2) ? 0.0 : Convert.ToDouble(r.GetValue(2));
                    int score = r.IsDBNull(3) ? 0 : Convert.ToInt32(r.GetValue(3));
                    rows.Add((role, level, secs, score));
                }
            }
            catch (MySqlException)
            {
                // unable to read saved runs
                LastCallFailed = true;
                LastErrorMessage = "Database unavailable";
            }
            finally
            {
                if (myConnection.State == System.Data.ConnectionState.Open)
                    myConnection.Close();
            }

            return rows;
        }


        public void SaveUserScore(string username, double elapsedSeconds, int score, int level, string role)
        {
            try
            {
                LastCallFailed = false;
                LastErrorMessage = "";
                if (myConnection.State != System.Data.ConnectionState.Open)
                    myConnection.Open();

                // record a single run for user with time, score, level and role
                using var cmd = new MySqlCommand(@"
            INSERT INTO tbl_scores (user_id, score, time, level, role)
            SELECT user_id, @score, @elapsed, @level, @role
            FROM tbl_user
            WHERE username = @u;", myConnection);

                cmd.Parameters.AddWithValue("@u", username.Trim());
                cmd.Parameters.AddWithValue("@elapsed", elapsedSeconds);
                cmd.Parameters.AddWithValue("@score", score);
                cmd.Parameters.AddWithValue("@level", level);
                cmd.Parameters.AddWithValue("@role", string.IsNullOrWhiteSpace(role) ? "" : role.Trim());

                cmd.ExecuteNonQuery();
            }
            catch (MySqlException)
            {
                // write failed (db offline or user missing)
                LastCallFailed = true;
                LastErrorMessage = "Database unavailable";
            }
            finally
            {
                if (myConnection.State == System.Data.ConnectionState.Open)
                    myConnection.Close();
            }
        }

        // PBKDF2 helper kept here to minimise file changes
        private static class PasswordHasher
        {
            public static string Create(string password, int iterations)
            {
                byte[] saltBytes = new byte[16];
                // salt stops two identical passwords ending up with the same stored hash
                RandomNumberGenerator.Fill(saltBytes);
                using var rfc = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
                byte[] hash = rfc.GetBytes(32);
                // store iterations so we can verify with the same cost later on
                return iterations + ":" + Convert.ToBase64String(saltBytes) + ":" + Convert.ToBase64String(hash);
            }

            public static bool Verify(string password, string stored, out bool badFormat)
            {
                badFormat = false;
                string[] parts = stored.Split(':');
                if (parts.Length != 3)
                {
                    badFormat = true;
                    return false;
                }

                if (!int.TryParse(parts[0], out int iterations))
                {
                    badFormat = true;
                    return false;
                }

                byte[] salt;
                byte[] expected;
                try
                {
                    salt = Convert.FromBase64String(parts[1]);
                    expected = Convert.FromBase64String(parts[2]);
                }
                catch (FormatException)
                {
                    badFormat = true;
                    return false;
                }

                using var rfc = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
                byte[] actual = rfc.GetBytes(expected.Length);
                // fixed time compare so timing does not leak which bytes matched
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
        }

    }
}


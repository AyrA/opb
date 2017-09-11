using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace opb
{
    public class TorrentModel
    {
        public enum SearchType
        {
            /// <summary>
            /// All terms must appear
            /// </summary>
            ALL,
            /// <summary>
            /// Any terms can appear
            /// </summary>
            ANY
        }

        private bool FromDB = false;

        public string Hash { get; set; }
        public DateTime UploadDate { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }

        public TorrentModel() { }

        private TorrentModel(DbDataReader rdr)
        {
            FromDB = true;
            Hash = rdr.GetString(rdr.GetOrdinal("Hash"));
            Name = rdr.GetString(rdr.GetOrdinal("Name"));
            Size = rdr.GetInt64(rdr.GetOrdinal("Size"));
            UploadDate = new DateTime(rdr.GetInt64(rdr.GetOrdinal("UploadDate")), DateTimeKind.Utc).ToLocalTime();
        }

        private static int ExecCmd(SQLiteConnection conn, string SQL, params object[] args)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SQL;
                if (args != null)
                {
                    args.Select((v, i) => cmd.Parameters.AddWithValue(i.ToString(), v)).ToArray();
                }
                try
                {
                    return cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to execute {SQL}", "SQL");
                    System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}", "SQL");
                    return -1;
                }
            }
        }

        private static async Task<int> ExecCmdAsync(SQLiteConnection conn, string SQL, params object[] args)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SQL;
                if (args != null)
                {
                    args.Select((v, i) => cmd.Parameters.AddWithValue(i.ToString(), v)).ToArray();
                }
                try
                {
                    return await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to execute {SQL}", "SQL");
                    System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}", "SQL");
                    return -1;
                }
            }
        }

        private static SQLiteDataReader ExecReader(SQLiteConnection conn, string SQL, params object[] args)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SQL;
                if (args != null)
                {
                    args.Select((v, i) => cmd.Parameters.AddWithValue(i.ToString(), v)).ToArray();
                }
                try
                {
                    return cmd.ExecuteReader();
                }
                catch
                {
                    return null;
                }
            }
        }

        private static async Task<DbDataReader> ExecReaderAsync(SQLiteConnection conn, string SQL, params object[] args)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SQL;
                if (args != null)
                {
                    args.Select((v, i) => cmd.Parameters.AddWithValue(i.ToString(), v)).ToArray();
                }
                try
                {
                    return await cmd.ExecuteReaderAsync();
                }
                catch
                {
                    return null;
                }
            }
        }

        public static void CreateTable(SQLiteConnection conn, bool Force = false)
        {
            if (Force)
            {
                ExecCmd(conn, "DROP TABLE IF EXISTS Torrents");
            }
            if (ExecCmd(conn, "CREATE TABLE IF NOT EXISTS Torrents(Hash VARCHAR(64) UNIQUE,UploadDate INT,Name VARCHAR(256),Size INT)") > 0)
            {
                ExecCmd(conn, "VACUUM");
            }
        }

        public static async Task<TorrentModel[]> SearchAsync(SQLiteConnection conn, SearchType Type, string Search)
        {
            var SearchQuery = Search
                .Split(Program.SPLITCHARS.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(m => $"%{m}%")
                .ToArray();
            var Entries = new List<TorrentModel>();
            using (var cmd = conn.CreateCommand())
            {
                //Very cheaply search for strings
                cmd.CommandText = "SELECT * FROM Torrents WHERE " +
                    string.Join($" {(Type == SearchType.ALL ? "AND" : "OR")} ", SearchQuery.Select(m => "Name LIKE ?"));
                SearchQuery
                    .Select(v => cmd.Parameters.AddWithValue(cmd.Parameters.Count.ToString(), v))
                    .ToArray();
                using (var rdr = await cmd.ExecuteReaderAsync())
                {
                    while (await rdr.ReadAsync())
                    {
                        Entries.Add(new TorrentModel(rdr));
                    }
                }
            }
            return Entries.ToArray();
        }

        public static TorrentModel[] GetAll(SQLiteConnection conn)
        {
            using (var rdr = ExecReader(conn, "SELECT * FROM Torrents"))
            {
                var Ret = new List<TorrentModel>();
                while (rdr.Read())
                {
                    Ret.Add(new TorrentModel(rdr));
                }
                return Ret.ToArray();
            }
        }

        public static async Task<int> CountEntriesAsync(SQLiteConnection conn)
        {
            using (var rdr = await ExecReaderAsync(conn, "SELECT COUNT(*) FROM Torrents"))
            {
                if (rdr == null)
                {
                    return -1;
                }
                else if (await rdr.ReadAsync())
                {
                    return rdr.GetInt32(0);
                }
                return -1;
            }
        }

        public static async Task<string[]> GetHashesAsync(SQLiteConnection conn)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Hash from Torrents";
                using (var rdr = await cmd.ExecuteReaderAsync())
                {
                    var Ret = new List<string>();
                    while (await rdr.ReadAsync())
                    {
                        Ret.Add(rdr.GetString(0));
                    }
                    return Ret.ToArray();
                }
            }
        }

        public void Save(SQLiteConnection conn)
        {
            if (!FromDB)
            {
                Insert(conn);
            }
            else
            {
                Update(conn);
            }
        }

        public void Delete(SQLiteConnection conn)
        {
            if (FromDB)
            {
                ExecCmd(conn, "DELETE FROM Torrents WHERE Hash=?", Hash);
                FromDB = false;
            }
        }

        private int Update(SQLiteConnection conn)
        {
            return ExecCmd(conn, "UPDATE Torrents SET Hash=?,Name=?,UploadDate=?,Size=? WHERE Hash=?", Hash, Name, UploadDate.ToUniversalTime().Ticks, Size, Hash);
        }

        private int Insert(SQLiteConnection conn)
        {
            return ExecCmd(conn, "INSERT INTO Torrents (Hash,Name,UploadDate,Size) VALUES(?,?,?,?)", Hash, Name, UploadDate.ToUniversalTime().Ticks, Size);
        }
    }
}

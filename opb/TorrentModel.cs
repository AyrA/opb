using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace opb
{
    /// <summary>
    /// Model for Torrent Entries
    /// </summary>
    public class TorrentModel
    {
        /// <summary>
        /// Types of Search
        /// </summary>
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

        /// <summary>
        /// True if from DB (used for Insert/Update)
        /// </summary>
        private bool FromDB = false;

        /// <summary>
        /// Torrent SHA1 Hash
        /// </summary>
        public string Hash { get; set; }
        /// <summary>
        /// Date this Torrent was uploaded to TPB
        /// </summary>
        public DateTime UploadDate { get; set; }
        /// <summary>
        /// Name of Torrent. This is limited to around 60 chars
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Total size of files contained in the torrent
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Creates an Empty torrent
        /// </summary>
        public TorrentModel() { }

        /// <summary>
        /// Reads a Torrent Model from Database
        /// </summary>
        /// <param name="rdr">Database Row</param>
        private TorrentModel(DbDataReader rdr)
        {
            FromDB = true;
            Hash = rdr.GetString(rdr.GetOrdinal("Hash"));
            Name = rdr.GetString(rdr.GetOrdinal("Name"));
            Size = rdr.GetInt64(rdr.GetOrdinal("Size"));
            UploadDate = new DateTime(rdr.GetInt64(rdr.GetOrdinal("UploadDate")), DateTimeKind.Utc).ToLocalTime();
        }

        /// <summary>
        /// Executes a command and returns the rows affected
        /// </summary>
        /// <param name="conn">Connection</param>
        /// <param name="SQL">Command</param>
        /// <param name="args">Command Arguments</param>
        /// <returns>Rows affected</returns>
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

        /// <summary>
        /// Executes a command and returns the rows affected
        /// </summary>
        /// <param name="conn">Connection</param>
        /// <param name="SQL">Command</param>
        /// <param name="args">Command Arguments</param>
        /// <returns>Rows affected</returns>
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

        /// <summary>
        /// Executes a command and returns a Data reader
        /// </summary>
        /// <param name="conn">Connection</param>
        /// <param name="SQL">Command</param>
        /// <param name="args">Command Arguments</param>
        /// <returns>Data Reader</returns>
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

        /// <summary>
        /// Executes a command and returns a Data reader
        /// </summary>
        /// <param name="conn">Connection</param>
        /// <param name="SQL">Command</param>
        /// <param name="args">Command Arguments</param>
        /// <returns>Data Reader</returns>
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

        /// <summary>
        /// Creates the SQL Table
        /// </summary>
        /// <param name="conn">Connection</param>
        /// <param name="Force">True to force creation (empties Database)</param>
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

        /// <summary>
        /// Searches for Torrents
        /// </summary>
        /// <param name="conn">Connection</param>
        /// <param name="Type">AND/OR Type</param>
        /// <param name="Search">Search Query</param>
        /// <returns>Torrents found</returns>
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

        /// <summary>
        /// Gets all Torrents from the Database
        /// </summary>
        /// <param name="conn">Torrents</param>
        /// <returns>Torrents</returns>
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

        /// <summary>
        /// Counts Torrent entries
        /// </summary>
        /// <param name="conn">Connection</param>
        /// <returns>Count of Entries, -1 on DB Failure</returns>
        /// <remarks>If -1 is returned, it is recommended to delete the DB file and recreate it</remarks>
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

        /// <summary>
        /// Gets all Hashes from the database
        /// </summary>
        /// <param name="conn">Connection</param>
        /// <returns>Hash List</returns>
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

        /// <summary>
        /// Saves this Torrent to the Database
        /// </summary>
        /// <param name="conn">Connection</param>
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

        /// <summary>
        /// Deletes this Torrent from the Database
        /// </summary>
        /// <param name="conn">Connection</param>
        public void Delete(SQLiteConnection conn)
        {
            if (FromDB)
            {
                ExecCmd(conn, "DELETE FROM Torrents WHERE Hash=?", Hash);
                FromDB = false;
            }
        }

        /// <summary>
        /// Updates this Torrent in the Database
        /// </summary>
        /// <param name="conn">Connection</param>
        /// <returns>Number of Rows affected</returns>
        /// <remarks>Should always Return 1</remarks>
        private int Update(SQLiteConnection conn)
        {
            return ExecCmd(conn, "UPDATE Torrents SET Hash=?,Name=?,UploadDate=?,Size=? WHERE Hash=?", Hash, Name, UploadDate.ToUniversalTime().Ticks, Size, Hash);
        }

        /// <summary>
        /// Inserts this Torrent into the Database
        /// </summary>
        /// <param name="conn">Connection</param>
        /// <returns>Number of Rows affected</returns>
        /// <remarks>Should always Return 1</remarks>
        private int Insert(SQLiteConnection conn)
        {
            return ExecCmd(conn, "INSERT INTO Torrents (Hash,Name,UploadDate,Size) VALUES(?,?,?,?)", Hash, Name, UploadDate.ToUniversalTime().Ticks, Size);
        }
    }
}

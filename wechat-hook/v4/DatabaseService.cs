using Microsoft.Data.Sqlite;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace WeChatHook
{
    public class Message
    {
        public string ServerSequence { get; set; }
        public string SenderId { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public string MessageContent { get; set; }

        private static Regex patternNormal = new Regex("收款(到账)?(\\d+\\.\\d{2})元");
        private static Regex patternReward = new Regex("二维码赞赏到账(\\d+\\.\\d{2})元");

        public Message(IDataRecord record)
        {
            var server_seq = record["server_seq"].ToString();
            if (server_seq is null)
            {
                throw new ArgumentException("Record does not contain server_seq");
            }
            var create_time_unix = Convert.ToInt64(record["create_time"].ToString());
            DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset create_time = unixEpoch.AddSeconds(create_time_unix).ToLocalTime();
            var sender_id = DecryptService.GetString(record, "sender_id");
            if (sender_id == string.Empty)
            {
                throw new ArgumentException("Record does not contain sender_id");
            }
            var message_content = DecryptService.GetString(record, "message_content");
            ServerSequence = server_seq;
            SenderId = sender_id;
            CreateTime = create_time;
            MessageContent = message_content;
        }

        public string? Money
        {
            get
            {
                var content = MessageContent;
                // 微信收款助手 消息
                if (SenderId == "gh_f0a92aa7146c" && content.Contains("<appname><![CDATA[微信收款助手]]></appname>"))
                {
                    // 同时满足 微信支付收款 和 店员消息 的格式
                    var match = patternNormal.Match(content);
                    if (match.Success)
                    {
                        return match.Groups[2].Value;
                    }

                }
                // 微信支付 消息
                if (SenderId == "gh_3dfda90e39d6" && content.Contains("<appname><![CDATA[微信支付]]></appname>"))
                {
                    // 二维码赞赏格式
                    var match = patternReward.Match(content);
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                    // 普通收款码 / 微信支付到账格式
                    var matchNormal = patternNormal.Match(content);
                    if (matchNormal.Success)
                    {
                        return matchNormal.Groups[2].Value;
                    }
                }
                return null;
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is not Message other) return false;

            return ServerSequence.Equals(other.ServerSequence) && SenderId.Equals(other.SenderId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ServerSequence, SenderId);
        }
    }
    public class DatabaseService
    {
        private static readonly List<string> AllowSenders = [ "gh_f0a92aa7146c", "gh_3dfda90e39d6" ];
        public static int RecentMinutes = 15;
        public static async Task<List<Message>> Scan(string dbFile)
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbFile,
                Mode = SqliteOpenMode.ReadOnly,
                Cache = SqliteCacheMode.Shared,
                Pooling = false,
            }.ToString();
            var conn = new SqliteConnection(connectionString);
            await conn.OpenAsync();
            var tables = new List<string>();
            var messages = new List<Message>();
            // 查询所有表，找到表名 Msg_ 开头的表
            try
            {
                using (var cmd = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table'", conn))
                {
                    using var reader = cmd.ExecuteReader();
                    foreach (var item in reader)
                    {
                        if (item is not IDataRecord record) continue;
                        var name = record["name"].ToString() ?? "";
                        if (name.StartsWith("Msg_", true, null))
                        {
                            tables.Add(name);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"在读取数据库文件 ${new FileInfo(dbFile).Name} 的所有数据表时出现异常", e);
            }
            var timestamp = DateTimeOffset.UtcNow.AddMinutes(-RecentMinutes).ToUnixTimeSeconds();
            foreach (var tableName in tables)
            {
                try
                {
                    using var cmd = new SqliteCommand($"SELECT m.*, " +
                            $"CASE WHEN m.real_sender_id = 1 THEN 1 ELSE 0 END AS is_send, " +
                            $"n.user_name AS sender_id " +
                            $"FROM {tableName} m " +
                            $"LEFT JOIN Name2Id n ON m.real_sender_id = n.rowid " +
                            $"WHERE m.create_time > {timestamp}", conn);
                    using var reader = cmd.ExecuteReader();
                    foreach (var item in reader)
                    {
                        if (item is not IDataRecord record) continue;
                        var sender_id = DecryptService.GetString(record, "sender_id");
                        if (!AllowSenders.Contains(sender_id)) continue;
                        try
                        {
                            var message = new Message(record);
                            if (message.Money != null) messages.Add(message);
                            //messages.Add(message);
                        }
                        catch { }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"在读取数据库文件 ${new FileInfo(dbFile).Name} 的数据表 {tableName} 时出现异常", e);
                }
            }
            await conn.CloseAsync();
            return messages;
        }
    }
}

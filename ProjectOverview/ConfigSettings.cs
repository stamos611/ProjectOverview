namespace ProjectOverview
{
    public interface IConnectionStrings
    {
        string dbOverViewConnString { get; set; }
    }

    public class ConnectionStrings : IConnectionStrings
    {
        public string dbOverViewConnString { get; set; } = string.Empty;
    }
    public interface IRedisConnString
    {
        string Password { get; set; }
        bool AllowAdmin { get; set; }
        bool Ssl { get; set; }
        int ConnectTimeout { get; set; }
        int ConnectRetry { get; set; }
        string Host { get; set; }
        string Port { get; set; }
        int Database { get; set; }
        bool Enable { get; set; }
    }
    public class RedisConnString : IRedisConnString
    {
        public string Password { get; set; } = string.Empty;
        public bool AllowAdmin { get; set; } = false;
        public bool Ssl { get; set; } = false;
        public int ConnectTimeout { get; set; } = 0;
        public int ConnectRetry { get; set; } = 0;
        public string Host { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public int Database { get; set; } = 0;
        public bool Enable { get; set; } = true;
    }
}

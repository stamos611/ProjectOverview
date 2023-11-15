using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System.ComponentModel;
using System.Reflection;

namespace ProjectOverview
{
    public static class Redis
    {
        public static bool KeyExistToRedisDB(string key, ref StackExchange.Redis.IDatabase redisDB)
        {
            bool keyExistToRedisDB = false;

            if (redisDB != null)
            {
                if (redisDB.KeyExists(key) && redisDB.IsConnected(key))
                {
                    keyExistToRedisDB = true;
                }
            }

            return keyExistToRedisDB;
        }

        //Deserialize from Redis format
        public static T ConvertFromRedis<T>(this HashEntry[] hashEntries)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            var obj = Activator.CreateInstance(typeof(T));
            foreach (var property in properties)
            {
                HashEntry entry = hashEntries.FirstOrDefault(g => g.Name.ToString().Equals(property.Name));

                var converter = TypeDescriptor.GetConverter(property.PropertyType);
                var result = converter.ConvertFrom(entry.Value.ToString());

                property.SetValue(obj, result);
            }
            return (T)obj!;
        }

        //Serialize in Redis format:
        public static HashEntry[] ToHashEntries(this object obj)
        {
            try
            {
                return obj.GetType().GetProperties()
                    .Where(x => x.GetValue(obj, null) != null) // <-- PREVENT NullReferenceException
                    .Select(property => new HashEntry(property.Name, property.GetValue(obj, null)!.ToString())).ToArray();
            }

            catch (Exception)
            {
                return null!;
            }
        }
    }
    public static class RedisKeys
    {
        public static string GetCountriesKey()
        {
            return RedisKeysConst.COUNTRY + RedisKeysConst.REDIS_DELIM;
        }
    }
    public class RedisKeysConst
    {
        public const string COUNTRY = "Country";
        public static readonly string REDIS_DELIM = ":";
    }
}

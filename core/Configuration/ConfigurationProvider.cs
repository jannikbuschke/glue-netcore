using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace Glow.Configurations
{
    public class EfConfigurationProvider : IConfigurationProvider
    {
        private ConfigurationReloadToken _reloadToken = new ConfigurationReloadToken();

        public EfConfigurationProvider(Action<DbContextOptionsBuilder> optionsAction)
        {
            OptionsAction = optionsAction;

            var builder = new DbContextOptionsBuilder<SqlServerConfigurationDataContext>();

            OptionsAction(builder);

            using var dbContext = new SqlServerConfigurationDataContext(builder.Options);
            dbContext.Database.Migrate();
        }

        public Action<DbContextOptionsBuilder> OptionsAction { get; }

        private Dictionary<string, string> data = null;

        protected IDictionary<string, string> Data
        {
            get
            {
                if (this.data != null && !reloadNecessary)
                {
                    return this.data;
                }

                var builder = new DbContextOptionsBuilder<SqlServerConfigurationDataContext>();

                OptionsAction(builder);

                using var dbContext = new SqlServerConfigurationDataContext(builder.Options);

                var data = dbContext.GlowConfigurations
                    .OrderByDescending(v => v.Version)
                    .ToList();

                var configurations = data
                    .GroupBy(v => v.Id + ":" + v.Name)
                    .SelectMany(v => v.First().Values)
                    .ToDictionary(v => v.Key, v => v.Value);
                var cfg = new Dictionary<string, string>();
                foreach (KeyValuePair<string, object> item in configurations)
                {
                    if (item.Value == null)
                    {
                        continue;
                    }
                    if (typeof(IEnumerable<object>).IsAssignableFrom(item.Value.GetType()))
                    {
                        var values = (item.Value as IEnumerable<object>).ToList();
                        for (var i = 0; i < values.Count(); i++)
                        {
                            if (values[i].GetType() == typeof(string))
                            {
                                cfg[item.Key + ":" + i] = values[i].ToString();
                            }
                            else if (values[i].GetType() == typeof(JValue))
                            {
                                var val = values[i] as JValue;
                                if (val.Type == JTokenType.String)
                                {
                                    cfg[item.Key + ":" + i] = val.ToString();
                                }
                                else if (val.Type == JTokenType.Boolean)
                                {
                                    var b = val.ToString();
                                    cfg[item.Key + ":" + i] = b;
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }
                            }
                            else
                            {
                                Type type = values[i].GetType();
                                if (type == typeof(JObject))
                                {
                                    var val = values[i] as JObject;
                                    IEnumerable<JProperty> properties = val.Properties();
                                    foreach (JProperty v in properties)
                                    {
                                        cfg[item.Key + ":" + i + ":" + v.Name] = v.Value.ToString();
                                    }
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }
                            }
                        }
                    }
                    else
                    {
                        cfg[item.Key] = item.Value.ToString();
                    }
                }

                this.data = cfg;
                reloadNecessary = false;
                return this.data;
                return cfg;
            }
        }

        public virtual bool TryGet(string key, out string value)
        {
            return Data.TryGetValue(key, out value);
        }

        private static bool reloadNecessary = true;

        public static void SetReloadNecessary()
        {
            reloadNecessary = true;
        }

        public virtual void Set(string key, string value)
        {
            //Data[key] = value;
            throw new NotImplementedException();
        }

        public virtual void Load()
        {
            this.data = null;
            // no-op
        }

        public virtual IEnumerable<string> GetChildKeys(
            IEnumerable<string> earlierKeys,
            string parentPath)
        {
            var prefix = parentPath == null ? string.Empty : parentPath + ConfigurationPath.KeyDelimiter;

            return Data
                .Where(kv => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(kv => Segment(kv.Key, prefix.Length))
                .Concat(earlierKeys)
                .OrderBy(k => k, ConfigurationKeyComparer.Instance);
        }

        private static string Segment(string key, int prefixLength)
        {
            var indexOf = key.IndexOf(ConfigurationPath.KeyDelimiter, prefixLength, StringComparison.OrdinalIgnoreCase);
            return indexOf < 0 ? key.Substring(prefixLength) : key.Substring(prefixLength, indexOf - prefixLength);
        }

        public IChangeToken GetReloadToken()
        {
            return _reloadToken;
        }

        protected void OnReload()
        {
            ConfigurationReloadToken previousToken = Interlocked.Exchange(ref _reloadToken, new ConfigurationReloadToken());
            previousToken.OnReload();
        }
    }
}

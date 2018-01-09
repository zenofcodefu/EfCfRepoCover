using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace EfCfRepoCoverTests
{
    public abstract class AppConfig : IDisposable
    {
        const string APP_DOMAIN_PROPERTY_NAME_APP_CONFIG_FILE = "APP_CONFIG_FILE";

        public static AppConfig Change(string path)
        {
            return new ChangeAppConfig(path);
        }

        public abstract void Dispose();

        private class ChangeAppConfig : AppConfig
        {
            private readonly string oldConfig = AppDomain.CurrentDomain.GetData(APP_DOMAIN_PROPERTY_NAME_APP_CONFIG_FILE).ToString();

            private bool disposedValue;

            public ChangeAppConfig(string path)
            {
                AppDomain.CurrentDomain.SetData(APP_DOMAIN_PROPERTY_NAME_APP_CONFIG_FILE, path);
                ResetConfigMechanism();
            }

            public override void Dispose()
            {
                if (!disposedValue)
                {
                    AppDomain.CurrentDomain.SetData(APP_DOMAIN_PROPERTY_NAME_APP_CONFIG_FILE, oldConfig);
                    ResetConfigMechanism();

                    disposedValue = true;
                }
                GC.SuppressFinalize(this);
            }

            private static void ResetConfigMechanism()
            {
                typeof(ConfigurationManager).GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, 0);

                typeof(ConfigurationManager).GetField("s_configSystem", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);

                typeof(ConfigurationManager).Assembly.GetTypes().Where(type => type.FullName == "System.Configuration.ClientConfigPaths").First().GetField("s_current", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
            }
        }
    }
}
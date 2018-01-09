using System;
using System.Configuration;

namespace EfCfRepoCoverLib.CustomErrors
{
    [Serializable]
    public class ConfigurationMissingException : Exception
    {
        public ConfigurationMissingException(string message) : base(message)
        {            
        }
    }
}

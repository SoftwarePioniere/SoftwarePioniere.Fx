using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace SoftwarePioniere.Builder
{
    public class DevOptions
    {
        public bool GetWithBadRequest { get; set; }
        public bool PostWithBadRequest { get; set; }
        public bool RaiseCommandFailed { get; set; }
    }

    public class DevOptionsConfigurationSource : IConfigurationSource
    {
        private DevOptionsConfigurationProvider Provider { get; }

        public DevOptionsConfigurationSource(DevOptionsConfigurationProvider provider)
        {
            Provider = provider;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return Provider;
        }
    }

    public class DevOptionsConfigurationProvider : ConfigurationProvider
    {
        public DevOptionsConfigurationProvider()
        {
            DevOptions = new DevOptions();
        }

        public DevOptions DevOptions { get; }

        public override void Load()
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {$"DevOptions:{nameof(DevOptions.GetWithBadRequest)}", DevOptions.GetWithBadRequest.ToString()},
                {$"DevOptions:{nameof(DevOptions.PostWithBadRequest)}", DevOptions.PostWithBadRequest.ToString()},
                {$"DevOptions:{nameof(DevOptions.RaiseCommandFailed)}", DevOptions.RaiseCommandFailed.ToString()}
            };

            Data = data;

            OnReload();
        }

        public void SetOption(Action<DevOptions> configureOption)
        {
            configureOption(DevOptions);
            Load();

        }

    }
}

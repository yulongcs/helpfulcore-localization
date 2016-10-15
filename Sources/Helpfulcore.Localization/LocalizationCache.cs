using System;
using Sitecore.Caching;
using Sitecore.Configuration;
using Sitecore.Events;

namespace Helpfulcore.Localization
{
	public class LocalizationCache : CustomCache
	{
		private const string CacheName = "Helpfulcore.Localization.Cache";

		public LocalizationCache()
			:base(CacheName, Settings.Caching.DefaultDataCacheSize)
		{
			Event.Subscribe("publish:end", new EventHandler(this.OnPublishEnd));
			Event.Subscribe("publish:end:remote", new EventHandler(this.OnPublishEnd));
		}

		public string Get(string key)
		{
			key = GenerateCacheKey(key);

			return this.GetString(key);
		}

		public void Set(string key, string value)
		{
			key = GenerateCacheKey(key);

			this.SetString(key, value);
		}

		private static string GenerateCacheKey(string key)
		{
			return string.Format("{0}_{1}", key, Sitecore.Context.Language.Name);
		}

		private void OnPublishEnd(object sender, EventArgs e)
		{
			this.Clear();
		}
	}
}

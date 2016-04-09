using Sitecore.Configuration;
using Sitecore.Exceptions;

namespace Helpfulcore.Localization
{
	public class LocalizationFactory
	{
		private static readonly object InitSyncRoot = new object();
		private static ILocalizationService localizationService;

		public static ILocalizationService LocalizationService
		{
			get
			{
				if (localizationService == null)
				{
					throw new ConfigurationException("The LocalizationService is not initialized. Please make sure to call LocalizationFactory.Initialize() method on application start.");
				}

				return localizationService;
			}
			private set { localizationService = value; }
		}

		public static void Initialize(ILocalizationService instance)
		{
			lock (InitSyncRoot)
			{
				LocalizationService = instance;
			}
		}

		public static void InitializeFromConfiguration()
		{
			lock (InitSyncRoot)
			{
				LocalizationService = FromConfiguration();
			}
		}

		private static ILocalizationService FromConfiguration()
		{
			return Factory.CreateObject("helpfulcore/localization/localizationService", true) as ILocalizationService;
		}
	}
}
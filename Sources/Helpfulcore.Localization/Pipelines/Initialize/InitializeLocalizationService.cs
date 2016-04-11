using Sitecore.Pipelines;

namespace Helpfulcore.Localization.Pipelines.Initialize
{
	public class InitializeLocalizationService
	{
		public void Process(PipelineArgs args)
		{
			LocalizationFactory.InitializeFromConfiguration();
		}
	}
}
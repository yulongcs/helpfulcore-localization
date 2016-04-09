using System.ComponentModel.DataAnnotations;

namespace Helpfulcore.Localization.Validation
{
    public class RegularExpressionLocalizedAttribute : RegularExpressionAttribute
    {
        public RegularExpressionLocalizedAttribute(string pattern) : base(pattern)
        {
        }

        public override string FormatErrorMessage(string name)
        {
			return LocalizationFactory.LocalizationService.Localize(this.ErrorMessageResourceName);
        }
    }
}

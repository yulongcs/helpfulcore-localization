using System.ComponentModel.DataAnnotations;

namespace Helpfulcore.Localization.Validation
{
    public class RequiredLocalizedAttribute : RequiredAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return LocalizationFactory.LocalizationService.Localize(this.ErrorMessageResourceName);
        }
    }
}

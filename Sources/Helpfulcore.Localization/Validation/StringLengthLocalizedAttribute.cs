using System.ComponentModel.DataAnnotations;

namespace Helpfulcore.Localization.Validation
{
    public class StringLengthLocalizedAttribute : StringLengthAttribute
    {
        public StringLengthLocalizedAttribute(int maximumLength) : base(maximumLength)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return LocalizationFactory.LocalizationService.Localize(this.ErrorMessageResourceName);
        }
    }
}
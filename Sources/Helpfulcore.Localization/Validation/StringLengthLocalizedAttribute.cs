using System.ComponentModel.DataAnnotations;

namespace Helpfulcore.Localization.Validation
{
    public class StringLengthLocalizedAttribute : StringLengthAttribute
    {
        public StringLengthLocalizedAttribute(int maximumLength) : base(maximumLength)
        {
        }

        public bool Editable { get; set; }

        public override string FormatErrorMessage(string name)
        {
            return LocalizationFactory.LocalizationService.Localize(this.ErrorMessageResourceName, this.ErrorMessage, this.Editable);
        }
    }
}
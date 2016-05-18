using System.ComponentModel.DataAnnotations;

namespace Helpfulcore.Localization.Validation
{
    public class RegularExpressionLocalizedAttribute : RegularExpressionAttribute
    {
        public RegularExpressionLocalizedAttribute(string pattern) : base(pattern)
        {
        }

        public bool Editable { get; set; }

        public override string FormatErrorMessage(string name)
        {
            return LocalizationFactory.LocalizationService.Localize(this.ErrorMessageResourceName, this.ErrorMessage, this.Editable);
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Helpfulcore.Localization.Validation
{
    public class CompareLocalizedAttribute : CompareAttribute
    {
        public CompareLocalizedAttribute(string otherProperty) : base(otherProperty)
        {
        }

        public bool Editable { get; set; }

        public override string FormatErrorMessage(string name)
        {
            return LocalizationFactory.LocalizationService.Localize(this.ErrorMessageResourceName, this.ErrorMessage, this.Editable);
        }
    }
}
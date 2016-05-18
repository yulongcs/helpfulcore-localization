using System.ComponentModel.DataAnnotations;
using System.Net.Configuration;

namespace Helpfulcore.Localization.Validation
{
    public class RequiredLocalizedAttribute : RequiredAttribute
    {
        public bool Editable { get; set; }

        public override string FormatErrorMessage(string name)
        {
            return LocalizationFactory.LocalizationService.Localize(this.ErrorMessageResourceName, this.ErrorMessage, this.Editable);
        }
    }
}
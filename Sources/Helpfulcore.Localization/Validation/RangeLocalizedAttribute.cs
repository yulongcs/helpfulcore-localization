﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Helpfulcore.Localization.Validation
{
    public class RangeLocalizedAttribute : RangeAttribute
    {
        public RangeLocalizedAttribute(int minimum, int maximum) : base(minimum, maximum)
        {
        }

        public RangeLocalizedAttribute(double minimum, double maximum) : base(minimum, maximum)
        {
        }

        public RangeLocalizedAttribute(Type type, string minimum, string maximum) : base(type, minimum, maximum)
        {
        }

        public bool Editable { get; set; }

        public override string FormatErrorMessage(string name)
        {
            return LocalizationFactory.LocalizationService.Localize(this.ErrorMessageResourceName, this.ErrorMessage, this.Editable);
        }

        public override bool IsValid(object value)
        {
            return base.IsValid(value);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return base.IsValid(value, validationContext);
        }
    }
}

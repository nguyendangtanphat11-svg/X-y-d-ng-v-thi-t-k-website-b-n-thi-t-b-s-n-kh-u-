
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Wedside_TVS_SFX.ViewModels.Admin
{
    public class DecimalRangeAttribute : ValidationAttribute
    {
        private readonly decimal minimum;
        private readonly decimal maximum;

        public DecimalRangeAttribute(string min, string max)
        {
            minimum = decimal.Parse(
                min,
                CultureInfo.InvariantCulture
            );

            maximum = decimal.Parse(
                max,
                CultureInfo.InvariantCulture
            );
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            decimal number;

            if (value is decimal)
            {
                number = (decimal)value;
            }
            else
            {
                string input = Convert.ToString(
                    value,
                    CultureInfo.InvariantCulture
                );

                if (!decimal.TryParse(
                    input,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out number))
                {
                    return false;
                }
            }

            return number >= minimum && number <= maximum;
        }
    }
}

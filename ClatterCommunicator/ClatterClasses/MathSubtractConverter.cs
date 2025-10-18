using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace ClatterCommunicator.ClatterClasses;

// public class MathSubtractConverter : IValueConverter
// {
//     public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
//     {
//         return (decimal?)value - (decimal?)parameter;
//     }
//
//     public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
//     {
//         return (decimal?)value + (decimal?)parameter;
//     }
// }

public class MathMultiConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // We need to validate if the provided values are valid. We need at least 3 values.
            // The first value is the operator and the other two values should be a decimal.
            if (values.Count != 3)
            {
                // We can write a message into the Trace if we want to inform the developer.
                Trace.WriteLine("Exactly three values expected");

                // return "BindingOperations.DoNothing" instead of throwing an Exception.
                // If you want, you can also return a BindingNotification with an Exception
                return BindingOperations.DoNothing;
            }

            // The first item of values is the operation.
            // The operation to use is stored as a string.
            string operation = values[0] as string ?? "+";

            // Create a variable result and assign the first value we have to if
            Double value1 = values[1] as Double? ?? 0;
            decimal value2 = values[2] as decimal? ?? 0;


            // depending on the operator calculate the result.
            switch (operation)
            {
                case "+":
                    return new decimal(value1) + value2;

                case "-":
                    return new decimal(value1) - value2;

                case "*":
                    return new decimal(value1) * value2;

                case "/":
                    // We cannot divide by zero. If value2 is '0', return an error.
                    if (value2 == 0)
                    {
                        return new BindingNotification(new DivideByZeroException("Don't do this!"), BindingErrorType.Error);
                    }

                    return new decimal(value1) / value2;
            }

            // If we reach this line, something was wrong. So we return an error notification
            return new BindingNotification(new InvalidOperationException("Something went wrong"), BindingErrorType.Error);
        }
    }
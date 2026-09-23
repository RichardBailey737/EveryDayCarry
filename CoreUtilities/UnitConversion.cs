using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Ensur.Core.Utilities.Properties;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace Ensur.Core.Utilities
{
    public static class UnitConversion
    {
        ///// <summary>
        ///// We have too many non-standard units of conversion
        ///// </summary>
        ///// <param name="SourceValue"></param>
        ///// <param name="Factor"></param>
        ///// <returns></returns>
        //public static double MMSConvert(Decimal SourceValue, double Factor, int MaxLength, double Precision)
        //{
        //    var src = Convert.ToDouble(SourceValue);
        //    double rslt = src * Factor;

        //    if (MaxLength > 0 && Convert.ToInt32(rslt).ToString().Length > MaxLength)
        //        throw new UserError(Resources.RESULT_EXCEEDED_MAX.Replace("{MAX}", MaxLength.ToString()), "RESULT_EXCEEDED_MAX");
        //    else
        //        if (Precision > 0.0 && (rslt > Precision)) throw new UserError(Resources.RESULT_EXCEEDED_MAX.Replace("{MAX}", Precision.ToString()), "RESULT_EXCEEDED_MAX");


        //    if (Precision > 0.0 && rslt.ToString().Contains("."))
        //    {
        //        int digits = (Precision.ToString().Split('.')[1].Length);
        //        rslt = Math.Round(rslt, digits);
        //    }

        //    return rslt;
        //}

        /// <summary>
        /// Obviously I have some contempt for the original writer of this code.  We have a very odd and particular way we limit number size and precision.  Mirroring the functionality turned out to be a pain, so I just recreated the function, line per line.
        /// </summary>
        /// <param name="aSourceValue"></param>
        /// <param name="aFactor"></param>
        /// <param name="MaxLength"></param>
        /// <param name="Precision"></param>
        /// <returns></returns>
        public static string MMSConvert(Decimal aSourceValue, double aFactor, string MaxLength, string Precision)
        {
            decimal decSrc;
            decimal decMaxVal;
            int intDecimals;
            int intDotPos;
            int intMaxLength;
            string strResult;

            try
            {
                decSrc = aSourceValue;
                decSrc = decSrc * (decimal)aFactor;
                strResult = decSrc.ToString();


                // If MaxType checks for length
                if (MaxLength != "0")
                {
                    intMaxLength = int.Parse(MaxLength);
                    if (strResult.Length > intMaxLength)
                    {
                        intDotPos = strResult.IndexOf(".");
                        if (intDotPos > 0)
                        {
                            if (intDotPos == intMaxLength)
                            {
                                decSrc = Math.Round(decSrc, 0, MidpointRounding.AwayFromZero);
                                strResult = decSrc.ToString();
                            }
                            else if (intDotPos < intMaxLength)
                            {
                                intDotPos += 1;
                                intDecimals = intMaxLength - intDotPos;
                                decSrc = Math.Round(decSrc, intDecimals, MidpointRounding.AwayFromZero);
                                strResult = decSrc.ToString();
                            }
                            else
                            {
                                throw new UserError(Resources.VALUE_RESULT_MAX.Replace("{MAX}", MaxLength), "VALUE_MAX_CHAR", "Value", strResult);
                            }
                        }
                        else
                        {
                            throw new UserError(Resources.VALUE_MAX_CHAR.Replace("{MAX}", MaxLength), "VALUE_MAX_CHAR", "Value", strResult);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Precision))
                    {
                        decMaxVal = Convert.ToDecimal(Precision);
                        if (decSrc > decMaxVal)
                        {
                            throw new UserError(Resources.CONVERSION_ERROR_RANGE.Replace("{VAL}", decSrc.ToString()).Replace("{RANGE}", "Maximum " + decMaxVal.ToString()), "CONVERSION_ERROR_RANGE");
                        }
                        else
                        {
                            if (DecimalPlaces(strResult) > DecimalPlaces(Precision))
                            {
                                if (DecimalPlaces(Precision) < 1)
                                {
                                    // integer rounding
                                    decSrc = Math.Round(decSrc, 0, MidpointRounding.AwayFromZero);
                                    strResult = decSrc.ToString();
                                }
                                else
                                {
                                    decSrc = Math.Round(decSrc, DecimalPlaces(Precision), MidpointRounding.AwayFromZero);
                                    strResult = decSrc.ToString();
                                }
                            }
                        }
                        if (!strResult.Contains(".")) strResult = strResult + ".0";
                        //Add back any following zeroes.  Why would we possibly want to do that?  Because that's what Ensur does..
                        if (DecimalPlaces(strResult) < DecimalPlaces(Precision))
                        {
                            strResult = strResult.Split('.')[0] + "." + (strResult.Split('.')[1] + "0000000000000000").Substring(0, DecimalPlaces(Precision));
                        }

                    }
                }

                if (strResult == "?") throw new UserError(Resources.INVALID_VALUE, "INVALID_VALUE", "VALUES", aSourceValue.ToString());

                return strResult;
            }
            catch (Exception ex)
            {
                if (ex is UserError) throw ex;
                var log = NLog.LogManager.GetCurrentClassLogger();
                log.Error(ex, "Error during MMSConvert");
                throw new UserError(Resources.INVALID_VALUE, "INVALID_VALUE", "VALUES", aSourceValue.ToString());
            }
        }

        public static int DecimalPlaces(string str)
        {
            int intPos = 0;
            int intReturn = 0;
            intPos = str.IndexOf(".");
            if (intPos > 0)
            {
                intPos++;
                intReturn = str.Length - intPos;
            }
            return intReturn;
        }

        public static string EnsurConvert(string SourceMetric, string DestMetric, Decimal Value, string MaxLength, string Precision)
        {
            DestMetric = DestMetric.ToUpper();
            switch (SourceMetric.ToUpper())
            {
                case "IN^2/LB":
                    return MMSConvert(Value, 0.001422, MaxLength, Precision);
                case "IN":
                case "INCHES":
                    switch (DestMetric)
                    {
                        case "MM":
                            return MMSConvert(Value, 25.4, MaxLength, Precision);
                        case "CM":
                            return MMSConvert(Value, 2.54, MaxLength, Precision);
                        case "KM":
                            return MMSConvert(Value, 0.0000254, MaxLength, Precision);
                    }
                    break;
                case "GAL":
                    return MMSConvert(Value, 3.7854, MaxLength, Precision);
                case "LBS":
                    switch (DestMetric)
                    {
                        case "GRAMS":
                        case "G":
                        case "GRMS":
                        case "GMS":
                        case "GM":
                            return MMSConvert(Value, 453.59237, MaxLength, Precision);
                        case "KG":
                            return MMSConvert(Value, 0.45359237, MaxLength, Precision);
                    }
                    break;
                case "LB/SQFT":
                    return MMSConvert(Value, 4882.42761, MaxLength, Precision);
                case "OZ (UK)":
                    switch (DestMetric)
                    {
                        case "L":
                            return MMSConvert(Value, 0.0284, MaxLength, Precision);
                        case "ML":
                            return MMSConvert(Value, 28.413, MaxLength, Precision);
                    }
                    break;
                case "OZ (US)":
                    switch (DestMetric)
                    {
                        case "L":
                            return MMSConvert(Value, 0.02957, MaxLength, Precision);
                        case "ML":
                            return MMSConvert(Value, 29.57, MaxLength, Precision);
                    }
                    break;
                case "M^2/KG":
                    return MMSConvert(Value, 703.235, MaxLength, Precision);
                case "MM":
                    return MMSConvert(Value, 0.03937007874016, MaxLength, Precision);
                case "CM":
                    switch (DestMetric)
                    {
                        case "IN":
                        case "INCHES":
                            return MMSConvert(Value, 0.3937007874016, MaxLength, Precision);
                        case "FT":
                        case "FEET":
                            return MMSConvert(Value, 0.03280839895013, MaxLength, Precision);
                    }
                    break;
                case "GRAMS":
                case "G":
                case "GRMS":
                case "GMS":
                case "GM":
                    return MMSConvert(Value, 0.0022046226218487759, MaxLength, Precision);
                case "G/SQM":
                    return MMSConvert(Value, 0.000205, MaxLength, Precision);
                case "KG":
                    return MMSConvert(Value, 2.2046226218487757, MaxLength, Precision);
                case "KM":
                    switch (DestMetric)
                    {
                        case "IN":
                        case "INCHES":
                            return MMSConvert(Value, 39370.07874016, MaxLength, Precision);
                        case "FT":
                        case "FEET":
                            return MMSConvert(Value, 3280.839895013, MaxLength, Precision);
                    }
                    break;
                case "L":
                    switch (DestMetric)
                    {
                        case "OZ (UK)":
                            return MMSConvert(Value, 35.195, MaxLength, Precision);
                        case "OZ (US)":
                            return MMSConvert(Value, 33.814, MaxLength, Precision);
                        case "GAL":
                            return MMSConvert(Value, 0.26417, MaxLength, Precision);
                    }
                    break;
                case "ML":
                    switch (DestMetric)
                    {
                        case "OZ (UK)":
                            return MMSConvert(Value, 0.035195, MaxLength, Precision);
                        case "OZ (US)":
                            return MMSConvert(Value, 0.033814, MaxLength, Precision);
                        case "GAL":
                            return MMSConvert(Value, 0.000264, MaxLength, Precision);
                    }
                    break;
            }
            return null;
        }


        /// <summary>
        /// Uses a fixed list of Source units to determine the type of conversion to make.  This is specicically coded to work with the conversions Ensur already does.  It's better to specify the unit type
        /// </summary>
        /// <param name="Amount">The number to convert</param>
        /// <param name="UnitType">The type of unit (length, mass, volume)</param>
        /// <param name="SourceUnit">The abbreviated source unit (cm, in, etc)</param>
        /// <param name="DestinationUnit">The abbreviated destination unit</param>
        /// <param name="Culture">Culture setting (en-US)</param>
        /// <returns></returns>
        public static double LengthByAbbreviation(double Amount, string UnitType, string SourceUnit, string DestinationUnit, string Culture)
        {

            return UnitConverter.ConvertByAbbreviation(Amount, UnitType, SourceUnit, DestinationUnit, Culture);
        }


        /// <summary>
        /// Uses a fixed list of Source units to determine the type of conversion to make.  This is specicically coded to work with the conversions Ensur already does.  It's better to specify the unit type
        /// </summary>
        /// <param name="Amount">The number to convert</param>
        /// <param name="UnitType">The type of unit (length, mass, volume)</param>
        /// <param name="SourceUnit">The abbreviated source unit (cm, in, etc)</param>
        /// <param name="DestinationUnit">The abbreviated destination unit</param>
        /// <returns></returns>
        public static double LengthByAbbreviation(double Amount, string UnitType, string SourceUnit, string DestinationUnit)
        {
            return UnitConverter.ConvertByAbbreviation(Amount, UnitType, SourceUnit, DestinationUnit);
        }

    }

    public struct SpecificSurfaceArea
    {
        private readonly double _valueInSquareMetersPerKilogram;

        public enum Unit
        {
            SquareMetersPerKilogram,
            SquareInchesPerPound
        }

        // Conversion factor: 1 in²/lb = 0.00142233 m²/kg
        private const double In2PerLbToM2PerKg = 0.00142233;

        // Constructor
        public SpecificSurfaceArea(double value, Unit unit)
        {
            if (unit == Unit.SquareMetersPerKilogram)
            {
                _valueInSquareMetersPerKilogram = value;
            }
            else if (unit == Unit.SquareInchesPerPound)
            {
                _valueInSquareMetersPerKilogram = value * In2PerLbToM2PerKg;
            }
            else
            {
                throw new NotSupportedException("Unsupported unit.");
            }
        }

        // Properties for conversion
        public double SquareMetersPerKilogram
        {
            get { return _valueInSquareMetersPerKilogram; }
        }

        public double SquareInchesPerPound
        {
            get { return _valueInSquareMetersPerKilogram / In2PerLbToM2PerKg; }
        }
    }

}

using Ensur.Core.Utilities.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities.Settings
{
    /// <summary>
    /// Which language the application should use.
    /// </summary>
    public static class CultureSettings
    {
        /// <summary>
        /// The language culture to load (en-EN, fr-FR)
        /// </summary>
        public static string Culture
        {
            set
            {
                Resources.Culture = new CultureInfo(value);

            }
        }
    }
}

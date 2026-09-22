using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities.Settings
{
    /// <summary>
    /// Determines if any error/logging functions use extra verbosity.
    /// </summary>
    /// 29-Jun-2021/RB 
    public static class LogLevel
    {
        /// <summary>
        /// Verbost logging boolean
        /// </summary>
        public static bool IsVerbose { get; set; } = true;
    }
}

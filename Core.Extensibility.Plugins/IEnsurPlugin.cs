using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensibility.Plugins
{
    /// <summary>
    /// DT-01141, DS-70138 Req 1: RB Basic interface for all plugins. 
    /// It's set with an In/Out type for maximum flexibility although only string, Dictionary<string, string>, and object, Dictionary<string, string> are currently supported in Ensur.Core.Utilities
    /// </summary>
    /// <typeparam name="inType">Data type for the input parameter for the ExecutePlugin function</typeparam>
    /// <typeparam name="outType">Data type for the return value of the ExecutePlugin function</typeparam>
    public interface ICorePlugin<in inType, out outType>
    { 


        /// <summary>
        /// This is the function that is called when the plugin executes
        /// </summary>
        /// <param name="inputObject">Any values the function might need to work</param>
        /// <returns>Anything the function returns or nothing</returns>
        outType ExecutePlugin(inType inputObject);


    }
}

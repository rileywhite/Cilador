using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Interface for cloners that contain parameters
    /// </summary>
    internal interface IParameterContainerCloner : ICloner
    {
        /// <summary>
        /// Gets the collection of parameter cloners for contained parameters
        /// </summary>
        List<ParameterCloner> ParameterCloners { get; }
    }
}

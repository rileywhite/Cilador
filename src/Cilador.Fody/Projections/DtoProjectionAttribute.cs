using System;
using System.Diagnostics.Contracts;

namespace Cilador.Fody.Projections
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class DtoProjectionAttribute : Attribute
    {
        private string name;
        /// <summary>
        /// Gets or sets the name of the projection.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                this.name = value;
            }
        }
    }
}

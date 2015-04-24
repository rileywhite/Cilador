using System;
using System.Diagnostics.Contracts;

namespace Cilador.Fody.Projections
{
    public class ProjectionAttributeBase : Attribute
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

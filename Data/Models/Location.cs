// -----------------------------------------------------------------------
// <copyright file="Location.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace PointOfNoReturn.Data.Models
{
    using System.Collections.Generic;
    using System.Numerics;

    /// <summary>
    /// Represents a location in the game.
    /// A location is a server that players can connect to.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Gets or sets the unique identifier for the location.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the username used to connect to the location virtual machine.
        public string? UserName { get; set; }

        /// <summary>
        /// Gets or sets the password used to connect to the location virtual machine.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the domain name used to connect to the location virtual machine.
        /// </summary>
        public string? DomainName { get; set; }
    }
}

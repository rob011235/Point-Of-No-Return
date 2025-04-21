// -----------------------------------------------------------------------
// <copyright file="GameServer.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace PointOfNoReturnServerDeploy
{
    using System;

    /// <summary>
    /// GameServer is a class that represents a game server configuration.
    /// It contains properties for the server's ID, domain name, port, username, password, and relative path.
    /// </summary>
    public class GameServer
    {
        /// <summary>
        /// Gets or sets the unique identifier for the game server.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the domain name of the game server.
        /// </summary>
        public string? DomainName { get; set; }

        /// <summary>
        /// Gets or sets the port number for the game server.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets the relative path to the game server's installation directory.
        /// </summary>
        public string? RelativePath { get; set; }
    }
}

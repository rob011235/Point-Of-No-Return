// -----------------------------------------------------------------------
// <copyright file="Player.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace PointOfNoReturn.Data.Models
{
    /// <summary>
    /// Represents a player in the game.
    /// This class contains properties for the player's ID, user ID, password, avatar name, and avatar path.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Gets or sets the unique identifier for the player.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user associated with the player.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the password for the player.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the name of the avatar associated with the player.
        /// </summary>
        public string? AvatarName { get; set; }

        /// <summary>
        /// Gets or sets the path to the avatar image associated with the player.
        /// </summary>
        public string? AvatarPath { get; set; }
    }
}

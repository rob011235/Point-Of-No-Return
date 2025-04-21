// -----------------------------------------------------------------------
// <copyright file="SpawnableObject.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace PointOfNoReturn.Data.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents an object that can be spawned in the game.
    /// </summary>
    public class SpawnableObject
    {
        /// <summary>
        /// Gets or sets the path to the spawnable object's resource.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the path to the spawnable object's resource.
        /// It is a readonly property that returns "res://Objects/{this.Name}.tscn".
        /// </summary>
        [NotMapped]
        public string Path
        {
            get => $"res://Objects/{this.Name}.tscn";
        }

        /// <summary>
        /// Gets or sets the distance at which the object is spawned.
        /// </summary>
        public float Distance { get; set; }

        /// <summary>
        /// Gets or sets the height at which the object is spawned.
        /// </summary>
        public float Height { get; set; }
    }
}

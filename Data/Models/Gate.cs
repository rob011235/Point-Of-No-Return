// -----------------------------------------------------------------------
// <copyright file="Gate.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace PointOfNoReturn.Data.Models
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;
    using Godot;

    /// <summary>
    /// Represents a gate in the game.
    /// A gate is a portal that allows players to travel to different locations (servers).
    /// </summary>
    public class Gate
    {
        /// <summary>
        /// Gets or setsthe unique identifier for the gate.
        /// </summary>
        public string? Id { get; set; } = default!;

        /// <summary>
        /// Gets or sets the domain name of the target server for the gate.
        /// </summary>
        public string? DomainName { get; set; } = default!;

        /// <summary>
        /// Gets or setsthe ID of the associated object.
        /// This is used so the server can figure out which Domain name to send the user to.
        /// </summary>
        public string? LocationObjectId { get; set; } = default!;

        /// <summary>
        /// Gets or sets the x position of the gate landing location.
        /// </summary>
        public float PositionX { get; set; }

        /// <summary>
        /// Gets or sets the y position of the gate landing location.
        /// </summary>
        public float PositionY { get; set; }

        /// <summary>
        /// Gets or sets the z position of the gate landing location.
        /// </summary>
        public float PositionZ { get; set; }

        /// <summary>
        /// Gets or sets a Godot Vector3 representing the position of the gate landing location.
        /// This property is not mapped to the database, but is used for convenience in the code.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public Vector3 Position
        {
            get => new Vector3(this.PositionX, this.PositionY, this.PositionZ);
            set
            {
                this.PositionX = value.X;
                this.PositionY = value.Y;
                this.PositionZ = value.Z;
            }
        }

        /// <summary>
        /// Gets or sets the x rotation of the gate landing location.
        /// </summary>
        public float RotationX { get; set; }

        /// <summary>
        /// Gets or sets the y rotation of the gate landing location.
        /// </summary>
        public float RotationY { get; set; }

        /// <summary>
        /// Gets or sets the z rotation of the gate landing location.
        /// </summary>
        public float RotationZ { get; set; }

        /// <summary>
        /// Gets or sets a Godot Vector3 representing the rotation of the gate landing location.
        /// This property is not mapped to the database, but is used for convenience in the code.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public Vector3 Rotation
        {
            get => new Vector3(this.RotationX, this.RotationY, this.RotationZ);
            set
            {
                this.RotationX = value.X;
                this.RotationY = value.Y;
                this.RotationZ = value.Z;
            }
        }
    }
}

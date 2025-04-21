// -----------------------------------------------------------------------
// <copyright file="LocationObject.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Godot;

namespace PointOfNoReturn.Data.Models
{
    /// <summary>
    /// Represents a location object in the game.
    /// A location object is an object that can be placed in the game world and has a position and rotation.
    /// </summary>
    public class LocationObject
    {
        /// <summary>
        /// Gets or sets the unique identifier for the location object.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the ScenePath of the location object.
        /// </summary>
        public string? ScenePath { get; set; }

        /// <summary>
        /// Gets or sets the x position of the location object.
        /// </summary>
        public float PositionX { get; set; }

        /// <summary>
        /// Gets or sets the y position of the location object.
        /// </summary>
        public float PositionY { get; set; }

        /// <summary>
        /// Gets or sets the z position of the location object.
        /// </summary>
        public float PositionZ { get; set; }

        /// <summary>
        /// Gets or sets a Godot Vector3 representing the position of the location object.
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
        /// Gets or sets the x rotation of the location object.
        /// </summary>
        public float RotationX { get; set; }

        /// <summary>
        /// Gets or sets the y rotation of the location object.
        /// </summary>
        public float RotationY { get; set; }

        /// <summary>
        /// Gets or sets the z rotation of the location object.
        /// </summary>
        public float RotationZ { get; set; }

        /// <summary>
        /// Gets or sets a Godot Vector3 representing the rotation of the location object.
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

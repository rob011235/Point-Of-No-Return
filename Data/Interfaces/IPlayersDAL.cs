// -----------------------------------------------------------------------
// <copyright file="IPlayersDAL.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace PointOfNoReturn.Data.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PointOfNoReturn.Data.Models;

    /// <summary>
    /// Interface for Player Data Access Layer.
    /// This interface defines the methods for managing player data in the database.
    /// </summary>
    public interface IPlayersDAL
    {
        /// <summary>
        /// Retrieves all players from the database.
        /// </summary>
        /// <returns>List of players.</returns>
        public Task<List<Player>> GetAllAsync();

        /// <summary>
        /// Retrieves a player by their identifier.
        /// </summary>
        /// <param name="id">The player identifier.</param>
        /// <returns>The player with the specified identifier.</returns>
        public Task<Player?> GetByIdAsync(string id);

        /// <summary>
        /// Retrieves a player by their user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The player with the specified user identifier.</returns>
        public Player? GetByUserId(string userId);

        /// <summary>
        /// Asynchronously retrieves a player by their user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The player with the specified user identifier.</returns>
        public Task<Player?> GetByUserIdAsync(string userId);

        /// <summary>
        /// Adds a new player to the database.
        /// </summary>
        /// <param name="player">The player to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task AddAsync(Player player);

        /// <summary>
        /// Updates an existing player in the database.
        /// </summary>
        /// <param name="player">The player to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task UpdateAsync(Player player);

        /// <summary>
        /// Deletes a player from the database by their identifier.
        /// </summary>
        /// <param name="id">The player identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task DeleteAsync(string id);
    }
}

// -----------------------------------------------------------------------
// <copyright file="ILocationsDAL.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace PointOfNoReturn.Data.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PointOfNoReturn.Data.Models;

    /// <summary>
    /// The ILocationsDAL interface defines the methods for data access layer (DAL) operations related to locations.
    /// It provides methods to add, delete, update, and retrieve locations from the data source.
    /// </summary>
    public interface ILocationsDAL
    {
        /// <summary>
        /// Retrieves all locations from the data source asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a list of locations.</returns>
        Task<List<Location>> GetAllAsync();

        /// <summary>
        /// Retrieves a location by its ID from the data source asynchronously.
        /// </summary>
        /// <param name="id">The ID of the location to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, containing the location.</returns>
        Task<Location?> GetByIdAsync(string id);

        /// <summary>
        /// Adds a new location to the data source asynchronously.
        /// </summary>
        /// <param name="location">The location to add.</param>
        /// <returns>The location with new ID set.</returns>
        Task<Location> AddAsync(Location location);

        /// <summary>
        /// Updates an existing location in the data source asynchronously.
        /// </summary>
        /// <param name="location">The location to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(Location location);

        /// <summary>
        /// Deletes a location from the data source asynchronously.
        /// </summary>
        /// <param name="id">The ID of the location to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(string id);
    }
}

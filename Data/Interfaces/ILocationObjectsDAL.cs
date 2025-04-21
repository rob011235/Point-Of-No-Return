// -----------------------------------------------------------------------
// <copyright file="ILocationObjectsDAL.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace PointOfNoReturn.Data.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PointOfNoReturn.Data.Models;

    /// <summary>
    /// Interface for LocationObjects Data Access Layer.
    /// </summary>
    public interface ILocationObjectsDAL
    {
        /// <summary>
        /// Retrieves all LocationObjects from the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of LocationObjects.</returns>
        public Task<List<LocationObject>> GetAllAsync();

        /// <summary>
        /// Retrieves a LocationObject by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the LocationObject to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the LocationObject with the specified identifier.</returns>
        public Task<LocationObject?> GetByIdAsync(string id);

        /// <summary>
        /// Adds a new LocationObject to the database.
        /// </summary>
        /// <param name="locationObject">The LocationObject to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task<LocationObject> AddAsync(LocationObject locationObject);

        /// <summary>
        /// Updates an existing LocationObject in the database.
        /// </summary>
        /// <param name="locationObject">The LocationObject to update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task UpdateAsync(LocationObject locationObject);

        /// <summary>
        /// Deletes a LocationObject from the database by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the LocationObject to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task DeleteAsync(string id);
    }
}

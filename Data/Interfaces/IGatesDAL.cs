// -----------------------------------------------------------------------
// <copyright file="IGatesDAL.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace PointOfNoReturn.Data.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PointOfNoReturn.Data.Models;

    /// <summary>
    /// Interface for Gates Data Access Layer.
    /// </summary>
    public interface IGatesDAL
    {
        /// <summary>
        /// Retrieves all gates from the database.
        /// </summary>
        /// <returns>List of gates.</returns>
        public Task<List<Gate>> GetAllAsync();

        /// <summary>
        /// Retrieves a gate by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the gate.</param>
        /// <returns>Gate with the specified identifier.</returns>
        public Task<Gate?> GetGateByIdAsync(string id);

        /// <summary>
        /// Retrieves a gate by its LocationObjectId.
        /// </summary>
        /// <param name="id">Identifier of the LocationObject.</param>
        /// <returns>Gate with the specified LocationObject identifier.</returns>
        public Task<Gate?> GetGateByLocationObjectIdAsync(string id);

        /// <summary>
        /// Adds a new gate to the database.
        /// </summary>
        /// <param name="gate">Gate to add.</param>
        /// <returns>Task.</returns>
        public Task<Gate> AddAsync(Gate gate);

        /// <summary>
        /// Updates an existing gate in the database.
        /// </summary>
        /// <param name="gate">Gate to update.</param>
        /// <returns>Task.</returns>
        public Task UpdateAsync(Gate gate);

        /// <summary>
        /// Deletes a gate from the database.
        /// </summary>
        /// <param name="id">Identifier of the gate to delete.</param>
        /// <returns>Task.</returns>
        public Task DeleteGateAsync(string id);
    }
}

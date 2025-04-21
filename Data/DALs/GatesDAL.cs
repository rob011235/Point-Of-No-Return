// -----------------------------------------------------------------------
// <copyright file="GatesDAL.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace PointOfNoReturn.Data.DALs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using PointOfNoReturn.Data.Interfaces;
    using PointOfNoReturn.Data.Models;

    /// <summary>
    /// Data Access Layer for managing gates.
    /// </summary>
    public class GatesDAL : IGatesDAL
    {
        private readonly ServerDbContext context = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatesDAL"/> class.
        /// </summary>
        /// <param name="context">The database connection string.</param>
        public GatesDAL(ServerDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Retrieves all gates from the database.
        /// </summary>
        /// <returns>A collection of gates.</returns>
        public async Task<List<Gate>> GetAllAsync()
        {
            return await this.context.Gates.ToListAsync();
        }

        /// <summary>
        /// Retrieves a gate by its identifier.
        /// </summary>
        /// <param name="id">The gate identifier.</param>
        /// <returns>The gate with the specified identifier.</returns>
        public async Task<Gate?> GetGateByIdAsync(string id)
        {
            return await this.context.Gates.FindAsync(id);
        }

        /// <summary>
        /// Retrieves a gate by its LocationObjectId.
        /// </summary>
        /// <param name="id">The LocationObject identifier.</param>
        /// <returns>The gate with the specified LocationObject identifier.</returns>
        public async Task<Gate?> GetGateByLocationObjectIdAsync(string id)
        {
             var gate = await this.context.Gates.Where(g => g.LocationObjectId == id).FirstOrDefaultAsync();
             return gate;
        }

        /// <summary>
        /// Adds a new gate to the database.
        /// </summary>
        /// <param name="gate">The gate to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<Gate> AddAsync(Gate gate)
        {
            gate.Id = Guid.NewGuid().ToString();
            this.context.Gates.Add(gate);
            await this.context.SaveChangesAsync();
            return gate;
        }

        /// <summary>
        /// Updates an existing gate in the database.
        /// </summary>
        /// <param name="gate">The gate to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(Gate gate)
        {
            if (string.IsNullOrEmpty(gate.Id))
            {
                // Find a gate with the same object ID and set the ID to that gate's ID
                var existingGate = await this.context.Gates.FirstOrDefaultAsync(g => g.LocationObjectId == gate.LocationObjectId);
                if (existingGate == null)
                {
                    return;
                }

                gate.Id = existingGate.Id;
            }

            this.context.Entry(gate).State = EntityState.Modified;
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a gate from the database.
        /// </summary>
        /// <param name="id">The identifier of the gate to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteGateAsync(string id)
        {
            var gate = await this.context.Gates.FindAsync(id);
            if (gate is not null)
            {
                this.context.Gates.Remove(gate);
                await this.context.SaveChangesAsync();
            }
        }
    }
}

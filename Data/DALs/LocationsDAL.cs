// -----------------------------------------------------------------------
// <copyright file="LocationsDAL.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace PointOfNoReturn.Data.DALs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using PointOfNoReturn.Data.Interfaces;
    using PointOfNoReturn.Data.Models;

    /// <summary>
    /// LocationsDAL is a data access layer class that provides methods to interact with the Locations table in the database.
    /// It implements the ILocationsDAL interface and uses Entity Framework Core to perform CRUD operations.
    /// </summary>
    public class LocationsDAL : ILocationsDAL
    {
        private readonly ClientDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationsDAL"/> class.
        /// </summary>
        /// <param name="context">The database context to use for data access.</param>
        public LocationsDAL(ClientDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets all locations from the database asynchronously.
        /// </summary>
        /// <returns>A list of all locations.</returns>
        public async Task<List<Location>> GetAllAsync()
        {
            return await this.context.Locations.ToListAsync();
        }

        /// <summary>
        /// Gets a location by its ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the location to retrieve.</param>
        /// <returns>The location with the specified ID, or null if not found.</returns>
        public async Task<Location?> GetByIdAsync(string id)
        {
            return await this.context.Locations.FindAsync(id);
        }

        /// <summary>
        /// Gets a location by its domain name asynchronously.
        /// </summary>
        /// <param name="location">The domain name of the location to retrieve.</param>
        /// <returns>An asynchronous Task.</returns>
        public async Task<Location> AddAsync(Location location)
        {
            location.Id = Guid.NewGuid().ToString();
            await this.context.Locations.AddAsync(location);
            await this.context.SaveChangesAsync();
            return location;
        }

        /// <summary>
        /// Updates an existing location in the database asynchronously.
        /// </summary>
        /// <param name="location">The location to update.</param>
        /// <returns>An asynchronous Task.</returns>
        public async Task UpdateAsync(Location location)
        {
            this.context.Entry(location).State = EntityState.Modified;
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a location by its ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the location to delete.</param>
        /// <returns>An asynchronous Task.</returns>
        public async Task DeleteAsync(string id)
        {
            var location = await this.context.Locations.FindAsync(id);
            if (location != null)
            {
                this.context.Locations.Remove(location);
                await this.context.SaveChangesAsync();
            }
        }
    }
}

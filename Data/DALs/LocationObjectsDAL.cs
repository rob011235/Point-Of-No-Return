// -----------------------------------------------------------------------
// <copyright file="LocationObjectsDAL.cs" company="Rob Garner">
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
    /// Data Access Layer for managing location objects.
    /// </summary>
    public class LocationObjectsDAL : ILocationObjectsDAL
    {
        private readonly ServerDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationObjectsDAL"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public LocationObjectsDAL(ServerDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Retrieves all LocationObjects from the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of LocationObjects.</returns>
        public async Task<List<LocationObject>> GetAllAsync()
        {
            return await this.context.LocationObjects.ToListAsync();
        }

        /// <summary>
        /// Retrieves a LocationObject by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the LocationObject to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the LocationObject with the specified identifier.</returns>
        public async Task<LocationObject?> GetByIdAsync(string id)
        {
            return await this.context.LocationObjects.FindAsync(id);
        }

        /// <summary>
        /// Adds a new LocationObject to the database.
        /// </summary>
        /// <param name="locationObject">The LocationObject to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task<LocationObject> AddAsync(LocationObject locationObject)
        {
            locationObject.Id = Guid.NewGuid().ToString();
            await this.context.LocationObjects.AddAsync(locationObject);
            await this.context.SaveChangesAsync();
            return locationObject;
        }

        /// <summary>
        /// Updates an existing LocationObject in the database.
        /// </summary>
        /// <param name="locationObject">The LocationObject to update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateAsync(LocationObject locationObject)
        {
            this.context.Entry(locationObject).State = EntityState.Modified;
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a LocationObject from the database by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the LocationObject to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeleteAsync(string id)
        {
            var locationObject = await this.context.LocationObjects.FindAsync(id);
            if (locationObject != null)
            {
                this.context.LocationObjects.Remove(locationObject);
                await this.context.SaveChangesAsync();
            }
        }
    }
}

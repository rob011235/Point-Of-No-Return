// -----------------------------------------------------------------------
// <copyright file="PlayersDAL.cs" company="Rob Garner">
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
    /// Players Data Access Layer.
    /// </summary>
    public partial class PlayersDAL : IPlayersDAL
    {
        private readonly ServerDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayersDAL"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public PlayersDAL(ServerDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets all players.
        /// </summary>
        /// <returns>A list of players.</returns>
        public async Task<List<Player>> GetAllAsync()
        {
            return await this.context.Players.ToListAsync();
        }

        /// <summary>
        /// Gets a player by ID.
        /// </summary>
        /// <param name="id">The player ID.</param>
        /// <returns>The player.</returns>
        public async Task<Player?> GetByIdAsync(string id)
        {
            return await this.context.Players.FindAsync(id);
        }

        /// <summary>
        /// Gets a player by ID.
        /// </summary>
        /// <param name="id">The player ID.</param>
        /// <returns>The player.</returns>
        public async Task<Player?> GetByUserIdAsync(string id)
        {
            return await this.context.Players.Where(p => p.UserId == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a player by user ID.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>The player.</returns>
        public Player? GetByUserId(string id)
        {
            return this.context.Players.Where(p => p.UserId == id).FirstOrDefault();
        }

        /// <summary>
        /// Adds a new player.
        /// </summary>
        /// <param name="player">The player to add.</param>
        /// <returns>The player.</returns>
        public async Task AddAsync(Player player)
        {
            player.Id = Guid.NewGuid().ToString();
            await this.context.Players.AddAsync(player);
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing player.
        /// </summary>
        /// <param name="player">The player to update.</param>
        /// <returns>A Task.</returns>
        public async Task UpdateAsync(Player player)
        {
            this.context.Entry(player).State = EntityState.Modified;
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a player by ID.
        /// </summary>
        /// <param name="id">The player ID.</param>
        /// <returns>A Task.</returns>
        public async Task DeleteAsync(string id)
        {
            var player = await this.context.Players.FindAsync(id);
            if (player != null)
            {
                this.context.Players.Remove(player);
                await this.context.SaveChangesAsync();
            }
        }
    }
}

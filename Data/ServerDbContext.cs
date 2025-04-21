// -----------------------------------------------------------------------
// <copyright file="ServerDbContext.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using PointOfNoReturn.Data.Models;

/// <summary>
/// Represents the database context for the server.
/// </summary>
public class ServerDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used initialize the context.</param>
    public ServerDbContext(DbContextOptions<ServerDbContext> options)
        : base(options)
    {
        this.Database.EnsureCreated();
    }

    /// <summary>
    /// Gets or sets the collection of players in the database.
    /// </summary>
    public DbSet<Player> Players { get; set; } = default!;

    /// <summary>
    /// Gets or sets the collection of items in the database.
    /// </summary>
    public DbSet<LocationObject> LocationObjects { get; set; } = default!;

    /// <summary>
    /// Gets or sets the collection of gates for the server in the database.
    /// </summary>
    public DbSet<Gate> Gates { get; set; } = default!;
}

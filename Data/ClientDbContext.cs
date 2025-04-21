// -----------------------------------------------------------------------
// <copyright file="ClientDbContext.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using PointOfNoReturn.Data.Models;

/// <summary>
/// Represents the database context for the client application.
/// </summary>
public class ClientDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the context.</param>
    public ClientDbContext(DbContextOptions<ClientDbContext> options)
        : base(options)
    {
        this.Database.EnsureCreated();
    }

    /// <summary>
    /// Gets or sets location data set.
    /// </summary>
    public DbSet<Location> Locations { get; set; } = default!;
}

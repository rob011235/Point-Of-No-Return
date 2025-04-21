// -----------------------------------------------------------------------
// <copyright file="Services.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using PointOfNoReturn.Data.DALs;
using PointOfNoReturn.Data.Interfaces;

/// <summary>
/// Services is a singleton class that provides access to the database contexts and data access layers.
/// It is used to manage the database connections and provide access to the data access layers.
/// The class is responsible for creating and managing the database contexts for both the server and client.
/// </summary>
public class Services
{
    private const string SERVERCONNECTIONSTRING = "Data Source=server_data.db";
    private const string CLIENTCONNECTIONSTRING = "Data Source=client_data.db";
    private static Services instance = default!;

    /// <summary>
    /// Gets the singleton instance of the Services class.
    /// This property is used to access the Services instance from other scripts.
    /// </summary>
    /// <returns>Returns the singleton instance of the Services class.</returns>
    public static Services Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Services();
            }

            return instance;
        }
    }

    /// <summary>
    /// Gets the server database context.
    /// This property is used to access the server database context.
    /// </summary>
    /// <returns>Returns an instance of the server database context.</returns>
    public ServerDbContext ServerDbContext
    {
        get
        {
            var optionsBuilder = new DbContextOptionsBuilder<ServerDbContext>();
            optionsBuilder.UseSqlite(SERVERCONNECTIONSTRING);
            return new ServerDbContext(optionsBuilder.Options);
        }
    }

    /// <summary>
    /// Gets the players data access layer.
    /// This property is used to access players data from the database.
    /// </summary>
    /// <returns>Returns an instance of the players data access layer.</returns>
    public IPlayersDAL PlayersDAL
    {
        get
        {
            return new PlayersDAL(this.ServerDbContext);
        }
    }

    /// <summary>
    /// Gets the location objects data access layer.
    /// This property is used to access location objects data from the database.
    /// </summary>
    /// <returns>Returns an instance of the location objects data access layer.</returns>
    public ILocationObjectsDAL LocationObjectsDAL
    {
        get
        {
            return new LocationObjectsDAL(this.ServerDbContext);
        }
    }

    /// <summary>
    /// Gets the client database context.
    /// This property is used to access the client database context.
    /// </summary>
    /// <returns>Returns an instance of the client database context.</returns>
    public ClientDbContext ClientDbContext
    {
        get
        {
            var optionsBuilder = new DbContextOptionsBuilder<ClientDbContext>();
            optionsBuilder.UseSqlite(CLIENTCONNECTIONSTRING);
            return new ClientDbContext(optionsBuilder.Options);
        }
    }

    /// <summary>
    /// Gets the locations data access layer.
    /// This property is used to access locations data from the database.
    /// </summary>
    /// <returns>Returns an instance of the locations data access layer.</returns>
    public ILocationsDAL LocationsDAL
    {
        get
        {
            return new LocationsDAL(this.ClientDbContext);
        }
    }

    /// <summary>
    /// Gets the gates data access layer.
    /// This property is used to access gates data from the database.
    /// </summary>
    /// <returns>Returns an instance of the gates data access layer.</returns>
    public IGatesDAL GatesDAL
    {
        get
        {
            return new GatesDAL(this.ServerDbContext);
        }
    }
}

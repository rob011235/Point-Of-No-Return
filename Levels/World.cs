// -----------------------------------------------------------------------
// <copyright file="World.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using Org.BouncyCastle.Tls;
using PointOfNoReturn.Data.Models;

/// <summary>
/// This class represents the game world and is responsible for managing the game state, players, and objects in the world.
/// It handles the multiplayer functionality, including spawning and removing players and objects.
/// </summary>
public partial class World : Node3D
{
    private List<LocationObject> locationObjects = default!;
    private List<Gate> gates = default!;
    // private PackedScene playerScene = GD.Load<PackedScene>("res://Actors/Player/Player3D.tscn");
    private MultiplayerSpawner multiplayerSpawner = default!;

    /// <summary>
    /// Gets a singleton instance of the World class.
    /// This instance is used to access the World class from other parts of the code.
    /// </summary>
    public static World Instance { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the multiplayer peer instance.
    /// </summary>
    public Player3D? Player { get; set; } = null!;

    /// <summary>
    /// Gets or sets a list of spawnable objects in the game world.
    /// This list is used to define the objects that can be spawned in the game world.
    /// </summary>
    public List<SpawnableObject> SpawnableObjects { get; set; } = new List<SpawnableObject>()
    {
        new SpawnableObject { Name = "BalloonPlatform", Distance = 30.0f, Height = 0.0f },
        new SpawnableObject { Name = "LargeBalloonPlatform", Distance = 70.0f, Height = 0.0f },
        new SpawnableObject { Name = "DraggableBox", Distance = 5.0f, Height = 0.5f },
        new SpawnableObject { Name = "GiffardAirship", Distance = 0.0f, Height = 0.0f },
        new SpawnableObject { Name = "Bridge", Distance = 7.0f, Height = 0.0f },
        new SpawnableObject { Name = "WalkInGate", Distance = 10.0f, Height = 1.0f },
        new SpawnableObject { Name = "Museum", Distance = 7.0f, Height = -1.0f },
    };

    /// <summary>
    /// Gets or sets a value indicating whether the current instance is the host.
    /// This property is used to determine if the current instance is responsible for managing the game state and objects.
    /// </summary>
    //public bool IsHost { get; set; } = false;

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// This method is used to initialize the World class and set up the game state.
    /// It is called when the scene is loaded and the node is added to the scene tree.
    /// </summary>
    public override async void _Ready()
    {
        this.multiplayerSpawner = this.GetNode<MultiplayerSpawner>("MultiplayerSpawner");
        foreach (SpawnableObject spawnableObject in this.SpawnableObjects)
        {
            this.multiplayerSpawner.AddSpawnableScene(spawnableObject.Path);
        }

        Instance = this;
        if (ServerManager.Instance.IsHost)
        {
            await this.SetUpServerAsync();
        }
    }

    /// <summary>
    /// Sets up the server by connecting to the multiplayer peer and spawning players and objects.
    /// This method is called when the instance is the host and is responsible for managing the game state.
    /// It connects to the multiplayer peer and sets up the game state for the server.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SetUpServerAsync()
    {
        // Input.MouseMode = Input.MouseModeEnum.Visible;
        // this.Multiplayer.PeerConnected += this.AddPlayer;
        // this.Multiplayer.PeerDisconnected += this.RemovePlayer;
        //this.IsHost = true;

        // Gui.Instance.SetTitle("Server");
        var dal = Services.Instance.LocationObjectsDAL;
        this.locationObjects = await Services.Instance.LocationObjectsDAL.GetAllAsync();
        this.gates = await Services.Instance.GatesDAL.GetAllAsync();
        await this.EnsureAtLeastOneObjectAsync(dal);
        foreach (var worldObject in this.locationObjects)
        {
            await this.ServerSpawnObjectAsync(worldObject);
        }
    }

    /// <summary>
    /// Spawns an object in the game world on the server.
    /// This method is called when the server needs to spawn an object in the game world.
    /// It creates a new instance of the object and adds it to the scene tree.
    /// </summary>
    /// <param name="locationObject">The location object to spawn.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ServerSpawnObjectAsync(LocationObject locationObject)
    {
        if (!this.Multiplayer.IsServer())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(locationObject.Id))
        {
            GD.PrintErr("Cannot spawn object: locationObject.Id is null or empty.");
            return;
        }

        var scene = (PackedScene)GD.Load($"res://Objects/{locationObject.ScenePath}.tscn");
        var spawnling = scene.Instantiate<Node3D>();
        spawnling.Name = locationObject.Id;
        spawnling.Position = locationObject.Position;
        spawnling.Rotation = locationObject.Rotation;
        this.AddChild(spawnling, true);

        // If this is a walk-in gate, set the domain name
        if (spawnling is WalkInGate)
        {
            var walkInGate = spawnling as WalkInGate;
            if (walkInGate != null)
            {
                Gate? gate = this.gates.Find(g => g.LocationObjectId == locationObject.Id);
                if (gate == null)
                {
                    // Create a new gate
                    gate = new Gate
                    {
                        DomainName = ServerManager.Instance.DomainName,
                        LocationObjectId = locationObject.Id,
                        Position = locationObject.Position,
                        Rotation = locationObject.Rotation,
                    };

                    // add new gate to database and local list
                    this.gates.Add(await Services.Instance.GatesDAL.AddAsync(gate));
                }

                walkInGate.Gate = gate;
            }
        }

        return;
    }

    // /// <summary>
    // /// Adds a player to the game world.
    // /// This method is called when a new player connects to the game world.
    // /// </summary>
    // /// <param name="id">Id assigned to player by Multiplayer Spawner.</param>
    // public void AddPlayer(long id)
    // {
    //     var player = this.playerScene.Instantiate() as Player3D;
    //     if (player == null)
    //     {
    //         GD.Print($"Could not instantiate player");
    //         return;
    //     }

    //     if (this.IsMultiplayerAuthority())
    //     {
    //         this.Player = player;
    //     }

    //     player.Name = id.ToString();
    //     this.AddChild(player);
    // }

    /// <summary>
    /// Spawns an object in the game world for the player.
    /// This method is called when the player needs to spawn an object in the game world.
    /// </summary>
    /// <param name="locationObject">The location object to spawn.</param>
    public void PlayerSpawnObject(LocationObject locationObject)
    {
        var locationObjectJson = JsonSerializer.Serialize(locationObject);
        this.RpcId(1, nameof(this.ServerSpawnPlayerObject), locationObjectJson);
    }

    /// <summary>
    /// Spawns an object in the game world on the server.
    /// This method is called when the server needs to spawn an object in the game world.
    /// </summary>
    /// <param name="locationObjectJson">The location object to spawn in JSON format.</param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    public async void ServerSpawnPlayerObject(string locationObjectJson)
    {
        var locationObject = JsonSerializer.Deserialize<LocationObject>(locationObjectJson);
        if (locationObject == null)
        {
            GD.PrintErr("Cannot spawn object: locationObject is null.");
            return;
        }

        locationObject = await Services.Instance.LocationObjectsDAL.AddAsync(locationObject);

        await this.ServerSpawnObjectAsync(locationObject);
    }

    /// <summary>
    /// Removes an object from the game world for the player.
    /// This method is called when the player needs to remove an object from the game world.
    /// Object removal has to be done on the server, so this method calls the server to remove the object.
    /// </summary>
    /// <param name="nodePath">Node path to object to be removed.</param>
    public void PlayerRemoveObject(NodePath nodePath)
    {
        this.RpcId(1, "RemoveObject", nodePath);
    }

    /// <summary>
    /// Removes an object from the game world on the server.
    /// This method is called from the server when it is triggered by the
    ///  PlayerRemoveObject method.
    /// </summary>
    /// <param name="nodePath">Node path to object to be removed.</param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    public async void RemoveObject(string nodePath)
    {
        if (!this.Multiplayer.IsServer())
        {
            return;
        }

        var objectToRemove = this.GetNodeOrNull(nodePath);
        if (objectToRemove != null)
        {
            objectToRemove.QueueFree();
            await Services.Instance.LocationObjectsDAL.DeleteAsync(objectToRemove.Name);
        }
        else
        {
            GD.Print("Object not found: " + nodePath);
        }
    }

    /// <summary>
    /// Ensures that there is at least one platform in the game world.
    /// This method checks if there are any objects in the game world and adds a GiffardAirship if none are found.
    /// It is called when the server is set up and is responsible for ensuring that the game world has at least
    ///  one object for the player to start on.
    /// </summary>
    /// <param name="dal">The data access layer for location objects.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task EnsureAtLeastOneObjectAsync(PointOfNoReturn.Data.Interfaces.ILocationObjectsDAL dal)
    {
        if (this.locationObjects.Count == 0)
        {
            LocationObject locationObject = new LocationObject
            {
                Id = Guid.NewGuid().ToString(),
                ScenePath = "GiffardAirship",
                PositionX = 0,
                PositionY = 0,
                PositionZ = 0,
                RotationX = 0,
                RotationY = 0,
                RotationZ = 0,
            };
            this.locationObjects.Add(locationObject);
            await dal.AddAsync(locationObject);
        }
    }

    // /// <summary>
    // /// Removes a player from the game world.
    // /// This method is called when a player disconnects from the game world.
    // /// </summary>
    // private void RemovePlayer(long id)
    // {
    //     var player = this.GetNodeOrNull<Player3D>(id.ToString());
    //     if (player != null)
    //     {
    //         player.QueueFree();
    //     }
    // }
}

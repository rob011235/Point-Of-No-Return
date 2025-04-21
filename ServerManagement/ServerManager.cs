// -----------------------------------------------------------------------
// <copyright file="ServerManager.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Godot;
using PointOfNoReturn.Data.Models;

/// <summary>
/// GameManager is a singleton class that manages the game state and multiplayer connections.
/// It handles the creation and destruction of game scenes, as well as the connection to a server.
/// It also manages the travel menu and the main menu.
/// The GameManager is responsible for creating and destroying the world and player scenes.
/// It also handles the input events for the travel menu.
/// </summary>
public partial class ServerManager : Node
{
    // private List<LoggedUser>? loggedUsers = new List<LoggedUser>();
    private PackedScene worldScene = GD.Load<PackedScene>("res://Levels/World.tscn");
    private World? worldNode = null!;

    // private PackedScene playerScene = GD.Load<PackedScene>("res://Actors/Player/Player3D.tscn");
    // private Player3D? player = null!;
    private PackedScene travelScene = GD.Load<PackedScene>("res://Levels/Travel.tscn");
    private Node3D? travelSceneNode = null!;
    private PackedScene travelMenuScene = GD.Load<PackedScene>("res://Menus/TravelMenu.tscn");
    private Control? travelMenu = null;
    private PackedScene pauseMenuScene = GD.Load<PackedScene>("res://Menus/PauseMenu.tscn");
    private PackedScene playerScene = GD.Load<PackedScene>("res://Actors/Player/Player3D.tscn");

    /// <summary>
    /// Gets or sets the instance of the GameManager.
    /// This is a singleton instance that is used to access the GameManager from other classes.
    /// </summary>
    public static ServerManager Instance { get; set; } = default!;

    /// <summary>
    /// Gets or sets the port number for the multiplayer connection.
    /// This is the port number that the server will listen on for incoming connections.
    /// The default port number is 9999.
    /// </summary>
    public int Port { get; set; } = 9999;

    /// <summary>
    /// Gets or sets the address of the server.
    /// This is the address that the client will connect to.
    /// The default address is "localhost".
    public string DomainName { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets a value indicating whether the current instance is the host.
    /// The default value is false, which means that the current instance is a client.
    /// </summary>
    public bool IsHost { get; set; } = false;

    /// <summary>
    /// Gets or sets the ENetMultiplayerPeer instance.
    /// This is the peer that will be used for the multiplayer connection.
    /// </summary>
    public ENetMultiplayerPeer? Peer { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether traveling is enabled.
    /// This property is used to enable or disable the travel menu.
    /// The default value is false, which means that the travel menu is disabled.
    /// </summary>
    public bool TravelingEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the Gate instance.
    /// This is gate information that the player will use to determine their spawn location and rotation.
    /// The default value is null, which means that the player will spawn at the default location.
    /// </summary>
    public Gate? Gate { get; set; }

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// </summary>
    public override void _Ready()
    {
        Instance = this;
        if (OS.HasFeature("dedicated_server"))
        {
            GD.Print("Hosting as dedicated server.");
            this.Host();
        }
        else
        {
            this.ShowTravelMenu();
        }
    }

    /// <summary>
    /// Called every frame. This method is responsible for processing input events.
    /// It checks for input events and handles them accordingly.
    /// </summary>
    /// <param name="event">Provides access to the event that triggered the call.</param>
    public override void _Input(InputEvent @event)
    {
        if (this.TravelingEnabled)
        {
            if (Input.IsActionJustPressed("travel"))
            {
                if (this.travelMenu == null)
                {
                    Player3D.Instance.IsUnderPlayerControl = false;
                    Input.MouseMode = Input.MouseModeEnum.Confined;
                    this.travelMenu = this.travelMenuScene.Instantiate<Control>();

                    this.AddChild(this.travelMenu);
                }
                else
                {
                    Player3D.Instance.IsUnderPlayerControl = true;
                    this.travelMenu.QueueFree();
                    this.travelMenu = null;
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                }
            }
        }

        if (Input.IsActionJustPressed("pause"))
        {
            var pauseMenu = this.GetNodeOrNull<PauseMenu>("PauseMenu");
            if (pauseMenu == null)
            {
                pauseMenu = this.pauseMenuScene.Instantiate<PauseMenu>();
                this.AddChild(pauseMenu);
                if (Player3D.Instance != null)
                {
                    Player3D.Instance.IsUnderPlayerControl = false;
                }
            }
        }
    }

    /// <summary>
    /// Start the game as a host.
    /// </summary>
    public void Host()
    {
        this.IsHost = true;

        this.worldNode = this.worldScene.Instantiate<World>();
        this.AddChild(this.worldNode);

        this.Peer ??= new ENetMultiplayerPeer();
        this.Peer.CreateServer(this.Port);
        this.Multiplayer.MultiplayerPeer = this.Peer;

        this.Multiplayer.PeerConnected += this.AddPlayer;
        this.Multiplayer.PeerDisconnected += this.RemovePlayer;
        GD.Print($"{this.Multiplayer.GetUniqueId()}: started server on port {this.Port}.");
    }

    /// <summary>
    /// Shows the trave menu.
    /// </summary>
    public void ShowTravelMenu()
    {
        var travelMenu = this.GetNodeOrNull<TravelMenu>("TravelMenu");
        if (travelMenu == null)
        {
            this.travelMenu = this.travelMenuScene.Instantiate<TravelMenu>();
            this.AddChild(this.travelMenu);
        }
    }

    /// <summary>
    /// Hides the travel menu.
    /// </summary>
    public void HideTravelMenu()
    {
        var travelMenu = this.GetNodeOrNull<TravelMenu>("TravelMenu");
        if (travelMenu != null)
        {
            travelMenu.QueueFree();
        }
    }

    /// <summary>
    /// Start the game as a client.
    /// </summary>
    /// <param name="domainName">The address of the server to connect to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartClientFromTravelMenu(string domainName)
    {
        this.travelMenu?.QueueFree();
        this.travelMenu = null;
        this.DomainName = domainName;
        this.IsHost = false;

        if (this.worldNode != null)
        {
            this.Leave();
            this.travelSceneNode = this.travelScene.Instantiate<Node3D>();
            this.AddChild(this.travelSceneNode);

            await this.ToSignal(this.GetTree().CreateTimer(0.01), "timeout");
            this.travelSceneNode.QueueFree();
            this.travelSceneNode = null;
        }

        this.Peer = new ENetMultiplayerPeer();
        var result = this.Peer.CreateClient(this.DomainName, this.Port);
        if (result == Error.Ok)
        {
            this.Multiplayer.MultiplayerPeer = this.Peer;
            this.worldNode = this.worldScene.Instantiate<World>();
            this.AddChild(this.worldNode);
            if (this.worldNode.Player != null)
            {
                this.worldNode.Player.Position = Godot.Vector3.Zero;
                this.worldNode.Player.Rotation = Godot.Vector3.Zero;
            }
            GD.Print($"{this.Multiplayer.GetUniqueId()}: started client with address {this.DomainName} and port {this.Port}.");
        }
        else
        {
            GD.Print($"Failed to connect to server. Error: {result.ToString()}");
        }
    }

    /// <summary>
    /// Start the game as a client.
    /// </summary>
    /// <param name="gate">The gate to connect to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartClientFromGate(Gate gate)
    {
        this.DomainName = gate.DomainName ?? "localhost";
        this.IsHost = false;

        if (this.worldNode != null)
        {
            this.Leave();
            await this.ToSignal(this.GetTree().CreateTimer(0.01), "timeout");
        }

        this.Peer = new ENetMultiplayerPeer();
        var result = this.Peer.CreateClient(this.DomainName, this.Port);
        if (result == Error.Ok)
        {
            this.Gate = gate;
            this.Multiplayer.MultiplayerPeer = this.Peer;
            this.worldNode = this.worldScene.Instantiate<World>();
            this.AddChild(this.worldNode);
            if (this.worldNode.Player != null)
            {
                this.worldNode.Player.Position = this.Gate.Position;
                this.worldNode.Player.Rotation = this.Gate.Rotation;
            }
        }
        else
        {
            GD.PrintErr($"Failed to connect to server. Error: {result.ToString()}");
        }
    }

    /// <summary>
    /// Adds a player to the game world.
    /// This method is called when a new player connects to the game world.
    /// </summary>
    /// <param name="id">Id assigned to player by Multiplayer Spawner.</param>
    public void AddPlayer(long id)
    {
        if (this.worldNode == null)
        {
            GD.Print($"World node is null, cannot add player {id}.");
            return;
        }

        Player3D? player = this.playerScene.Instantiate() as Player3D;
        if (player == null)
        {
            GD.Print($"Failed to instantiate player scene for player {id}.");
            return;
        }

        player.Name = id.ToString();
        this.worldNode.AddChild(player);
        GD.Print($"Player {id} added to world.");
    }

    /// <summary>
    /// Removes a player from the game world.
    /// This method is called when a player disconnects from the game world.
    /// </summary>
    /// <param name="id">Id assigned to player by Multiplayer Spawner.</param>
    public void RemovePlayer(long id)
    {
        if (this.worldNode == null)
        {
            GD.Print($"World node is null, cannot remove player {id}.");
            return;
        }

        var player = this.worldNode.GetNodeOrNull<Player3D>(id.ToString());
        if (player != null)
        {
            player.QueueFree();
            GD.Print($"Player {id} removed from world.");
        }
        else
        {
            GD.Print($"Player {id} not found in world.");
        }
    }

    /// <summary>
    /// Leave the current multiplayer session.
    /// This will close the ENet peer and disconnect from the server.
    /// </summary>
    public void Leave()
    {
        if (this.Peer != null)
        {
            this.Multiplayer.MultiplayerPeer.DisconnectPeer(1);
            this.worldNode?.QueueFree();
            this.worldNode = null;
        }
    }
}

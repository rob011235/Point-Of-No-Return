// -----------------------------------------------------------------------
// <copyright file="WalkInGate.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using PointOfNoReturn.Data.Models;

/// <summary>
/// WalkInGate is a class that represents a walk-in gate in the game.
/// It inherits from Area3D and implements the IDraggableObject interface.
/// The class is used to handle the functionality of the walk-in gate, including dragging and setting travel locations.
/// It also handles the mouse events for the walk-in gate.
/// </summary>
public partial class WalkInGate : Area3D, IDraggableObject
{
    private bool isDragging = false;
    private MeshInstance3D meshInstance = default!;
    private Material hightlightMaterial = null!;
    private Control setTravelLocationPanel = default!;
    private LineEdit domainNameLineEdit = default!;
    private LineEdit posXLineEdit = default!;
    private LineEdit posYLineEdit = default!;
    private LineEdit posZLineEdit = default!;
    private LineEdit rotXLineEdit = default!;
    private LineEdit rotYLineEdit = default!;
    private LineEdit rotZLineEdit = default!;
    private Gate gate = default!;

    /// <summary>
    /// Gets or sets the list of players that just arrived at the gate.
    /// This property is used to set the location name for the travel location.
    /// </summary>
    public List<string> JustArrived { get; set; } = new List<string>();

    /// <summary>
    /// Gets the name of the location to travel to.
    /// This property is used to set the location name for the travel location.
    /// It is set to null by default, which means that the location name is not set.
    /// </summary>
    public Node3D RootNode => this;

    /// <summary>
    /// Gets or Sets a value indicating whether or not a node is being dragged.
    /// This property is used to set the location name for the travel location.
    /// It is set to null by default, which means that the location name is not set.
    /// </summary>
    public bool IsDragging
    {
        get => this.isDragging;
        set
        {
            this.isDragging = value;
            if (this.isDragging)
            {
                this.meshInstance.MaterialOverlay = this.hightlightMaterial;
                this.setTravelLocationPanel.Visible = true;
            }
            else
            {
                this.meshInstance.MaterialOverlay = null;
                this.setTravelLocationPanel.Visible = false;
            }
        }
    }

    /// <summary>
    /// Gets or sets the gate information.
    /// </summary>
    public Gate Gate
    {
        get => this.gate;
        set
        {
            this.gate = value;
        }
    }

    /// <summary>
    /// Called when the node is added to the scene tree.
    /// This method is responsible for loading the highlight material and getting the MeshInstance3D node.
    /// It also gets the SetTravelLocationPanel node.
    /// </summary>
    public override void _Ready()
    {
        this.hightlightMaterial = GD.Load<Material>("res://Materials/HighlightMaterial.tres");

        this.setTravelLocationPanel = this.GetNode<Control>("SetTravelLocationPanel");
        this.meshInstance = this.GetNode<MeshInstance3D>("MeshInstance3D");
        this.domainNameLineEdit = this.GetNode<LineEdit>("SetTravelLocationPanel/DomainNameLineEdit");
        this.posXLineEdit = this.GetNode<LineEdit>("SetTravelLocationPanel/PosXLineEdit");
        this.posYLineEdit = this.GetNode<LineEdit>("SetTravelLocationPanel/PosYLineEdit");
        this.posZLineEdit = this.GetNode<LineEdit>("SetTravelLocationPanel/PosZLineEdit");
        this.rotXLineEdit = this.GetNode<LineEdit>("SetTravelLocationPanel/RotXLineEdit");
        this.rotYLineEdit = this.GetNode<LineEdit>("SetTravelLocationPanel/RotYLineEdit");
        this.rotZLineEdit = this.GetNode<LineEdit>("SetTravelLocationPanel/RotZLineEdit");

        // calll back to server and get the domain name, target position and rotation
        if (!this.Multiplayer.IsServer())
        {
            this.RpcId(1, nameof(this.RequestGateInfo));
        }
    }

    /// <summary>
    /// Called by the client to request the domain name for the gate.
    /// This method is responsible for sending a reply to the client with the domain name,
    /// targetposition and rotation.
    /// </summary>
    /// <param name="body">Body detected by Godot.</param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    public void RequestGateInfo()
    {
        var clientId = this.Multiplayer.GetRemoteSenderId();
        if (this.Gate == null)
        {
            GD.PrintErr($"{this.Multiplayer.GetUniqueId()}: RequestGateInfo: Gate is null.");
            return;
        }

        string? gateJson = JsonSerializer.Serialize(this.Gate);
        this.RpcId(clientId, nameof(this.ReplyWithGateInfo), gateJson);
    }

    /// <summary>
    /// Called when the server replies with the domain name for the gate.
    /// This method is responsible for setting the domain name for the gate and updating the domain name label.
    /// </summary>
    /// <param name="gateJson">Domain name of the gate.</param>
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
    public void ReplyWithGateInfo(string gateJson)
    {
        Gate? gate = JsonSerializer.Deserialize<Gate>(gateJson);
        if (gate == null)
        {
            GD.PrintErr($"{this.Multiplayer.GetUniqueId()}: ReplyWithGateInfo: Gate is null.");
            return;
        }

        this.SetGateInfo(gate);
    }

    /// <summary>
    /// Called when the node is removed from the scene tree.
    /// This method is responsible for removing the highlight material and setting the IsDragging property to false.
    /// </summary>
    /// <param name="body">Body detected by Godot.</param>
    public async void OnBodyEntered(Node3D body)
    {
        if (this.Multiplayer.IsServer())
        {
            return;
        }

        if (body is not Player3D)
        {
            return;
        }

        if (this.Gate != null)
        {
            await ServerManager.Instance.StartClientFromGate(this.Gate);
        }
        else
        {
            GD.PrintErr($"{this.Multiplayer.GetUniqueId()}: {body.Name} DomainName is null can't travel.");
        }
    }

    /// <summary>
    /// Called when mouse enters a Node. Highlights that node so user knows it is selectable.
    /// </summary>
    public void OnMouseEntered()
    {
        ObjectController.Instance.MouseOverObject = this;
        this.meshInstance.MaterialOverlay = this.hightlightMaterial;
    }

    /// <summary>
    /// Called when mouse exits a Node. Removes highlight from the node.
    /// </summary>
    public void OnMouseExited()
    {
        ObjectController.Instance.MouseOverObject = null;
        if (!this.isDragging)
        {
            this.meshInstance.MaterialOverlay = null;
        }
    }

    /// <summary>
    /// Called when the domain name line edit gains focus.
    /// This method is responsible for disabling wasd object movement
    /// while text is being entered.
    /// </summary>
    public void DomainNameLineEditOnFocusEntered()
    {
        ObjectController.Instance.DisableMovement = true;
    }

    /// <summary>
    /// Called when the domain name line edit loses focus.
    /// This method is responsible for re-enabling wasd object movement.
    /// </summary>
    public void DomainNameLineEditOnFocusExited()
    {
       ObjectController.Instance.DisableMovement = false;
    }

    /// <summary>
    /// Called when the user presses the SetTravelLocation button.
    /// This method is responsible for setting the travel location for the walk-in gate.
    /// It calls the server to set the travel location and updates the domain name.
    /// </summary>
    public void OnSetTravelLocationButtonPressed()
    {
        string domainName = this.domainNameLineEdit.Text;
        if (string.IsNullOrEmpty(domainName))
        {
            GD.PrintErr("Domain name cannot be empty.");
            return;
        }

        this.Gate.DomainName = domainName;
        this.Gate.Position = new Vector3(float.Parse(this.posXLineEdit.Text), float.Parse(this.posYLineEdit.Text), float.Parse(this.posZLineEdit.Text));
        this.Gate.Rotation = new Vector3(float.Parse(this.rotXLineEdit.Text), float.Parse(this.rotYLineEdit.Text), float.Parse(this.rotZLineEdit.Text));
        var gateJson = JsonSerializer.Serialize(this.Gate);
        this.Rpc(nameof(this.UpdateGateAsync), this.Name, gateJson);
    }

    /// <summary>
    /// Called when the user presses the submit button.
    /// This method is responsible for closing the travel location panel and resetting the domain name.
    /// </summary>
    /// <param name="name">Guid of the gate to update.</param>
    /// <param name="gateJson">New Domain Name for the gate.</param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    public async void UpdateGateAsync(string name, string gateJson)
    {
        if (this.Name != name)
        {
            return;
        }

        Gate? gate = JsonSerializer.Deserialize<Gate>(gateJson);
        if (gate == null)
        {
            GD.PrintErr($"{this.Multiplayer.GetUniqueId()}: UpdateGateAsync: Gate is null.");
            return;
        }

        if (this.Multiplayer.IsServer())
        {
            // Update the gate info in the database
            await Services.Instance.GatesDAL.UpdateAsync(gate);
        }

        this.Gate = gate;
    }

    /// <summary>
    /// Called when the user presses the SyncGate button.
    /// This method is responsible for syncing the gate with the server.
    /// It calls the server to sync the gate and updates the domain name.
    /// </summary>
    public void SyncGateWithServer()
    {
        if (this.Gate == null)
        {
            GD.PrintErr($"{this.Multiplayer.GetUniqueId()}: SyncGateWithServer: Gate is null.");
            return;
        }

        string gateJson = JsonSerializer.Serialize(this.Gate);
        this.Rpc(nameof(this.SyncGate), this.Name, gateJson);
        this.RpcId(1, nameof(this.SyncGate), this.Name, gateJson);
    }

    /// <summary>
    /// Called when gate domain name, position and rotation are changed.
    /// This method is responsible for syncing the domain name, position and rotation across all clients.
    /// </summary>
    /// <param name="name">Name of the gate (this is a guid).</param>
    /// <param name="gateJson">Domain name of the gate.</param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void SyncGate(StringName name, string gateJson)
    {
        Gate? gate = JsonSerializer.Deserialize<Gate>(gateJson);
        if (gate == null)
        {
            GD.PrintErr($"{this.Multiplayer.GetUniqueId()}: SyncGate: Gate is null.");
            return;
        }

        if (this.Name == name)
        {
            this.SetGateInfo(gate);
        }
    }

    private void SetGateInfo(Gate gate)
    {
        this.Gate = gate;
        this.domainNameLineEdit.Text = gate.DomainName;
        this.posXLineEdit.Text = gate.Position.X.ToString();
        this.posYLineEdit.Text = gate.Position.Y.ToString();
        this.posZLineEdit.Text = gate.Position.Z.ToString();
        this.rotXLineEdit.Text = gate.Rotation.X.ToString();
        this.rotYLineEdit.Text = gate.Rotation.Y.ToString();
        this.rotZLineEdit.Text = gate.Rotation.Z.ToString();
    }
}

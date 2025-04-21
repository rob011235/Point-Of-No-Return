// -----------------------------------------------------------------------
// <copyright file="CreateServerMenu.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Godot;
using PointOfNoReturn.Data.Models;
using PointOfNoReturnServerDeploy;

/// <summary>
/// This class is responsible for creating a server menu.
/// It allows the user to enter a name, user ID, and password for the server.
/// It also provides a button to create the server and a back button.
/// The server is created using the VMBuilder class.
/// </summary>
public partial class CreateServerMenu : MarginContainer
{
    // private BalloonAtollDAL _dal = default!;
    // List<BalloonAtoll> BalloonAtolls = new List<BalloonAtoll>();
    // Container createBalloonAtollContainer = default!;
    private LineEdit nameLineEdit = default!;
    private LineEdit userIdLineEdit = default!;
    private LineEdit passwordLineEdit = default!;
    private RichTextLabel statusLabel = default!;

    // Called when the node enters the scene tree for the first time.
    private string status = string.Empty;
    private PackedScene worldScene = GD.Load<PackedScene>("res://Levels/World.tscn");

    /// <summary>
    /// Called when the node is added to the scene.
    /// </summary>
    public override void _Ready()
    {
        // createBalloonAtollContainer = GetNode<Container>("Panel/ScrollContainer/BoxContainer/NameEntry");
        this.nameLineEdit = this.GetNode<LineEdit>("Panel/ScrollContainer/BoxContainer/NameLineEdit");
        this.userIdLineEdit = this.GetNode<LineEdit>("Panel/ScrollContainer/BoxContainer/UserIdLineEdit");
        this.passwordLineEdit = this.GetNode<LineEdit>("Panel/ScrollContainer/BoxContainer/PasswordLineEdit");
        this.statusLabel = this.GetNode<RichTextLabel>("Panel/ScrollContainer/BoxContainer/StatusLabel");
    }

    /// <summary>
    /// Called every frame.
    /// This method updates the status label with the current status.
    /// It is called every frame to ensure that the status label is always up to date.
    /// The delta parameter is the time since the last frame.
    /// It is not used in this method, but it is required by the Godot engine.
    /// </summary>
    /// <param name="delta">Time ellapsed since last frame.</param>
    public override void _Process(double delta)
    {
        this.statusLabel.Text = this.status;
    }

    /// <summary>
    /// Called when the create location button is pressed.
    /// This method creates a new location using the VMBuilder class.
    /// It retrieves the name, user ID, and password from the input fields.
    /// It then creates a new VMBuilder instance and calls the BuildAsync method to create the server.
    /// If the server is created successfully, it deploys the server using the GameServerManager class.
    /// It also adds the location to the database and starts the client.
    /// If the server creation fails, it updates the status label with the error message.
    /// </summary>
    public async void OnCreateLocationButtonPressed()
    {
        try
        {
            ServerManager.Instance.Leave();

            Location location = new Location();
            location.Name = this.nameLineEdit?.Text;
            location.UserName = this.userIdLineEdit?.Text;
            location.Password = this.passwordLineEdit?.Text;

            // TODO: Assess whether to specify azure.location here at a later date.
            if (location.Name != null && location.UserName != null && location.Password != null)
            {
                VMBuilder builder = new VMBuilder(location.Name, location.UserName, location.Password, this.UpdateStatus,  "ffe4dd42-6573-45fe-895f-8791b3688919");

                // TODO: Add a field in the UI to select the tenant id. Maybe under an advanced settings button.
                var vmResult = await builder.BuildAsync();
                var vm = vmResult.VirtualMachineResource;

                await this.UpdateStatus(vmResult.ToString());
                if (vmResult.Success)
                {
                    location.DomainName = vmResult?.DomainName;
                    if (location.DomainName != null)
                    {
                        var manager = new GameServerManager(location, 22, "dedicated_server", this.UpdateStatus);
                        await manager.DeployAsync();
                        await this.UpdateStatus($"Server deployed to {location.DomainName}");
                        await Services.Instance.LocationsDAL.AddAsync(location);
                        await this.UpdateStatus($"Location {location.Name} created at {location.DomainName}");
                        await ServerManager.Instance.StartClientFromTravelMenu(location.DomainName);
                    }
                    else
                    {
                        await this.UpdateStatus($"Location {location.Name} created but domain name is null");
                    }
                }
                else
                {
                    await this.UpdateStatus($"Could not create Location. {vmResult.Exception?.Message}");
                }
            }
            else
            {
                await this.UpdateStatus("Location missing required fields");
            }
        }
        catch (Exception e)
        {
            await this.UpdateStatus($"Error: {e.Message}");
        }
    }

    /// <summary>
    /// Called when the back button is pressed.
    /// This method hides the create server menu and shows the locations menu.
    /// It also frees the current node from the scene tree.
    /// </summary>
    public void OnBackButtonPressed()
    {
        this.QueueFree();
    }

    /// <summary>
    /// This method updates the status label with the current status.
    /// </summary>
    /// <param name="message">Text to add to the status label.</param>
    /// <returns>A Task.</returns>
    public Task UpdateStatus(string message)
    {
        return Task.FromResult(this.status += message + "\n");
    }
}

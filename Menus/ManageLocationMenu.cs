// -----------------------------------------------------------------------
// <copyright file="ManageLocationMenu.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// File dialog configuration is based on tutorial here:
// https://www.youtube.com/watch?v=e7FXYwcDCQU
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Godot;
using PointOfNoReturn.Data.Models;
using PointOfNoReturnServerDeploy;

/// <summary>
/// ManageLocationMenu is a menu for managing locations in the game.
/// </summary>
public partial class ManageLocationMenu : Control
{
    private Label titleLabel = default!;
    private RichTextLabel statusLabel = default!;
    private FileDialog downloadFileDialog = default!;
    private FileDialog uploadFileDialog = default!;
    private List<Location> locations = default!;
    private ItemList locationList = default!;
    private Location location = default!;
    private LineEdit nameLineEdit = default!;
    private LineEdit domainNameLineEdit = default!;
    private LineEdit userNameLineEdit = default!;
    private LineEdit passwordLineEdit = default!;

    /// <summary>
    /// Called when the node is added to the scene.
    /// Initializes the status label.
    /// </summary>
    public override async void _Ready()
    {
        this.titleLabel = this.GetNode<Label>("VBoxContainer/TitleLabel");
        this.statusLabel = this.GetNode<RichTextLabel>("VBoxContainer/StatusLabel");
        this.statusLabel.Text = "Manage Location Menu Initialized.";

        this.downloadFileDialog = this.GetNode<FileDialog>("DownloadFileDialog");
        this.downloadFileDialog.Visible = false;
        this.downloadFileDialog.CurrentDir = OS.GetDataDir() + "/../../Downloads/";
        this.downloadFileDialog.CurrentFile = "server_data.db";

        this.uploadFileDialog = this.GetNode<FileDialog>("UploadFileDialog");
        this.uploadFileDialog.Visible = false;
        this.uploadFileDialog.CurrentDir = OS.GetDataDir() + "/../../Downloads/";
        this.uploadFileDialog.CurrentFile = "server_data.db";

        this.nameLineEdit = this.GetNode<LineEdit>("VBoxContainer/NameLineEdit");
        this.domainNameLineEdit = this.GetNode<LineEdit>("VBoxContainer/DomainNameLineEdit");
        this.userNameLineEdit = this.GetNode<LineEdit>("VBoxContainer/UserNameLineEdit");
        this.passwordLineEdit = this.GetNode<LineEdit>("VBoxContainer/PasswordLineEdit");

        this.locationList = this.GetNode<ItemList>("VBoxContainer/LocationList");
        this.locations = await Services.Instance.LocationsDAL.GetAllAsync();
        this.locationList.Clear();
        foreach (var location in this.locations)
        {
            this.locationList.AddItem(location.Name);
        }

        this.AddNewItemElementToLocationsList();

        this.locationList.Select(0);
        this.location = this.locations[0];
        this.UpdateLineEdits();
    }

    /// <summary>
    /// Called when the user selects a location from the list.
    /// This method updates the location object with the selected location's details.
    /// </summary>
    /// <param name="index">Index of the selected item.</param>
    public void OnItemSelected(int index)
    {
        this.location = this.locations[index];
        this.UpdateLineEdits();
    }

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// </summary>
    public void OnBackupDatabaseButtonPressed()
    {
        GD.Print("Backing up database...");
        this.downloadFileDialog.Visible = true;
    }

    /// <summary>
    /// Called when the file dialog is closed.
    /// </summary>
    /// <param name="path">Path returned by file dialog.</param>
    public async void OnDownloadFileSelectedAsync(string path)
    {
        GD.Print($"File selected: {path}");
        this.downloadFileDialog.Visible = false;
        var manager = new GameServerManager(this.location, 22, "dedicated_server", this.UpdateStatus);
        await manager.DownloadFileAsync($"/home/{this.location.UserName}/server_data.db", path);
    }

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// </summary>
    public void OnUploadDatabaseButtonPressed()
    {
        GD.Print("Uploading database...");
        this.uploadFileDialog.Visible = true;
    }

    /// <summary>
    /// Called when the file dialog is closed.
    /// </summary>
    /// <param name="path">Path returned by file dialog.</param>
    public async void OnUploadFileSelectedAsync(string path)
    {
        GD.Print($"File selected: {path}");
        this.uploadFileDialog.Visible = false;
        var manager = new GameServerManager(this.location, 22, "dedicated_server", this.UpdateStatus);
        await manager.UploadFileAsync(path, $"/home/{this.location.UserName}/server_datatest.db");
    }

    /// <summary>
    /// Called when cancel button is pressed.
    /// </summary>
    public void OnCancelButtonPressed()
    {
        GD.Print("Closing Manage Location Menu...");
        this.QueueFree();
    }

    /// <summary>
    /// This method updates the status label with the current status.
    /// </summary>
    /// <param name="message">Text to add to the status label.</param>
    /// <returns>A Task.</returns>
    public Task UpdateStatus(string message)
    {
        return Task.FromResult(this.statusLabel.Text += message + "\n");
    }

    /// <summary>
    /// Called when the start server button is pressed.
    /// </summary>
    public async void OnStartServerButtonPressed()
    {
        GD.Print($"Starting server...");
        var manager = new GameServerManager(this.location, 22, "dedicated_server", this.UpdateStatus);
        await manager.StartServerAsync();
    }

    /// <summary>
    /// Called when the stop server button is pressed.
    /// </summary>
    public async void OnStopServerButtonPressed()
    {
        GD.Print($"Stopping server...");
        var manager = new GameServerManager(this.location, 22, "dedicated_server", this.UpdateStatus);
        await manager.StopServerAsync();
    }

    /// <summary>
    /// Called when the user presses the "Edit" button.
    /// This method populates the Add Location Menu with the selected location's details for editing.
    /// </summary>
    public void OnItemSelected()
    {
        var index = this.locationList.GetSelectedItems()[0];
        this.location = this.locations[index];
        this.UpdateLineEdits();
    }

    /// <summary>
    /// Called when the user presses the "Delete" button.
    /// This method deletes the selected location from the list.
    /// </summary>
    public async void OnDeleteButtonPressedAsync()
    {
        if (this.location == null)
        {
            GD.Print("No location selected.");
            return;
        }

        var index = this.locationList.GetSelectedItems()[0];
        this.locations.RemoveAt(index);
        this.locationList.RemoveItem(index);
        if (this.location.Id != null)
        {
            await Services.Instance.LocationsDAL.DeleteAsync(this.location.Id);
        }
        else
        {
            this.AddNewItemElementToLocationsList();
        }

        this.locationList.Select(0);
        this.location = this.locations[0];
        this.UpdateLineEdits();
        this.statusLabel.Text += "Location deleted.";
    }

    /// <summary>
    /// Called when the user presses the "Update" button.
    /// This method updates the selected location with the details from the UI.
    /// </summary>
    public async void OnUpdateButtonPressedAsync()
    {
        if (this.location == null)
        {
            GD.Print("No location selected.");
            return;
        }

        // Create a copy of the old location before updating
        // This is useful for undo functionality or logging changes
        var oldLocation = new Location
        {
            Name = this.location.Name,
            DomainName = this.location.DomainName,
            UserName = this.location.UserName,
            Password = this.location.Password,
        };

        // Update the location with new values from the UI
        this.location.Name = this.nameLineEdit.Text;
        this.location.DomainName = this.domainNameLineEdit.Text;
        this.location.UserName = this.userNameLineEdit.Text;
        this.location.Password = this.passwordLineEdit.Text;

        // if username or password changes, update the server connection
        if (this.location.UserName != (oldLocation.UserName ?? string.Empty)
            || this.location.Password != (oldLocation.Password ?? string.Empty))
        {
            var manager = new GameServerManager(this.location, 22, "dedicated_server", this.UpdateStatus);
            await manager.UpdateServerCredentialsAsync(oldLocation, this.location);
        }

        var index = this.locationList.GetSelectedItems()[0];
        if (this.location.Id == null)
        {
            this.location = await Services.Instance.LocationsDAL.AddAsync(this.location);
            this.locationList.SetItemText(index, this.location.Name);

            // If we just updated the new location item, we need to add a new one
            if (oldLocation.Name == null)
            {
                this.AddNewItemElementToLocationsList();
            }
        }
        else
        {
            await Services.Instance.LocationsDAL.UpdateAsync(this.location);
            this.locationList.SetItemText(index, this.location.Name);
        }

        this.UpdateLineEdits();
    }

    private void UpdateLineEdits()
    {
        this.nameLineEdit.Text = this.location.Name ?? "Add New Location";
        this.domainNameLineEdit.Text = this.location.DomainName ?? string.Empty;
        this.userNameLineEdit.Text = this.location.UserName ?? string.Empty;
        this.passwordLineEdit.Text = this.location.Password ?? string.Empty;
    }

    private void AddNewItemElementToLocationsList()
    {
        var newLocation = new Location
        {
            Name = null,
            DomainName = null,
            UserName = null,
            Password = null,
        };
        this.locations.Add(newLocation);
        this.locationList.AddItem("Add New Location");
    }
}

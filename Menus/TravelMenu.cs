// -----------------------------------------------------------------------
// <copyright file="TravelMenu.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using PointOfNoReturn.Data.Models;

/// <summary>
/// The TravelMenu class is responsible for managing the travel menu in the game.
/// It allows players to select a location to travel to, create a new server, and manage their locations.
/// The class handles user interactions, such as button presses and input validation.
/// It also communicates with the game manager to start the game and manage multiplayer connections.
/// The class uses the Godot game engine's scene system to manage the user interface and display different menus.
/// </summary>
public partial class TravelMenu : Control
{
    private Control locationsMenu = default!;
    private Control createServerMenu = default!;
    private Control costWarningMenu = default!;
    private Control addLocationMenu = default!;

    private Button startButton = default!;
    private ItemList locationList = default!;
    private Label addLocationTitle = default!;
    private LineEdit nameLineEdit = default!;
    private LineEdit domaineNameLineEdit = default!;
    private List<Location> locations = default!;
    private Location location = default!;
    private PackedScene worldScene = GD.Load<PackedScene>("res://Levels/World.tscn");
    private PackedScene manageMenuScene = GD.Load<PackedScene>("res://Menus/ManageLocationMenu.tscn");

    /// <summary>
    /// Called when the node is added to the scene.
    /// </summary>
    public override async void _Ready()
    {
        this.locationsMenu = this.GetNode<Control>("LocationsMenu");
        this.costWarningMenu = this.GetNode<Control>("CostWarningMenu");
        this.createServerMenu = this.GetNode<Control>("CreateServerMenu");

        this.locationList = this.GetNode<ItemList>("LocationsMenu/LocationList");

        this.locations = await Services.Instance.LocationsDAL.GetAllAsync();
        this.locations.Add(new Location
        {
            Name = "Travel to a new place and make a homestead.",
            DomainName = null,
        });

        if (this.locations.Count == 0 || this.locations.Where(l => l.DomainName == "localhost").Count() == 0)
        {
            this.locations.Add(new Location { Name = "localhost", DomainName = "localhost" });
        }

        foreach (var location in this.locations)
        {
            this.locationList.AddItem(location.Name);
        }

        this.locationList.Select(0);

        Gui.Instance.AddHelpText("Please select a location to travel to.");
    }

    /// <summary>
    /// Called when the user presses the "Start" button.
    /// This method is responsible for starting the game by changing the scene to the world scene.
    /// </summary>
    public async void OnTravelButtonPressed()
    {
        var location = this.locations[this.locationList.GetSelectedItems()[0]];
        if (location.Name == "Travel to a new place and make a homestead.")
        {
            this.costWarningMenu.Show();
            this.locationsMenu.Hide();
            return;
        }

        string? domainName = this.locations[this.locationList.GetSelectedItems()[0]].DomainName;
        if (domainName == null)
        {
            GD.Print("No valid domain name.");
            return;
        }

        await ServerManager.Instance.StartClientFromTravelMenu(domainName);
        Gui.Instance.RemoveHelpText("Please select a location to travel to.");
    }

    /// <summary>
    /// Called when the user presses the "Continue" button in the Warning Menu.
    /// This method displays the Create Server Menu.
    /// </summary>
    public void OnContinueButtonPressed()
    {
        this.costWarningMenu.Visible = false;
        this.createServerMenu.Visible = true;
    }

    /// <summary>
    /// Called when the user presses the Cancel button in the Create Server Menu.
    /// This method hides the Warning Menu and displays the Locations Menu.
    /// </summary>
    public void OnCancelCreateServerButtonPressed()
    {
        this.locationsMenu.Visible = true;
        this.costWarningMenu.Visible = false;
    }

    /// <summary>
    /// Called when the user presses the "Create Server" button.
    /// This method creates a new server and starts the game.
    /// </summary>
    public void OnAddButtonPressed()
    {
        this.location = new Location();
        this.addLocationMenu.Show();
        this.addLocationTitle.Text = "Add Location";
    }

    /// <summary>
    /// Called when the user presses the Edit button.
    /// This method populates the Add Location Menu with the selected location's details for editing.
    /// </summary>
    public void OnEditButtonPressed()
    {
        var index = this.locationList.GetSelectedItems()[0];
        this.location = this.locations[index];
        this.nameLineEdit.Text = this.location.Name;
        this.domaineNameLineEdit.Text = this.location.DomainName;
        this.addLocationMenu.Show();
        this.addLocationTitle.Text = "Edit Location";
    }

    /// <summary>
    /// Called when the user presses the Submit button in the Add Location Menu.
    /// This method saves the location details and updates the location list.
    /// If the location is new, it adds it to the list and database.
    /// If the location already exists, it updates the existing location in the list and database.
    /// </summary>
    public void OnSubmitButtonPressed()
    {
        this.location.Name = this.nameLineEdit.Text;
        this.location.DomainName = this.domaineNameLineEdit.Text;
        if (this.location.Id == null)
        {
            this.location.Id = Guid.NewGuid().ToString();
            this.locations.Add(this.location);
            this.locationList.AddItem(this.location.Name);
            Services.Instance.LocationsDAL.AddAsync(this.location);
        }
        else
        {
            var index = this.locations.FindIndex(l => l.Id == this.location.Id);
            if (index != -1)
            {
                this.locations[index] = this.location;
                this.locationList.SetItemText(index, this.location.Name);
            }

            Services.Instance.LocationsDAL.UpdateAsync(this.location);
        }

        this.addLocationMenu.Hide();
    }

    /// <summary>
    /// Called when the user presses the Cancel button in the Add Location Menu.
    /// This method hides the Add Location Menu.
    /// </summary>
    public void OnCancelButtonPressed()
    {
        this.addLocationMenu.Hide();
    }

    /// <summary>
    /// Called when the user presses the Delete button.
    /// This method removes the selected location from the list and database.
    /// </summary>
    public void OnDeleteButtonPressed()
    {
        var index = this.locationList.GetSelectedItems()[0];
        var location = this.locations[index];
        this.locations.RemoveAt(index);
        this.locationList.RemoveItem(index);
        Services.Instance.LocationsDAL.DeleteAsync(location.Id!);
    }
}

// -----------------------------------------------------------------------
// <copyright file="PauseMenu.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Godot;

/// <summary>
/// PauseMenu is a control that displays the credits of the game.
/// It is used to show the credits when the game is paused.
/// The credits are loaded from a text file and displayed in a RichTextLabel.
/// The credits file is located at "res://Menus/Credits.txt".
/// The PauseMenu also provides a method to exit the game.
/// </summary>
public partial class PauseMenu : Control
{
    private PackedScene quitMenuScenne = GD.Load<PackedScene>("res://Menus/QuitMenu.tscn");
    private PackedScene manageLocationMenuScene = GD.Load<PackedScene>("res://Menus/ManageLocationMenu.tscn");

    /// <summary>
    /// Gets or sets the starting mouse mode.
    /// This property is used to store the initial mouse mode when the pause menu is opened.
    /// It is used to restore the mouse mode when the pause menu is closed.
    /// </summary>
    public Input.MouseModeEnum StartingMouseMode { get; set; } = default!;

    /// <summary>
    /// Called when the node is added to the scene.
    /// This method is used to initialize the pause menu and set the mouse mode to visible.
    /// It sets the mouse mode to visible so that the user can interact with the menu.
    /// </summary>
    public override void _Ready()
    {
        this.StartingMouseMode = Input.MouseMode;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    /// <summary>
    /// Called when the input event is received.
    /// This method is used to handle input events for the pause menu.
    /// </summary>
    /// <param name="event">InputEvent passed in by Godot.</param>
    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("pause"))
        {
            this.CloseMenu();
        }
    }

    /// <summary>
    /// Called when the user clicks the run as host button in the pause menu.
    /// This method is used to open the run as host menu.
    /// </summary>
    public void OnRunAsHostButtonPressed()
    {
        ServerManager.Instance?.Host();
        ServerManager.Instance?.HideTravelMenu();
        Gui.Instance?.SetTitle("Local Host Server");
        Gui.Instance?.AddHelpText("Press Esc to open the pause menu.");
        this.CloseMenu();
    }

    /// <summary>
    /// Called when the user clicks the leave server button in the pause menu.
    /// This method is used to leave the server and close the pause menu.
    /// </summary>
    public void OnLeaveServerButtonPressed()
    {
        ServerManager.Instance?.Leave();
        ServerManager.Instance?.ShowTravelMenu();
        this.StartingMouseMode = Input.MouseModeEnum.Confined;
        this.CloseMenu();
    }

    /// <summary>
    /// Opens manage gamelocation menu.
    /// </summary>
    public void OnManageButtonPressed()
    {
        var manageMenu = this.GetNodeOrNull<ManageLocationMenu>("ManageLocationMenu");
        if (manageMenu == null)
        {
            manageMenu = this.manageLocationMenuScene.Instantiate<ManageLocationMenu>();
            this.AddChild(manageMenu);
        }
        else
        {
            manageMenu.QueueFree();
        }
    }

    /// <summary>
    /// Opens exit game menu.
    /// This method is called when the user clicks the exit button in the right-click menu.
    /// It quits the game and closes the application.
    /// </summary>
    public void OnQuitGameButtonPressed()
    {
        var quitMenu = this.GetNodeOrNull<QuitMenu>("QuitMenu");
        if (quitMenu == null)
        {
            quitMenu = this.quitMenuScenne.Instantiate<QuitMenu>();
            this.AddChild(quitMenu);
        }
        else
        {
            quitMenu.QueueFree();
        }
    }

    /// <summary>
    /// Resumes the game.
    /// This method is called when the user clicks the resume button in the pause menu.
    /// It closes the pause menu and restores the mouse mode to its original state.
    /// </summary>
    public void OnResumeButtonPressed()
    {
        this.CloseMenu();
    }

    private void CloseMenu()
    {
        this.CloseChildMenus();

        this.QueueFree();
        Input.MouseMode = this.StartingMouseMode;
        if (Player3D.Instance != null)
        {
            Player3D.Instance.IsUnderPlayerControl = true;
        }
    }

    private void CloseChildMenus()
    {
        var manageMenu = this.GetNodeOrNull<ManageLocationMenu>("ManageLocationMenu");
        if (manageMenu != null)
        {
            manageMenu.QueueFree();
        }

        var quitMenu = this.GetNodeOrNull<QuitMenu>("QuitMenu");
        if (quitMenu != null)
        {
            quitMenu.QueueFree();
        }
    }
}

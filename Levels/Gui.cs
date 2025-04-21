// -----------------------------------------------------------------------
// <copyright file="Gui.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Godot;

/// <summary>
/// Gui is a singleton class that manages the graphical user interface (GUI) of the game.
/// It inherits from CanvasLayer, which allows it to be drawn on top of the game scene.
/// The class provides functionality for displaying a right-click menu, spawning objects,
/// and managing help text.
/// It also includes methods for setting the title of the GUI and exiting the game.
/// </summary>
public partial class Gui : CanvasLayer
{
    private Label titleLabel = default!;
    private RichTextLabel helpLabel = default!;
    private Label transformLabel = default!;

    private List<string> helpText = new List<string>();

    /// <summary>
    /// Gets the singleton instance of the Gui class.
    /// This property allows other classes to access the Gui instance easily.
    /// </summary>
    public static Gui Instance { get; private set; } = default!;

    /// <summary>
    /// Called when the node is added to the scene tree.
    /// This method initializes the GUI by setting up the right-click menu and populating the spawnable objects list.
    /// </summary>
    public override void _Ready()
    {
        if (!this.IsMultiplayerAuthority())
        {
            return;
        }

        Instance = this;
        this.titleLabel = this.GetNode<Label>("TitleLabel");
        this.titleLabel.Text = "Multiplayer Game";
        this.helpLabel = this.GetNode<RichTextLabel>("HelpLabel");
        this.transformLabel = this.GetNode<Label>("TransformLabel");
    }

    /// <summary>
    /// Sets the title of the GUI.
    /// This method is called when the game starts.
    /// It sets the title label to the specified text.
    /// </summary>
    /// <param name="text">Text to set the title label to.</param>
    public void SetTitle(string text)
    {
        if (!this.IsMultiplayerAuthority())
        {
            return;
        }

        this.titleLabel.Text = text;
    }

    /// <summary>
    /// Adds the specified help text to the help label.
    /// This method is called when the user clicks the help button in the right-click menu.
    /// It adds the specified text to the help label.
    /// If the text is already in the help label, it will not be added again.
    /// </summary>
    /// <param name="text">Text to add to the help text label.</param>
    public void AddHelpText(string text)
    {
        if (!this.helpText.Contains(text))
        {
            this.helpText.Add(text);
        }

        this.helpLabel.Text = string.Join(" | ", this.helpText);
    }

    /// <summary>
    /// Removes the specified help text from the help label.
    /// This method is called when the user clicks the exit button in the right-click menu.
    /// It removes the specified text from the help label.
    /// </summary>
    /// <param name="text">The text to remove from the help label.</param>
    public void RemoveHelpText(string text)
    {
        while (this.helpText.Contains(text))
        {
            this.helpText.Remove(text);
        }

        this.helpLabel.Text = string.Join(" | ", this.helpText);
    }

    /// <summary>
    /// Updates the transform label with the specified location and rotation.
    /// </summary>
    /// <param name="location">The location to display in the transform label.</param>
    /// <param name="rotation">The rotation to display in the transform label.</param>
    public void UpdateTransformLabel(Vector3 location, Vector3 rotation)
    {
        // Format the location and rotation values to a string representation
        string locationString = $"{location.X:F2}, {location.Y:F2}, {location.Z:F2}";
        string rotationString = $"{rotation.X:F2}, {rotation.Y:F2}, {rotation.Z:F2}";
        this.transformLabel.Text = $"Location: {locationString}\nRotation: {rotationString}";
    }
}

// -----------------------------------------------------------------------
// <copyright file="GiffardAirship.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Godot;

/// <summary>
/// GiffardAirship is a class that represents the Giffard Airship in the game.
/// It inherits from Node3D and handles the functionality of the airship, including entering and exiting the helm area.
/// </summary>
public partial class GiffardAirship : Node3D
{
    private Control? menu = null;

    /// <summary>
    /// Called when the node is added to the scene tree.
    /// This method is responsible for initializing the airship and setting up the menu.
    /// </summary>
    /// <param name="body">Body that enters area.</param>
    public void EnterHelmArea(Node3D body)
    {
        if (this.Multiplayer.IsServer())
        {
            return;
        }

        if (body is Player3D)
        {
            if (body is not Player3D player || Player3D.Instance == null || player.Name != Player3D.Instance.Name)
            {
                return;
            }

            ServerManager.Instance.TravelingEnabled = true;

            if (Gui.Instance == null)
            {
                GD.Print("Gui.Instance is null");
                return;
            }
            else
            {
                Gui.Instance.AddHelpText("Press 'T' to open and close the travel menu");
            }
        }
    }

    /// <summary>
    /// Called when body exits the helm area.
    /// This method is responsible for removing the menu and resetting the player reference.
    /// </summary>
    /// <param name="body">Body that exits area.</param>
    public void ExitHelmArea(Node3D body)
    {
        if (this.Multiplayer.IsServer())
        {
            return;
        }

        if (body is Player3D)
        {
            var exitingPlayer = body as Player3D;
            if (exitingPlayer?.Name != Player3D.Instance.Name)
            {
                return;
            }

            ServerManager.Instance.TravelingEnabled = false;
            this.menu?.QueueFree();
            this.menu = null;
            Input.MouseMode = Input.MouseModeEnum.Captured;
            Gui.Instance.RemoveHelpText("Press 'T' to open and close the travel menu");
        }
    }
}

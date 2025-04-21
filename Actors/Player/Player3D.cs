// -----------------------------------------------------------------------
// <copyright file="Player3D.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Godot;
using PointOfNoReturn.Data.Models;

/// <summary>
/// The Player3D class represents a player character in a 3D game.
/// It inherits from the CharacterBody3D class and provides functionality
/// for player movement, animation, and interaction with the game world.
/// </summary>
/// remarks>
/// Based on code by Lukky
/// Github repo source: https://github.com/lukky-nl/third_person_controller_assets
/// Based on tutorial found here: https://youtu.be/EP5AYllgHy8
/// </remarks>
public partial class Player3D : CharacterBody3D
{
    private Node3D cameraMount = default!;
    private AnimationPlayer animationPlayer = default!;
    private Node3D visuals = default!;
    private bool isFlying = false;
    private bool isRunning = false;
    private bool isLocked = false;
    private float speed = 3.0f;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    /// <summary>
    /// Gets a Singleton instance of the Player3D class.
    /// This instance is used to access the player from other scripts.
    /// </summary>
    public static Player3D Instance { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the rotation speed of the camera in degrees per second.
    /// This property is used to control the speed at which the camera rotates when the player moves the mouse.
    /// The rotation speed is set to 0.5 degrees per second by default.
    /// </summary>
    [Export]
    public float HorizontalRotRate { get; set; } = 0.5f;

    /// <summary>
    /// Gets or sets he rotation speed of the camera in degrees per second.
    /// This property is used to control the speed at which the camera rotates when the player moves the mouse.
    /// The rotation speed is set to 0.5 degrees per second by default.
    /// </summary>
    [Export]
    public float VerticalRotRate { get; set; } = 0.5f;

    /// <summary>
    /// Gets or sets the speed at which the player moves when walking.
    /// This property is used to control the speed of the player when walking.
    /// The walking speed is set to 3.0 units per second by default.
    /// </summary>
    [Export]
    public float WalkingSpeed { get; set; } = 3.0f;

    /// <summary>
    /// Gets or sets the speed at which the player moves when running.
    /// This property is used to control the speed of the player when running.
    /// The running speed is set to 5.0 units per second by default.
    /// </summary>
    [Export]
    public float RunningSpeed { get; set; } = 5.0f;

    /// <summary>
    /// Gets or sets speed at which the player moves when jumping.
    /// This property is used to control the speed of the player when jumping.
    /// The jump speed is set to 5.0 units per second by default.
    /// </summary>
    [Export]
    public float JumpVelocity { get; set; } = 5.0f;

    /// <summary>
    /// Gets or sets the vehicle that the player is currently controlling.
    /// This property is set when the player enters a vehicle and is used to control the vehicle's behavior.
    /// If the player is not controlling a vehicle, this property is null.
    /// </summary>
    public GiffardAirship? Vehicle { get; set; } = null;

    /// <summary>
    /// Gets or sets the camera that is used to view the player.
    /// This property is set to the camera node in the scene tree.
    /// The camera is used to follow the player and provide a first-person or third-person view.
    /// </summary>
    public Camera3D Camera { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether or not the player is currently under player control.
    /// This property is used to determine if the player can move and interact with the game world.
    /// If this property is set to false, the player cannot move or interact with the game world.
    /// This property is used to enable or disable player control, for example, when the player is in a vehicle or when the game is paused.
    /// </summary>
    public bool IsUnderPlayerControl { get; set; } = true;

    /// <summary>
    /// _Ready is called when the node is added to the scene.
    /// This is where you can initialize the node and set up any necessary connections.
    /// </summary>
    public override void _Ready()
    {
        this.animationPlayer = this.GetNode<AnimationPlayer>("Visuals/mixamo_base/AnimationPlayer");
        this.animationPlayer.Play("idle");
        this.Velocity = Vector3.Zero;

        if (!this.IsMultiplayerAuthority())
        {
            return;
        }

        Instance = this;
        this.cameraMount = this.GetNode<Node3D>("camera_mount");
        this.Camera = this.cameraMount.GetNode<Camera3D>("Camera3D");
        this.Camera.Current = true;

        this.visuals = this.GetNode<Node3D>("Visuals");
        Input.MouseMode = Input.MouseModeEnum.Captured;
        Gui.Instance.AddHelpText("Esc: pause");
        Gui.Instance.AddHelpText("Q: fly");
        Gui.Instance.AddHelpText("Right Click: world edit mode");

        Gate? gate = ServerManager.Instance.Gate;
        if (gate != null)
        {
            this.GlobalPosition = gate.Position;
            this.GlobalRotation = gate.Rotation;
        }
    }

    /// <summary>
    /// _EnterTree is called when the node is added to the scene tree.
    /// This is where you can set up any necessary connections or initialize variables.
    /// When the player enters the scene tree, it sets the multiplayer authority to the player ID.
    /// This is used to determine which player has control over the player character.
    /// The player ID is set to the name of the node, which is expected to be a unique identifier for each player.
    /// </summary>
    public override void _EnterTree()
    {
        this.SetMultiplayerAuthority(int.Parse(this.Name));
    }

    /// <summary>
    /// _Input is called when the player presses a key or clicks the mouse.
    /// This is where you can handle input events and perform actions based on the input.
    /// The method checks if the player has authority over the input and if the player is under player control.
    /// If the player is under player control, it handles the input events for creating objects, pausing the game, and moving the player.
    /// </summary>
    /// <param name="event">The input event that was triggered.</param>
    public override void _Input(InputEvent @event)
    {
        if (!this.IsMultiplayerAuthority())
        {
            return;
        }

        if (!this.IsUnderPlayerControl)
        {
            return;
        }

        if (@event is InputEventMouseMotion mm)
        {
            this.RotateY(Mathf.DegToRad(-mm.Relative.X * this.HorizontalRotRate));
            this.visuals.RotateY(Mathf.DegToRad(mm.Relative.X * this.HorizontalRotRate));
            this.cameraMount.RotateX(Mathf.DegToRad(mm.Relative.Y * this.VerticalRotRate));
        }
    }

    /// <summary>
    /// _PhysicsProcess is called every frame and is used to handle physics-related updates.
    /// This method is responsible for handling player movement, jumping, and animation.
    /// It checks if the player is under player control and if the connection to the multiplayer server is active.
    /// If the player is under player control, it handles the input events for moving the player and playing the appropriate animations.
    /// The method also handles the player's speed, gravity, and jumping.
    /// </summary>
    /// <param name="delta">The time since the last frame.</param>
    public override void _PhysicsProcess(double delta)
    {
        if (this.Multiplayer.IsServer())
        {
            return;
        }

        if (!this.IsMultiplayerAuthority())
        {
            return;
        }

        if (!this.IsUnderPlayerControl)
        {
            return;
        }

        if (Input.IsActionJustPressed("fly"))
        {
            this.isFlying = !this.isFlying;
            this.Velocity = Vector3.Zero;
            if (this.isFlying)
            {
                Gui.Instance.AddHelpText("Press 'up' to fly up and 'down' to fly down.");
            }
            else
            {
                Gui.Instance.RemoveHelpText("Press 'up' to fly up and 'down' to fly down.");
            }
        }

        // Check if the connection is up
        if (!this.Multiplayer.HasMultiplayerPeer())
        {
            Gui.Instance.AddHelpText("Connection lost. Attempting to reconnect...");

            // Optionally, you can add reconnection logic here
            return;
        }

        Gui.Instance.UpdateTransformLabel(this.Position, this.Rotation);

        if (!this.animationPlayer.IsPlaying())
        {
            this.isLocked = false;
        }

        if (Input.IsActionPressed("kick"))
        {
            if (this.animationPlayer.CurrentAnimation != "kick")
            {
                this.animationPlayer.Play("kick");
                this.isLocked = true;
            }
        }

        if (!this.isLocked)
        {
            if (Input.IsActionPressed("running"))
            {
                this.speed = this.RunningSpeed;
                this.isRunning = true;
            }
            else
            {
                this.speed = this.WalkingSpeed;
                this.isRunning = false;
            }

            Vector3 velocity = this.Velocity;

            // Get the input direction and handle the movement/deceleration.
            // Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
            float inputDirY = 0;
            if (this.isFlying)
            {
                // animationPlayer.Play("flying");
                inputDirY = Input.GetActionStrength("up") - Input.GetActionStrength("down");
                velocity.Y = inputDirY * this.speed;
            }
            else
            {
                // Add the gravity.
                if (!this.IsOnFloor())
                {
                    velocity.Y -= this.gravity * (float)delta;
                }

                // Handle Jump.
                if (this.IsOnFloor() && Input.IsActionJustPressed("ui_accept"))
                {
                    velocity.Y = this.JumpVelocity;
                }
            }

            var inputDirX = Input.GetActionStrength("right") - Input.GetActionStrength("left");
            var inputDirZ = Input.GetActionStrength("backward") - Input.GetActionStrength("forward");
            Vector3 direction = (this.GlobalTransform.Basis * new Vector3(inputDirX, 0, inputDirZ)).Normalized();

            if (direction != Vector3.Zero)
            {
                if (this.isRunning)
                {
                    if (this.animationPlayer.CurrentAnimation != "running")
                    {
                        this.animationPlayer.Play("running");
                    }
                }
                else
                {
                    if (this.animationPlayer.CurrentAnimation != "walking")
                    {
                        this.animationPlayer.Play("walking");
                    }
                }

                this.visuals.LookAt(this.GlobalPosition + direction);

                velocity.X = direction.X * this.speed;
                velocity.Z = direction.Z * this.speed;
            }
            else
            {
                if (this.animationPlayer.CurrentAnimation != "idle")
                {
                    this.animationPlayer.Play("idle");
                }

                velocity.X = Mathf.MoveToward(this.Velocity.X, 0, this.speed);
                velocity.Z = Mathf.MoveToward(this.Velocity.Z, 0, this.speed);
            }

            // TODO: This is a hack to fix the second player in the scene from getting yeeted. Find a better way to do this.
            if (velocity.Length() > this.speed + 0.1f)
            {
                velocity = new Vector3(0, 0, 0);
            }

            this.Velocity = velocity;
            this.MoveAndSlide();
        }
    }

    /// <summary>
    /// This method is called when the player enters a vehicle.
    /// It sets the vehicle property to the specified vehicle and adds a help text to the GUI.
    /// The help text informs the player that they can press 'T' to travel.
    /// </summary>
    /// <param name="vehicle">The vehicle that the player is entering.</param>
    public void EnableVehicleMode(GiffardAirship vehicle)
    {
        if (!this.IsMultiplayerAuthority())
        {
            return;
        }

        if (vehicle != null)
        {
            this.Vehicle = vehicle;
            Gui.Instance.AddHelpText("Press 'T' to travel.");
        }
    }

    /// <summary>
    /// This method is called when the player exits a vehicle.
    /// It sets the vehicle property to null and removes the help text from the GUI.
    /// The help text informs the player that they can no longer press 'T' to travel.
    /// </summary>
    /// <param name="vehicle">The vehicle that the player is exiting.</param>
    public void DisableVehicleMode(GiffardAirship vehicle)
    {
        if (!this.IsMultiplayerAuthority())
        {
            return;
        }

        if (this.Vehicle == vehicle)
        {
            this.Vehicle = null;
            Gui.Instance.RemoveHelpText("Press 'T' to travel.");
        }
    }
}

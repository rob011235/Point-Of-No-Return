// -----------------------------------------------------------------------
// <copyright file="ObjectController.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Godot;
using PointOfNoReturn.Data.Models;

/// <summary>
/// CameraDragger is used to drag objects in the 3D world.
/// It uses the mouse to select objects and move them around.
/// It is used to movem, select, and delete objects in the scene.
/// </summary>
public partial class ObjectController : Node
{
    private ItemList spawnableObjectsItemList = default!;
    private IDraggableObject? selectedObject = null;
    private float speed = 3.0f;
    private bool isRotating = false;
    private float depth = 0;
    private bool isInWorldEditMode = false;

    // private Vector3? relativePosition = null;
    // private Vector3? relativeRotation = null;
    private Control rightClickMenu = default!;

    /// <summary>
    /// Gets or Sets a singleton instance of the ObjectController.
    /// This property is used to access the ObjectController instance from other scripts.
    /// It is set to null when the instance is created.
    /// </summary>
    public static ObjectController Instance { get; set; } = null!;

    /// <summary>
    /// Gets or Sets the object that a mouse is currently over.
    /// This property is set by draggable objects when the mouse is over them.
    /// </summary>
    public IDraggableObject? MouseOverObject { get; set; } = null;

    /// <summary>
    /// Gets or Sets the rotation speed of the camera in degrees per second.
    /// This property is used to control the speed at which the camera rotates when the player moves the mouse.
    /// The rotation speed is set to 0.5 degrees per second by default.
    /// </summary>
    [Export]
    public float HorizontalRotRate { get; set; } = 0.5f;

    /// <summary>
    /// Gets or Sets the rotation speed of the camera in degrees per second.
    /// This property is used to control the speed at which the camera rotates when the player moves the mouse.
    /// The rotation speed is set to 0.5 degrees per second by default.
    /// </summary>
    [Export]
    public float VerticalRotRate { get; set; } = 0.5f;

    /// <summary>
    /// Gets or Sets a value indicating whether the ObjectController is disabled.
    /// When set to true, the ObjectController will not process input or update objects.
    /// </summary>
    public bool DisableMovement { get; set; } = false;

    /// <summary>
    /// Called when the node is added to the scene.
    /// </summary>
    public override void _Ready()
    {
        this.spawnableObjectsItemList = this.GetNode<ItemList>("RightClickMenu/VBoxContainer/SpawnableObjectsItemList");
        foreach (SpawnableObject spawnableObject in World.Instance.SpawnableObjects)
        {
            this.spawnableObjectsItemList.AddItem(spawnableObject.Name);
        }

        this.spawnableObjectsItemList.Select(0);

        this.rightClickMenu = this.GetNode<Control>("RightClickMenu");
        Instance = this;
    }

    /// <summary>
    /// Called when the node is added to the scene tree.
    /// </summary>
    /// <param name="event">InputEvent passed to _Input method when called.</param>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!this.IsMultiplayerAuthority())
        {
            return;
        }

        if (Input.IsActionJustPressed("right_click"))
        {
            this.isInWorldEditMode = !this.isInWorldEditMode;

            if (this.isInWorldEditMode)
            {
                this.rightClickMenu.Show();
                Player3D.Instance.IsUnderPlayerControl = false;
                Input.MouseMode = Input.MouseModeEnum.Confined;
                Gui.Instance.AddHelpText("Right click to close menu, ctrl + left click to select object, delete to remove object, ctrl + wasd to drag object, shift + mouse to rotate object.");
            }
            else
            {
                this.rightClickMenu.Hide();
                Player3D.Instance.IsUnderPlayerControl = true;
                Input.MouseMode = Input.MouseModeEnum.Captured;
                Gui.Instance.RemoveHelpText("Right click to close menu, ctrl + left click to select object, delete to remove object, ctrl + wasd to drag object, shift + mouse to rotate object.");
            }
        }

        if (this.isInWorldEditMode)
        {
            if (Input.IsActionJustPressed("pause"))
            {
                this.isInWorldEditMode = false;
            }

            if (Input.IsActionPressed("left_click"))
            {
                if (this.selectedObject is not null && this.selectedObject != this.MouseOverObject)
                {
                    this.selectedObject.IsDragging = false;
                    this.selectedObject = null;
                }
                else if (this.selectedObject is null && this.MouseOverObject is not null)
                {
                    this.selectedObject = this.MouseOverObject;
                    this.selectedObject.IsDragging = true;
                }
            }

            if (Input.IsActionJustPressed("delete_world_object"))
            {
                if (this.selectedObject is not null)
                {
                    World.Instance.PlayerRemoveObject(this.selectedObject.RootNode.GetPath());
                    this.selectedObject = null;
                    this.MouseOverObject = null;
                }
            }

            if (Input.IsActionJustPressed("shift"))
            {
                this.isRotating = true;
                Gui.Instance.AddHelpText("Move mouse to rotate object");
            }

            if (Input.IsActionJustReleased("shift"))
            {
                this.isRotating = false;
                Gui.Instance.RemoveHelpText("Move mouse to rotate object");
            }

            if (this.isRotating && this.selectedObject != null && @event is InputEventMouseMotion)
            {
                var mm = (InputEventMouseMotion)@event;
                var rotY = Mathf.DegToRad(mm.Relative.X * this.HorizontalRotRate);
                this.selectedObject.RootNode.RotateY(rotY);
                NodePath? path = this.selectedObject.RootNode.GetPath();

                this.RpcId(1, "UpdatePosition", path, this.selectedObject.RootNode.Position, this.selectedObject.RootNode.Rotation);
            }
        }
    }

    /// <summary>
    /// Called every frame.
    /// This method is used to update the position of the selected object based on user input.
    /// It checks if the user is in world edit mode and if an object is selected.
    /// If so, it calculates the velocity based on user input and moves the object accordingly.
    /// It also updates the position of the object on the server.
    /// </summary>
    /// <param name="delta">Time since the last frame.</param>
    public override void _PhysicsProcess(double delta)
    {
        if (this.isInWorldEditMode && this.selectedObject != null && !this.DisableMovement)
        {
            Vector3 velocity = new Vector3(0, 0, 0);
            var inputDirX = Input.GetActionStrength("right") - Input.GetActionStrength("left");
            var inputDirZ = Input.GetActionStrength("backward") - Input.GetActionStrength("forward");
            var inputDirY = Input.GetActionStrength("down") - Input.GetActionStrength("up");
            Vector3 direction = (this.selectedObject.RootNode.Basis * new Vector3(inputDirX, inputDirY, inputDirZ)).Normalized();

            if (direction != Vector3.Zero)
            {
                velocity.X = direction.X * this.speed;
                velocity.Z = direction.Z * this.speed;
                velocity.Y = direction.Y * this.speed;
                this.selectedObject.RootNode.GlobalPosition += velocity * (float)delta;
                NodePath? path = this.selectedObject.RootNode.GetPath();
                this.RpcId(1, "UpdatePosition", path, this.selectedObject.RootNode.Position, this.selectedObject.RootNode.Rotation);
            }
        }
    }

    /// <summary>
    /// Updates the position of the object on the server.
    /// </summary>
    /// <param name="path">NodePath for the object to move.</param>
    /// <param name="position">New position of the object.</param>
    /// <param name="rotation">New rotation of the object.</param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    public async void UpdatePosition(NodePath path, Vector3 position, Vector3 rotation)
    {
        var serverObj = this.GetNode(path) as Node3D;
        if (serverObj != null)
        {
            serverObj.GlobalPosition = position;
            serverObj.GlobalRotation = rotation;
            var dbId = serverObj.Name;

            // Update position in database
            var dal = Services.Instance.LocationObjectsDAL;
            var locationObject = await dal.GetByIdAsync(dbId);
            if (locationObject == null)
            {
                GD.PrintErr("LocationObject not found in database");
                return;
            }

            locationObject.PositionX = position.X;
            locationObject.PositionY = position.Y;
            locationObject.PositionZ = position.Z;
            locationObject.RotationX = rotation.X;
            locationObject.RotationY = rotation.Y;
            locationObject.RotationZ = rotation.Z;
            await dal.UpdateAsync(locationObject);
        }
    }

    /// <summary>
    /// Spawns an object based on the selected item in the spawnable objects list.
    /// This method is called when the user clicks the spawn button in the right-click menu.
    /// It retrieves the selected index from the spawnable objects item list and gets the corresponding object name.
    /// It then prints a message to the console indicating which object will be spawned.
    /// </summary>
    public void OnSpawnObjectButtonPressed()
    {
        int selectedIndex = this.spawnableObjectsItemList.GetSelectedItems()[0];
        SpawnableObject spawnableObject = World.Instance.SpawnableObjects[selectedIndex];
        string scenePath = spawnableObject.Name!;
        var player = Player3D.Instance;

        // Calculate position in front of the player
        Vector3 spawnPosition = player.Position + (player.GlobalTransform.Basis.Z.Normalized() * -spawnableObject.Distance) + new Vector3(0, spawnableObject.Height, 0);

        LocationObject locationObject = new LocationObject
        {
            ScenePath = scenePath,
            Position = spawnPosition,
            Rotation = player.Rotation,
        };
        World.Instance.PlayerSpawnObject(locationObject);
    }
}

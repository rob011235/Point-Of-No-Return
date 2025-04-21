// -----------------------------------------------------------------------
// <copyright file="CameraDragger.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Godot;

/// <summary>
/// CameraDragger is used to drag objects in the 3D world.
/// It uses the mouse to select objects and move them around.
/// It is used to movem, select, and delete objects in the scene.
/// </summary>
public partial class CameraDragger : Camera3D
{
    private const float RAYLENGTH = 1000;
    private float depth = 0;
    private bool isInWorldEditMode = false;
    private Vector3? relativePosition = null;
    private Control rightClickMenu = default!;
    private IDraggableObject? draggedObject = null;

    /// <summary>
    /// Gets or sets the singleton instance of the CameraDragger class.
    /// This property is used to access the CameraDragger instance from other scripts.
    /// </summary>
    public static CameraDragger Instance { get; set; } = null!;

    /// <summary>
    /// Gets or sets the object that is currently being dragged.
    /// This property is used to store the object that is currently being dragged by the player.
    /// </summary>
    public IDraggableObject? MouseOverObject { get; set; } = null;

    /// <summary>
    /// Gets or sets the horizontal rotation speed of the camera in degrees per second.
    /// This property is used to control the speed at which the camera rotates when the player moves the mouse.
    /// The rotation speed is set to 0.5 degrees per second by default.
    /// </summary>
    [Export]
    public float HorizontalRotRate { get; set; } = 0.5f;

    /// <summary>
    /// Gets or sets the vertical rotation speed of the camera in degrees per second.
    /// This property is used to control the speed at which the camera rotates when the player moves the mouse.
    /// The rotation speed is set to 0.5 degrees per second by default.
    /// </summary>
    [Export]
    public float VerticalRotRate { get; set; } = 0.5f;

    /// <summary>
    /// Called when the node is added to the scene.
    /// This method is used to initialize the CameraDragger instance and set up the right-click menu.
    /// </summary>
    public override void _Ready()
    {
        this.rightClickMenu = this.GetNode<Control>("RightClickMenu");
        Instance = this;
    }

    /// <summary>
    /// Called every frame.
    /// This method is used to update the camera's rotation based on the mouse movement.
    /// It also handles the dragging of objects in the 3D world.
    /// </summary>
    /// <param name="delta">The time elapsed since the last frame.</param>
    public override void _PhysicsProcess(double delta)
    {
        if (!this.IsMultiplayerAuthority())
        {
            return;
        }

        if (this.draggedObject != null)
        {
            var newPos = Player3D.Instance.GlobalPosition + this.relativePosition!.Value;
            var newRot = Player3D.Instance.GlobalRotation;
            NodePath? path = this.draggedObject.RootNode.GetPath();
            this.RpcId(1, "UpdatePosition", path, newPos, newRot);
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

            locationObject.Position = position;
            locationObject.Rotation = rotation;
            await dal.UpdateAsync(locationObject);
        }
    }
}

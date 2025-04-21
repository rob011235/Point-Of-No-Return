using System;
using Godot;

public partial class DragBox3d : Area3D, IDraggableObject
{
    private MeshInstance3D meshInstance = default!;
    private Material hightlightMaterial = null!;

    // private bool isInWorldEditMode = false;

    [Export]
    public Node3D RootNode { get; set; } = default!;

    private bool isDragging = false;

    public bool IsDragging
    {
        get => this.isDragging;
        set
        {
            this.isDragging = value;
            if (this.isDragging)
            {
                this.meshInstance.MaterialOverlay = this.hightlightMaterial;
            }
            else
            {
                this.meshInstance.MaterialOverlay = null;
            }
        }
    }

    // private Vector3 relativePosition = Vector3.Zero;
    // private Vector3 relativeRotation = Vector3.Zero;
    override public void _Ready()
    {
        this.hightlightMaterial = GD.Load<Material>("res://Materials/HighlightMaterial.tres");
        this.meshInstance = this.GetNode<MeshInstance3D>("MeshInstance3D");
    }

    public void OnMouseEntered()
    {
        ObjectController.Instance.MouseOverObject = this;
        this.meshInstance.MaterialOverlay = this.hightlightMaterial;
    }

    public void OnMouseExited()
    {
        ObjectController.Instance.MouseOverObject = null;
        if (!this.isDragging)
        {
            this.meshInstance.MaterialOverlay = null;
        }
    }
}

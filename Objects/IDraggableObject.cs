using Godot;
/// <summary>
/// IDraggableObject is an interface that defines a contract for draggable objects in the 3D world.
/// It requires implementing classes to have a RootNode property of type Node3D.
/// </summary>
public interface IDraggableObject
{
    public Node3D RootNode { get; }

    public bool IsDragging { get; set; }
}
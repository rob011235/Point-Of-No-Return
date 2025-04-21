using System.Threading.Tasks;
using Godot;

public static class ExtensionMethods
{
    public static async Task Wait(this Node node, float seconds)
    {
        var timer = node.GetTree().CreateTimer(seconds);
        await node.ToSignal(timer, "timeout");
    }
}
// -----------------------------------------------------------------------
// <copyright file="QuitMenu.cs" company="Rob Garner">
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
public partial class QuitMenu : Panel
{
    private const string CREDITS = @"@BatteryAcidDev. 2024. Export and Run a Godot Dedicated Serverj (Youtube). Accessed March 30, 2025. https://youtu.be/jgJuX04cq7k.

Amazon. 2025. Launch a Linux Virtual Machine. Accessed March 30, 2025. https://aws.amazon.com/getting-started/launch-a-virtual-machine-B-0/.

Astle, Aaron. 2024. Multiplatform and multiplayer games overtake single format and single player. March 18. Accessed March 31, 2025. https://www.pocketgamer.biz/multiplatform-and-multiplayer-games-overtake-single-format-and-single-player/.

Campos, Henrique. 2023. The Essential Guide to Creating Multiplayer Games with Godot 4.0. Birmingham UK: Packt Publishing.

—. 2023. The Essential Guide to Creating Multiplayer Games with Godot 4.0. Birmingham: Packt Publishing.

Consortium, The SQLite. 2025. SQLite. Feb 25. Accessed Mar 30, 2025. https://www.sqlite.org/.

Cope, James. 2002. What’s a Peer-to-Peer (P2P) Network? April 8. https://www.computerworld.com/article/1355655/networking-peer-to-peer-network.html.

DevDrache. n.d. Drag and Drop 3D Objects - Learn Godot 4 3D - no taliking. Accessed Mar 24, 2025. https://youtu.be/QsPP0uueedA.

Discord. 2025. OAuth2. Accessed March 31, 2025. https://discord.com/developers/docs/topics/oauth2.

Doroshenko, Andrii. 2020. Allow to import and load external assets at run-time (in exported projects, without extra PCK files) #1632. Oct 8. Accessed March 31, 2025. https://github.com/godotengine/godot-proposals/issues/1632.

Driesen, Gert. 1. Software Developer. Hasselt, Mar 2025. Accessed 1 Mar 2025. https://github.com/sshnet/SSH.NET.

Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides, Erich Gamma, Richard Helm, Ralph Johnson, and John Vlissides. 1994. Design Patterns: Elements of Reusable Object-Oriented Software. Addison-Wesley Professional.

Evans, Eric. 2003. Domain-Driven Design: Tackling Complexity in the Heart of Software. Addison-Wesley Professional.

Foundation, The Godot. 2025. The Godot Foundation. Accessed Mar 30, 2025. https://godot.foundation/.

Godot. 2025. Exporting for dedicated servers. Accessed March 30, 2025. https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_dedicated_servers.html#doc-exporting-for-dedicated-servers.

—. 2025. High-level multiplayer. Accessed March 30, 2025. https://docs.godotengine.org/en/stable/tutorials/networking/high_level_multiplayer.html.

—. 2025. MultiplayerSpawner. Accessed March 30, 2025. https://docs.godotengine.org/en/stable/classes/class_multiplayerspawner.html.

—. 2025. MultiplayerSynchronizer. Accessed March 30, 2025. https://docs.godotengine.org/en/stable/classes/class_multiplayersynchronizer.html.

—. 2025. Singletons (Autoload). Accessed March 30, 2025. https://docs.godotengine.org/en/latest/tutorials/scripting/singletons_autoload.html.

Google. 2025. Compute Engine API. Accessed March 30, 2025. https://cloud.google.com/compute/docs/reference/rest/v1.

Microsoft. 2024. .NET dependency injection. July 18. Accessed March 30, 2025. https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection.

—. 2025. .NET is open source. Accessed March 30, 2025. https://dotnet.microsoft.com/en-us/platform/open-source.

—. 2025. Azure Virtual Machines. Accessed March 30, 2025. https://azure.microsoft.com/en-us/products/virtual-machines/.

—. 2025. Entity Framework. Accessed March 30, 2025. https://learn.microsoft.com/en-us/aspnet/entity-framework.

—. 2025. Guid Struct. Accessed Mar 30, 2025. https://learn.microsoft.com/en-us/dotnet/api/system.guid?view=net-8.0.

—. 2025. Language Integrated Query (LINQ). Accessed March 30, 2025. https://learn.microsoft.com/en-us/dotnet/csharp/linq/#iqueryable-linq-providers.

—. 2009. Microsoft Application Architecture Guide, Chapter 8: Data Layer Guidelines. Accessed March 30, 2025. https://learn.microsoft.com/en-us/previous-versions/msp-n-p/ee658127(v=pandp.10).

—. 2025. Microsoft Teams SDK reference. October 2. Accessed March 31, 2025. https://learn.microsoft.com/en-us/javascript/api/overview/msteams-client?view=msteams-client-js-latest&tabs=npm.

—. 2025. NuGet documentation. Accessed March 30, 2025. https://learn.microsoft.com/en-us/nuget/.

2025. OAuth Client Implementation. Accessed March 31, 2025. https://docs.bsky.app/docs/advanced-guides/oauth-client.

OAuth. 2025. Why OAuth. Accessed March 31, 2025. https://oauth.net/.

Rafael. 2022. Making peer-to-peer multiplayer seamless with Godot. January 2. Accessed Jul 15, 2024. https://www.rafa.ee/articles/godot-peer-to-peer-multiplayer/.

Sriram. 2024. Scopes(Lifetimes) — Dependency Injection in .Net Core. January 23. Accessed March 30, 2025. https://medium.com/@ggcsriram/scopes-lifetimes-dependency-injection-in-net-core-a5420d4f427f.

W3 Schools. 2025. CSS Fonts. Accessed March 31, 2025. https://www.w3schools.com/css/css_font.asp.";

    private RichTextLabel creditsLabel = default!;

    /// <summary>
    /// Called when the node is added to the scene tree.
    /// This method is used to initialize the credits label and load the credits from a text file.
    /// It sets the text of the credits label to the contents of the credits file.
    /// The credits file is located at "res://Menus/Credits.txt".
    /// If the file cannot be opened, it prints an error message to the console.
    /// </summary>
    public override void _Ready()
    {
        this.creditsLabel = this.GetNode<RichTextLabel>("CreditsRichTextLabel");
        this.LoadCredits();
    }

    /// <summary>
    /// Exits the game.
    /// This method is called when the user clicks the exit button in the right-click menu.
    /// It quits the game and closes the application.
    /// </summary>
    public void OnExitGameButtonPressed()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        this.GetTree().Quit();
    }

    /// <summary>
    /// Exits the game.
    /// This method is called when the user clicks the exit button in the right-click menu.
    /// It quits the game and closes the application.
    /// </summary>
    public void OnCancelButtonPressed()
    {
        this.QueueFree();
    }

    /// <summary>
    /// Loads the credits from a text file and displays them in the credits label.
    /// This method is called when the node is added to the scene tree.
    /// It opens the credits file, reads its contents, and sets the text of the credits label.
    /// If the file cannot be opened, it prints an error message to the console.
    /// </summary>
    private void LoadCredits()
    {
        // var file = FileAccess.Open("res://Menus/Credits.txt", FileAccess.ModeFlags.Read);
        // if (file == null)
        // {
        //     GD.PrintErr("Failed to open credits file.");
        //     return;
        // }

        // this.creditsLabel.Text = file.GetAsText();
        this.creditsLabel.Text = CREDITS;
    }
}

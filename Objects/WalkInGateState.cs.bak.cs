// -----------------------------------------------------------------------
// <copyright file="WalkInGateState.cs.bak.cs" company="Rob Garner">
// Copyright 2025 (c) Rob Garner. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// using Godot;

// /// <summary>
// /// WalkInGateState is a class that represents the state of a walk-in gate in the game.
// /// </summary>
// public partial class WalkInGateState : Node
// {
//     private Label domainNameLabel = default!;
//     private string domainName = string.Empty;

//     /// <summary>
//     /// Gets or sets the name of the location to travel to.
//     /// This property is used to set the location name for the travel location.
//     /// </summary>
//     public string DomainName
//     {
//         get => this.domainName;
//         set
//         {
//             this.domainName = value;
//             if (this.domainNameLabel != null)
//             {
//                 this.domainNameLabel.Text = this.domainName ?? string.Empty;
//             }
//         }
//     }

//     /// <summary>
//     /// Called when the node is added to the scene tree.
//     /// This method is responsible for initializing the domain name label with the current domain name.
//     /// </summary>
//     public override void _Ready()
//     {
//         this.domainNameLabel = this.GetNode<Label>("SetTravelLocationPanel/DomainNameLabel");
//         this.domainNameLabel.Text = this.domainName ?? string.Empty;
//     }
// }

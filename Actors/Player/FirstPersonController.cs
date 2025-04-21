// // Programmer: Rob Garner(rgarner011235@gmail.com)
// // Date: 2021-08-26
// // Description: A simple first-person controller that uses the CharacterBody3D class.
// // Based on tutorial by LegionGames at https://www.youtube.com/watch?v=A3HLeyaBCq4

// using System;
// using System.Runtime.CompilerServices;
// using Godot;
// /// <summary>
// /// A simple first-person controller that uses the CharacterBody3D class.
// /// It includes basic movement, jumping, head bobbing, and camera rotation.
// /// The head bobbing is a simple sinusoidal function that moves the camera up and down.
// /// The camera rotation is limited to a minimum and maximum angle.
// /// The player can sprint by holding the sprint button.
// /// The player can jump by pressing the jump button.
// /// </summary>
// public partial class FirstPersonController : CharacterBody3D
// {
//     // Constants for the player.
//     private const float WALK_SPEED = 5.0f;
//     private const float RUN_SPEED = 10.0f;
//     private const float JUMP_VELOCITY = 4.5f;
//     private const float SENSITIVITY = 0.005f;
//     private const float MIN_HEAD_ROTATION = -40.0f;
//     private const float MAX_HEAD_ROTATION = 60.0f;
//     private const float BOB_FREQUENCY = .05f;
//     private const float BOB_AMPLITUDE = 0.05f;
//     private const float BOB_SPEED = 0.1f;
//     private const float BASE_FOV = 75.0f;
//     private const float FOV_CHANGE = 1.5f;

//     private float speed;
//     private float bobTimer = 0.0f;

//     // Get the gravity from the project settings to be synced with RigidBody nodes.
//     private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

//     Node3D head = default!;
//     Camera3D camera = default!;

//     /// <summary>
//     /// The ready function sets the mouse mode to captured.
//     /// </summary>
//     public override void _Ready()
//     {
//         Input.MouseMode = Input.MouseModeEnum.Captured;
//         this.head = this.GetNode<Node3D>("Head");
//         this.camera = this.head.GetNode<Camera3D>("Camera");
//     }

//     /// <summary>
//     /// The unhandled input function handles the mouse movement to rotate the head and camera.
//     /// </summary>
//     /// <param name="event">Event that triggered call.</param>
//     public override void _UnhandledInput(InputEvent @event)
//     {
//         if (@event is InputEventMouseMotion) {
//             InputEventMouseMotion? mouseEvent = @event as InputEventMouseMotion;
//             if (mouseEvent != null)
//             {
//                 this.head.RotateY(-mouseEvent.Relative.X * SENSITIVITY);
//                 this.camera.RotateX(-mouseEvent.Relative.Y * SENSITIVITY);
//                 Vector3 angles = this.camera.RotationDegrees;
//                 angles.X = Mathf.Clamp(angles.X, MIN_HEAD_ROTATION, MAX_HEAD_ROTATION);
//                 this.camera.RotationDegrees = angles;
//             }
//         }
//     }

//     /// <summary>
//     /// The input function handles the cancel button to show the mouse cursor.
//     /// </summary>
//     /// <param name="event">Event that triggered call.</param>
//     public override void _Input(InputEvent @event)
//     {
//         if (@event.IsActionPressed("ui_cancel"))
//         {
//             Input.MouseMode = Input.MouseModeEnum.Visible;
//         }
//     }

//     /// <summary>
//     /// The physics process handles the movement, jumping, and head bobbing.
//     /// </summary>
//     /// <param name="delta">Time ellapsed for this frame.</param>
//     public override void _PhysicsProcess(double delta)
//     {
//         Vector3 velocity = this.Velocity;

//         // Add the gravity.
//         if (!this.IsOnFloor())
//             velocity.Y -= this.gravity * (float)delta;

//         // Handle Jump.
//         if (Input.IsActionJustPressed("ui_accept") && this.IsOnFloor())
//             velocity.Y = JUMP_VELOCITY;

//         if (Input.IsActionPressed("sprint"))
//             this.speed = RUN_SPEED;
//         else
//             this.speed = WALK_SPEED;

//         // Get the input direction and handle the movement/deceleration.
//         // As good practice, you should replace UI actions with custom gameplay actions.
//         Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
//         Vector3 direction = (this.head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

//         if (this.IsOnFloor())
//         {
//             // If the player is not moving, stop the player.
//             if (direction == Vector3.Zero)
//             {
//                 velocity.X = Mathf.Lerp(velocity.X, direction.X * this.speed, (float)delta * 7.0f);
//                 velocity.Z = Mathf.Lerp(velocity.Z, direction.Z * this.speed, (float)delta * 7.0f);
//             }

//             // If the player is moving, apply the movement.
//             else
//             {
//                 velocity.X = direction.X * this.speed;
//                 velocity.Z = direction.Z * this.speed;
//             }
//         }

//         // If the player is not on the floor, apply the movement.
//         else
//         {
//             velocity.X = Mathf.Lerp(velocity.X, direction.X * this.speed, (float)delta * 3.0f);
//             velocity.Z = Mathf.Lerp(velocity.Z, direction.Z * this.speed, (float)delta * 3.0f);
//         }

//         // Apply the head bobbing.
//         this.bobTimer += (float)delta + velocity.Length() * (this.IsOnFloor() ? 1 : 0);
//         this.camera.Transform = new Transform3D(this.camera.Transform.Basis, this.HeadBob(this.bobTimer));

//         // Apply the FOV change.
//         var velocity_clamped = Mathf.Clamp(velocity.Length(), 0.5, RUN_SPEED * 2);
//         var taget_fov = BASE_FOV + FOV_CHANGE * velocity_clamped;
//         this.camera.Fov = (float)Mathf.Lerp(this.camera.Fov, taget_fov, (float)delta * 8.0f);

//         // Apply the movement.
//         this.Velocity = velocity;
//         this.MoveAndSlide();
//     }

//     /// <summary>
//     /// A simple sinusoidal function that moves the camera up and down.
//     /// </summary>
//     /// <param name="timer">A float representing current time in cycle.</param>
//     /// <returns>Vector3 with delta movement</returns>
//     private Vector3 HeadBob(float timer)
//     {
//         float y = Mathf.Sin(timer * BOB_FREQUENCY) * BOB_AMPLITUDE;
//         float x = Mathf.Cos(timer * BOB_FREQUENCY / 2.0f) * BOB_AMPLITUDE;
//         return new Vector3(x, y, 0);
//     }
// }

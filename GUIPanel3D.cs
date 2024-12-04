
using Godot;

/// Translation of the gui_panel_3d.tscn tutorial.
public partial class GUIPanel3D : Node3D
{
    public GUIPanel3D() { }
    [Export]
    public string PanelText { get => Text.Text; set => Text.Text = value; }

    // Used for checking if the mouse is inside the Area3D.
    private bool IsMouseInside = false;

    // The last processed input touch/mouse event. Used to calculate relative movement.
    private Vector2 LastEventPosition2D = new Vector2(0.0f, 0.0f);

    // The time of the last event in seconds since engine start.
    private float LastEventTime = -1.0f;

    private SubViewport NodeViewport;
    private MeshInstance3D NodeQuad;
    private Area3D NodeArea;
    private Label Text;

    public override void _Ready()
    {
        Visible = false;
        // Equivalent of @onready on GDScript.
        NodeViewport = GetNode<SubViewport>("SubViewport");
        NodeQuad = GetNode<MeshInstance3D>("Quad");
        NodeArea = GetNode<Area3D>("Quad/Area3D");
        Text = GetNode<Label>("SubViewport/GUI/Panel/VBoxContainer/Label");

        NodeArea.Connect(CollisionObject3D.SignalName.MouseEntered, new Callable(this, MethodName.NodeArea_MouseEntered));
        NodeArea.Connect(CollisionObject3D.SignalName.MouseExited, new Callable(this, MethodName.NodeArea_MouseExited));
        NodeArea.Connect(CollisionObject3D.SignalName.InputEvent, new Callable(this, MethodName.NodeArea_InputEvent));

        // If the material is NOT set to use billboard settings, then avoid running billboard specific code
        // Note: cast to StandardMaterial3D added to access BillboardMode.
        StandardMaterial3D material = (StandardMaterial3D)NodeQuad.GetSurfaceOverrideMaterial(0);
        //material.BillboardMode = BaseMaterial3D.BillboardModeEnum.Enabled;
        //if (material.BillboardMode is BaseMaterial3D.BillboardModeEnum.Disabled) SetProcess(false);
    }

    public override void _Process(double delta)
    {
        RotateAreaToBillboard();
    }



    public override void _UnhandledInput(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseButton:
            case InputEventMouseMotion:
            case InputEventScreenDrag:
            case InputEventScreenTouch:
                return;
        }

        NodeViewport.PushInput(@event);
    }
    public void NodeArea_MouseEntered() => IsMouseInside = true;

    public void NodeArea_MouseExited() => IsMouseInside = false;

    public void NodeArea_InputEvent(Node camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIdx)
    {
        $"{@event.AsText}".ToConsole();
        // C# difference: to obtain the dimensions of a Mesh, it needs to be cast to its underlying type. In this case, Godot.QuadMesh.
        QuadMesh quadMeshResource = NodeQuad.Mesh as QuadMesh;

        // Get mesh size to detect edges and make conversions. This code only supports PlaneMesh and QuadMesh.
        Vector2 quadMeshSize = quadMeshResource.Size;
        $"quadmeshsize: {quadMeshSize.X}, {quadMeshSize.Y}".ToConsole();
        // Event position in Area3D in world coordinate space.
        Vector3 eventPosition3D = eventPosition;
        $"eventposition3d: {eventPosition.X}, {eventPosition.Y}, {eventPosition.Z}".ToConsole();
        // Current time in seconds since engine start.
        double now = Time.GetTicksMsec() / 1000.0;

        // Convert position to a coordinate space relative to the Area3D node.
        // NOTE: `affine_inverse()` accounts for the Area3D node's scale, rotation, and position in the scene!
        eventPosition3D = NodeQuad.GlobalTransform.AffineInverse() * eventPosition3D;
        $"[affine_inverse]eventposition3d: {eventPosition.X}, {eventPosition.Y}, {eventPosition.Z}".ToConsole();
        // Comment from original:
        // TODO: Adapt to bilboard mode or avoid completely.

        Vector2 eventPosition2D = new();
        $"[round_1]eventposition2d: {eventPosition2D.X}, {eventPosition2D.Y}".ToConsole();
        if (IsMouseInside)
        {
            // Convert the relative event position from 3D to 2D.
            eventPosition2D = new Vector2(eventPosition3D.X, -eventPosition3D.Y);
            $"[round_2:mouse_inside]eventposition2d: {eventPosition2D.X}, {eventPosition2D.Y}".ToConsole();
            // Right now the event position's range is the following: (-quad_size/2) -> (quad_size/2)
            // We need to convert it into the following range: -0.5 -> 0.5
            eventPosition2D.X /= quadMeshSize.X;
            eventPosition2D.Y /= quadMeshSize.Y;
            $"[round_3:convert_range]eventposition2d: {eventPosition2D.X}, {eventPosition2D.Y}".ToConsole();
            // Then we need to convert it into the following range: 0 -> 1
            //
            // C# difference: usually, internal Godot floating point numbers are in `float`.
            // In Vectors (and other Godot types), append an "f" at the end of a number to make it a float: 0.75f
            // Else, cast it to float: (float)0.75
            eventPosition2D.X += 0.5f;
            eventPosition2D.Y += 0.5f;
            $"[round_4:convert_to_range_0-1]eventposition2d: {eventPosition2D.X}, {eventPosition2D.Y}".ToConsole();
            // Finally, we convert the position to the following range: 0 -> NodeViewport.Size
            eventPosition2D.X *= NodeViewport.Size.X;
            eventPosition2D.Y *= NodeViewport.Size.Y;
            $"[round_5:clamp_to_NodeViewportSize]eventposition2d: {eventPosition2D.X}, {eventPosition2D.Y}".ToConsole();
            // We need to do these conversions so the event's position is in the viewport's coordinate system.
        }
        else if (LastEventPosition2D != Vector2.Zero) // Vector2 is not nullable in C#, 
        {
            eventPosition2D = LastEventPosition2D;
            $"[null_check]eventposition2d: {eventPosition2D.X}, {eventPosition2D.Y}".ToConsole();
        }

        var e = @event as InputEventMouse;

        e.Position = eventPosition2D;
        $"[event]position: {e.Position.X}, {e.Position.Y}".ToConsole();
        $"[event]eventposition2d: {eventPosition2D.X}, {eventPosition2D.Y}".ToConsole();
        e.GlobalPosition = eventPosition2D;
        $"[event]global_position: {e.GlobalPosition.X}, {e.GlobalPosition.Y}".ToConsole();
        //if (@event is InputEventMouse)
        //{
        //    e.GlobalPosition = eventPosition2D;
        //}

        // C# difference: have to check for both types, sadly.
        var mouse = @event as InputEventMouseMotion;
        var screen = @event as InputEventScreenDrag;

        // Calculate the relative event distance.
        if (@event is InputEventMouseMotion || @event is InputEventScreenDrag)
        {
            //If there is not a stored previous position, then we'll assume there is no relative motion.
            if (LastEventPosition2D == Vector2.Zero)
            {
                $"[calculating_distance]last_event_position_2d==Zero: {LastEventPosition2D.X}, {LastEventPosition2D.Y}".ToConsole();
                switch (@event)
                {
                    case InputEventMouseMotion:
                        mouse.Relative = eventPosition2D;
                        break;
                    case InputEventScreenDrag:
                        screen.Relative = eventPosition2D;
                        break;
                }
            }
            // If there is a stored previous position, then we'll calculate the relative position by subtracting
            // the previous position from the new position. This will give us the distance the event traveled from prev_pos.
            else
            {
                switch (@event)
                {
                    case InputEventMouseMotion:
                        $"[calculating_distance]lastEventPosition: {LastEventPosition2D.X}, {LastEventPosition2D.Y}".ToConsole();
                        mouse.Relative = eventPosition2D - LastEventPosition2D;
                        $"[calculating_distance_mouse]last_event_position_2d!=zero:relative: {mouse.Relative.X}, {mouse.Relative.Y}".ToConsole();

                        mouse.Velocity = mouse.Relative / ((float)now - LastEventTime);
                        $"[calculating_distance_mouse]last_event_position_2d!=zero:velocity:  {mouse.Velocity.X} ,  {mouse.Velocity.Y}".ToConsole();
                        break;
                    case InputEventScreenDrag:
                        screen.Relative = eventPosition2D - LastEventPosition2D;
                        $"[calculating_distance_screen]last_event_position_2d!=zero:relative: {screen.Relative.X}, {screen.Relative.Y}".ToConsole();
                        screen.Velocity = screen.Relative / ((float)now - LastEventTime);
                        $"[calculating_distance_screen]last_event_position_2d!=zero:velocity:  {screen.Velocity.X} ,  {screen.Velocity.Y}".ToConsole();
                        break;
                }
            }

            // Update last_event_pos2D with the position we just calculated.
            LastEventPosition2D = eventPosition2D;
            $"[final_value]last_event_position: {LastEventPosition2D.X}, {LastEventPosition2D.Y}".ToConsole();
            // Update last_event_time to current time.
            LastEventTime = (float)now;

            // Finally, send the processed input event to the viewport.
            NodeViewport.PushInput(@event, true);
            $"{@event.AsText}".ToConsole();
        }
    }

    private void RotateAreaToBillboard()
    {
        StandardMaterial3D material = (StandardMaterial3D)NodeQuad.GetSurfaceOverrideMaterial(0);
        BaseMaterial3D.BillboardModeEnum billboardMode = material.BillboardMode;

        // Try to match the area with the material's billboard setting, if enabled.
        if (billboardMode > 0)
        {
            // Get the camera.
            var camera = GetViewport().GetCamera3D();
            // Look in the same direction as the camera.
            var look = camera.ToGlobal(new Vector3(0.0f, 0.0f, -100.0f)) - camera.GlobalTransform.Origin;
            look = NodeArea.Position + look;

            // Y-Billboard: Lock Y rotation, but gives bad results if the camera is tilted.
            if (billboardMode is BaseMaterial3D.BillboardModeEnum.FixedY)
            {
                look = new Vector3(look.X, 0.0f, look.Z);
            }

            NodeArea.LookAt(look, Vector3.Up);

            // Rotate in the Z axis to compensate camera tilt.
            NodeArea.RotateObjectLocal(Vector3.Back, camera.Rotation.Z);
        }
    }
}

public static class ConsoleExtensions
{
    public static void ToConsole(string what) => GD.Print(what);
}

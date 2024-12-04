# GUI in 3D
## 📜 Original Description
A demo showing a GUI instanced within a 3D scene using viewports, as well as forwarding mouse and keyboard input to the GUI.

### My own description
This is something I adaped fromt the original to add point-and-show UI elements on the remake of [Premonition](https://framebuffers.itch.io/premonition). Might as well give back something to the community.
It has been **heavily** modified, but kept a more "faithful" initial translation. Here it is!

## 📝 Modifications
- This is an attempt to translate the original [gui-in-3d](https://github.com/godotengine/godot-demo-projects/tree/master/viewport/gui_in_3d) tutorial to C#.
- **⚠️THIS IS NOT 100% FUNCTIONAL YET⚠️**
  - You can't click on it. `Billboard` mode works, the `SubViewport` works, but you cannot interact with it _still._ Dunno why.
- **⚠️IT CONTAINS CUSTOM CODE NOT ON THE ORIGINAL TUTORIAL⚠️**
  - A `.ToConsole();` extension to the `string` type. This is my own method to simplify a way to print strings to console. It is invoked as `"something".ToConsole();` or `stringThing.ToConsole();`.
  - A reference to a `GameDirector`. This will be removed at some point when I get to rewire the code back from the improvements/edits I did from within my game.

## ✅ To do
- Re-wire and clean up the code from the original GDScript codebase. It was forked from the original GDScript, initialised a new solution file, and worked from there. It's **very** janky.
- There's an error (present on the original as well):
  ```
  scene\main\viewport.cpp:136 - Viewport Texture must be set to use it.
  scene\main\viewport.cpp:126 - Viewport Texture must be set to use it.
  ```
  - Haven't tried if it's my (custom) Godot version or something else. Probably it's me.
  - Fix up the issue with `Control` Nodes inside the UI viewport.

## ℹ️ Information
- Language: C#
- Renderer: Compatibility

Check out (the original GDScript) demo on the asset library: https://godotengine.org/asset-library/asset/2807


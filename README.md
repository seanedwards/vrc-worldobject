# vrc-worldobject

`vrc-worldobject` allows you to create world space props that are network-synced for late joiners.

This is a very early prototype. Do not expect a one-click experience yet. Please read the installation instructions carefully.

If you encounter any issues, feel free to open a message here, or message me (`Goat#5718`) on Discord.

## Installation

1. Drag the World Constraint prefab under your avatar.
2. Assign the `WorldObjectParameters` expression parameters to your avatar descriptor, or create similar parameters in your own expression parameters.
3. Assign the `FxLayerController` animation controller to your avatar descriptor, or create similar layers in your own Fx layer controller.
4. Send OSC messages to control the `WorldX` `WorldY` `WorldZ` and `WorldR` parameters.
   * X, Y, Z are in a range of -1.0 to 1.0, representing +/- 1km from the world origin.
   * Rotation is sent in a range from 0.0 to 1.0, representing 0 to 360 degrees of rotation along the Y axis.

## Methodology

    * Create a game object, with a Parent Constraint that's set to the world at 0,0,0. No reset targets. Create 8 animations for it:
      * +1000 X
      * -1000 X
      * +1000 Y
      * -1000 Y
      * +1000 Z
      * -1000 Z
      * 0 degrees rotation on Z
      * 360 degrees rotation on Z 
    * Create a blend tree, which maps 4 different animation parameters (X, Y, Z, rot) to blend between those animations.
    * Now, your animation parameters (which are network synced) literally set the exact coordinates for the object (+/- 1km in any direction from the origin). You can use OSC to set those parameters directly.
    * You can write down these coordinates, to restore stuff like stages to an exact position in the world.


## Contributing

Pull requests are welcome. Contributors will be listed below.

### Contributors

`Goat#5718` aka [Oli__](https://vrchat.com/home/user/usr_d9a5fde5-9a01-4623-b868-1182d4434d35)
## TODO

Because no self-respecting project is ever complete.

* It is very difficult to figure out the coordiates for a point in space where you want to place the object. See [this issue](https://github.com/vrchat-community/osc/issues/43) for details.
* Is there a simpler way to blend the X/Y/Z animations, so we don't have to have 3 separate "mover" objects in a hierarchy? (I don't think it's a problem, it's just untidy.)
* Put an actual explanation in the methodology section, instead of just dumping my napkin notes.

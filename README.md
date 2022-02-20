# vrc-worldobject

`vrc-worldobject` allows you to create world space props that are network-synced for late joiners.

This is a very early prototype. Do not expect a one-click experience yet. Please read the installation instructions carefully.
At this point, nothing in here is so complex that you couldn't just rebuild it yourself, but this repository is offered as an example.

If you encounter any issues, feel free to open an issue or pull request here, or message me (`Goat#5718`) on Discord.

## Prerequisites

* 19 bits of available parameter space (2 floats and 3 bools)
* AudioLink (only if using the `PositionPanel` material/shader)
* TouchOSC

## Installation

1. Drag the World Constraint prefab under your avatar.
2. Assign the `WorldObjectParameters` expression parameters to your avatar descriptor, or create similar parameters in your own expression parameters.
3. Assign the `FxLayerController` animation controller to your avatar descriptor, or create similar layers in your own Fx layer controller.
4. Open `WorldObject.tosc` to control your new world object parameters.
   * Don't forget to set up the correct mappings in your JSON config!
   * Closing TouchOSC will break syncing for late joiners.

## Contributing

Pull requests are welcome. Contributors will be listed below.

### Contributors

`Goat#5718` aka [Oli__](https://vrchat.com/home/user/usr_d9a5fde5-9a01-4623-b868-1182d4434d35)

Major credit goes to VRLabs for the development of the original [World Constraint](https://vrlabs.dev/item/world-constraint) on which this project builds.

## TODO

Because no self-respecting project is ever complete.

* Is there a simpler way to blend the X/Y/Z animations, so we don't have to have 3 separate "mover" objects in a hierarchy? (I don't think it's a problem, it's just untidy.)
* Put an actual explanation in the methodology section, instead of just dumping my napkin notes.

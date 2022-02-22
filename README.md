# vrc-worldobject

`vrc-worldobject` allows you to create world space props that are network-synced for late joiners.

This is a very early prototype. Do not expect a one-click experience yet. Please read the installation instructions carefully.
At this point, nothing in here is so complex that you couldn't just rebuild it yourself, but this repository is offered as an example.

If you encounter any issues, feel free to open an issue or pull request here, or message me (`Goat#5718`) on Discord.

## Prerequisites

* 19 bits of available parameter space (2 floats and 3 bools)
* AudioLink (only if using the `PositionPanel` material/shader)
* TouchOSC
* A custom Fx layer and expression parameters already configured (even if they're empty)

## Installation

**MAKE A BACKUP**

1. Drag the `WorldObject` prefab into the root of your scene. Then, drag it under the root of your avatar. This will ensure that the WorldObject scale is correct, even if your avatar is scaled.
2. Add the `WorldObjectSetup` script to your Avatar.
3. (Optional) Set the max range of your object. A smaller range will give you more resolution in placement, while a larger range will allow you to place your object farther from the world origin. 1000 (1 kilometer!!) is a good default that will work for most worlds.
4. Press the "Build Animator" button.
5. Remove the `WorldObjectSetup` script from your avatar before uploading.
6. Open `WorldObject.tosc` to control your new world object parameters.
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

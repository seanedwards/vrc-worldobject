# vrc-worldobject

`vrc-worldobject` allows you to create world space props that are network-synced for late joiners.

If you encounter any issues, feel free to open an issue or pull request here, or message me (`Oli__#0001`) on Discord.

[Check out the brief demo on YouTube!](https://www.youtube.com/watch?v=9rUnCouqyPs)

## Prerequisites

* 19 bits of available parameter space (2 floats and 3 bools)
* TouchOSC
* A custom Fx layer and expression parameters already configured (even if they're empty)
* 1 material slot

## Installation

Don't forget to **MAKE A BACKUP**

1. Drag the `WorldObject` prefab into the root of your scene. Then, drag it under the root of your avatar. This will ensure that the WorldObject scale is correct, even if your avatar is scaled.
2. Add the `WorldObjectSetup` script to your Avatar.
3. (Optional) Set the max range of your object. A smaller range will give you more resolution in placement, while a larger range will allow you to place your object farther from the world origin. 1000 (1 kilometer!!) is a good default that will work for most worlds.
4. Press the "Build Animator" button.
5. Remove the `WorldObjectSetup` script from your avatar before uploading.
6. Open `WorldObject.tosc` to control your new world object parameters.
   * Don't forget to set up the correct mappings in your JSON config!
   * Closing TouchOSC will break syncing for late joiners.

## Change Log

### 1.0.2

* Fixes issue where TouchOSC was sending float params, but a default VRC OSC config file would require bool params. TouchOSC will now properly send bools.

### 1.0.1

Bug fixes:

* Hopefully fixes #2 (NullReferenceException when building animator)
* Fixes PositionPanel compile error

## Contributing

Pull requests are welcome. Contributors will be listed below.

### Contributors

`Oli__#0001` aka [Oli__](https://vrchat.com/home/user/usr_d9a5fde5-9a01-4623-b868-1182d4434d35)

Major credit goes to VRLabs for the development of the original [World Constraint](https://vrlabs.dev/item/world-constraint) on which this project builds.

## TODO

Because no self-respecting project is ever complete.

* Is there a simpler way to blend the X/Y/Z animations, so we don't have to have 3 separate "mover" objects in a hierarchy? (I don't think it's a problem, it's just untidy.)
* Put an actual explanation in the methodology section, instead of just dumping my napkin notes.

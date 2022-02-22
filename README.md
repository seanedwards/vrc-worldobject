# vrc-worldobject

`vrc-worldobject` allows you to create world space props that are network-synced for late joiners.

This is a very early prototype. Do not expect a one-click experience yet. Please read the installation instructions carefully.
At this point, nothing in here is so complex that you couldn't just rebuild it yourself, but this repository is offered as an example.

If you encounter any issues, feel free to open an issue or pull request here, or message me (`Goat#5718`) on Discord.

## Prerequisites

* 19 bits of available parameter space (2 floats and 3 bools)
* AudioLink (only if using the `PositionPanel` material/shader)
* TouchOSC
* A custom Fx layer already configured (even if it's empty)

## Installation

**MAKE A BACKUP**

1. Add the `WorldObjectSetup` script to your Avatar.
2. Press the "Build Animator" button.
3. Create the 5 new parameters in your expression parameters list:
   * `WorldPosCoarse` (Float)
   * `WorldPosFine` (Float)
   * `WorldAxis0` (Bool)
   * `WorldAxis1` (Bool)
   * `WorldAxisLock` (Bool)
4. Open `WorldObject.tosc` to control your new world object parameters.
   * Don't forget to set up the correct mappings in your JSON config!
   * Closing TouchOSC will break syncing for late joiners.

## Troubleshooting

If your WorldObject is not working correctly, before submitting an issue, please open the
[Unity Editor Test Runner](https://docs.unity3d.com/2017.4/Documentation/Manual/testing-editortestsrunner.html)
and run the `WorldObjectTests.dll` test suite. Include the **entire** log output in your issue report.

## Contributing

Pull requests are welcome. Contributors will be listed below.

### Contributors

`Goat#5718` aka [Oli__](https://vrchat.com/home/user/usr_d9a5fde5-9a01-4623-b868-1182d4434d35)

Major credit goes to VRLabs for the development of the original [World Constraint](https://vrlabs.dev/item/world-constraint) on which this project builds.

## TODO

Because no self-respecting project is ever complete.

* Is there a simpler way to blend the X/Y/Z animations, so we don't have to have 3 separate "mover" objects in a hierarchy? (I don't think it's a problem, it's just untidy.)
* Put an actual explanation in the methodology section, instead of just dumping my napkin notes.

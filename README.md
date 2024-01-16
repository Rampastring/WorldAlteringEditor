# C&C World-Altering Editor (WAE)

Modern open-source map and scenario editor for

- Command & Conquer: Red Alert 2 Yuri's Revenge
- Command & Conquer: Tiberian Sun
- Dawn of the Tiberium Age (DTA) https://www.moddb.com/mods/the-dawn-of-the-tiberium-age

## Motivation

The World-Altering Editor is a new map editor for the second-generation classic Command & Conquer games,
designed to replace the old FinalSun/FinalAlert2 (FS/FA2) map editor developed in the TS/RA2 modding community in the early 2000s.

To make it familiar for existing mappers, the editor is designed to follow the FS/FA2 UI design,
but with modernizations and changes to make the editor smoother and more efficient to use.

## State of the project

The editor includes practically all tools present in FS/FA2 and a significant number of
new ones. Many familiar tools have been improved with significant quality-of-life improvements.
These improvements make it much more efficient to create maps with WAE than the old FS/FA2 map editors.

New functionality and helpful features are being constantly added to make the mapping experience smoother and more efficient.
Some examples include a zoom function, a new, powerful trigger editor interface, and a terrain generator for rapid detailing of large areas.

Graphically the editor is more demanding than FS/FA2, but can also utilize modern hardware
much more efficiently, meaning that if you have a decently modern computer, WAE achieves far smoother
performance with better graphical quality than the old editors.

## System requirements

This editor uses MonoGame with Rampastring's custom XNAUI library. 
A DirectX11 compatible GPU and a 64-bit system is required.
If you absolutely need a 32-bit build, you can modify the source to produce one.

The editor needs considerable VRAM, as the sprite graphics of the game are currently
converted into full 32-bit ARGB textures prior to drawing. The exact amount of VRAM
required depends on the map and the game or mod and can range from a hundred megabytes
on a light map in original Tiberian Sun to some gigabytes on a heavy map in a mod that has
tons of custom assets.

## Downloads

For most end-users, it is recommended that you download our latest official release: https://github.com/Rampastring/WorldAlteringEditor/releases

Development builds are available through our automated build workflow which is run whenever new commits are pushed. You can download the editor from the build artifacts:
https://github.com/Rampastring/WorldAlteringEditor/actions

If you are mapping for Dawn of the Tiberium Age, the editor is bundled with the mod.

## Contributing

We gladly accept contributions as long as they are well made and we deem them as beneficial for a significant part of the editor's userbase.

Follow the guidelines in `Contributions.md` when creating pull requests.

## License

The editor is licensed under the GNU General Public License, version 3.
If you create and publish a derivate, you need to also release your source code for the fork.
Please see LICENSE.txt for more details.

EA has not endorsed and does not support the World-Altering Editor.

## Screenshot

![Screenshot of the editor](https://github.com/Rampastring/WorldAlteringEditor/raw/master/mapeditor.jpg "Map Editor Screenshot")

## Introduction video

[![Dawn of the Tiberium Age Scenario Editor Introduction](https://github.com/Rampastring/WorldAlteringEditor/raw/master/videopreview.jpg)](https://www.youtube.com/watch?v=jIcr3nCqx7M "Dawn of the Tiberium Age Scenario Editor Introduction")

The World-Altering Editor was originally developed by the Dawn of the Tiberium Age staff for their mod. TS and YR support were added later to offer a boost in mapping efficiency to the rest of the second-generation C&C community.

[![Dawn of the Tiberium Age Scenario Editor Introduction](https://github.com/Rampastring/WorldAlteringEditor/raw/master/dtalogo.png)](https://www.moddb.com/mods/the-dawn-of-the-tiberium-age "Dawn of the Tiberium Age Homepage")

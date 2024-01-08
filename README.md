# C&C World-Altering Editor (WAE)

Modern open-source map and scenario editor for

- Command & Conquer: Red Alert 2 Yuri's Revenge
- Command & Conquer: Tiberian Sun
- Dawn of the Tiberium Age (DTA) https://www.moddb.com/mods/the-dawn-of-the-tiberium-age

## Motivation

The World-Altering Editor is a new map editor for the second-generation classic Command & Conquer games,
designed to replace the old FinalSun/FinalAlert (FS/FA) map editor developed in the TS/RA2 modding community in the early 2000s.

To make it familiar for existing mappers, the editor is designed to follow the FS/FA UI design,
but with modernizations and changes to make the editor smoother and more efficient to use.

## State of the project

The editor includes practically all tools present in FS/FA and a significant number of
new ones. Many familiar tools have also been improved with significant quality-of-life improvements.
These improvements make it much more efficient to create maps with WAE than the old FS/FA map editors.

New functionality and helpful features are being constantly added to make the mapping experience smoother and more efficient.

Graphically the editor is better than FinalSun aside from lacking voxel support
and requiring a higher-end system. However, if you do have a powerful system, the editor
runs far more smoothly than the original FinalSun/FinalAlert editors.

## System requirements

This editor uses MonoGame with Rampastring's custom XNAUI library. 
A DirectX11 compatible GPU and a 64-bit system is required.
If you absolutely need a 32-bit build, you can modify the source to produce one.

The editor needs considerable VRAM, as the sprite graphics of the game are currently
converted into full 32-bit ARGB textures prior to drawing. In case of
Dawn of the Tiberium Age, the editor appears to allocate roughly 1 GB of VRAM.

## Downloads

For most end-users, it is recommended that you download our latest official release: https://github.com/Rampastring/TSMapEditor/releases

Development builds are available through our automated build workflow which is run whenever new commits are pushed. You can download the editor from the build artifacts:
https://github.com/Rampastring/TSMapEditor/actions

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

![Screenshot of the editor](https://github.com/Rampastring/TSMapEditor/raw/master/mapeditor.jpg "Map Editor Screenshot")

## Introduction video

[![Dawn of the Tiberium Age Scenario Editor Introduction](https://github.com/Rampastring/TSMapEditor/raw/master/videopreview.jpg)](https://www.youtube.com/watch?v=jIcr3nCqx7M "Dawn of the Tiberium Age Scenario Editor Introduction")

The World-Altering Editor was originally developed by the Dawn of the Tiberium Age staff for their mod. TS and YR support were added later to offer a boost in mapping efficiency to the rest of the second-generation C&C community.

[![Dawn of the Tiberium Age Scenario Editor Introduction](https://github.com/Rampastring/TSMapEditor/raw/master/dtalogo.png)](https://www.moddb.com/mods/the-dawn-of-the-tiberium-age "Dawn of the Tiberium Age Homepage")

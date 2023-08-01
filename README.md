![Dawn of the Tiberium Age Logo](https://github.com/Rampastring/TSMapEditor/raw/master/dtalogo.png "DTA Logo")

# World-Altering Editor (WAE)

Work-in-progress scenario editor for

- Dawn of the Tiberium Age (DTA) https://www.moddb.com/mods/the-dawn-of-the-tiberium-age
- Command & Conquer: Tiberian Sun
- Command & Conquer: Red Alert 2 Yuri's Revenge

## Motivation

The purpose of the World-Altering Editor project is to develop a new scenario editor for
Dawn of the Tiberium Age, Tiberian Sun and Red Alert 2,
replacing the old FinalSun map editor developed in the TS/RA2 modding community in the early 2000s.
I think it has been sad to see the ancient map editor restrict the community as much as it has done,
and instead of developing a proper replacement for it (which could be achieved relatively quickly),
the community has instead started writing complex hacks for the old editor by reverse-engineering it,
modifying its executable and injecting code into the editor in the form of custom DLLs.

I think it'd be much better in the long term to focus these efforts on building a new editor
instead of modifying one that has no available source and that is based on a very outdated technological base.

To make it familiar for existing mappers, the editor is designed to follow the FinalSun UI design,
but with modernizations and changes to make the editor smoother and more efficient to use.

## Current state of the project

The editor includes nearly all tools present in FinalSun and a significant number of
new ones. Many familiar tools have also been improved with significant quality-of-life improvements. 
Some tools might still require some testing and tweaking to achieve perfection. New functionality
and helpful features are being constantly added to make the mapping experience smoother and more efficient.

Graphically the editor is better than FinalSun aside from lacking voxel support
and requiring a higher-end system. However, if you do have a powerful system, the editor
runs far more smoothly than the original FinalSun/FinalAlert editors.

## System requirements

This editor uses MonoGame with Rampastring's custom XNAUI library. 
A DirectX11 compatible GPU and a 64-bit system is required.
If you absolutely need a 32-bit build, you can modify the source to produce one.

The editor needs considerable VRAM, as the sprite graphics of the game are currently
converted into full 32-bit ARGB textures prior to drawing. In case of
Dawn of the Tiberium Age, the editor appears to allocate roughly 500 MB of VRAM.

The renderer is also relatively heavy on both the CPU and GPU.

## Downloads

There is a build workflow on new commits being pushed, which allows you to download the editor from the build artifacts:
https://github.com/Rampastring/TSMapEditor/actions

Aside from that, there is currently no publicly hosted download other than the editor being included with Dawn of the Tiberium Age.
If you need a separate download, you can contact me and ask for one on DTA's Discord server: https://discord.gg/6UtC289

## License

The editor is licensed under the GNU General Public License, version 2.
If you create and publish a derivate, you need to also release your source code for the fork.
Please see LICENSE.txt for more details.

## Screenshot

![Screenshot of the editor](https://github.com/Rampastring/TSMapEditor/raw/master/mapeditor.jpg "Map Editor Screenshot")

## Introduction video

[![Dawn of the Tiberium Age Scenario Editor Introduction](https://github.com/Rampastring/TSMapEditor/raw/master/videopreview.jpg)](https://www.youtube.com/watch?v=jIcr3nCqx7M "Dawn of the Tiberium Age Scenario Editor Introduction")

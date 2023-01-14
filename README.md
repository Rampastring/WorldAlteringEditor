![Dawn of the Tiberium Age Logo](https://github.com/Rampastring/TSMapEditor/raw/master/dtalogo.png "DTA Logo")

# Dawn of the Tiberium Age Scenario Editor

Work-in-progress scenario editor for Dawn of the Tiberium Age (DTA) https://www.moddb.com/mods/the-dawn-of-the-tiberium-age

## Motivation

The purpose of the project is to develop a new scenario editor for Dawn of the Tiberium Age, Tiberian Sun and Red Alert 2,
replacing the old FinalSun map editor developed in the TS/RA2 modding community in the early 2000s.
I think it has been sad to see the ancient map editor restrict the community as much as it has done,
and instead of developing a proper replacement for it (which could be achieved relatively quickly),
the community has instead started writing complex hacks for the old editor by reverse-engineering it,
modifying its executable and injecting code into the editor in the form of custom DLLs.

I personally think it'd be much better in the long term to focus these efforts on building a new editor
instead of modifying one that has no available source and that is based on a very outdated technological base.

To make it familiar for existing mappers, the editor is designed to follow the FinalSun UI design,
but with modernizations and changes to make the editor smoother and more efficient to use.

## Current state of the project

The editor has two operating modes: flat and non-flat.

Flat mode is meant for mods like DTA that do not make use of height levels. For these
mods, the editor covers almost everything of what can be done with FinalSun.
All basic and commonly used editing tools are included and they're generally more
efficient than the FinalSun equivalents. This mode is most complete as it was the original
purpose that the editor was created for.

For non-flat maps of TS and RA2, the editing tools are still work-in-progress. The scripting
side is completely ready and can be used for efficient mission scripting,
but editing tools that deal with terrain height are still being worked on.

There are also some limitations in the editor, particularly on the rendering side of things.
The current map renderer, while functional and usable, is very primitive and does not
properly keep track of areas to refresh, which can lead to graphical glitches or poor performance at times.
The current renderer should be considered more as a proof-of-concept of DTA/TS/RA2 map
rendering done with MonoGame than an actual serious implementation.

As the entire editor is currently work-in-progress, the code-base is so as well.
If you browse the code, you might run into unfinished feature implementations.

## System requirements

This editor uses MonoGame with Rampastring's custom XNAUI library. 
A DirectX11 compatible GPU and a 64-bit system is required.
If you absolutely need a 32-bit build, you can modify the source to produce one.

The editor needs considerable VRAM, as the sprite graphics of the game are currently
converted into full 32-bit ARGB textures prior to drawing. In case of
Dawn of the Tiberium Age, the editor appears to allocate roughly 500 MB of VRAM.

The renderer for non-flat worlds is currently also heavy on both the CPU and GPU.

## Downloads

There is a build workflow on new commits being pushed, which allows you to download the editor from the build artifacts:
https://github.com/Rampastring/TSMapEditor/actions

Aside from that, there is currently no publicly hosted download other than the editor being included with Dawn of the Tiberium Age.
If you need a separate download, you can contact me and ask for one on DTA's Discord server: https://discord.gg/6UtC289

## License

The editor is licensed under the GNU General Public License, version 2.
If you create and publish a derivate, you need to also release your source code for the fork.
Please see LICENSE.txt for more details.

## On remaining TS&RA2 features

As the editor was originally developed for DTA, it is most feature-complete for DTA's purposes.
However, Tiberian Sun and Red Alert 2 features are being worked on, particularly height support.

Regarding voxel graphics of TS&RA2, I don't have any plans of implementing support for them right now.
However, I'll gladly accept contributions and provide assistance
with implementation if someone in the community wants to adopt this editor for TS or RA2/YR and
is willing to write the required code for voxels.

## Screenshot

![Screenshot of the editor](https://github.com/Rampastring/TSMapEditor/raw/master/mapeditor.jpg "Map Editor Screenshot")

## Introduction video

[![Dawn of the Tiberium Age Scenario Editor Introduction](https://github.com/Rampastring/TSMapEditor/raw/master/videopreview.jpg)](https://www.youtube.com/watch?v=jIcr3nCqx7M "Dawn of the Tiberium Age Scenario Editor Introduction")

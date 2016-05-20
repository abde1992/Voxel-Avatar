
Implementation of the Marching Cubes algorithm to generate a Voxel Terrain of arbitrary dimensions, using C# and Unity3d game engine. This repo contains the entire Unity3d project files.

The Voxel Terrain is divided into (npart^3) number of VTPart objects, where npart is an attribute of the VoxelTerrain object. It can be edited through the inspector.
Each VTPart is divided into a number of chunks (VTChunk) which are volumetric regions and contain the actual mesh of that region.
Also contains editor scripts to edit the Voxel terrain in the editor.

To run:
Open the "Main Scene.unity" file.
OR
Add the VoxelTerrain.cs script to any Gameobject. Click on Init button in the Unity Inspector to initialize.
In the editor, you can select a part of the terrain by using the keyboard. Then, click on "Make Part" to generate a voxel terrain in that region.

This implementation generates terrain in parts and chunks, as then an update operation on a region (at runtime, for instance) will cost less, as it will only have to update the mesh specific to that region.


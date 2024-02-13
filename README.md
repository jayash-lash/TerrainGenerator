# Terrain Generator Project

This project is a system for generating a game world in Unity using noise texture to determine the landscape and biomes for environmental diversity.

## Key Components of the Project:

### TerrainGenerator:
- Responsible for generating the world and distributing blocks according to predefined biomes and noise texture.
- Utilizes a noise texture to determine the height of each point in the world.
- Iterates through each point in the world and selects the block type based on its height and biome color.
- Identifies locations for spawning objects in the world based on the biome and other factors.

### BlockDataBase:
- Represents the block database used to store information about various types of blocks in the game.
- Data is used during world generation to determine block types in different parts of the world.

### BlockInfo and BlockInfoSides:
- Provide information about specific block types in the game.
- Contains data about the block type, its texture, sound effects, and other characteristics.

## World Generation Process:
<p align="center">
  <img src="https://img001.prntscr.com/file/img001/LnNaSciWSW2c-3WOi-ROAw.png" width="200" />
  <img src="https://img001.prntscr.com/file/img001/4CBywu6TQBGhSdUAnp9FnA.png" width="200" /> 
  <img src="https://img001.prntscr.com/file/img001/N7Es3selTu-hiw2VPUwHdw.png" width="200" />
</p>



### Loading Noise Texture:
![](https://img001.prntscr.com/file/img001/VbvnLzQJTb-s-31Web1KDQ.png)
- The project starts by loading a noise texture, which will be used for world generation.
- This texture determines the height of each point in the world.

### Defining Biomes:
![](https://img001.prntscr.com/file/img001/VbvnLzQJTb-s-31Web1KDQ.png)
- Users define different biomes in the TerrainGenerator object inspector.
- Each biome is represented by a unique color, primary block type, and additional parameters such as object types for spawning.

### Processing Each World Point:
- The TerrainGenerator script iterates through each point in the world and uses the noise texture to determine its height.
- It then determines the biome color of that point and selects the block type based on this information.

### Object Spawning:
- For each world point representing a vertex, TerrainGenerator determines if any objects should be spawned.
- If so, a random object from the spawnable list is chosen, and it's spawned at that point.

## Using the Project:
1. Load the project in Unity.
2. Create a noise texture for use in world generation.
3. Configure biomes and noise texture in the TerrainGenerator object inspector.
4. Edit block types and their characteristics in the BlockInfo and BlockInfoSides scripts.
5. Run the project to see world generation in action.

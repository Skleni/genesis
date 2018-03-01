## The Genesis Project - A Cold War Crisis Map Generator

The goal of Genesis Project is to create an extensible, customizable random map generator for the real-time strategy game Cold War Crisis.

[http://www.cold-war-crisis.de](http://www.cold-war-crisis.de)

CWC is based on Command & Conquer Generals: Zero Hour by EA which comes with a powerful map editor called Worldbuilder. This editor enables players to create their own single- and multiplayer maps, and while that can be fun and rewarding all by itself, it also requires quite a lot of experience, patience and time.

Creating a map for CWC manually involves (among other things):
* Creating the landscape
* "Painting" the landscape with textures
* Adding objects such a trees, rocks and buildings
* Adding player positions and resources
* Adding areas and waypoints needed for the AI

Creating a complete map can easily take several hours.
The Genesis Project aims to create a tool which can automatically create fully playable maps that can be loaded in the game without any further manual editing.

The generator should have the following characteristics:
### Customizability
The user should be able to define the setting of the generated map (climate, number of mountains, ...). Check the [wiki](https://github.com/Skleni/genesis/wiki) for advanced customization options.
### Modularity
The generation process consists of many different steps. It should be possible to implement each of these steps independently.
### Extensibility
It should easily be possible to add or modify such steps to the generation pipeline. For example, initially there are no lakes on the generated maps, but it should be possible to implement an additional step that adds lakes.
### Reusability
While the main goal of the project is to create a map generator for CWC, large parts of the code also apply to Zero Hour maps (or possibly even other related games based on the SAGE engine). The code should be structured in a way so that these parts can easily be reused or for other tools that might not necessarily target CWC alone.


![](https://cdn.rawgit.com/Skleni/genesis/master/Images/Home_genesis.png)
A variety of parameters such as the general scenery or number of mountains and trees can be adjusted.


![](https://cdn.rawgit.com/Skleni/genesis/master/Images/Home_wb.jpg)
The final map generated with the settings above can be played immediately without further ado.


# Unity Turn Based Strategy Game
This was a project that I made in order to understand how one would approach making a turn based strategy game. This project is a stripped down version of the completed game named [Skeletons](https://angusfan.itch.io/skeletons)!
![skeletonsGameplayReduced](https://user-images.githubusercontent.com/33101170/142507405-30fc89fb-6269-4f43-a07a-325f963b2434.gif)
If you would like to hear how I programmed and designed certain aspects you can follow this video [here](https://www.youtube.com/watch?v=MNSQWPhalGQ).
If you would like to download the Unity Project please download the **projectFiles.Zip** file and open it in Unity (you may need an older version).
# Requirements 💻
- Unity Version 2019.2.7f2 or above (this project was oringally built in 2019.2.7f2) it may be incompatible with earlier versions.
# Installation & Set-Up ⚙️
- Download the **projectFiles.Zip**, and unzip the file.
- Open the project by selecting the un-zipped directiory with Unity Hub.
- Once the project is open you may find yourself in an empty scene. Open the project files and look for the **firstLevel** scene. You can find it by following this path Assets/Scenes/firstLevel. Once located, double click the scene.
![skeletonsSetup](https://user-images.githubusercontent.com/33101170/142502691-5ee084d0-ebfc-4c23-8218-2e342abd6496.gif)
# Modifications 🛠️
I get a few questions on the youtube video in regards to modification. I'll cover some of the basics in this section.
## Map & Tiles 🟩
While in the scene view, you may notice that tiles don't appear. This is because they are created during run time, by selecting the **gameManager**  gameObject in the hierarchy  you can see **tileMap** script in the inspector. Under this script you will find a list of **tileTypes** , if you want to modify the tiles or add additional ones this is where you can make the changes.

![tiles](https://user-images.githubusercontent.com/33101170/142509123-7b03ec4a-d3d0-44d5-b796-86cc67d78288.PNG)

The **tileMap** script includes most if not all of the code required to generate the map. You can change the map size and other variables in the inspector. In terms of map generation, you will need to open the script in your IDE of choice to modify the information. Look for the **generatMapInfo()** function within the **tileMap** script, here you will see that the map is by default generated with dirtBlocks which is element 0 of **tileTypes**. Afterwards change whichever tile[x,y] co-ordinate to your preferred tile.

![generation](https://user-images.githubusercontent.com/33101170/142509129-cf253ed5-435b-4e3f-91f5-028e9e567e20.PNG)

## Units ♟️
If you would like to make your own units, you can look at how units are setup in the **prefabs** folder. To modify a unit select it and modify the **unitScript** in the hierarchy. Team number is used to denote what team the unit is part of (we'll get into this a bit more later). There are also other properties which you can change, the name, attack damage, movement speed, etc can all be found in the inspector. 

![modify](https://user-images.githubusercontent.com/33101170/142512324-25bb1faa-489f-4421-b73a-c8a2cb9a5eb8.gif)

## Adding Units to the game
If you take a look back at the scene you may notice that the units are stored under the **unitsOnBoard** gameObject. Under this section there are two gameObjects called **Team1** & **Team2**, the units are placed under these gameObjects. If you want to add a new unit to the map, add them to their respective team and change the following in the inspector. Under the **Team Num** variable place their team number as **0 for Team1** or **1 for Team2**. The unit's X and Y values are actually set on runtime from their transform properties. If we take toonSkeleSoldier from Team1 and change it's transform values you can see it update at runTime. Please try to ensure that units spawn on their own unique tile which they can move on, you may get unwanted results otherwise.

![setupPosition](https://user-images.githubusercontent.com/33101170/142512348-774784d6-d3f2-46bc-9a93-8118d6660e8a.gif)


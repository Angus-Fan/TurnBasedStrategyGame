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

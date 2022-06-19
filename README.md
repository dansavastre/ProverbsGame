# Proverbs Game

## Description
Proverbs Game is an educational video game for Android and iOS that will help you learn and understand proverbs better! It utilizes learning methods such as spaced learning to help you learn in a gamified manner. 

The game features a single-player game mode, as well as a multi-player game mode to help you achieve your goal of studying proverbs in an engaging and entertaining manner.

The single-player game mode is played by answering questions about proverbs, their meaning, and visual representations in the form of cute images. This game mode is carried out in short sessions of 10 questions each. What questions you will receive in a session is highly dependent on your proficiency with certain proverbs. All the proverbs available to the game are split into 4 categories according to your proficiency with those specific proverbs. The categories ranked from lowest to highest are the following: Apprentice, Journeyman, Expert, Master.
In the beginning, all proverbs will be put into the Apprentice category. By answering questions about a certain proverb, that proverb will be then moved in a lower/higher category, depending on whether your answer was correct or not. The questions that you will receive about a proverb is then determined by the category it is currently in, ranging from recognizing a proverb in images for the Apprentice category to forming a proverb based on a set of given words for the Master category.
The single-player sessions should be a relaxing, but engaging way for you to practice learning proverbs.

The multi-player game mode is a cooperative fill in the blank mode. It is played in groups of 2-4 players, each player is assigned 2 proverbs with keywords removed from them and a set of possible keywords that they can fill in to the proverbs. The purpose of the game is for the players to communicate and exchange keywords between them, such that all players can complete their assigned proverbs. The game ends when all players completed all of their proverbs.

## Prerequisites
1. Since this game is intended for Android and iOS mobile devices, you need to own such a device to play the game.
2. This game was developed using the Unity game engine, so to open the project you must have [Unity](https://unity.com/) installed on your computer.

## Installation 
To install the game on your device, follow these steps:
1. Clone this repository to a desired location on your computer;
2. Navigate to the repository's location on your computer and open it inside Unity. Here is a [tutorial on opening projects in Unity](https://www.youtube.com/watch?v=XIlZNbQ8kzo);
3. Once the project is open inside Unity, open the "File" menu on the toolbar (it should be the first button on the left at the top of the window);
4. Inside the "File" menu click the "Build and Run" button.
5. Now you should be faced with a new window, asking you to select a location for your build. Save it on a desired location on your computer;
6. Once the build has finished, you can send it to your mobile device by file transfer (easiest way would be by using the USB cable of your mobile device's charger);
7. Once the file has been successfully transfered to the mobile device, open and install it;
8. After the installation completes, the game should be ready to play. Enjoy!

## usage

### Updating the database with new proverbs

The first step is creating the Excel file following the template of the excel file in Assets > Resources > Proverbs.xls. This file must then be converted into a .csv file and to do so in Excel go to File > Export > Change File Type > From the options choose CSV and then Save As to save the .csv file.

After this has been done, run the ProverbsUpdater.exe and the application should display where the .csv file should be before uploading the database. Once the .csv file is in the correct location by clicking the upload button the database will update.

The database can be accessed through the browser at: https://console.firebase.google.com/u/0/project/sp-proverb-game/database/sp-proverb-game-default-rtdb/data and in the proverbs subtree all the new proverbs have replaced the old ones.

The pictures have to be uploaded here: https://console.firebase.google.com/u/0/project/sp-proverb-game/storage/sp-proverb-game.appspot.com/files in the proverbs folder.

### Building the application
In order to build the application in Unity go to File > Build Settings. A new window will open. Make sure all 14 scenes are in the Scenes in Build tab and the first one should be called "Scenes/FirstScreen"(scene number 0). Below that there is a menu on the left where the build platform must be selected. After selecting the platform to build on click the "Switch platform" button if not already on that platform. Then by clicking "Build" the application will be built.

For building the ProverbsUpdater in Build Settings select windows as the platform and switch to it. Then in the "Scenes in Build" menu select only the "Scenes/ExcelConverter" scene and build the application.

More advanced build settings can be found in Build Settings > Player Settings.

## Support
If you need support with installing/using the application, don't hesitate to contact the development team or the course staff.

The development team can be contacted via the following

The course staff can be contacted via the following

## Contributing
This video game is not yet in its final state, so it is open for contribution from anyone who wishes to help!

One way of contributing is by providing new proverb entries to the game, or even sets of proverbs in other languages. The database that is used for the current build is scalable and easy to manipulate, but rather small. If you wish to contribute with new proverb phrases, for each of them you also need to provide its meaning, an example of usage, a fun fact about the proverb, an accompanying image, as well as some other wrong phrases, meanings, examples, keywords. These will all be added to the possible question types that a player might be faced with.

Other ways of contributing is by adding new functionality to the game. One of the functionalities that the development team thought of doing was to add a second multi-player game mode, a cooperative meaning matching game mode that should be similar in style to the already-existing multi-player mode. Another possible option would be adding a daily challenge mode, where players receive a more challenging question about a proverb in a daily fashion, while their streak of correctly-answered daily questions is being shown as an incentive to keep playing. The possibilities are endless.

But even if you don't feel like getting your hands dirty with modifying the game yourself, you can contribute by testing the game and providing feedback to the team. For this, I refer you back to the previous section on Support.

## Authors and acknowledgment
This video game was created by a small team of 2nd year students at the Delft University of Technology in the Netherlands as part of the Software Project course.

The project was created for the client Otilia Ramos, with the help of the Teaching Assistant Ana Băltărețu and the Coach Rafael Bidarra.

The members of the development team
1. Dan Savastre
2. Elvira Voorneveld
3. Ferhan Yildiz
4. Shayan Ramezani
5. Vlad Iftimescu

## License
All rights of this project are being reserved by the Delft University of Technology in the Netherlands.

## Project status
The product intended for the Software Project course has been completed, but the game can be further worked on by the development team.

# Specification for Adv. C# Final Project

## Project (Game) Description

My final project is going to be a 2D factory-like strategy game called **Car Factory Architect**, inspired by a game called [**Shapez**](https://shapez.io/). The point of this game is to utilize raw resources accross the map, transport them using belts and use various components to create more complex materials or car parts. Unlike Shapez, Car Factory Architect will introduce currency for any material sold (the more complex, the more expensive). For this currency, the player will be able to upgrade the efficiency of many factory components, as well as unlock new crafters, blueprints or other areas of a map. The goal of the game is to unlock every blueprint, crafter, proccessor, extractor possible to create the final object, a car (and make the development process as efficient as possible).

## Technical Aspect

For the development of this game I want to use a C# framework called [**MonoGame**](https://monogame.net/) using Visual Studio 2022. The reason why I chose this framework, is that is a light-weight framework which only provides the bare essentials for game development, such as a working game loop and basic rendering/input support, which gives me a chance to utilize my knowledge gained from both courses Programming in C#, and Advanced Programming in C#, for instance, in form of making this game run on multiple threads, optimize it and make it thread-safe. I want to make the code as expandable and modular as possible, for possible future expansion of the game to be easier.

## How to play

In case of how I want the game to be played, is very similar to before mentioned Shapez. The view will be from the top, and the entire map will be grid-like. A player will select what structure to place on the grid, and will do so by clicking on a specific place in the grid.

## GUI

I want to make a simple main menu screen, where an option to start a new game or continue playing will be showed. Then a pause menu which will be available during gameplay. The art-style of the game will be similar to that of Shapez, however, perhaps more pixelated (less pixels per texture).

## Music

Every scene in the game will be accompanied by sounds and/or music, which I will create using FL Studio. To import the sounds, I will use MonoGame's built in option to add music/sounds.
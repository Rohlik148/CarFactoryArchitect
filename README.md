# Car Factory Architect

A game inspired by **Shapez**. Started as a UNI project for Advanced Programming in C#.

Extract resources, craft other resources needed and car parts, assemble car parts to create a car and sell it.

# Simple Guide

To begin, place an **extractor** on any ore tile. Connect a conveyor to this extracor.

Move items with conveyors to other machines, such as smelter, forge,...

Move 2 or more items to assemblers to create car parts.

At any point, car parts or car itself can be sold via **Seller**

# Controls

- **1** -> switch to conveyors
- **2** -> switch to machines

- **R** -> rotate conveyor/machine
- **T** -> cycle between machine types

# Recipes

- **Smelter**
    - Raw Iron Ore -> Iron Ingot
    - Raw Copper Ore -> Copper Ingot
    - Raw Sand Ore -> Glass

- **Forge**
    - Iron Ingot -> Iron Plate

- **Cutter**
    - Copper Ingot -> Copper Wire

- **Assembler**
    - Wheel <- Iron Ingot + Raw Rubber Ore
    - ECU <- Iron Plate + Copper Wire
    - Chassis <- Iron Ingot + Wheel
    - Engine <- ECU + Iron Ingot
    - Car <- Engine + Chassis + Glass

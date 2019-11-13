# Final Project Proposal 


# Names

Nicholas Maltbie

Samuel Buzas

Saylee Dharne


# Background for Final Project

**Background and Motivation: Discuss your motivations and reasons for choosing this**

**project, especially any backgrounds or research interests that may have influenced your**

**decision. Discuss potential mentors or advisors for your project.**

Our primary motivation is to learn more about Unity, Compute Shaders, and how to apply these. These ideas are inspired by Sebastian 
Lague's YouTube channel [https://www.youtube.com/channel/UCmtyQOKKmrMVaKuRXz02jbQ](https://www.youtube.com/channel/
UCmtyQOKKmrMVaKuRXz02jbQ) and all focus on general simulation of a dynamic and realistic environment. We are all interested in concepts
associated with making these environments as well as game development. These concepts have all already been implemented before but we 
want to look into understanding them and implementing them in a highly parallel environment and calculating the levels of improvement. 

Potential Mentors/Advisors:
*   Dr. Annexstein
*   Dr. Bhatnagar 

Languages Used:
*   C# : for scripting in Unity
*   HLSL (High-Level Shader Language) : for GPU processes (similar to Direct X languages)

Reference Material:

Coding Adventure: Clouds — [https://www.youtube.com/watch?v=4QOcCGI6xOU](https://www.youtube.com/watch?v=4QOcCGI6xOU)

Coding Adventure: Hydraulic Erosion — [https://www.youtube.com/watch?v=eaXk97ujbPQ](https://www.youtube.com/watch?v=eaXk97ujbPQ) 

Coding Adventure: Marching Cubes — [https://www.youtube.com/watch?v=M3iI2l0ltbE](https://www.youtube.com/watch?v=M3iI2l0ltbE)

Coding Adventure: Boids — [https://www.youtube.com/watch?v=bqtqltqcQhw](https://www.youtube.com/watch?v=bqtqltqcQhw)

Coding Adventure: Compute Shaders — [https://www.youtube.com/watch?v=9RHGLZLUuwc](https://www.youtube.com/watch?v=9RHGLZLUuwc)

Ray Tracing Article: [http://blog.three-eyed-games.com/2018/05/03/gpu-ray-tracing-in-unity-part-1/](http://blog.three-eyed-games.com/2018/05/03/gpu-ray-tracing-in-unity-part-1/)


# Application-level Objectives

**Discuss your goals for this project, what you would like to accomplish as a result.**

**List the benefits of your end product. List the must-have features and functionalities for the project. List the optional features and functionalities for the project.**

The goals for our project are to:



*   Learn about using Unity and create a fun project
*   Create version of code in parallel and in sequential
*   Create a procedurally generated terrain with
    *   Dynamic HeightMap
    *   Clouds 
    *   Birds (boids)
*   Stretch Goals:
    *   Trees
    *   Coloring Terrain and adding grass
    *   Overhangs
        *   No Floating Islands
    *   Interacting erosion, clouds and birds
    *   Infinite generation

As a result, we would like to learn about computer graphics and Unity. The benefits of our end product is that we get to learn about procedural terrain generation and apply parallel computing concepts. We also get a pretty terrain out of this project.


# Design Overview

**Draw a block diagram of the overview of the project, including any preliminary flow charts of your algorithms and discuss where you believe the parallelism in this algorithm is. Break down your project into components you plan to have assigned to host and device/kernels, list them and describe the interface, inputs and outputs of each module and what it accomplishes.**

Components:

*   Terrain Generation
    *   Inputs:
        *   Noise Function
        *   Erosion Configuration
        *   Dimensions
        *   Erosion Iterations
    *   Outputs:
        *   Generated heightmap
        *   Texture, bump, AO maps
        *   Mesh with generated values 
*   Boids
    *   Inputs:
        *   Number of boids
        *   Boid behaviour
        *   Surrounding Terrain
    *   Outputs:
        *   Set of boids that move based on terrain and neighbors
*   Clouds
    *   Inputs:
        *   Volumetric noise function
        *   Shader rendering function
        *   Cloud generation settings
        *   Position and speed
        *   Terrain heightmap
    *   Outputs:
        *   Rendered clouds
        *   Volumetric plot of cloud information


## Block Diagrams


### Interaction Overview

![Diagram of Interaction Overview with three components and under interface. The three components are Terrain Generation, Boids, and Rendering Clouds. There are arrows from Terrain Generation to Boids and from Terrain Generation to Rendering Clouds. The Three components and two arrows are all in a box Labeled Dynamic Environment. There is an arrow going to and from User Interface to Dynamic Environment with the label Configures and Interacts with.](Images/InteractionOverview.png)


### Control Flow

![There is a set of boxes in different rows. The first row has a box called Configure Settings with an arrow pointing to User Interactions. The second row has boxes labeled Uer Interactions and Generate Terrain. The User Interactions box is connected to Generate Terrain and Movement boxes. The Third Row has boxes Movement, Generate Clouds and Generate Boids. All three of those boxes have arrows pointing towards Environment. Environment has arrows pointing towards Render Clouds and Render Boids. Render Clouds and Render Boids are in the last row with a loop around them.](Images/ControlFlow.png)


### Terrain Generation

![Terrain Generation Diagram](Images/TerrainGeneration.png)

![Generating Section of Terrain Diagram](Images/GenerateSectionTerrain.png)

![Eroding Terrain Diagram](Images/ErodeTerrain.png)

### Boids

![Boids Control Flow Diagram](Images/Boids.png)

### Render Clouds

![Render Clouds Diagram](Images/RenderClouds.png)

# Performance Goals and Validation

**Set a minimum performance requirement for what your application is trying to accomplish. It should (ideally) also be a non-trivial speedup over currently available solutions. Where are the potential bottlenecks in terms of your block diagram in Part 3? What does it take to achieve this performance goal? What are the new capabilities or new level of result quality that was not possible before? Provide a testing procedure of how you would test your program to make sure its operation is robust and correct. In case that the accuracy may be compromised in order to use the single precision hardware and/or to achieve a higher level of parallelism, provide an acceptance test for your output. Think about how you would convince someone that the result of this program is sound and trustworthy.**


## Overview

Hopefully our goal is to be able to quickly generate a terrain heightmap during runtime for the boids to explore this and allow the environment to be rendered at at least 32 frames per second on a computer with a graphics card.

Any other goals are beyond that are purely stretch goals if we are ahead of schedule for the previous sections of the project.


## Bottlenecks

Going to split the process as the bottlenecks are different for each process (the three main processes we hope to implement). 


### Terrain Generation

Possible bottlenecks:

* **Noise Function** 
    * The noise function might be difficult to calculate serially, thankfully this can be spread out to work over a GPU and a map function.

* **Creating Mesh**
    * Making the mesh involves creating a set of vertices, faces, textures and associated information from configuration settings.

* **Simulating Erosion**
    * Erosion can be done using raindrops and these raindrops will interact with the environment as they move so it will be interesting to evaluate how their changes to the environment will affect each other when running in parallel. It may be that the changes are small and they can be mostly avoided but it will be interesting nonetheless. 



### Boids

Possible bottlenecks:



* **Finding Neighbors** 
    * Finding neighbors will be interesting whether it is represented in cells, distance matrices, or a tree of neighbors. Moving it to run in parallel will be an interesting challenge. 
* **Updating State**
    * After the boids have all their information calculated, they need to decide what to do, then update their state accordingly. Avoiding conflicts may cause collisions especially when run in parallel if these steps are not split into batches properly. 

### Clouds

Possible bottlenecks:

* **Updating State**
    *   Rendering the clouds should be straightforward with a shader script but this might prove to be a bottleneck if the clouds need to change before each render as many render processes will be waiting on the update function to finish. This could be fixed by making the update run independently of the render but sorting out all those steps will be an interesting challenge

## Requirements for Performance

Theoretically, this should run in real time as in a minimum of 27-32 frames per second and update in a way that looks continuous to the human eye. Also, it should be able to run on a decent computer with a graphics card. 

In order to achieve this performance, we would want to be able to run in real time, we would have to overcome most of these bottlenecks at least enough so that when it runs it can fool humans into thinking it is natural movement. 


## New Capabilities

With this new set of parallel scripts and procedures, we will be able to generate and interact with this type of system in real time. Many of these systems exist to generate terrain, clouds, environment… but most of them only work when run beforehand. We hope to be able to run this terrain generation and cloud generation within a few seconds to minutes as part of a load time and then render and update the environment in real time. 

This isn’t a grand goal or far beyond what has already been done but hopefully it will be enough to make a good improvement and help us a team to learn more about these systems and Unity’s capabilities. 


## Testing Procedure

Since we are using Unity for the project, this should be able to be exported to multiple systems seamlessly so we could evaluate the performance on different computers with various types and strengths of graphics cards.

For our testing procedure, we will be running all of these systems in both parallel and serial on the same device with a graphics card. We will take the results of interacting with the environment and save performance statistics for later comparison. To make it a bit interesting, we could also try running this on a computer without a discrete graphics card and only integrated graphics on the CPU and evaluate if there is any performance improvement. 


## Levels of Parallelism

We can vary how parallel the process is and which parts of the processes are being run in parallel through code and configuration. Through this configuration and code, we can develop a system to determine what works best for a given system and allow an end user to select between various settings for the best performance. 


## Conclusions

In order to convince someone that our work has improved the speed of the system, we will be running it in both serial, partially parallel, and fully parallel. We will collect statistics about how long each component of each module takes to perform and compare them to how they run in parallel or serialy. Then hopefully we can show that we can simulate larger and more interesting environments when we run the program in parallel. 


# Schedule and Division of Work

**Make sure that you plan your work so that you can avoid a big rush right before the final demonstration deadline. Also, make plans so that you can have a few generations of implementations; each should be successively better than its predecessor. It would be a good idea to use this to delegate different modules and responsibilities among your team members. You will not necessarily be graded on how true your schedule is to final reality, but failure to plan is equivalent to planning for failure. Describe the division of work. What will each member of your team be responsible for? Make sure that you can self-access, and that each member can justify the work that is claimed.**

Task division



*   Dynamic Heightmap — Nick
*   Clouds — Nick
*   Birds (boids) — Saylee, Sam

|Task|Duration|People|
|----|--------|------|
|Project Proposal|10-27-19 to 11-6-19|Nick, Sam, Saylee|
|Setup environment|11-3-19 to 11-6-19|Nick|
|Read and watch reference material|11-3-19 to 11-6-19|Nick, Sam, Saylee|
|Setup Serial Hydraulic Erosion|11-6-19 to 11-13-19|Nick|
|Setup Basic Serial Environment for Boids|11-6-19 to 11-16-19|Sam, Saylee|
|Setup Parallel environment for Erosion and height maps |11-10-19 to 11-16-19|Nick|
|Setup Parallel Environment for boids|11-13-19 to 11-23-19|Sam, Saylee|
|Setup Serial and shader Environment for clouds|11-13-19 to 11-23-19|Nick|
|Combine sections of programs|11-16-19 to 11-27-19|Nick, Sam, Saylee|
|Make Interactive User for environment|By 11-27-19|Saylee|
|Setup Interactive configuration values|By 11-27-19|Sam|
|Test and evaluate system|11-27-19 to 12-1-19|NIck, Sam, Saylee

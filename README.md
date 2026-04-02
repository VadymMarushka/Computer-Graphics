# Computer Graphics

This repository contains my academic projects for the Computer Graphics course at Lviv Polytechnic National University. The focus is on building interactive desktop applications using **C#** and **WPF/WinUI 3**, implementing rendering algorithms, and managing local data persistence.

## 🚀 Projects Overview

| Project | Description | Tech Stack |
| :--- | :--- | :--- |
| [**01. Geometric Shapes Visualizer**](./01_SquaresCircles) | A WPF application for real-time rendering of squares and their inscribed/circumscribed circles. Features automated coordinate scaling, SQLite data persistence with cascading deletes, and Dependency Injection. | C#, WPF, Entity Framework Core, SQLite, ScottPlot, DI |
| [**02. Bezier Curve Editor**](./02_BezierCurves) | An interactive editor for drawing and manipulating Bézier curves. Implements mathematical matrix and parametrical formulas calculations for curve rendering and utilizes modern UI components. | C#, WPF, MVVM, WPF-UI, Math Algorithms |
| [**03. Fractals Visualizer**](./03_Fractals) | A WinUI 3 application that lets you construct some of algebraic and geometric fractals and then save them to a local SQLite gallery! Uses ComputeSharp, Escape Time Algorithm and Inigo Quilez gradient pallete formula for rendering and painting algebraic fractals. ScottPlot is used for drawing geometrical fractals. Fancу additional features such as chromatic color pallete for algebraic fractals and evolution mode for geometrical fractals are implemented. | C#, ComputeSharp, WinUI 3, MVVM, Entity Framework Core, SQLite |

---

## 📸 Previews

### 01. Geometric Shapes Visualizer

<img width="1920" height="1017" alt="creen" src="https://github.com/user-attachments/assets/67690d26-570a-4cc4-a7b4-9477cca77701" />

### 02. Bezier Curve Editor

<img width="1920" height="1027" alt="screen" src="https://github.com/user-attachments/assets/22e5f5c2-f574-410f-863c-b5b43f558205" />
<img width="1920" height="1040" alt="screen2" src="https://github.com/user-attachments/assets/4b92856e-b210-4f81-b5f1-a744a6288bc0" />

### 03. Fractals Visualizer

<img width="1920" height="1030" alt="fractals1" src="https://github.com/user-attachments/assets/53b9fe5b-158b-4aa5-9219-05ebc5028505" />
<img width="1920" height="1032" alt="fractals2" src="https://github.com/user-attachments/assets/bf814236-1729-428c-b92c-c7807fce2894" />
<img width="1920" height="1031" alt="fractals3" src="https://github.com/user-attachments/assets/fd1f4897-cbbd-4603-9325-a6d2beb4aabc" />


## 🛠️ How to Run
1. Clone the repository: `https://github.com/VadymMarushka/Computer-Graphics`
2. Open the respective `.slnx` or `.sln` file in **Visual Studio**.
3. Restore NuGet packages.
4. *For Project 01 and 03:* The SQLite database is created automatically upon launching the application via EF Core `EnsureCreated()`.
5. Build and Run the project.

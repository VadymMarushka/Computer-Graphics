# Computer Graphics & WPF Projects

This repository contains my academic projects for the Computer Graphics course at Lviv Polytechnic National University. The focus is on building interactive desktop applications using **C#** and **WPF**, implementing rendering algorithms, and managing local data persistence.

## 🚀 Projects Overview

| Project | Description | Tech Stack |
| :--- | :--- | :--- |
| [**01. Geometric Shapes Visualizer**](./01_SquaresCircles) | A WPF application for real-time rendering of squares and their inscribed/circumscribed circles. Features automated coordinate scaling, SQLite data persistence with cascading deletes, and Dependency Injection. | C#, WPF, Entity Framework Core, SQLite, ScottPlot, DI |
| [**02. Bezier Curve Editor**](./02_BezierCurves) | An interactive editor for drawing and manipulating Bézier curves. Implements mathematical matrix and parametrical formulas calculations for curve rendering and utilizes modern UI components. | C#, WPF, MVVM, WPF-UI, Math Algorithms |

---

## 📸 Previews

### Geometric Shapes Visualizer

<img width="1920" height="1017" alt="creen" src="https://github.com/user-attachments/assets/67690d26-570a-4cc4-a7b4-9477cca77701" />

### Bezier Curve Editor

<img width="1920" height="1027" alt="screen" src="https://github.com/user-attachments/assets/22e5f5c2-f574-410f-863c-b5b43f558205" />
<img width="1920" height="1040" alt="screen2" src="https://github.com/user-attachments/assets/4b92856e-b210-4f81-b5f1-a744a6288bc0" />

---

## 🛠️ How to Run
1. Clone the repository: `https://github.com/VadymMarushka/Computer-Graphics`
2. Open the respective `.slnx` or `.sln` file in **Visual Studio**.
3. Restore NuGet packages.
4. *For Project 01:* The SQLite database is created automatically upon launching the application via EF Core `EnsureCreated()`.
5. Build and Run the project.

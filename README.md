# Adaptive Pathfinding and Decision Systems

A C# system for generating maze environments, benchmarking deterministic pathfinding against reinforcement learning, and analysing how agent behaviour changes under different structural and reward conditions.

---

## Overview

This project is an interactive environment for testing and comparing two different approaches to sequential decision-making:

- **deterministic search**, using A* to compute an optimal path with full knowledge of the maze
- **adaptive learning**, using Q-learning to learn behaviour through repeated interaction and reward feedback

The system allows the user to generate mazes with different algorithms, run search and learning agents inside the same environment, and observe how performance changes as the maze structure and learning parameters are adjusted.

Although implemented as a grid-based maze system, the project models a broader class of problems involving navigation, optimisation, and decision-making in environments where the structure matters.

---

## What the System Does

At a practical level, the project allows a user to:

- create a new maze using one of multiple maze generation algorithms
- visualise the generated layout in real time
- run an A* agent to compute an optimal path from start to goal
- run a Q-learning agent to repeatedly explore the same environment and update its policy over time
- compare learned behaviour against the A* baseline
- adjust parameters that affect environment difficulty and learning behaviour
- store user and performance data in a SQL-backed system

This turns the project from a simple maze solver into a controlled comparison system for algorithmic behaviour.

---

## Core Functionality

### 1. Dynamic Maze Generation

The environment is not static. Each maze is generated programmatically when requested by the user.

The system currently supports multiple generation methods, including:

- **Prim's Algorithm**
- **Recursive Backtracking / DFS**

These algorithms create mazes with different structural characteristics. For example:

- different corridor lengths
- different branching patterns
- different densities of dead ends
- different path complexity between start and goal

This matters because the structure of the environment directly affects how both search and learning agents perform.

A deterministic algorithm such as A* will still compute an optimal path, but the learning agent may converge faster or slower depending on the type of maze it is exposed to.

---

### 2. A* Pathfinding Baseline

A* is used as the deterministic benchmark.

The algorithm evaluates nodes using:

- the known path cost from the start
- a heuristic estimate of remaining distance to the goal

This allows the system to compute an efficient shortest-path solution when the maze layout is fully known.

In this project, A* is important for two reasons:

1. it provides a correct baseline for what "good" performance looks like
2. it allows the learned policy to be compared against an optimal route rather than being judged in isolation

This means the system is not only showing whether the Q-learning agent improves, but also how close it gets to an optimal solution.

---

### 3. Q-Learning Agent

The learning component uses Q-learning to estimate the value of taking a given action in a given state.

For each state-action pair, the agent updates its Q-value based on:

- the immediate reward received
- the estimated value of the best future action
- a learning rate
- a discount factor

Over repeated episodes, the agent gradually improves its policy by exploring the environment, receiving feedback, and updating its estimates.

The implementation includes practical reinforcement learning controls such as:

- **learning rate**
- **discount factor**
- **exploration vs exploitation behaviour**
- **reward shaping**

Reward shaping is used to encourage useful behaviour, such as:

- moving closer to the goal
- completing the maze efficiently
- avoiding unnecessary movement or wasteful paths

This is important because naïve reward functions often lead to unstable or slow learning. A major part of the project was designing rewards that improved convergence without making the learning process artificial.

---

### 4. User Interaction and Experimentation

The project is designed to be interactive rather than purely background computation.

The user can:

- log in or register through the account system
- open the main environment
- generate a maze
- choose which algorithm or agent to run
- observe the environment visually as the system updates
- compare the behaviour of the A* and Q-learning approaches
- rerun the environment with different parameters or generation methods

This makes the project usable both as a game-like environment and as an experimentation tool.

The real value is that the user does not just see a final answer. They can observe:

- how the maze structure changes
- how the path produced by A* differs from learned behaviour
- how repeated training affects the Q-learning agent
- how parameter changes alter outcomes

---

## Technical Design

### Environment Representation

The environment is represented as a grid of cells and walls.

This representation supports:

- maze generation
- collision and movement checks
- pathfinding traversal
- reinforcement learning state transitions
- real-time rendering

Using a grid also keeps the state space interpretable, which is useful when debugging agent behaviour and comparing algorithmic performance.

---

### State and Action Model

For the learning agent, each state corresponds to the agent's current position in the maze.

The action set is based on movement between neighbouring cells, such as:

- up
- down
- left
- right

From each state, the agent selects an action, transitions to a new state if the move is valid, and receives a reward based on the result.

This gives a clear and manageable framework for Q-table updates while still allowing meaningful differences in policy quality.

---

### Episode-Based Learning

The Q-learning agent is trained over repeated episodes.

In each episode:

1. the agent begins at the start state
2. it repeatedly selects actions
3. rewards are assigned after each transition
4. Q-values are updated
5. the episode ends when the goal is reached or another stopping condition is met

Over time, the agent shifts from exploratory behaviour to more reliable path selection.

This lets the project show not just a final trained result, but the actual process of learning.

---

### Performance Comparison

The project is structured to compare algorithm performance in a measurable way.

The comparison focuses on factors such as:

- path efficiency
- number of steps taken
- convergence behaviour over repeated runs
- sensitivity to maze structure
- dependence on parameter selection

This matters because a learning agent can appear successful while still being inefficient or unstable. Comparing it to A* helps distinguish genuine improvement from superficial progress.

---

## Database Integration

The system includes SQL-backed persistence for user and result data.

This allows it to move beyond a temporary local simulation and support repeatable use.

The database layer is used for tasks such as:

- storing user account data
- validating logins
- storing score or completion records
- linking results to specific users

This adds practical software engineering value to the project by introducing persistent state, account handling, and data management rather than keeping the whole system as a single-session prototype.

---

## Security Implementation

User account handling was treated as an engineering problem rather than a placeholder feature.

The project includes secure password storage, with the original approach improved during development by replacing weaker iterative hashing with a stronger password-hashing method.

This was done to make the account system more realistic and to avoid treating authentication as a superficial add-on.

---

## Real-Time Visualisation

One of the main strengths of the project is that the system is visual.

The user can see:

- the generated maze layout
- the traversal behaviour of agents
- the difference between deterministic and learned routes
- the effect of parameter changes on learning outcomes

This makes debugging easier and also makes the comparison between algorithms much more meaningful than a text-only output.

Rather than simply printing path lengths or scores, the project exposes the process.

---

## What the User Can Change

The system is designed so that behaviour is not fixed at compile time.

Depending on the current version of the interface, the user can experiment with factors such as:

- maze generation algorithm
- maze dimensions
- learning rate
- exploration settings
- reward behaviour
- number of training runs
- agent selection

This makes the project useful as an experimentation platform rather than a single scripted demonstration.

---

## Example Workflow

A typical use of the project looks like this:

1. the user signs in or creates an account
2. the user generates a new maze
3. the user selects a maze generation method
4. the user runs A* to observe an optimal route
5. the user runs the Q-learning agent and observes early, inefficient behaviour
6. the user repeats training and watches the learned route improve over time
7. the user changes parameters or maze type and compares the outcome
8. the system stores relevant result data for later comparison

This workflow makes the project both interactive and analytical.

---

## Key Technical Ideas Explored

This project was used to explore several technical ideas in practice:

- how environment structure affects algorithmic performance
- the difference between exact search and learned decision policies
- the importance of reward design in reinforcement learning
- the trade-off between guaranteed optimality and adaptive behaviour
- how real-time visualisation helps reveal behaviour that raw metrics can hide
- how persistent data storage improves a prototype into a more complete software system

---

## Results and Observations

Across testing, several patterns emerged:

- A* consistently produced optimal or near-optimal solutions when the environment was fully known
- Q-learning improved over repeated episodes, but the speed and quality of convergence depended heavily on reward design and maze structure
- simpler mazes allowed faster convergence, while more complex branching structures produced slower and less stable learning
- tuning exploration and reward parameters had a major effect on whether the learning process remained productive

This reinforced the central idea of the project: the quality of decision-making cannot be judged separately from the structure of the environment and the assumptions of the method being used.

---

## Technical Stack

- **Language:** C#
- **Framework:** WinForms
- **Database:** SQL
- **Concepts used:** A*, Q-learning, maze generation, real-time rendering, authentication, data persistence

---

## Why This Project Is Interesting

This project is not just a maze game and not just an RL demo.

It combines:

- algorithm design
- reinforcement learning
- environment generation
- user interaction
- visualisation
- persistent data storage

The result is a system that can be used to study how different decision-making methods behave under controlled structural variation, while still being practical and interactive to use.

---
Home Form- 
<img width="895" height="548" alt="image" src="https://github.com/user-attachments/assets/485c67e4-a38c-423d-b10e-6c8d0b415538" />
Lab Mode-
<img width="1644" height="743" alt="image" src="https://github.com/user-attachments/assets/603d6bfd-073f-4704-944b-00a05d13a248" />
Challenge Mode-
<img width="1440" height="950" alt="Screenshot 2026-04-11 200601" src="https://github.com/user-attachments/assets/97f80a19-414d-4b8b-9a50-259c6d9e6187" />
Runner Role Form-
<img width="509" height="696" alt="image" src="https://github.com/user-attachments/assets/db268a16-0d21-43f9-9e0e-c06b92b56b9b" />
Login/Register Form-
<img width="865" height="480" alt="image" src="https://github.com/user-attachments/assets/e5326fa0-b01c-427e-8d45-0c9472e0dfca" />




## Future Improvements

Planned and possible extensions include:

- deeper reinforcement learning methods such as DQN
- richer state representations
- additional environment types beyond grid mazes
- more advanced evaluation metrics
- improved visual analytics for learning progression
- further performance optimisation for larger experiments

---

## Running the Project

1. Clone the repository
2. Open the solution in Visual Studio
3. Configure the database connection if required
4. Build and run the application
5. Register or log in
6. Generate a maze and select an algorithm
7. Run experiments and compare behaviour visually

---

## Repository Notes

This repository focuses on the implemented system itself: environment generation, agent behaviour, comparison logic, user interaction, and persistence.

The emphasis is on building a working platform for experimenting with navigation and decision systems, rather than presenting a one-off scripted demonstration.

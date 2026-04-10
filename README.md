# Adaptive Pathfinding and Decision Systems

A system for modelling and comparing deterministic and learning-based decision-making in dynamically generated environments.

---

## Overview

This project builds a controlled environment for analysing how different decision strategies perform under structural variation.

It addresses a core question:

> In environments that are structured but not fixed, how does learned behaviour compare to deterministic optimisation?

To explore this, the system integrates:
- dynamic environment generation
- classical search (A*)
- reinforcement learning (Q-learning)

and evaluates performance across varying conditions.

---

## System Architecture

The system is composed of three core components:

### 1. Environment Engine

Maze environments are generated dynamically using:
- Prim’s Algorithm
- Recursive Backtracking (DFS)

These algorithms produce structurally different environments, allowing controlled variation in:
- path length and density
- branching factor
- navigational complexity

This enables consistent testing across multiple environment classes.

---

### 2. Decision Engines

#### A* Search (Deterministic Baseline)

- Heuristic-based pathfinding algorithm
- Guarantees optimal solutions given full state visibility
- Used as a benchmark for efficiency and correctness

#### Q-Learning Agent (Adaptive Strategy)

- Model-free reinforcement learning approach
- Learns state-action values through iterative interaction
- Implements:
  - reward shaping to encourage efficient paths
  - exploration vs exploitation balancing
- Converges towards optimal or near-optimal policies depending on environment structure

---

### 3. Evaluation Layer

The system records and compares:

- path optimality (relative to A*)
- convergence rate of learned policies
- sensitivity to environment complexity
- behavioural consistency across runs

This enables direct comparison between:
- static optimisation
- adaptive learning

---

## Key Insights

- Deterministic algorithms consistently achieve optimal paths but do not adapt
- Learning-based agents improve with exposure but are highly sensitive to reward design
- Environment structure significantly impacts convergence speed and stability
- Trade-offs emerge between:
  - optimality
  - adaptability
  - computational cost

---

## Features

- Dynamic maze generation using multiple algorithms
- Reinforcement learning agent with configurable parameters
- Deterministic baseline for benchmarking (A*)
- Real-time visualisation of agent behaviour
- Adjustable system parameters for experimentation

---

## Technical Stack

- **Language:** C#
- **Framework:** WinForms
- **Database:** SQL (user data and performance tracking)

---

## Example Use Cases

While implemented as a grid-based environment, the system models patterns relevant to:

- autonomous navigation
- routing and logistics optimisation
- decision-making under uncertainty
- reinforcement learning experimentation

---

## Future Work

- Integration of Deep Q-Learning (DQN)
- Improved state representation for scalability
- Larger and non-grid-based environments
- Performance optimisation for real-time experimentation

---

## Running the Project

1. Clone the repository
2. Open the solution in Visual Studio
3. Build and run the project
4. Adjust parameters and observe behaviour in real time

---

## Notes

This project focuses on system design and behavioural analysis rather than domain-specific optimisation. The emphasis is on comparing approaches under controlled variation.

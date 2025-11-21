# RandomTowerDefense

A technically advanced tower defense game with AI-driven gameplay, built with Unity DOTS and ML-Agents. Created by KingsLab as a third-year university project.

## Overview

RandomTowerDefense puts a unique twist on the tower defense genre: tower types are randomly assigned when placed, creating a roguelike strategic experience. Players must adapt to RNG while using a merge system to create powerful upgraded towers.

**Development Time:** 3 months
**Engine:** Unity (DOTS/ECS Architecture)

## Key Features

### Gameplay Mechanics
- **Random Tower Assignment:** Each tower placement randomly selects from 4 types (Nightmare, SoulEater, TerrorBringer, Usurper)
- **Tower Merging System:** Combine 3 towers of the same type and rank to create one higher-rank tower (Ranks 1-4)
- **Progressive Leveling:** Towers gain experience and level up through combat
- **Collection Bonuses:** Rewards for collecting all ranks of a type or all types of a rank
- **Magic Skills System:** Meteor, Blizzard, Minions, and Petrification with upgradeable effects

### AI Training with ML-Agents
- **Dual-Agent System:** Tower builder AI vs Enemy spawner AI
- **Competitive Training:** Both agents learn to counter each other's strategies
- **Spatial Reasoning:** Quadrant-based observations for tactical decision-making
- **Reinforcement Learning:** PPO (Proximal Policy Optimization) training

### Procedural Systems
- **Map Generation:** Dynamic pillar placement with pathfinding validation
- **Inverse Kinematics:** Procedural creature leg animation without animation clips
- **Boids Flocking:** GPU-accelerated creature behavior simulation
- **Dynamic Wave Composition:** Adaptive enemy spawning based on difficulty

## Technical Architecture

### DOTS (Data-Oriented Technology Stack)
The game uses a hybrid ECS architecture combining traditional Unity GameObjects with DOTS for performance:

**DOTS-Powered Systems:**
- Multi-threaded tower targeting with spatial partitioning
- Burst-compiled collision detection
- Job-based pathfinding
- Entity component system for enemies and projectiles

**Traditional MonoBehaviour:**
- UI and player input
- Visual effects and animations
- Scene management
- Game state controllers

### Performance Optimizations
- **Burst Compiler:** Native code generation for critical systems
- **Job System:** Multi-threaded processing for combat calculations
- **Spatial Partitioning:** Quadrant-based system for efficient neighbor searches
- **Object Pooling:** Reusable entities for enemies, towers, projectiles, and VFX
- **NativeCollections:** Memory-efficient data structures

## Technologies Used

- **Unity 2021+** with DOTS/ECS
- **Unity ML-Agents** for reinforcement learning
- **Unity VFX Graph** for GPU-accelerated particle effects
- **Unity Burst Compiler** for performance optimization
- **Unity Job System** for multi-threading
- **Compute Shaders** for boids simulation
- **A* Pathfinding** with grid-based navigation

## Project Structure

```
RandomTowerDefense/
├── Assets/
│   ├── Scripts/
│   │   ├── DOTS/              # Entity Component System implementation
│   │   │   ├── Components/    # ECS data components
│   │   │   ├── Systems/       # Job-based systems (targeting, collision, timers)
│   │   │   ├── Pathfinding/   # A* pathfinding with ECS
│   │   │   ├── Spawner/       # Entity spawning systems
│   │   │   └── Tags/          # ECS entity tags
│   │   ├── Managers/          # Game state controllers
│   │   │   ├── System/        # Core systems (resource, stage, wave)
│   │   │   └── Macro/         # High-level game management
│   │   ├── Units/             # Tower, Enemy, Castle implementations
│   │   ├── Info/              # Data definitions (tower types, enemy stats)
│   │   ├── Boids/             # Flocking simulation
│   │   ├── ProcedualAnimation/ # IK system for creature locomotion
│   │   ├── MapGenerator/      # Procedural level generation
│   │   ├── Scene/             # Scene management and transitions
│   │   ├── Tools/             # Development utilities
│   │   └── Common/            # Shared utilities
│   ├── Materials/             # Rendering materials
│   ├── Prefabs/               # Game object prefabs
│   ├── Scenes/                # Unity scenes
│   ├── Shaders/               # Custom shaders
│   ├── ML-Agents/             # AI training configurations
│   └── ParticleSystems/       # VFX Graph effects
├── ProjectSettings/           # Unity project settings
└── Packages/                  # Package dependencies
```

## Game Systems

### Tower Types
1. **Nightmare (Yellow/Lightning):** Long-range, slow attack speed
2. **SoulEater (Green/Poison):** Fast attacking with area damage
3. **TerrorBringer (Cyan/Projectile):** Very slow but high damage
4. **Usurper (Red/Fire):** Very fast with area damage

### Tower Progression
- **Ranks:** 1 → 2 → 3 → 4 (merge 3 same-type towers)
- **Max Levels per Rank:** 5, 10, 20, 40
- **Experience System:** Towers level up through combat

### Skills System
- **Meteor:** Area damage with explosive impact
- **Blizzard:** Slowing effect over large area
- **Minions:** Spawn temporary ally units
- **Petrification:** Freeze enemies in place

## Setup Instructions

### Prerequisites
- Unity 2021.x or later
- ML-Agents Python package (for AI training)
- Git LFS (for large assets)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/kkingfung/KingsLab.git
   ```
2. Open the project in Unity Hub
3. Let Unity import all assets (first import may take several minutes)
4. Open the main scene: `Assets/Scenes/[MainScene].unity`

### Building
Before building for production:
1. Inactive/delete non-necessary items in the project (see `RandomTowerDefense/readme.txt`)
2. Configure build settings for target platform
3. Ensure all asset bundles are properly configured

### ML-Agents Training (Optional)
1. Install ML-Agents: `pip install mlagents`
2. Configure training parameters in `Assets/ML-Agents/`
3. Run training: `mlagents-learn [config].yaml --run-id=[name]`

## Design Patterns

- **Entity Component System (ECS):** Data-oriented design for performance
- **Manager Pattern:** Separation of concerns for game systems
- **Object Pooling:** Memory-efficient entity reuse
- **Data-Driven Design:** Configuration-based game balance
- **State Machine:** Game flow and wave progression control

## Performance Targets

- 60+ FPS with hundreds of active entities
- Mobile-ready architecture with touch controls
- Efficient memory usage through pooling and ECS
- Multi-threaded workload distribution

## Notable Achievements

- Successfully integrated DOTS with traditional Unity workflow
- Competitive multi-agent reinforcement learning
- Procedural inverse kinematics without animation clips
- Efficient spatial partitioning for real-time targeting
- Burst-compiled job systems for combat calculations
- VFX Graph integration with gameplay systems

## Credits

**Created by:** KingsLab
**Project Type:** Third-Year University Project
**Development Period:** 3 months

## License

This project is created for educational purposes as part of university coursework.

---

**Note:** This is a student project showcasing advanced Unity development techniques including DOTS, ML-Agents, procedural generation, and performance optimization.

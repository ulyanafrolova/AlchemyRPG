AlchemyRPG is a multiplayer, authoritative-server roguelike RPG built in C# (.NET 9.0). It features procedural dungeon generation, a deeply decoupled event-driven architecture, and terminal-based rendering. The system is designed with a strong emphasis on strict Object-Oriented Programming (OOP) standards and Gang of Four (GoF) design patterns to ensure extensibility and maintainability.

## Tech Stack & Core Technologies

* **Platform:** .NET 9.0 (C#)
* **Networking:** Asynchronous TCP Sockets (`System.Net.Sockets`) with non-blocking message queues (`System.Threading.Channels`).
* **Serialization:** Polymorphic JSON serialization (`System.Text.Json`) utilizing type discriminators for seamless DTO payload resolution over the network.
* **Concurrency:** Thread-safe state mutation via `ConcurrentDictionary`, `ConcurrentQueue`, and explicit locking mechanisms (`SyncRoot`) to prevent race conditions during server ticks.

## Architecture & OOP Principles

The project adheres strictly to **SOLID** principles:
* **Single Responsibility Principle (SRP):** Complete separation between Domain state (`GameState`), Network transportation (`NetworkServer`/`NetworkClient`), and UI rendering (`ConsoleView`).
* **Open/Closed Principle (OCP):** Core systems (combat, item interactions, map generation) rely on abstractions. New items, weapons, or dungeon themes can be added without modifying existing engine logic.
* **Liskov Substitution Principle (LSP):** Base classes (`Entity`, `BaseWeapon`) are seamlessly interchangeable with their derived counterparts.
* **Interface Segregation Principle (ISP):** Interfaces like `IHeavyWeapon`, `ISlottedWeapon`, and `IKinsmanDeathBehavior` ensure highly specific contracts.
* **Dependency Inversion Principle (DIP):** High-level modules depend on abstractions (e.g., `GameEngine` depends on `ILogger` and `IVisionService`, not concrete implementations).

## Design Patterns Applied

The codebase heavily utilizes GoF design patterns to solve architectural challenges intuitively and data-drivenly:

* **Visitor:** Used in `IItemVisitor`, `IAttackVisitor`, and `INetworkCommandVisitor`. 
    * *Purpose:* Eliminates fragile runtime type-checking (`is`/`as`) and massive `switch` statements. It leverages double dispatch to dynamically resolve combat calculations based on weapon categories, determine item drop noise, and route deserialized network DTOs to the correct execution pipeline.
* **Builder & Director:** Implemented via `IDungeonBuilder`, `DungeonDirector`, and `IDungeonModifier`.
    * *Purpose:* Orchestrates procedural map generation. The Director controls the pipeline, while modular modifiers (e.g., `RoomsModifier`, `CorridorsModifier`) independently mutate the grid, allowing for highly composable map algorithms.
* **Abstract Factory:** Implemented via `IThemeFactory` (`CrystalMineThemeFactory`, `LaboratoryThemeFactory`, etc.).
    * *Purpose:* Encapsulates the instantiation of biome-specific entities, loot, and artifacts, ensuring thematic consistency without hardcoding dependencies in the dungeon generator.
* **State:** Used in client input handling (`IInputState`) and enemy AI (`IEnemyState`).
    * *Purpose:* Replaces complex conditional logic. Input flows smoothly transition between states (e.g., `NormalState` -> `WaitingForAttackDirectionState` -> `WaitingForAttackTypeState`). AI state shifts dynamically based on spatial awareness (Aggressive, Cowardly).
* **Strategy:** Implemented via `IKinsmanDeathBehavior`.
    * *Purpose:* Defines interchangeable behavioral algorithms for enemies. For example, when a kinsman dies, enemies dynamically evaluate morale, resulting in either a buff (`AggressiveBehavior`) or debuff (`CowardlyBehavior`).
* **Observer:** Implemented via generic `ISubject<T>` and `IObserver<T>`.
    * *Purpose:* Decouples domain events (like `NoiseData` and `EnemyDeathData`) from their handlers. Utilizes a lock-free, Copy-On-Write (COW) list to broadcast events across threads securely.
* **Decorator:** Implemented via `WeaponDecorator` (`StrongModifier`, `UnluckyModifier`).
    * *Purpose:* Allows dynamic, runtime aggregation of weapon statistics without exponential subclass explosion.
* **Composite:** Implemented via `ISlotContainer` (`SlottedSword`, `ItemHolder`).
    * *Purpose:* Treats individual items and containers uniformly. Calculates aggregated stats recursively across the nested item tree. Prevents cyclical graphs using domain-specific Visitors (`ContainsItemVisitor`).
* **Command:** Implemented via `ICommand` (`MoveCommand`, `AttackCommand`, `DropCommand`).
    * *Purpose:* Encapsulates player intent as discrete objects. Sent from the client as DTOs, mapped to executable commands on the server, and queued for processing during the authoritative server tick.

## Getting Started

### Prerequisites
* .NET 9.0 SDK

### Build & Run
Compile and start the application via the CLI. The program acts as both a headless authoritative server and a rendering client depending on the arguments.

**1. Start the Server:**
```bash
dotnet run --server=5555

```
*Alternatively, run the executable without arguments and press S.*
**2. Start a Client:**
```bash
dotnet run --client=127.0.0.1:5555

```
*Alternatively, run the executable without arguments, press C, and enter the IP:Port.*
## Gameplay & Controls
The game operates on a real-time, tick-based authoritative server logic. Vision is calculated via raycasting, and sound propagation utilizes Breadth-First Search (BFS) distance calculations.
### Core Keybinds
 * W, A, S, D — Movement.
 * E — Pick up item from the ground.
 * X — Drop an item from inventory.
 * 0-9 — Interact with an inventory slot (equip/drop/insert).
 * Q / R — Specify Left or Right hand during equip operations.
 * C — Initiate an attack sequence (followed by direction W/A/S/D, then type 1/2/3).
 * I — Insert an item into a slotted container.
 * J — Open Adventure Journal.
 * H — View Dungeon Instructions & Lore.
 * Esc — Cancel current multi-step action.

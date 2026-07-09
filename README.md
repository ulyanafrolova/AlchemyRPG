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

## Controls
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

## Game Rules & Mechanics

The game revolves around exploring a procedurally generated, themed dungeon (Crystal Mine, Laboratory, or Greenhouse) where you must survive, fight enemies, and gather loot. The game operates on a turn-based system where every player action is processed during the server tick.

### The Goal & Game Over
* **Survival:** Your primary objective is to survive the dungeon.
* **Defeat:** The game ends for you (GAME OVER) if your health drops to 0 or below during combat. When this happens, a fatal error screen will appear stating: "You have been slain in the dungeon. Your journey ends here."

### Items & Equipment
The dungeon is filled with various items that you can pick up and store in your backpack:
* **Weapons:** Different weapons scale with different RPG statistics. 
  * *Heavy Weapons* (e.g., Swords, Axes) scale with Strength and Aggression.
  * *Light Weapons* (e.g., Daggers) scale with Dexterity.
  * *Magic Weapons* (e.g., Magic Staff) bypass physical armor and scale with Wisdom.
* **Slotted Items & Modifiers:** Some weapons and holders (like the `Slotted Broadsword`) act as containers. You can insert passive items (like the Stone of Strength, Wisdom, or Luck) into them to permanently boost your stats.
* **Currency:** Gold and Coins do not take up inventory space; they are instantly consumed upon pickup and added to your statistics.
* **Junk:** Items like Skulls, Old Bones, and Broken Glass are useless items that simply take up valuable backpack space.

### Combat System
Combat is strictly melee (limited to adjacent tiles). You can choose between three attack styles:
* **Normal Attack:** Standard damage scaling. Good for heavy and light weapons.
* **Stealth Attack:** Deals massive double damage with light weapons but halves the effectiveness of heavy weapons.
* **Magic Attack:** Extremely effective with magic weapons, but renders physical weapons almost useless (reducing damage to 1).

### Enemy AI & Ecology
Enemies are highly dynamic and react to both sight and sound (acoustic events like dropping heavy items or combat):
* **Aggressive Enemies** (e.g., Gargoyles, Flesh Golems): If they see you, they will chase and attack. If they hear a noise, they will investigate the source.
* **Cowardly Enemies** (e.g., Basilisks, Acid Slimes): They will actively try to move away from players and noises. If they are cornered (surrounded by 4 players), they will freeze in fear.
* **Neutral Enemies:** They wander randomly until provoked. If attacked, their reaction depends on their health: they become *Enraged* (Aggressive) if their HP is >= 50%, or *Panicked* (Cowardly) if their HP falls below 50%.
* **Pack Mentality:** When an enemy dies, it broadcasts a death event to others of its species. Depending on their behavior, witnessing a kinsman's death might make them enraged (+3 damage) or cowardly (-2 damage).

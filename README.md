````mermaid
classDiagram
    %% --- CORE GAME ---
    class Game {
        -GameState _state
        -bool _isRunning
        -InputHandler _inputHandler
        +Run()
    }

    class GameConfig {
        +string PlayerName
        +string DungeonTheme
        +string LogDirectory
        +Load(string path)$ GameConfig
    }

    class GameState {
        +Player Player
        +Map Map
        +string Log
        +string Instructions
        +string TutorialText
        +bool IsGameOver
        +EventManager Events
    }

    class Map {
        +int Width
        +int Height
        +char[,] Grid
        -List~IItem~ _items
        +List~Enemy~ Enemies
        +bool IsWalkable(int x, int y)
        +List~IItem~ GetItemsAt(int x, int y)
        +Enemy GetEnemyAt(int x, int y)
        +void PlaceItemAt(int x, int y, IItem item)
        +void AddItem(int x, int y, IItem item)
        +void RemoveItem(int x, int y, IItem item)
        +void Draw(GameState state)
        -char GetSymbolToDraw(GameState state, int x, int y)
    }

    class MapExtensions {
        <<static>>
        +void SpawnItemRandomly(this Map map, Random rand, IItem item)$
        +void SpawnEnemyRandomly(this Map map, Random rand, Enemy enemy)$
    }

    %% --- EVENT SYSTEM (Observer Pattern) ---
    class EventManager {
        -Dictionary~Type, List~object~~ _listeners
        +Subscribe~TEvent~(IEventListener~TEvent~ listener)
        +Unsubscribe~TEvent~(IEventListener~TEvent~ listener)
        +Notify~TEvent~(TEvent eventData)
    }

    class IEventListener~TEvent~ {
        <<interface>>
        +OnEvent(TEvent eventData)
    }

    class INoiseListener {
        <<interface>>
    }

    class IEnemyDeathListener {
        <<interface>>
    }

    class NoiseData {
        +int SourceX
        +int SourceY
        +Dictionary~Tuple, int~ ReachedTiles
    }

    class EnemyDeathData {
        +string Species
    }

    %% --- STRATEGY PATTERN (Enemy Behavior) ---
    class IKinsmanDeathBehavior {
        <<interface>>
        +React(Enemy enemy)
    }

    class CowardlyBehavior {
        +React(Enemy enemy)
    }

    class AggressiveBehavior {
        +React(Enemy enemy)
    }

    class NeutralBehavior {
        +React(Enemy enemy)
    }

    %% --- LOGGING SYSTEM (Singleton + Strategy) ---
    class ILogger {
        <<interface>>
        +Log(LogType type, string message)
        +GetFullMemoryBuffer() List~LogEntry~
        +GetRecentLogs(int count) List~LogEntry~
    }

    class FileLogger {
        -Queue~LogEntry~ _memoryBuffer
        +Log(LogType type, string message)
        +GetFullMemoryBuffer() List~LogEntry~
        +GetRecentLogs(int count) List~LogEntry~
    }

    class GameLogger {
        <<static>>
        -ILogger _instance$
        +Initialize(ILogger logger)$
        +Instance$ ILogger
    }

    %% --- ENTITIES ---
    class Entity {
        <<abstract>>
        +string Name
        +char Symbol
        +int Health
        +int X
        +int Y
    }

    class Player {
        +int Strength
        +int TotalStrength
        +int Dexterity
        +int Luck
        +int TotalLuck
        +int Aggression
        +int Wisdom
        +int Coins
        +int Gold
        +string LogMessage
        +List~IInventoryItem~ Backpack
        +IInventoryItem LeftHand
        +IInventoryItem RightHand
        +void Move(int dx, int dy)
        +void TryEquipFromBackpack(int index, HandSide side)
        +IInventoryItem DropItem(int index)
        +void EquipLeftHand(IInventoryItem item)
        +void EquipRightHand(IInventoryItem item)
        +void EquipTwoHanded(IInventoryItem item)
    }

    class Enemy {
        +string Species
        +int AttackDamage
        +int Armor
        -IKinsmanDeathBehavior _deathBehavior
        -EventManager _events
        +ModifyAttackDamage(int delta)
        +TriggerDeathProcessing()
        +MoveRandomly(Map map, Random rand, Player player)
        +OnEvent(NoiseData noise)
        +OnEvent(EnemyDeathData deathInfo)
    }

    %% --- DUNGEON BUILDER & ABSTRACT FACTORY (THEMES) ---
    class IThemeFactory {
        <<interface>>
        +GetWelcomeMessage() string
        +CreateLoot(Random rand) IItem
        +CreateArtifact() IWeapon
        +CreateEnemy(int index, EventManager events) Enemy
        +ConfigureBuilder(IDungeonBuilder builder)
    }

    class LaboratoryThemeFactory
    class GreenhouseThemeFactory
    class CrystalMineThemeFactory

    class IDungeonBuilder {
        <<interface>>
        +CreateEmpty(int width, int height) IDungeonBuilder
        +CreateFilled(int width, int height) IDungeonBuilder
        +ApplyModifier(IDungeonModifier modifier) IDungeonBuilder
        +GetMap() Map
        +GetInstructions() string
        +GetTutorialText() string
    }

    class DungeonBuilder {
        -Map _map
        -Random _rand
        -HashSet~string~ _instructions
        -List~string~ _tutorialText
        -AddBorders()
    }

    class DungeonDirector {
        -IDungeonBuilder _builder
        +ConstructThemedDungeon(IThemeFactory themeFactory, EventManager events)
    }

    class IDungeonModifier {
        <<interface>>
        +Apply(Map map, HashSet~string~ controls, List~string~ tutorialText, Random rand)
    }

    class CorridorsModifier
    class RoomsModifier
    class CentralRoomModifier
    class ThemePopulatorModifier {
        -IThemeFactory _factory
        -int _lootCount
        -int _enemyCount
        -EventManager _events
    }

    class Labyrinth {
        <<static>>
        +Generate(int width, int height)$ char[,]
    }

    %% --- COMMANDS ---
    class ICommand {
        <<interface>>
        +CanExecute(GameState state) bool
        +Execute(GameState state)
    }

    class MoveCommand {
        -int _dx
        -int _dy
    }
    class PickUpCommand
    class DropCommand
    class EquipCommand {
        -int _inventoryIndex
    }
    class HelpCommand
    class AttackCommand
    class JournalCommand

    class InputHandler {
        -Dictionary~ConsoleKey, ICommand~ _commands
        +HandleInput(ConsoleKey key, GameState state) bool
    }

    %% --- ITEMS & WEAPONS ---
    class HandSide {
        <<enumeration>>
        Left
        Right
    }

    class IItem {
        <<interface>>
        +string Name
        +char Symbol
        +void OnPickUp(GameState state)
    }

    class IInventoryItem {
        <<interface>>
        +bool IsTwoHanded
        +void Equip(Player player, HandSide side)
        +void Accept(IAttackVisitor visitor, IInventoryItem context)
    }

    class IWeapon {
        <<interface>>
        +int Damage
        +int LuckBonus
        +int NoiseRange
    }

    class BaseWeapon {
        <<abstract>>
    }

    class WeaponDecorator {
        <<abstract>>
        -IWeapon _innerWeapon
    }

    class StrongModifier
    class UnluckyModifier

    %% --- VISITOR PATTERN ---
    class IAttackVisitor {
        <<interface>>
        +VisitHeavyWeapon(IWeapon weapon)
        +VisitLightWeapon(IWeapon weapon)
        +VisitMagicWeapon(IWeapon weapon)
        +VisitNonWeapon()
    }

    class AttackVisitor {
        <<abstract>>
        #int CalculatedDamage
        #int CalculatedDefense
        #Player _player
    }

    class NormalAttack
    class StealthAttack
    class MagicAttack

    %% ==========================================
    %% RELATIONSHIPS
    %% ==========================================
    
    %% Core game
    Game *-- InputHandler
    Game *-- GameConfig
    Game --> GameState
    GameState --> Player
    GameState --> Map
    GameState *-- EventManager
    
    %% Events & Observer
    IEventListener~NoiseData~ <|-- INoiseListener
    IEventListener~EnemyDeathData~ <|-- IEnemyDeathListener
    INoiseListener <|.. Enemy
    IEnemyDeathListener <|.. Enemy
    EventManager ..> NoiseData
    EventManager ..> EnemyDeathData

    %% Entity / Strategy
    Entity <|-- Player
    Entity <|-- Enemy
    IKinsmanDeathBehavior <|.. CowardlyBehavior
    IKinsmanDeathBehavior <|.. AggressiveBehavior
    IKinsmanDeathBehavior <|.. NeutralBehavior
    Enemy *-- IKinsmanDeathBehavior

    %% Map
    Map o-- IItem
    Map o-- Enemy
    MapExtensions ..> Map

    %% Logging
    ILogger <|.. FileLogger
    GameLogger o-- ILogger
    
    %% Abstract Factory & Builder
    IThemeFactory <|.. LaboratoryThemeFactory
    IThemeFactory <|.. GreenhouseThemeFactory
    IThemeFactory <|.. CrystalMineThemeFactory
    DungeonDirector o-- IThemeFactory
    DungeonDirector o-- IDungeonBuilder
    
    IDungeonBuilder <|.. DungeonBuilder
    IDungeonModifier <|.. CorridorsModifier
    IDungeonModifier <|.. RoomsModifier
    IDungeonModifier <|.. CentralRoomModifier
    IDungeonModifier <|.. ThemePopulatorModifier
    DungeonBuilder ..> IDungeonModifier
    ThemePopulatorModifier *-- IThemeFactory
    ThemePopulatorModifier --> EventManager
    CorridorsModifier ..> Labyrinth
    
    %% Commands
    ICommand <|.. MoveCommand
    ICommand <|.. PickUpCommand
    ICommand <|.. DropCommand
    ICommand <|.. EquipCommand
    ICommand <|.. HelpCommand
    ICommand <|.. AttackCommand
    ICommand <|.. JournalCommand
    InputHandler *-- ICommand
    PickUpCommand ..> NoiseData

    %% Items
    Player o-- IInventoryItem
    IItem <|-- IInventoryItem
    IInventoryItem <|.. BaseWeapon
    IWeapon <|.. BaseWeapon
    IWeapon <|.. WeaponDecorator
    WeaponDecorator <|-- StrongModifier
    WeaponDecorator <|-- UnluckyModifier

    %% Visitor
    IAttackVisitor <|.. AttackVisitor
    AttackVisitor <|-- NormalAttack
    AttackVisitor <|-- StealthAttack
    AttackVisitor <|-- MagicAttack
    AttackCommand --> AttackVisitor
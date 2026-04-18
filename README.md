````mermaid
classDiagram
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

    %% --- LOGGING SYSTEM (Singleton + Strategy) ---
    class ILogger {
        <<interface>>
        +Log(string message)
        +GetFullHistory() List~string~
        +GetRecentLogs(int count) List~string~
        +SaveToFile(string directory, string playerName)
    }

    class FileLogger {
        -List~string~ _logs
        +Log(string message)
        +GetFullHistory() List~string~
        +GetRecentLogs(int count) List~string~
        +SaveToFile(string directory, string playerName)
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
        +int AttackDamage
        +int Armor
    }

    %% --- DUNGEON BUILDER & ABSTRACT FACTORY (THEMES) ---
    class IThemeFactory {
        <<interface>>
        +GetWelcomeMessage() string
        +CreateLoot(Random rand) IItem
        +CreateArtifact() IItem
        +CreateEnemy(Random rand) Enemy
        +ApplyThemeModifiers(IDungeonBuilder builder)
    }

    class LibraryThemeFactory
    class ForgeThemeFactory
    class VaultThemeFactory

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
        +ConstructThemedDungeon(IThemeFactory themeFactory)
        +ConstructArena()
    }

    class IDungeonModifier {
        <<interface>>
        +Apply(Map map, HashSet~string~ controls, List~string~ tutorialText, Random rand)
    }

    class CorridorsModifier
    class RoomsModifier
    class CentralRoomModifier
    class JunkItemsModifier
    class WeaponsModifier
    class EnemiesModifier

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
        +int StrengthBonus
    }

    class IHeavyWeapon { <<interface>> }
    class ILightWeapon { <<interface>> }
    class IMagicWeapon { <<interface>> }

    class BaseWeapon {
        <<abstract>>
        +string Name
        +char Symbol
        +int Damage
        +int LuckBonus
        +int StrengthBonus
        +bool IsTwoHanded
        +void OnPickUp(GameState state)
        +void Equip(Player player, HandSide side)
        +void Accept(IAttackVisitor visitor, IInventoryItem context)
    }

    class WeaponDecorator {
        <<abstract>>
        -IWeapon _innerWeapon
        +string Name
        +int Damage
        +int LuckBonus
        +int StrengthBonus
        +char Symbol
        +bool IsTwoHanded
        +void OnPickUp(GameState state)
        +void Equip(Player player, HandSide side)
        +void Accept(IAttackVisitor visitor, IInventoryItem context)
    }

    class StrongModifier
    class UnluckyModifier
    class StrengtheningModifier

    class Dagger
    class Sword
    class TwoHandedAxe
    class MagicStaff

    class Junk {
        <<abstract>>
    }

    class Skull
    class OldBone
    class BrokenGlass
    class Gold
    class Coin

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

    %% --- RELATIONSHIPS ---
    Game *-- InputHandler
    Game *-- GameConfig
    Game --> GameState
    GameState --> Player
    GameState --> Map
    
    %% Entity / Map Hierarchy
    Entity <|-- Player
    Entity <|-- Enemy
    Map o-- IItem
    Map o-- Enemy
    MapExtensions ..> Map

    %% Logging System
    ILogger <|.. FileLogger
    GameLogger o-- ILogger
    
    %% Abstract Factory System
    IThemeFactory <|.. LibraryThemeFactory
    IThemeFactory <|.. ForgeThemeFactory
    IThemeFactory <|.. VaultThemeFactory
    DungeonDirector o-- IThemeFactory
    DungeonDirector o-- IDungeonBuilder
    
    %% Builder System
    IDungeonBuilder <|.. DungeonBuilder
    IDungeonModifier <|.. CorridorsModifier
    IDungeonModifier <|.. RoomsModifier
    IDungeonModifier <|.. CentralRoomModifier
    IDungeonModifier <|.. JunkItemsModifier
    IDungeonModifier <|.. WeaponsModifier
    IDungeonModifier <|.. EnemiesModifier
    DungeonBuilder ..> IDungeonModifier
    CorridorsModifier ..> Labyrinth
    DungeonBuilder ..> Map

    %% Commands
    ICommand <|.. MoveCommand
    ICommand <|.. PickUpCommand
    ICommand <|.. DropCommand
    ICommand <|.. EquipCommand
    ICommand <|.. HelpCommand
    ICommand <|.. AttackCommand
    ICommand <|.. JournalCommand
    InputHandler *-- ICommand

    %% Items
    Player o-- IInventoryItem
    IItem <|-- IInventoryItem
    IItem <|.. Gold
    IItem <|.. Coin
    
    IInventoryItem <|.. BaseWeapon
    IInventoryItem <|.. Junk
    IWeapon <|.. BaseWeapon
    IWeapon <|.. WeaponDecorator
    IWeapon <|.. IHeavyWeapon
    IWeapon <|.. ILightWeapon
    IWeapon <|.. IMagicWeapon
    
    BaseWeapon <|-- Sword
    BaseWeapon <|-- Dagger
    BaseWeapon <|-- TwoHandedAxe
    BaseWeapon <|-- MagicStaff
    Sword --|> IHeavyWeapon
    Dagger --|> ILightWeapon
    TwoHandedAxe --|> IHeavyWeapon
    MagicStaff --|> IMagicWeapon
    
    WeaponDecorator <|-- StrongModifier
    WeaponDecorator <|-- UnluckyModifier
    WeaponDecorator <|-- StrengtheningModifier
    
    Junk <|-- Skull
    Junk <|-- OldBone
    Junk <|-- BrokenGlass
    
    IInventoryItem ..> HandSide
    IInventoryItem ..> IAttackVisitor

    %% Visitor
    IAttackVisitor <|.. AttackVisitor
    AttackVisitor <|-- NormalAttack
    AttackVisitor <|-- StealthAttack
    AttackVisitor <|-- MagicAttack
    AttackCommand --> AttackVisitor
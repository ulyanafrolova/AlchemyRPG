````mermaid
classDiagram
    class Game {
        -GameState _state
        -bool _isRunning
        -InputHandler _inputHandler
        +Run()
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
        -List~(int, int, IItem)~ _items
        +bool IsWalkable(int x, int y)
        +List~IItem~ GetItemsAt(int x, int y)
        +void PlaceItemAt(int x, int y, IItem item)
        +void AddItem(int x, int y, IItem item)
        +void RemoveItem(int x, int y, IItem item)
        +void Draw(GameState state)
    }

    class MapExtensions {
        <<static>>
        +void SpawnItemRandomly(this Map map, Random rand, IItem item)
    }

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
        +ConstructStandardDungeon()
        +ConstructArena()
    }

    class IDungeonModifier {
        <<interface>>
        +Apply(Map map, HashSet~string~ controls, List~string~ tutorialText, Random rand)
    }

    class CorridorsModifier {
        +Apply(Map map, HashSet~string~ controls, List~string~ tutorialText, Random rand)
    }

    class RoomsModifier {
        -int _numberOfRooms
        +Apply(Map map, HashSet~string~ controls, List~string~ tutorialText, Random rand)
    }

    class CentralRoomModifier {
        -int _roomWidth
        -int _roomHeight
        +Apply(Map map, HashSet~string~ controls, List~string~ tutorialText, Random rand)
    }

    class JunkItemsModifier {
        -int _count
        +Apply(Map map, HashSet~string~ controls, List~string~ tutorialText, Random rand)
    }

    class WeaponsModifier {
        -int _count
        +Apply(Map map, HashSet~string~ controls, List~string~ tutorialText, Random rand)
    }

    class EnemiesModifier {
        -int _count
        +Apply(Map map, HashSet~string~ controls, List~string~ tutorialText, Random rand)
    }

    class Labyrinth {
        <<static>>
        +Generate(int width, int height) char[,]
    }

    class ICommand {
        <<interface>>
        +CanExecute(GameState state) bool
        +Execute(GameState state)
    }

    class MoveCommand {
        -int _dx
        -int _dy
        +CanExecute(GameState state) bool
        +Execute(GameState state)
    }

    class PickUpCommand {
        +CanExecute(GameState state) bool
        +Execute(GameState state)
    }

    class DropCommand {
        +CanExecute(GameState state) bool
        +Execute(GameState state)
    }

    class EquipCommand {
        -int _inventoryIndex
        +CanExecute(GameState state) bool
        +Execute(GameState state)
    }

    class HelpCommand {
        +CanExecute(GameState state) bool
        +Execute(GameState state)
    }

    class AttackCommand {
        +CanExecute(GameState state) bool
        +Execute(GameState state)
    }

    class InputHandler {
        -Dictionary~ConsoleKey, ICommand~ _commands
        +HandleInput(ConsoleKey key, GameState state) bool
    }

    class Player {
        +int X
        +int Y
        +int Health
        +int Strength
        +int Dexterity
        +int Luck
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
        +string Name
        +char Symbol
        +int Health
        +int AttackDamage
        +int Armor
        +void OnPickUp(GameState state)
    }

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
    }

    class IHeavyWeapon {
        <<interface>>
    }

    class ILightWeapon {
        <<interface>>
    }

    class IMagicWeapon {
        <<interface>>
    }

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
        +AttackVisitor(Player player)
    }

    class BaseWeapon {
        <<abstract>>
        +string Name
        +char Symbol
        +int Damage
        +int LuckBonus
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
        +char Symbol
        +bool IsTwoHanded
        +void OnPickUp(GameState state)
        +void Equip(Player player, HandSide side)
        +void Accept(IAttackVisitor visitor, IInventoryItem context)
    }

    class StrongModifier {
        +Name
        +Damage
    }

    class UnluckyModifier {
        +Name
        +LuckBonus
    }

    class Dagger
    class Sword
    class TwoHandedAxe
    class MagicStaff

    class Junk {
        <<abstract>>
        +string Name
        +char Symbol
        +bool IsTwoHanded
        +void OnPickUp(GameState state)
        +void Equip(Player player, HandSide side)
        +void Accept(IAttackVisitor visitor, IInventoryItem context)
    }

    class Skull
    class OldBone
    class BrokenGlass

    class Gold {
        -int _amount
        +string Name
        +char Symbol
        +void OnPickUp(GameState state)
    }

    class Coin {
        -int _amount
        +string Name
        +char Symbol
        +void OnPickUp(GameState state)
    }

    class NormalAttack {
        +VisitHeavyWeapon(IWeapon weapon)
        +VisitLightWeapon(IWeapon weapon)
        +VisitMagicWeapon(IWeapon weapon)
        +VisitNonWeapon()
    }

    class StealthAttack {
        +VisitHeavyWeapon(IWeapon weapon)
        +VisitLightWeapon(IWeapon weapon)
        +VisitMagicWeapon(IWeapon weapon)
        +VisitNonWeapon()
    }

    class MagicAttack {
        +VisitHeavyWeapon(IWeapon weapon)
        +VisitLightWeapon(IWeapon weapon)
        +VisitMagicWeapon(IWeapon weapon)
        +VisitNonWeapon()
    }

    IDungeonBuilder <|.. DungeonBuilder
    DungeonDirector o-- IDungeonBuilder
    IDungeonModifier <|.. CorridorsModifier
    IDungeonModifier <|.. RoomsModifier
    IDungeonModifier <|.. CentralRoomModifier
    IDungeonModifier <|.. JunkItemsModifier
    IDungeonModifier <|.. WeaponsModifier
    IDungeonModifier <|.. EnemiesModifier
    DungeonBuilder ..> IDungeonModifier
    CorridorsModifier ..> Labyrinth
    DungeonBuilder ..> Map
    MapExtensions ..> Map
    ICommand <|.. MoveCommand
    ICommand <|.. PickUpCommand
    ICommand <|.. DropCommand
    ICommand <|.. EquipCommand
    ICommand <|.. HelpCommand
    ICommand <|.. AttackCommand
    InputHandler *-- ICommand
    Game *-- InputHandler
    Game --> GameState
    GameState --> Player
    GameState --> Map
    IItem <|-- IInventoryItem
    IItem <|.. Gold
    IItem <|.. Coin
    IItem <|.. Enemy
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
    Junk <|-- Skull
    Junk <|-- OldBone
    Junk <|-- BrokenGlass
    Map o-- IItem
    Player o-- IInventoryItem
    IInventoryItem ..> HandSide
    IInventoryItem ..> IAttackVisitor
    AttackVisitor <|.. NormalAttack
    AttackVisitor <|.. StealthAttack
    AttackVisitor <|.. MagicAttack
    AttackCommand --> AttackVisitor
    AttackCommand --> Enemy
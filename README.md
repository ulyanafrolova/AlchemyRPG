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
        -List _items
        +bool IsWalkable(int x, int y)
        +List~IItem~ GetItemsAt(int x, int y)
        +void PlaceItemAt(int x, int y, IItem item)
        +void AddItem(int x, int y, IItem item)
        +void RemoveItem(int x, int y, IItem item)
        +void Draw(GameState state)
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
        -HashSet~string~ _controls
        -List~string~ _tutorialText
        -AddBorders(int width, int height)
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
        -SpawnItemRandomly(Map map, Random rand, IItem item)
        +Apply(Map map, HashSet~string~ controls, List~string~ tutorialText, Random rand)
    }

    class WeaponsModifier {
        -int _count
        -SpawnItemRandomly(Map map, Random rand, IItem item)
        +Apply(Map map, HashSet~string~ controls, List~string~ tutorialText, Random rand)
    }

    class Labyrinth {
        <<static>>
        -Random Rand
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
        +int Amount
        +void OnPickUp(GameState state)
    }

    class Coin {
        +int Amount
        +void OnPickUp(GameState state)
    }

    class NormalAttack {
        +CalculatedDamage
        +CalculatedDefense
        +VisitHeavyWeapon(IWeapon weapon)
        +VisitLightWeapon(IWeapon weapon)
        +VisitMagicWeapon(IWeapon weapon)
        +VisitNonWeapon()
    }

    class StealthAttack {
        +CalculatedDamage
        +CalculatedDefense
        +VisitHeavyWeapon(IWeapon weapon)
        +VisitLightWeapon(IWeapon weapon)
        +VisitMagicWeapon(IWeapon weapon)
        +VisitNonWeapon()
    }

    class MagicAttack {
        +CalculatedDamage
        +CalculatedDefense
        +VisitHeavyWeapon(IWeapon weapon)
        +VisitLightWeapon(IWeapon weapon)
        +VisitMagicWeapon(IWeapon weapon)
        +VisitNonWeapon()
    }

    IDungeonBuilder <|.. DungeonBuilder : implements
    DungeonDirector o-- IDungeonBuilder : uses
    IDungeonModifier <|.. CorridorsModifier : implements
    IDungeonModifier <|.. RoomsModifier : implements
    IDungeonModifier <|.. CentralRoomModifier : implements
    IDungeonModifier <|.. JunkItemsModifier : implements
    IDungeonModifier <|.. WeaponsModifier : implements
    DungeonBuilder ..> IDungeonModifier : uses
    CorridorsModifier ..> Labyrinth : uses
    DungeonBuilder ..> Map : creates
    ICommand <|.. MoveCommand : implements
    ICommand <|.. PickUpCommand : implements
    ICommand <|.. DropCommand : implements
    ICommand <|.. EquipCommand : implements
    ICommand <|.. HelpCommand : implements
    ICommand <|.. AttackCommand : implements
    InputHandler *-- ICommand : owns
    Game *-- InputHandler : owns
    Game --> GameState
    GameState --> Player
    GameState --> Map
    IItem <|-- IInventoryItem : extends
    IItem <|.. Gold : implements
    IItem <|.. Coin : implements
    IInventoryItem <|.. Weapon : implements
    IInventoryItem <|.. Junk : implements
    IWeapon <|.. BaseWeapon : implements
    IWeapon <|.. WeaponDecorator : implements
    IWeapon <|.. IHeavyWeapon : implements
    IWeapon <|.. ILightWeapon : implements
    IWeapon <|.. IMagicWeapon : implements
    BaseWeapon <|-- Sword
    BaseWeapon <|-- Dagger
    BaseWeapon <|-- TwoHandedAxe
    BaseWeapon <|-- MagicStaff
    WeaponDecorator <|-- StrongModifier
    WeaponDecorator <|-- UnluckyModifier
    Junk <|-- Skull
    Junk <|-- OldBone
    Junk <|-- BrokenGlass
    Map o-- IItem
    Player o-- IInventoryItem
    IInventoryItem ..> HandSide : uses
    IInventoryItem ..> IAttackVisitor : uses
    NormalAttack ..|> IAttackVisitor : implements
    StealthAttack ..|> IAttackVisitor : implements
    MagicAttack ..|> IAttackVisitor : implements
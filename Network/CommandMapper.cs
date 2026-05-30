namespace AlchemyRPG;

/// <summary>
/// Maps network command DTOs to game commands. This allows the network layer to be decoupled from the game logic, and makes it easier to add new commands in the future without modifying the network code.
/// </summary>
public interface ICommandMapper { ICommand? Map(NetworkCommandDTO dto); }

/// <summary>
/// Visitor that maps network command DTOs to game commands. Each visit method creates a new instance of the corresponding game command based on the data in the DTO.
/// </summary>
public class CommandMappingVisitor : INetworkCommandVisitor<ICommand?>
{
    public ICommand? VisitMove(MoveCommandDTO dto) => new MoveCommand(dto.Dx, dto.Dy);
    public ICommand? VisitEquip(EquipCommandDTO dto) => new EquipCommand(dto.ItemId, dto.HandSide == 0 ? new LeftHandSlot() : new RightHandSlot());
    public ICommand? VisitDrop(DropCommandDTO dto) => new DropCommand(dto.ItemId);
    public ICommand? VisitPickUp(PickUpCommandDTO dto) => new PickUpCommand();
    public ICommand? VisitInsert(InsertCommandDTO dto) => new InsertCommand(dto.ItemIdToInsert, dto.TargetContainerId);
    public ICommand? VisitNormalAttack(NormalAttackCommandDTO dto) => new AttackCommand(dto.TargetX, dto.TargetY, p => new NormalAttack(p));
    public ICommand? VisitStealthAttack(StealthAttackCommandDTO dto) => new AttackCommand(dto.TargetX, dto.TargetY, p => new StealthAttack(p));
    public ICommand? VisitMagicAttack(MagicAttackCommandDTO dto) => new AttackCommand(dto.TargetX, dto.TargetY, p => new MagicAttack(p));
}

/// <summary>
/// Implements the ICommandMapper interface by using the CommandMappingVisitor to map network command DTOs to game commands. This class serves as the main entry point for mapping commands in the network layer.
/// </summary>
public class CommandMapper : ICommandMapper
{
    public ICommand? Map(NetworkCommandDTO dto) => dto.Accept(new CommandMappingVisitor());
}
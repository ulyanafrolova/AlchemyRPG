namespace AlchemyRPG;

/// <summary>
/// Defines a contract for mapping a network command DTO to an executable domain command.
/// </summary>
public interface ICommandMapper
{
    /// <summary>
    /// Maps the provided Data Transfer Object to a corresponding domain <see cref="ICommand"/>.
    /// </summary>
    ICommand? Map(NetworkCommandDTO dto);
}

/// <summary>
/// A concrete visitor that resolves the specific type of a <see cref="NetworkCommandDTO"/> 
/// and instantiates the correct domain command.
/// </summary>
public class CommandMappingVisitor : INetworkCommandVisitor<ICommand?>
{
    public CommandMappingVisitor()
    {
    }

    public ICommand? VisitMove(MoveCommandDTO dto)
        => new MoveCommand(dto.Dx, dto.Dy);

    public ICommand? VisitEquip(EquipCommandDTO dto)
        => new EquipCommand(dto.ItemId, (HandSide)dto.HandSide);

    public ICommand? VisitDrop(DropCommandDTO dto)
        => new DropCommand(dto.ItemId);

    public ICommand? VisitPickUp(PickUpCommandDTO dto)
        => new PickUpCommand();

    public ICommand? VisitAttack(AttackCommandDTO dto)
        => new AttackCommand(dto.TargetX, dto.TargetY, dto.AttackType);
}

/// <summary>
/// A factory translator that isolates the network layer from the core domain logic.
/// It utilizes the Visitor pattern (Double Dispatch) to determine the exact type 
/// of the incoming DTO and convert it into an executable command.
/// </summary>
public class CommandMapper : ICommandMapper
{
    public ICommand? Map(NetworkCommandDTO dto)
    {
        var visitor = new CommandMappingVisitor();
        return dto.Accept(visitor);
    }
}
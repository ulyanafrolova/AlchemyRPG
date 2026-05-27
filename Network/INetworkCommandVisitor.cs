namespace AlchemyRPG;

/// <summary>
/// Defines a contract for the Visitor design pattern applied to incoming network commands.
/// Ensures type-safe, polymorphic unwrapping of command DTOs received from the client.
/// </summary>
/// <typeparam name="T">The return type of the visitor operations (typically an executable ICommand).</typeparam>
public interface INetworkCommandVisitor<T>
{
    /// <summary>Processes a movement command request.</summary>
    T VisitMove(MoveCommandDTO dto);

    /// <summary>Processes an equipment change command request.</summary>
    T VisitEquip(EquipCommandDTO dto);

    /// <summary>Processes an item drop command request.</summary>
    T VisitDrop(DropCommandDTO dto);

    /// <summary>Processes an item pick-up command request.</summary>
    T VisitPickUp(PickUpCommandDTO dto);

    /// <summary>Processes a combat attack command request.</summary>
    T VisitAttack(AttackCommandDTO dto);
}
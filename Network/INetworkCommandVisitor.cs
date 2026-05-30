namespace AlchemyRPG;

/// <summary>
/// Visitor interface for mapping network command DTOs to game commands. Each visit method corresponds to a specific type of command and takes the corresponding DTO as a parameter. The return type is generic, allowing for flexibility in the type of result produced by the visitor (e.g., it could return a game command, a validation result, etc.).
/// </summary>
/// <typeparam name="T"></typeparam>
public interface INetworkCommandVisitor<T>
{
    T VisitMove(MoveCommandDTO dto);
    T VisitEquip(EquipCommandDTO dto);
    T VisitDrop(DropCommandDTO dto);
    T VisitPickUp(PickUpCommandDTO dto);
    T VisitInsert(InsertCommandDTO dto);
    T VisitNormalAttack(NormalAttackCommandDTO dto);
    T VisitStealthAttack(StealthAttackCommandDTO dto);
    T VisitMagicAttack(MagicAttackCommandDTO dto);
}
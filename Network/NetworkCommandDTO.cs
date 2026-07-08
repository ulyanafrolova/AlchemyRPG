using System;
using System.Text.Json.Serialization;

namespace AlchemyRPG;

/// <summary>
/// The foundational Data Transfer Object (DTO) contract for all outbound client commands.
/// </summary>
/// <remarks>
/// By explicitly defining the inheritance hierarchy via <see cref="JsonPolymorphicAttribute"/> 
/// and <see cref="JsonDerivedTypeAttribute"/>, we guarantee exact type recovery during TCP stream deserialization 
/// on the authoritative server. The "Intent" discriminator prevents the need for custom, fragile JSON converters 
/// or runtime type-casting (`is`/`as`). Furthermore, this class acts as the Element in the Visitor pattern, 
/// forcing derived DTOs to implement double-dispatch for type-safe routing into the domain command pipeline.
/// </remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "Intent")]
[JsonDerivedType(typeof(MoveCommandDTO), typeDiscriminator: "Move")]
[JsonDerivedType(typeof(EquipCommandDTO), typeDiscriminator: "Equip")]
[JsonDerivedType(typeof(DropCommandDTO), typeDiscriminator: "Drop")]
[JsonDerivedType(typeof(PickUpCommandDTO), typeDiscriminator: "PickUp")]
[JsonDerivedType(typeof(InsertCommandDTO), typeDiscriminator: "Insert")]
[JsonDerivedType(typeof(NormalAttackCommandDTO), typeDiscriminator: "NormalAttack")]
[JsonDerivedType(typeof(StealthAttackCommandDTO), typeDiscriminator: "StealthAttack")]
[JsonDerivedType(typeof(MagicAttackCommandDTO), typeDiscriminator: "MagicAttack")]
public abstract class NetworkCommandDTO
{
    /// <summary>
    /// Routes the deserialized payload to the appropriate handler in the server's command mapping layer.
    /// </summary>
    public abstract T Accept<T>(INetworkCommandVisitor<T> visitor);
}

/// <summary>
/// A payload transmitting a request to translate the player's spatial coordinates.
/// </summary>
public class MoveCommandDTO : NetworkCommandDTO
{
    public int Dx { get; set; }
    public int Dy { get; set; }
    public override T Accept<T>(INetworkCommandVisitor<T> visitor) => visitor.VisitMove(this);
}

/// <summary>
/// A payload transmitting a request to map an inventory item to a physical equipment slot.
/// </summary>
public class EquipCommandDTO : NetworkCommandDTO
{
    /// <summary>The globally unique identifier of the target item in the client's backpack.</summary>
    public Guid ItemId { get; set; }

    /// <summary>An integer index representing the structural hand constraint (e.g., 0 for Left, 1 for Right).</summary>
    public int HandSide { get; set; }

    public override T Accept<T>(INetworkCommandVisitor<T> visitor) => visitor.VisitEquip(this);
}

/// <summary>
/// A payload transmitting a request to decouple an item from the player and instantiate it on the map grid.
/// </summary>
public class DropCommandDTO : NetworkCommandDTO
{
    /// <summary>The globally unique identifier of the item to discard.</summary>
    public Guid ItemId { get; set; }

    public override T Accept<T>(INetworkCommandVisitor<T> visitor) => visitor.VisitDrop(this);
}

/// <summary>
/// A payload transmitting a request to retrieve the topmost item from the player's current spatial coordinate.
/// </summary>
public class PickUpCommandDTO : NetworkCommandDTO
{
    public override T Accept<T>(INetworkCommandVisitor<T> visitor) => visitor.VisitPickUp(this);
}

/// <summary>
/// A payload transmitting a request to embed a modifier item into a Composite structure 
/// </summary>
public class InsertCommandDTO : NetworkCommandDTO
{
    /// <summary>The identifier of the item intended to act as the modifier.</summary>
    public Guid ItemIdToInsert { get; set; }

    /// <summary>The identifier of the root container accepting the modifier.</summary>
    public Guid TargetContainerId { get; set; }

    public override T Accept<T>(INetworkCommandVisitor<T> visitor) => visitor.VisitInsert(this);
}

/// <summary>
/// A payload transmitting a request to initiate standard physical combat calculations against a coordinate.
/// </summary>
public class NormalAttackCommandDTO : NetworkCommandDTO
{
    public int TargetX { get; set; }
    public int TargetY { get; set; }
    public override T Accept<T>(INetworkCommandVisitor<T> visitor) => visitor.VisitNormalAttack(this);
}

/// <summary>
/// A payload transmitting a request to initiate dexterity-weighted combat calculations against a coordinate.
/// </summary>
public class StealthAttackCommandDTO : NetworkCommandDTO
{
    public int TargetX { get; set; }
    public int TargetY { get; set; }
    public override T Accept<T>(INetworkCommandVisitor<T> visitor) => visitor.VisitStealthAttack(this);
}

/// <summary>
/// A payload transmitting a request to initiate wisdom-weighted, armor-piercing combat calculations.
/// </summary>
public class MagicAttackCommandDTO : NetworkCommandDTO
{
    public int TargetX { get; set; }
    public int TargetY { get; set; }
    public override T Accept<T>(INetworkCommandVisitor<T> visitor) => visitor.VisitMagicAttack(this);
}
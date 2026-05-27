using System;
using System.Text.Json.Serialization;

namespace AlchemyRPG;

/// <summary>
/// The abstract base class for all network commands transmitted from the client to the server.
/// Uses System.Text.Json polymorphic attributes to ensure seamless deserialization into specific concrete types.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "Intent")]
[JsonDerivedType(typeof(MoveCommandDTO), typeDiscriminator: "Move")]
[JsonDerivedType(typeof(EquipCommandDTO), typeDiscriminator: "Equip")]
[JsonDerivedType(typeof(DropCommandDTO), typeDiscriminator: "Drop")]
[JsonDerivedType(typeof(PickUpCommandDTO), typeDiscriminator: "PickUp")]
[JsonDerivedType(typeof(AttackCommandDTO), typeDiscriminator: "Attack")]
public abstract class NetworkCommandDTO
{
    /// <summary>Gets or sets the ID of the player issuing the command.</summary>
    public int PlayerId { get; set; }

    /// <summary>
    /// The abstract entry point for the Visitor pattern (Double Dispatch).
    /// Forces concrete DTOs to route themselves to the appropriate mapping method.
    /// </summary>
    public abstract T Accept<T>(INetworkCommandVisitor<T> visitor);
}

public class MoveCommandDTO : NetworkCommandDTO
{
    public int Dx { get; set; }
    public int Dy { get; set; }
    public override T Accept<T>(INetworkCommandVisitor<T> visitor) => visitor.VisitMove(this);
}

public class EquipCommandDTO : NetworkCommandDTO
{
    public Guid ItemId { get; set; }
    public int HandSide { get; set; }
    public override T Accept<T>(INetworkCommandVisitor<T> visitor) => visitor.VisitEquip(this);
}

public class DropCommandDTO : NetworkCommandDTO
{
    public Guid ItemId { get; set; }
    public override T Accept<T>(INetworkCommandVisitor<T> visitor) => visitor.VisitDrop(this);
}

public class PickUpCommandDTO : NetworkCommandDTO
{
    public override T Accept<T>(INetworkCommandVisitor<T> visitor) => visitor.VisitPickUp(this);
}

public class AttackCommandDTO : NetworkCommandDTO
{
    public int TargetX { get; set; }
    public int TargetY { get; set; }
    public AttackType AttackType { get; set; }
    public override T Accept<T>(INetworkCommandVisitor<T> visitor) => visitor.VisitAttack(this);
}
namespace AlchemyRPG;
/// <summary>
/// A visitor that resolves item DTOs to their appropriate ASCII representation for UI rendering.
/// </summary>
public class ClientSymbolVisitor : IItemDTOVisitor<char>
{
    public char VisitWeapon(WeaponDTO dto) => Tiles.Weapon;
    public char VisitGold(GoldDTO dto) => Tiles.Gold;
    public char VisitCoin(CoinDTO dto) => Tiles.Coin;
    public char VisitJunk(JunkDTO dto) => Tiles.Unknown;
    public char VisitSlottedWeapon(ISlottedWeapon weapon) => Tiles.Weapon;
}
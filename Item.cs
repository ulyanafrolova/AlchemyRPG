namespace AlchemyRPG;

public enum HandSide { Left, Right }

public interface IItem
{
    string Name { get; }
    char Symbol { get; }
    void OnPickUp(GameState state);
}

public interface IInventoryItem : IItem
{
    bool IsTwoHanded { get; }
    void Equip(Player player, HandSide side);
}

public class Gold(int amount) : IItem
{
    public string Name => "Gold";
    public char Symbol => '$';
    private readonly int _amount = amount;

    public void OnPickUp(GameState state)
    {
        state.Player.Gold += _amount;
        state.Log = $"Picked up {_amount} gold.";
        state.Map.RemoveItem(state.Player.X, state.Player.Y, this);
    }
}

public class Coin(int amount) : IItem
{
    public string Name => "Coin";
    public char Symbol => 'c';
    private readonly int _amount = amount;

    public void OnPickUp(GameState state)
    {
        state.Player.Coins += _amount;
        state.Log = $"Picked up {_amount} coins.";
        state.Map.RemoveItem(state.Player.X, state.Player.Y, this);
    }
}

public abstract class Weapon : IInventoryItem
{
    public string Name { get; protected set; } = "";
    public char Symbol => 'W';
    public int Damage { get; protected set; }
    public bool IsTwoHanded { get; protected set; }

    public void OnPickUp(GameState state)
    {
        state.Player.Backpack.Add(this);
        state.Map.RemoveItem(state.Player.X, state.Player.Y, this);
        state.Log = $"Picked up weapon: {Name}";
    }

    public void Equip(Player player, HandSide side)
    {
        if (IsTwoHanded)
        {
            player.EquipTwoHanded(this);
        }
        else
        {
            if (side == HandSide.Left)
                player.EquipLeftHand(this);
            else
                player.EquipRightHand(this);
        }
    }
}

public class Dagger : Weapon
{
    public Dagger() { Name = "Dagger"; Damage = 5; IsTwoHanded = false; }
}
public class Sword : Weapon
{
    public Sword() { Name = "Sword"; Damage = 10; IsTwoHanded = false; }
}
public class TwoHandedAxe : Weapon
{
    public TwoHandedAxe() { Name = "Two-Handed Axe"; Damage = 25; IsTwoHanded = true; }
}
public abstract class Junk : IInventoryItem
{
    public abstract string Name { get; }
    public char Symbol => '?';
    public bool IsTwoHanded => false;
    public void OnPickUp(GameState state)
    {
        {
            state.Player.Backpack.Add(this);
            state.Map.RemoveItem(state.Player.X, state.Player.Y, this);
            state.Log = $"Picked up junk: {Name}";
        }
    }
    public void Equip(Player player, HandSide side) { player.LogMessage = $"Cannot equip item: {Name}"; }
}
public class Skull : Junk { public override string Name => "Skull"; }
public class OldBone : Junk { public override string Name => "Old Bone"; }
public class BrokenGlass : Junk { public override string Name => "Broken Glass"; }
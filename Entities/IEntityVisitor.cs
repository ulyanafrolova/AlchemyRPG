namespace AlchemyRPG;

public interface IEntityVisitor
{
    void VisitPlayer(Player player);
    void VisitEnemy(Enemy enemy);
}
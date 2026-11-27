public class BlockDamageResolver
{
    private readonly BlockGrid _grid;

    public BlockDamageResolver(BlockGrid grid)
    {
        _grid = grid;
    }

    public void Damage(Block target, double damage)
    {
        if (target == null || !target.IsAlive) { return; }

        if (target.SharesDamage)
        {
            ShareDamage(target, damage);
            return;
        }

        Apply(target, damage);
    }

    private void ShareDamage(Block target, double damage)
    {
        Block right = GetLinkedNeighbor(target.Column + 1, target.Row);
        Block left = GetLinkedNeighbor(target.Column - 1, target.Row);
        Block up = GetLinkedNeighbor(target.Column, target.Row + 1);
        Block down = GetLinkedNeighbor(target.Column, target.Row - 1);

        int linkedCount = CountLinked(right) + CountLinked(left) + CountLinked(up) + CountLinked(down);
        double share = damage / (linkedCount + 1);

        Apply(target, share);
        ApplyLinked(right, share);
        ApplyLinked(left, share);
        ApplyLinked(up, share);
        ApplyLinked(down, share);
    }

    private Block GetLinkedNeighbor(int column, int row)
    {
        if (_grid.TryGetAliveCell(column, row, out Block neighbor) && neighbor.SharesDamage)
        {
            return neighbor;
        }

        return null;
    }

    private static int CountLinked(Block neighbor)
    {
        return neighbor != null ? 1 : 0;
    }

    private void ApplyLinked(Block neighbor, double damage)
    {
        if (neighbor == null) { return; }

        Apply(neighbor, damage);
    }

    private void Apply(Block cell, double damage)
    {
        cell.ApplyDamage(damage, GetNeighborGuardPercent(cell));
    }

    private double GetNeighborGuardPercent(Block cell)
    {
        double maxPercent = 0;

        for (int offsetY = -1; offsetY <= 1; offsetY++)
        {
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                if (offsetX == 0 && offsetY == 0) { continue; }

                if (!_grid.TryGetAliveCell(cell.Column + offsetX, cell.Row + offsetY, out Block neighbor)) { continue; }

                if (neighbor.GuardPercent > maxPercent)
                {
                    maxPercent = neighbor.GuardPercent;
                }
            }
        }

        return maxPercent;
    }
}

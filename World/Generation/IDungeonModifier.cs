using System;
using System.Collections.Generic;

namespace AlchemyRPG;

/// <summary>
/// Defines a single step (modifier)
/// To add a new feature we simply create a new class implementing this interface, without touching the Builder.
/// </summary>
public interface IDungeonModifier
{
    /// <summary>
    /// Applies the specific generation algorithm.
    /// </summary>
    /// <param name="map">The physical map being constructed.</param>
    /// <param name="controls">The UI control hints (e.g., "[E] Pick Up").</param>
    /// <param name="tutorialText">The detailed gameplay manual text being assembled.</param>
    /// <param name="rand">The shared random number generator.</param>
    void Apply(Map map, HashSet<string> controls, List<string> tutorialText, Random rand);
}
using AlchemyRPG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Defines the contract for the Dungeon Builder.
/// Accepts external IDungeonModifier objects
/// </summary>
public interface IDungeonBuilder
{
    /// <summary>
    /// Initializes the map with floor tiles (' ') everywhere.
    /// This (or CreateFilled) MUST be called before any modifiers are applied.
    /// </summary>
    /// <param name="width">The total width of the map.</param>
    /// <param name="height">The total height of the map.</param>
    /// <returns>The current builder instance to allow method chaining.</returns>
    IDungeonBuilder CreateEmpty(int width, int height);

    /// <summary>
    /// Initializes the map entirely filled with wall tiles ('█').
    /// This is useful as a base for digging out corridors and rooms later.
    /// </summary>
    /// <param name="width">The total width of the map.</param>
    /// <param name="height">The total height of the map.</param>
    /// <returns>The current builder instance to allow method chaining.</returns>
    IDungeonBuilder CreateFilled(int width, int height);

    /// <summary>
    /// Injects a specific modification algorithm (e.g., carving rooms, spawning loot) into the map.
    /// </summary>
    /// <param name="modifier">The strategy/command to apply to the current map.</param>
    /// <returns>The builder instance to support method chaining.</returns>
    IDungeonBuilder ApplyModifier(IDungeonModifier modifier);

    /// <summary>
    /// Finalizes the building process and returns the fully constructed Map object.
    /// </summary>
    /// <returns>The complete, playable map data container.</returns>
    Map GetMap();

    /// <summary>
    /// Retrieves the dynamically generated string of gameplay instructions (tutorial).
    /// These instructions adapt automatically depending on which modifier methods were called.
    /// </summary>
    /// <returns>A formatted string of tutorial prompts to be displayed.</returns>
    string GetInstructions();

    /// <summary>
    /// Tutorial text (instructions)
    /// </summary>
    /// <returns></returns>
    string GetTutorialText();
}
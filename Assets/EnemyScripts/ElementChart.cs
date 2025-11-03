using UnityEngine;

using System.Collections.Generic;

public static class ElementChart
{
    // Attack element vs defender element Å® multiplier
    // Your rules: Weak = 2x, Same = 0x (immune), Other/Neutral = 0.5x
    private static readonly Dictionary<Element, Dictionary<Element, float>> table = new()
    {
        [Element.Fire] = new() { [Element.Fire] = 0f, [Element.Water] = 0.5f, [Element.Wind] = 2f, [Element.Neutral] = 0.5f },
        [Element.Water] = new() { [Element.Fire] = 2f, [Element.Water] = 0f, [Element.Wind] = 0.5f, [Element.Neutral] = 0.5f },
        [Element.Wind] = new() { [Element.Fire] = 0.5f, [Element.Water] = 2f, [Element.Wind] = 0f, [Element.Neutral] = 0.5f },
        [Element.Neutral] = new() { [Element.Fire] = 0.5f, [Element.Water] = 0.5f, [Element.Wind] = 0.5f, [Element.Neutral] = 0.5f },
    };

    public static float GetMultiplier(Element attack, Element defense)
        => table.TryGetValue(attack, out var row) && row.TryGetValue(defense, out var m) ? m : 1f;
}

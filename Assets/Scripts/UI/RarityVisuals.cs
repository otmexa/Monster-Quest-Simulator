using UnityEngine;
using MonsterQuest.Inventory;

namespace MonsterQuest.UI
{
    public static class RarityVisuals
    {
        public static Color GetStaticColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Uncommon: return Color.green;
                case ItemRarity.Rare: return new Color(0f, 0.5f, 1f); // Blue
                case ItemRarity.Epic: return new Color(0.6f, 0f, 1f); // Purple
                case ItemRarity.Legendary: return new Color(1f, 0.7f, 0f); // Gold/Orange
                case ItemRarity.Mythical: return Color.red;
                case ItemRarity.Eternal: return Color.white;
                case ItemRarity.Exotic: return new Color(0.2f, 0f, 0.5f); // Deep Purple
                case ItemRarity.Holo: return new Color(1f, 0.4f, 0.7f); // Pinkish
                case ItemRarity.Celestial: return new Color(0.4f, 0.8f, 1f); // Cyan
                default: return Color.white;
            }
        }
        
        public static bool IsAnimated(ItemRarity rarity)
        {
            return rarity == ItemRarity.Eternal || rarity == ItemRarity.Exotic || 
                   rarity == ItemRarity.Holo || rarity == ItemRarity.Celestial;
        }

        public static Color GetAnimatedColor(ItemRarity rarity, float time)
        {
            if (!IsAnimated(rarity)) return GetStaticColor(rarity);

            switch (rarity)
            {
                case ItemRarity.Eternal:
                    // Arcoíris
                    return Color.HSVToRGB(Mathf.Repeat(time * 0.5f, 1f), 1f, 1f);
                    
                case ItemRarity.Exotic:
                    // Degradado Azul Púrpura a Azul Intenso/Negro
                    Color exotic1 = new Color(0.4f, 0f, 1f);
                    Color exotic2 = new Color(0f, 0f, 0.2f);
                    return Color.Lerp(exotic1, exotic2, Mathf.PingPong(time * 0.8f, 1f));
                    
                case ItemRarity.Holo:
                    // Rosa, dorado, rojo, blanco
                    float tHolo = Mathf.PingPong(time * 1.2f, 1f);
                    if (tHolo < 0.33f) return Color.Lerp(Color.magenta, new Color(1f, 0.8f, 0f), tHolo * 3f);
                    else if (tHolo < 0.66f) return Color.Lerp(new Color(1f, 0.8f, 0f), Color.white, (tHolo - 0.33f) * 3f);
                    else return Color.Lerp(Color.white, Color.red, (tHolo - 0.66f) * 3f);
                    
                case ItemRarity.Celestial:
                    // Celeste, morado, amarillo
                    float tCel = Mathf.PingPong(time * 0.8f, 1f);
                    if (tCel < 0.5f) return Color.Lerp(Color.cyan, new Color(0.6f, 0f, 1f), tCel * 2f);
                    else return Color.Lerp(new Color(0.6f, 0f, 1f), Color.yellow, (tCel - 0.5f) * 2f);
            }

            return Color.white;
        }
    }
}

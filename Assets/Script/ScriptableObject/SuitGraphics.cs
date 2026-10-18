using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "SuitGraphics", menuName = "Data/SuitGraphics")]
public class SuitGraphics : ScriptableObject
{
    [System.Serializable]
    public class SuitGraphic
    {
        [System.Serializable]
        public struct OverlaySprites
        {
            public int rank;
            public Sprite graphics;
        }
        public SuitBlueprint suit;
        public Color suitColor;
        public Sprite sprite;
        public OverlaySprites[] rankOverlaySprites;
        public LocalizedString suitName;
        public Sprite GetRankSprite(int rank)
        {
            foreach (var rankOverlay in rankOverlaySprites)
            {
                if (rankOverlay.rank == rank)
                    return rankOverlay.graphics;
            }
            return null;
        }
    }
    public string Name;
    public SuitGraphic[] suitGraphics;
    public Sprite backGraphics;

    public SuitGraphic GetSuitGraphics(SuitBlueprint suit)
    {
        foreach (var suitGraphic in suitGraphics)
            if (suit == suitGraphic.suit)
                return suitGraphic;
        return null;
    }

    public Color GetSuitColor(SuitBlueprint suit)
    {
        foreach (var suitGraphic in suitGraphics)
            if (suit == suitGraphic.suit)
                return suitGraphic.suitColor;
        return Color.white;
    }
    public Sprite GetSuitSprite(SuitBlueprint suit)
    {
        foreach (var suitGraphic in suitGraphics)
            if (suit == suitGraphic.suit)
                return suitGraphic.sprite;
        return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DiceType { d3, d4, d6, d8, d10, d12, d20, d100 }
public enum CombatStatus { NA, Alive, Death, Retreated }
public enum CharacterState {Alive, Dead, Missing }
public enum WorldMapTypes {OutOfBounds, Ground, PlayerShip, City, POI, Ship }
public enum MovementState { Stationary, Moving }
public enum SpeedState { EngineOff, Stealth, Normal, FullBurn }
public enum AddRemoved { Added, Removed}
public enum CheckCondition { And, Or}
//DeckMania
public enum ScoreType {Nothing, HighCard, Pair, TwoPairs, ThreeOfAKind, FourOfAKind, FiveOfAKind, FullHouse, Straight, Flush, StraightFlush, RoyalFlush, FlushHouse, FlushFive, Unknown }
public enum SortType { Rank, Suit}
public enum Rarity {None, Common, Uncommon, Rare, Epic, Legendary, Mythic }
public enum BuySell {Buy, Sell, Neither, Both }
public enum GameState {None, Ante, Store, AnteSelection}
public enum DropableResponse {None, Allowed, CantMove, NotValid, NotReceivingAnything }

public static class EnumUtility
{
    
}

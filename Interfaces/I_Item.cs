using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Mythic,
    Legendary,
}

public interface I_Item
{
    public Rarity rarity {get; set;}
    public Sprite spriteImage {get; set;}
    public string itemName {get; set;}
    public string description {get; set;}
    public string itemBlurb {get; set;}
    public int itemID {get; set;}
    public I_Unit owner {get; set;}
    public static readonly Dictionary<Rarity,String> rarityColor = new Dictionary<Rarity, string> {
        {Rarity.Common, "white"}, 
        {Rarity.Uncommon, "black"}, 
        {Rarity.Rare, "blue"}, 
        {Rarity.Mythic, "red"}, 
        {Rarity.Legendary, "purple"}, 
};
} 

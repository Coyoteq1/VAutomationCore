namespace VAutomationCore.Models
{
    /// <summary>
    /// Canonical blood type prefab hashes with compatibility aliases.
    /// </summary>
    public enum BloodType
    {
        Frail = 447918373,
        Creature = 524822543,
        Warrior = -516976528,
        Rogue = -1620185637,
        Brute = 804798592,
        Scholar = 1476452791,
        Worker = -1776904174,
        Mutant = 1821108694,
        Dracula = 2010023718,
        Draculin = 1328126535,
        BloodSoul = 910644396,
        VBlood = -338774148,
        Corrupted = -1382693416,

        // Compatibility aliases used by older configs/scripts.
        Frailed = Frail,
        Immortal = Dracula
    }
}

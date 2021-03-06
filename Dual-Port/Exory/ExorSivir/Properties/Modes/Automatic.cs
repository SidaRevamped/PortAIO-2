using System;
using System.Linq;
using ExorAIO.Utilities;
using EloBuddy;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.Utils;
using LeagueSharp.Data.Enumerations;

namespace ExorAIO.Champions.Sivir
{
    /// <summary>
    ///     The logics class.
    /// </summary>
    internal partial class Logics
    {
        /// <summary>
        ///     Called when the game updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void Automatic(EventArgs args)
        {
            if (Bools.HasSheenBuff())
            {
                return;
            }

            /// <summary>
            ///     The Automatic Q Logic.
            /// </summary>
            if (Vars.Q.IsReady() &&
                Menus.getCheckBoxItem(Vars.QMenu, "logical"))
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(
                    t =>
                        Bools.IsImmobile(t) &&
                        !Invulnerable.Check(t) &&
                        t.LSIsValidTarget(Vars.Q.Range)))
                {
                    Vars.Q.Cast(target.ServerPosition);
                }
            }
        }

        /// <summary>
        ///     Called while processing Spellcasting operations.
        ///     Port this berbb :^)
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void AutoShield(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null ||
                !sender.IsEnemy)
            {
                return;
            }

            switch (sender.Type)
            {
                case GameObjectType.AIHeroClient:

                    if (Invulnerable.Check(GameObjects.Player, DamageType.True, false))
                    {
                        return;
                    }

                    /// <summary>
                    ///     Block Gangplank's Barrels 1st Part.
                    /// </summary>
                    if ((sender as AIHeroClient).ChampionName.Equals("Gangplank"))
                    {
                        if (args.Target == null ||
                            !(args.Target is Obj_AI_Minion))
                        {
                            return;
                        }

                        if (AutoAttack.IsAutoAttack(args.SData.Name) ||
                            args.SData.Name.Equals("GangplankQProceed"))
                        {
                            if ((args.Target as Obj_AI_Minion).Health == 1 &&
                                (args.Target as Obj_AI_Minion).DistanceToPlayer() < 450 &&
                                (args.Target as Obj_AI_Minion).CharData.BaseSkinName.Equals("gangplankbarrel"))
                            {
                                Vars.E.Cast();
                            }
                        }
                    }

                    /// <summary>
                    ///     Block Gangplank's Barrels 2nd Part.
                    /// </summary>
                    else if (GameObjects.Player.Distance(args.End) < 450 &&
                        args.SData.Name.Equals("GangplankEBarrelFuseMissile"))
                    {
                        Vars.E.Cast();
                    }

                    /// <summary>
                    ///     Check for Special On-Hit CC AutoAttacks & Melee AutoAttack Resets.
                    /// </summary>
                    if (AutoAttack.IsAutoAttack(args.SData.Name))
                    {
                        if (!args.Target.IsMe)
                        {
                            return;
                        }

                        /// <summary>
                        ///     Whitelist Block.
                        /// </summary>
                        if (Vars.WhiteListMenu[$"{(sender as AIHeroClient).ChampionName.ToLower()}.{(sender as AIHeroClient).Buffs.First(b => AutoAttack.IsAutoAttackReset(b.Name)).Name.ToLower()}"] == null)  
                        {
                            if (Vars.WhiteListMenu[$"{(sender as AIHeroClient).ChampionName.ToLower()}.{args.SData.Name.ToLower()}"] == null ||
                                !Menus.getCheckBoxItem(Vars.WhiteListMenu, $"{(sender as AIHeroClient).ChampionName.ToLower()}.{args.SData.Name.ToLower()}"))
                            {
                                return;
                            }
                        }

                        switch (args.SData.Name)
                        {
                            case "BraumBasicAttackPassiveOverride":
                                Vars.E.Cast();
                                break;

                            case "UdyrBearAttack":
                                if (!GameObjects.Player.HasBuff("udyrbearstuncheck"))
                                {
                                    Vars.E.Cast();
                                }
                                break;

                            default:
                                Vars.E.Cast();
                                break;
                        }
                    }

                    /// <summary>
                    ///     Shield all the Targetted Spells.
                    /// </summary>
                    else if (SpellDatabase.GetByName(args.SData.Name) != null)
                    {
                        /// <summary>
                        ///     Whitelist Block.
                        /// </summary>
                        if (Vars.WhiteListMenu[$"{(sender as AIHeroClient).ChampionName.ToLower()}.{args.SData.Name.ToLower()}"] == null ||
                            !Menus.getCheckBoxItem(Vars.WhiteListMenu, $"{(sender as AIHeroClient).ChampionName.ToLower()}.{args.SData.Name.ToLower()}"))
                        {
                            return;
                        }

                        switch (SpellDatabase.GetByName(args.SData.Name).SpellType)
                        {
                            /// <summary>
                            ///     Check for Targetted Spells.
                            /// </summary>
                            case SpellType.Targeted:
                            case SpellType.TargetedMissile:

                                if (!args.Target.IsMe ||
                                    !SpellDatabase.GetByName(args.SData.Name).CastType.Contains(CastType.EnemyChampions))
                                {
                                    return;
                                }

                                switch ((sender as AIHeroClient).ChampionName)
                                {
                                    case "Zed":
                                        DelayAction.Add(200, () => { Vars.E.Cast(); });
                                        break;

                                    case "Caitlyn":
                                        DelayAction.Add(1050, () => { Vars.E.Cast(); });
                                        break;

                                    case "Nocturne":
                                        DelayAction.Add(350, () => { Vars.E.Cast(); });
                                        break;

                                    default:
                                        DelayAction.Add(Menus.getSliderItem(Vars.EMenu, "delay"), () => { Vars.E.Cast(); });
                                        break;
                                }
                                break;

                            /// <summary>
                            ///     Check for AoE Spells.
                            /// </summary>
                            case SpellType.SkillshotCircle:

                                switch ((sender as AIHeroClient).ChampionName)
                                {
                                    case "Alistar":
                                        if ((sender as AIHeroClient).DistanceToPlayer() <
                                                355 + GameObjects.Player.BoundingRadius)
                                        {
                                            Vars.E.Cast();
                                        }
                                        break;

                                    default:
                                        break;
                                }
                                break;

                            default:
                                break;
                        }
                        break;
                    }
                    break;

                case GameObjectType.obj_AI_Minion:

                    if (args.Target == null ||
                        !args.Target.IsMe)
                    {
                        return;
                    }

                    /// <summary>
                    ///     Block Dragon/Baron/RiftHerald's AutoAttacks.
                    /// </summary>
                    if (Menus.getCheckBoxItem(Vars.WhiteListMenu, "minions"))
                    {
                        if (sender.CharData.BaseSkinName.Equals("SRU_Baron") ||
                            sender.CharData.BaseSkinName.Contains("SRU_Dragon") ||
                            sender.CharData.BaseSkinName.Equals("SRU_RiftHerald"))
                        {
                            Vars.E.Cast();
                        }
                    }
                    break;

                default:
                    break;
            }
        }
    }
}

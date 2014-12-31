﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Reflection;
using System.Diagnostics;

namespace PerplexedEzreal
{
    class Program
    {
        static Obj_AI_Hero Player = ObjectManager.Player;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Ezreal")
                return;

            SpellManager.Initialize();
            Config.Initialize();

            Utility.HpBarDamageIndicator.DamageToUnit = DamageCalc.GetTotalDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            CustomDamageIndicator.Initialize(DamageCalc.GetTotalDamage); //Credits to Hellsing for this! Borrowed it from his Kalista assembly.

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            Game.PrintChat("<font color=\"#ff3300\">Perplexed Ezreal ({0})</font> - <font color=\"#ffffff\">Loaded!</font>", Assembly.GetExecutingAssembly().GetName().Version);
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.RecallBlock && (Player.HasBuff("Recall") || Player.IsWindingUp))
                return;
            switch (Config.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                default:
                    Auto();
                    break;
            }
            KillSteal();
        }

        static void Combo()
        {
            if (Config.ComboQ && SpellManager.Q.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.Q.Range, TargetSelector.DamageType.Physical);
                SpellManager.CastSpell(SpellManager.Q, target, HitChance.High, Config.UsePackets);
            }
            if (Config.ComboW && SpellManager.W.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.W.Range, TargetSelector.DamageType.Physical);
                SpellManager.CastSpell(SpellManager.W, target, HitChance.High, Config.UsePackets);
            }
        }

        static void Harass()
        {
            if (Config.HarassQ && SpellManager.Q.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.Q.Range, TargetSelector.DamageType.Physical);
                SpellManager.CastSpell(SpellManager.Q, target, HitChance.High, Config.UsePackets);
            }
            if (Config.HarassW && SpellManager.W.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.W.Range, TargetSelector.DamageType.Physical);
                SpellManager.CastSpell(SpellManager.W, target, HitChance.High, Config.UsePackets);
            }
        }

        static void Auto()
        {
            if (Config.AutoQ && SpellManager.Q.IsReady())
            {
                if (Config.ManaR && SpellManager.R.IsReady() && (Player.Mana - Player.Spellbook.GetSpell(SpellManager.Q.Slot).ManaCost) < Player.Spellbook.GetSpell(SpellManager.R.Slot).ManaCost)
                    return;
                var target = TargetSelector.GetTarget(SpellManager.Q.Range, TargetSelector.DamageType.Physical);
                SpellManager.CastSpell(SpellManager.Q, target, HitChance.High, Config.UsePackets);
            }
            if (Config.AutoW && SpellManager.W.IsReady())
            {
                if (Config.ManaR && SpellManager.R.IsReady() && (Player.Mana - Player.Spellbook.GetSpell(SpellManager.W.Slot).ManaCost) < Player.Spellbook.GetSpell(SpellManager.R.Slot).ManaCost)
                    return;
                var target = TargetSelector.GetTarget(SpellManager.W.Range, TargetSelector.DamageType.Physical);
                SpellManager.CastSpell(SpellManager.W, target, HitChance.High, Config.UsePackets);
            }
        }

        static void KillSteal()
        {
            if (Config.KillSteal && SpellManager.R.IsReady())
            {
                var target = TargetSelector.GetTarget(Config.KillSteal_Range, TargetSelector.DamageType.Magical);
                var ultDamage = DamageCalc.GetUltDamage(target);
                var targetHealth = target.Health;
                if (ultDamage >= targetHealth)
                    SpellManager.CastSpell(SpellManager.R, target, HitChance.High, Config.UsePackets);
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.DrawQ)
                Utility.DrawCircle(Player.ServerPosition, SpellManager.Q.Range, Config.Settings.Item("drawQ").GetValue<Circle>().Color, 1);
            if (Config.DrawW)
                Utility.DrawCircle(Player.ServerPosition, SpellManager.W.Range, Config.Settings.Item("drawW").GetValue<Circle>().Color, 1);
            if (Config.DrawR)
                Utility.DrawCircle(Player.ServerPosition, Config.KillSteal_Range, Config.Settings.Item("drawR").GetValue<Circle>().Color, 1);
        }
    }
}

﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts.Model.Weapons;
using Assets.Scripts.Model.Tiles;

namespace Assets.Scripts.Model.Units {
    public abstract class Unit : MonoBehaviour, IMover {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Class { get; set; }

        // Health Points are the life points of the Unit
        // If Health Points is zero, the unit is dead
        // Health Points can never exceed max health points
        public double HealthPoints { get; set; }
        public double MaxHealthPoints { get; set; }

        public int Level { get; protected set; }
        public int ExperiencePoints { get; protected set; }

        public int Strength { get; set; }
        public int Magic { get; set; }

        public int Defense { get; set; }
        public int Resistance { get; set; }

        public int Speed { get; set; }
        public int Skill { get; set; }

        public int Luck { get; set; }

        public int MoveRange { get; set; }

        public Weapon MainWeapon { get; set; }

        // TODO: Add Item Properties

        // Abstract methods that must be overridden by Unit sub classes
        public abstract bool CanMove(Tile tile);
        public abstract int MoveCost(Tile tile);

        public string UnitInformation {
            get {
                var info =
                    $"Name: {Name}\n" +
                    $"Level: {Level}\n" +
                    $"Strength: {Strength}\n" +
                    $"Magic: {Magic}\n" +
                    $"Defense: {Defense}\n" +
                    $"Resistance: {Resistance}\n" +
                    $"Speed: {Speed}\n" +
                    $"Skill: {Skill}\n" +
                    $"Luck: {Luck}\n" +
                    $"Move Range: {MoveRange}\n";
                return info;
            }
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assets.Scripts.Model.Weapons;

namespace Assets.Scripts.Model.Units {
    [Serializable]
    public class UnitWrapper {
        public string unitName;
        public string unitType;
        public string unitClass;

        public int unitLevel;
        public int unitExperiencePoints;

        public Stat unitMaxHealthPoints;
        public Stat unitStrength;
        public Stat unitMagic;
        public Stat unitDefense;
        public Stat unitResistance;
        public Stat unitSpeed;
        public Stat unitSkill;
        public Stat unitLuck;
        public Stat unitMovement;

        public string unitWeapon;
        public List<string> unitSkills;

        public UnitWrapper(Unit unit) {
            unitName = unit.Name;
            unitType = unit.Type;
            unitClass = unit.Class;

            unitLevel = unit.Level;
            unitExperiencePoints = unit.ExperiencePoints;

            unitMaxHealthPoints = unit.MaxHealthPoints;
            unitStrength = unit.Strength;
            unitMagic = unit.Magic;
            unitDefense = unit.Defense;
            unitResistance = unit.Resistance;
            unitSpeed = unit.Speed;
            unitSkill = unit.Skill;
            unitLuck = unit.Luck;
            unitMovement = unit.Movement;

            unitWeapon = unit.MainWeapon.Name;
            unitSkills = new List<string>();
            unitSkills.AddRange(unit.Skills.Where(skill => !unit.MainWeapon.Skills.Contains(skill)).Select(skill => skill.SkillName));
        }
    }
}
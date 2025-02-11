﻿using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyBox.Multiclass {
    //public enum MulticlassSaveBABType {
    //    SumOfAll = 0,
    //    UseMaxIncrement = 1,
    //    UseMaxValue = 2,
    //    OnlyPrimary = 3
    //};
    public static class SavesBAB {
        public static void ApplySingleStat(UnitDescriptor unit, LevelUpState state, BlueprintCharacterClass[] appliedClasses, StatType stat, BlueprintStatProgression[] statProgs, ProgressionPolicy policy = ProgressionPolicy.Largest) {
            if (appliedClasses.Count() <= 0) return;
            //Main.Debug($"stat: {stat}  baseValue: {unit.Stats.GetStat(stat).BaseValue}");
            int[] newClassLvls = appliedClasses.Select(cd => unit.Progression.GetClassLevel(cd)).ToArray();
            int appliedClassCount = newClassLvls.Length;
            int[] oldBonuses = new int[appliedClassCount];
            int[] newBonuses = new int[appliedClassCount];

            for (int i = 0; i < appliedClassCount; i++) {
                newBonuses[i] = statProgs[i].GetBonus(newClassLvls[i]);
                oldBonuses[i] = statProgs[i].GetBonus(newClassLvls[i] - 1);
            }
            
            int mainClassIndex = appliedClasses.ToList().FindIndex(cd => cd == state.SelectedClass);
            //v($"mainClassIndex = {mainClassIndex}");
            int mainClassInc = newBonuses[mainClassIndex] - oldBonuses[mainClassIndex];
            int increase = 0;

            switch (policy) {
                case ProgressionPolicy.Average:
                    for (int i = 0; i < appliedClassCount; i++) increase += Math.Max(0, newBonuses[i] - oldBonuses[i]);
                    unit.Stats.GetStat(stat).BaseValue += (increase/appliedClassCount - mainClassInc);
                    break;
                case ProgressionPolicy.Largest:
                    int maxOldValue = 0, maxNewValue = 0;
                    for (int i = 0; i < appliedClassCount; i++) maxOldValue = Math.Max(maxOldValue, oldBonuses[i]);
                    for (int i = 0; i < appliedClassCount; i++) maxNewValue = Math.Max(maxNewValue, newBonuses[i]);
                    increase = maxNewValue - maxOldValue;
                    unit.Stats.GetStat(stat).BaseValue += (increase - mainClassInc);
                    break;
                case ProgressionPolicy.Sum:
                    for (int i = 0; i < appliedClassCount; i++) increase += Math.Max(0, newBonuses[i] - oldBonuses[i]);
                    unit.Stats.GetStat(stat).BaseValue += (increase - mainClassInc);
                    break;
                default:
                    break; ;
            }
        }

        public static void ApplySaveBAB(UnitDescriptor unit, LevelUpState state, BlueprintCharacterClass[] classes) {
            ApplySingleStat(
                unit, 
                state, 
                classes, 
                StatType.BaseAttackBonus, 
                classes.Select(a => a.BaseAttackBonus).ToArray(), 
                Main.settings.multiclassBABPolicy
                );
            ApplySingleStat(
                unit, 
                state, 
                classes, 
                StatType.SaveFortitude, 
                classes.Select(a => a.FortitudeSave).ToArray(), 
                Main.settings.multiclassSavingThrowPolicy
                );
            ApplySingleStat(
                unit, 
                state, 
                classes, 
                StatType.SaveReflex, 
                classes.Select(a => a.ReflexSave).ToArray(),
                Main.settings.multiclassSavingThrowPolicy
                );
            ApplySingleStat(
                unit, 
                state, 
                classes, 
                StatType.SaveWill, 
                classes.Select(a => a.WillSave).ToArray(), 
                Main.settings.multiclassSavingThrowPolicy
                );

        }
    }
}

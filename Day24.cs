using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using AdventOfCode2018.CSharp.Utils;
using FluentAssertions;
using Microsoft.Z3;
using Parser;
using Utils;
using P = Parser.ParserBuiltins;

namespace AdventOfCode2018.CSharp;

public class Day24
{

  [Theory]
  // [InlineData("Day24.Sample", 5216)]
  [InlineData("Day24", 0)] // 26175 too low
  public void Part1(string path, int expected)
  {
    var armies = Convert(AoCLoader.LoadFile(path));

    foreach(var army in armies.immuneArmy.Concat(armies.infectionArmy)) {
      Console.WriteLine($"{army.Id}: weak='{army.Weaknesses.Join(",")}'; immune='{army.Immunities.Join(",")}'");
    }
    while (armies.immuneArmy.Count > 0 && armies.infectionArmy.Count > 0) {
      armies = Fight(armies);
    }

    (armies.immuneArmy.Sum(it => it.Units) + armies.infectionArmy.Sum(it => it.Units))
      .Should().Be(expected);
  }

  private static (List<ArmyGroup> immuneArmy, List<ArmyGroup> infectionArmy) Fight((List<ArmyGroup> immuneArmy, List<ArmyGroup> infectionArmy) armies)
  {
    var (immuneArmy, infectionArmy) = armies;
    // Selection phase
    Dictionary<long, ArmyGroup> idToArmy = immuneArmy.Concat(infectionArmy).ToDictionary(it => it.Id, it => it);
    Dictionary<long, long> allTargets = [];
    HashSet<ArmyGroup> selected = [];
    var orderedGroups = immuneArmy.Concat(infectionArmy).OrderByDescending(army => army.EffectivePower).ThenByDescending(army => army.Initiative).ToList();
    foreach(var army in orderedGroups) {
      var targets = immuneArmy.Contains(army) ? infectionArmy.Except(selected).ToList() : immuneArmy.Except(selected).ToList();
      var targets2 = targets.Where(t => t.Weaknesses.Contains(army.AttackElement)).ToList();
      if (targets2.Count == 0) {
        targets2 = targets.Where(t => !t.Immunities.Contains(army.AttackElement)).ToList();
      }
      if (targets2.Count == 0) {
        targets2 = targets.Where(t => t.Immunities.Contains(army.AttackElement)).ToList();
      }
      var target = targets2.OrderByDescending(t => t.EffectivePower).ThenByDescending(t => t.Initiative).Take(1).ToList();
      if (target.Count == 1) {
        allTargets[army.Id] = target[0].Id;
        selected.Add(target[0]);
      }
    }

    // Deal damage phase
    foreach(var id in immuneArmy.Concat(infectionArmy).OrderByDescending(it => it.Initiative).Select(it => it.Id)) {
      if (!idToArmy.TryGetValue(id, out var attacker)) continue;
      if (!allTargets.TryGetValue(id, out var targetId)) continue;
      var target = idToArmy[targetId];
      var damage = attacker.EffectivePower * (target.Weaknesses.Contains(attacker.AttackElement) ? 2 : 1) * (target.Immunities.Contains(attacker.AttackElement) ? 0 : 1);
      if (damage == 0) continue;
      var losses = damage / target.HitPointsPerUnit;
      if (losses >= target.Units) idToArmy.Remove(targetId);
      else idToArmy[targetId] = target with {Units = target.Units - losses};
    }

    return (idToArmy.Values.Where(army => immuneArmy.Select(it => it.Id).Contains(army.Id)).ToList(), 
      idToArmy.Values.Where(army => infectionArmy.Select(it => it.Id).Contains(army.Id)).ToList());
  }

  public record ArmyGroup(long Id, long Units, long HitPointsPerUnit, List<string> Immunities, List<string> Weaknesses, 
    long AttackDamage, string AttackElement, long Initiative)
    {
      public long EffectivePower => Units * AttackDamage;
    }

  private static (List<ArmyGroup> immuneArmy, List<ArmyGroup> infectionArmy) Convert(string data)
  {
    long id = 1;
    var listOfWords = P.Word.Trim().Plus(",");
    var immunities1 = P.Format("immune to {}; weak to {}", listOfWords, listOfWords) 
      | P.Format("weak to {}; immune to {}", listOfWords, listOfWords).Select(it => (First: it.Second, Second: it.First))
      | P.Format("immune to {}", listOfWords).Select(it => (First: it, Second: new List<string>()))
      | P.Format("weak to {}", listOfWords).Select(it => (First: new List<string>(), Second: it));
    var immunities = P.Format("({})", immunities1.Require()).Optional().Select(it => it.Count == 1 ? it[0] : (First: [], Second: []));
    var group = P.Format("{} units each with {} hit points {} with an attack that does {} {} damage at initiative {}",
      P.Long, P.Long, immunities, P.Long, P.Word, P.Long).Select(it => new ArmyGroup(id++, it.First, it.Second, it.Third.First, it.Third.Second, it.Fourth, it.Fifth, it.Sixth));
    var army = group.Plus();
    var complete = P.Format("Immune System: {} Infection: {}", army, army);
    return complete.Before(P.EndOfInput).Parse(data);
  }
}

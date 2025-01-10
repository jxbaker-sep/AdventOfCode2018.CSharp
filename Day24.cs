using AdventOfCode2018.CSharp.Utils;
using FluentAssertions;
using Parser;
using Utils;
using P = Parser.ParserBuiltins;

namespace AdventOfCode2018.CSharp;

public class Day24
{

  [Theory]
  [InlineData("Day24.Sample", 5216)]
  [InlineData("Day24", 0)] // 26175 too low
  public void Part1(string path, int expected)
  {
    var armies = Convert(AoCLoader.LoadFile(path));

    foreach(var army in armies) {
      Console.WriteLine($"{army.Id}: weak='{army.Weaknesses.Join(",")}'; immune='{army.Immunities.Join(",")}'");
    }
    while (armies.Any(army => army.Side == 1) && armies.Any(it => it.Side == 2)) {
      armies = Fight(armies);
    }

    armies.Sum(it => it.Units)
      .Should().Be(expected);
  }

  private static List<ArmyGroup> Fight(List<ArmyGroup> armies)
  {
    // Selection phase
    Dictionary<long, ArmyGroup> idToArmy = armies.ToDictionary(it => it.Id, it => it);
    Dictionary<long, long> selectedTargets = [];
    HashSet<ArmyGroup> selected = [];
    var orderedGroups = armies.OrderByDescending(army => army.EffectivePower).ThenByDescending(army => army.Initiative).ToList();
    foreach(var army in orderedGroups) {
      var availableTargets = armies.Where(it => it.Side != army.Side).Except(selected).ToList();
      var preferredTargets = availableTargets.Where(t => t.Weaknesses.Contains(army.AttackElement)).ToList();
      if (preferredTargets.Count == 0) {
        preferredTargets = availableTargets.Where(t => !t.Immunities.Contains(army.AttackElement)).ToList();
      }
      if (preferredTargets.Count == 0) {
        preferredTargets = availableTargets.Where(t => t.Immunities.Contains(army.AttackElement)).ToList();
      }
      var target = preferredTargets.OrderByDescending(t => t.EffectivePower).ThenByDescending(t => t.Initiative).Take(1).ToList();
      if (target.Count == 1) {
        selectedTargets[army.Id] = target[0].Id;
        selected.Add(target[0]);
      }
    }

    // Deal damage phase
    foreach(var id in armies.OrderByDescending(it => it.Initiative).Select(it => it.Id)) {
      if (!idToArmy.TryGetValue(id, out var attacker)) continue; // group has been destroyed
      if (!selectedTargets.TryGetValue(id, out var targetId)) continue; // group did not select a target
      var target = idToArmy[targetId];
      var damage = attacker.EffectivePower * (target.Weaknesses.Contains(attacker.AttackElement) ? 2 : 1) * (target.Immunities.Contains(attacker.AttackElement) ? 0 : 1);
      var losses = damage / target.HitPointsPerUnit;
      if (losses >= target.Units) idToArmy.Remove(targetId);
      else idToArmy[targetId] = target with {Units = target.Units - losses};
    }
    return [.. idToArmy.Values];
  }

  public record ArmyGroup(long Id, long Side, long Units, long HitPointsPerUnit, List<string> Immunities, List<string> Weaknesses, 
    long AttackDamage, string AttackElement, long Initiative)
    {
      public long EffectivePower => Units * AttackDamage;
    }

  private static List<ArmyGroup> Convert(string data)
  {
    long id = 1;
    long side = 1;
    var listOfWords = P.Word.Trim().Plus(",");
    var immunities1 = P.Format("immune to {}; weak to {}", listOfWords, listOfWords) 
      | P.Format("weak to {}; immune to {}", listOfWords, listOfWords).Select(it => (First: it.Second, Second: it.First))
      | P.Format("immune to {}", listOfWords).Select(it => (First: it, Second: new List<string>()))
      | P.Format("weak to {}", listOfWords).Select(it => (First: new List<string>(), Second: it));
    var immunities = P.Format("({})", immunities1.Require()).Optional().Select(it => it.Count == 1 ? it[0] : (First: [], Second: []));
    var group = P.Format("{} units each with {} hit points {} with an attack that does {} {} damage at initiative {}",
      P.Long, P.Long, immunities, P.Long, P.Word, P.Long).Select(it => new ArmyGroup(id++, side, it.First, it.Second, it.Third.First, it.Third.Second, it.Fourth, it.Fifth, it.Sixth));
    var army = group.Plus();
    var complete = P.Format("Immune System: {} Infection: {}", army.Select(it => {side += 1; return it;}), army);
    return complete.Before(P.EndOfInput).Select(it => it.First.Concat(it.Second).ToList()).Parse(data);
  }
}

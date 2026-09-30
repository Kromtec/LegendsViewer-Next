using LegendsViewer.Backend.Legends.Interfaces;
using System.Text;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;
using LegendsViewer.Backend.Utilities;

namespace LegendsViewer.Backend.Legends.Parser;

public class HistoryParser : IDisposable
{
    private readonly IWorld _world;
    private readonly StreamReader _history;
    private readonly StringBuilder _log;

    private string _currentLine = string.Empty;
    private Entity? _currentCiv;

    public HistoryParser(IWorld world, string historyFile)
    {
        _world = world;
        _history = new StreamReader(historyFile, Encoding.UTF8);
        _log = new StringBuilder();
    }

    private bool CivStart()
    {
        return !_currentLine.StartsWith(" ");
    }

    private void SkipAnimalPeople()
    {
        while (!_currentLine.Contains(",") || _currentLine.StartsWith(" "))
        {
            ReadLine();
        }
    }

    private void SkipToNextCiv()
    {
        while (!_history.EndOfStream && !CivStart())
        {
            ReadLine();
        }
    }

    private void ReadLine()
    {
        _currentLine = _history.ReadLine() ?? string.Empty;
    }

    private bool ReadCiv()
    {
        string civName = _currentLine.Substring(0, _currentLine.IndexOf(",", StringComparison.Ordinal));
        CreatureInfo civRace = _world.GetCreatureInfo(_currentLine.Substring(_currentLine.IndexOf(",", StringComparison.Ordinal) + 2,
            _currentLine.Length - _currentLine.IndexOf(",", StringComparison.Ordinal) - 2).ToLower());
        var entities = _world.Entities
            .Where(entity => string.Equals(entity.Name, civName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (entities.Count == 1)
        {
            _currentCiv = entities[0];
        }
        else if (entities.Count == 0)
        {
            _world.ParsingErrors.Report($"Couldn\'t Find Entity by Name:\n{civName}");
        }
        else
        {
            var possibleEntities = entities.Where(entity => entity.Race == civRace).ToList();
            if (possibleEntities.Count == 1)
            {
                _currentCiv = possibleEntities[0];
            }
            else if (possibleEntities.Count == 0)
            {
                _world.ParsingErrors.Report($"Couldn\'t Find Entity by Name and Race:\n{civName}");
            }
            else
            {
                var possibleCivilizations = possibleEntities.Where(entity => entity.EntityType == EntityType.Civilization).ToList();
                if (possibleCivilizations.Count == 1)
                {
                    _currentCiv = possibleEntities[0];
                }
                else
                {
#if DEBUG
                    _world.ParsingErrors.Report($"Ambiguous Civilizations ({entities.Count}) Name:\n{civName}");
#endif
                }
            }
        }
        if (_currentCiv != null)
        {
            _currentCiv.Race = civRace;
            foreach (Entity group in _currentCiv.Groups)
            {
                group.Race = _currentCiv.Race;
            }

            _currentCiv.IsCiv = true;
            _currentCiv.EntityType = EntityType.Civilization;
        }
        ReadLine();
        return true;
    }

    private void ReadWorships()
    {
        if (_currentLine.Contains("Worship List"))
        {
            ReadLine();
            while (_currentLine?.StartsWith("  ") == true)
            {
                if (_currentCiv != null)
                {
                    string deityName = Formatting.InitCaps(Formatting.ReplaceNonAscii(_currentLine.Substring(2,
                        _currentLine.IndexOf(",", StringComparison.Ordinal) - 2)));
                    var deities = _world.HistoricalFigures.Where(h => h.Name.Equals(deityName.Replace("'", "`"), StringComparison.OrdinalIgnoreCase) && (h.IsDeity || h.Force)).ToList();
                    if (deities.Count == 1)
                    {
                        var deity = deities[0];
                        deity.WorshippedBy = _currentCiv;
                        if (!_currentCiv.Worshipped.Contains(deity))
                        {
                            _currentCiv.Worshipped.Add(deity);
                        }
                    }
                    else if (deities.Count == 0)
                    {
                        _world.ParsingErrors.Report($"Couldn\'t Find Deity:\n{deityName}, Deity of {_currentCiv.Name}");
                    }
#if DEBUG
                    else
                    {
                        _world.ParsingErrors.Report($"Ambiguous ({deities.Count}) Deity Name:\n{deityName}, Deity of {_currentCiv.Name}");
                    }
#endif
                }
                ReadLine();
            }
        }
    }

    private bool LeaderStart()
    {
        return !_history.EndOfStream && _currentLine.Contains(" List") && !_currentLine.Contains("[*]") && !_currentLine.Contains("%");
    }

    private void ReadLeaders()
    {
        while (LeaderStart())
        {
            string leaderTypeString = _currentLine?.Substring(1, _currentLine.IndexOf("List", StringComparison.Ordinal) - 2) ?? "Unknown LeaderType";
            string leaderType = Formatting.InitCaps(leaderTypeString);
            _currentCiv?.LeaderTypes.Add(leaderType);
            _currentCiv?.Leaders.Add([]);
            ReadLine();
            while (_currentLine?.StartsWith("  ") == true)
            {
                if (_currentCiv != null && _currentLine.Contains("[*]"))
                {
                    string leaderName = Formatting.ReplaceNonAscii(_currentLine.Substring(_currentLine.IndexOf("[*]", StringComparison.Ordinal) + 4,
                        _currentLine.IndexOf("(b", StringComparison.Ordinal) - _currentLine.IndexOf("[*]", StringComparison.Ordinal) - 5));
                    var leaders = _world.HistoricalFigures.Where(hf =>
                            string.Equals(hf.Name, leaderName.Replace("'", "`"), StringComparison.OrdinalIgnoreCase)).ToList();
                    if (leaders.Count == 1)
                    {
                        var leader = leaders[0];
                        int reignBegan = Convert.ToInt32(_currentLine.Substring(_currentLine.IndexOf(":", StringComparison.Ordinal) + 2,
                            _currentLine.IndexOf("), ", StringComparison.Ordinal) - _currentLine.IndexOf(":", StringComparison.Ordinal) - 2));
                        if (_currentCiv.Leaders[_currentCiv.LeaderTypes.Count - 1].Count > 0) //End of previous leader's reign
                        {
                            HistoricalFigure? lastLeader = _currentCiv.Leaders[_currentCiv.LeaderTypes.Count - 1].LastOrDefault();
                            HfPosition? lastLeadersLastHfPosition = lastLeader?.Positions?.OrderBy(p => p.StartYear ?? -1).LastOrDefault();
                            if (lastLeader != null && lastLeadersLastHfPosition != null) //End of leader's last leader position (move up rank etc.)
                            {
                                lastLeader.EndPositionAssignment(_currentCiv, reignBegan - 1, lastLeadersLastHfPosition.PositionId, lastLeadersLastHfPosition.Title);
                            }
                        }

                        if (leader.Positions?.Count > 0) //End of leader's last leader position (move up rank etc.)
                        {
                            HfPosition? lastHfPosition = leader.Positions.OrderBy(p => p.StartYear ?? -1).LastOrDefault(p => !string.Equals(p.Title, leaderType, StringComparison.OrdinalIgnoreCase));
                            if (lastHfPosition != null) //End of leader's last leader position (move up rank etc.)
                            {
                                leader.EndPositionAssignment(_currentCiv, reignBegan - 1, lastHfPosition.PositionId, lastHfPosition.Title);
                            }
                        }

                        leader.StartPositionAssignment(_currentCiv, reignBegan, 0, leaderType);
                        _currentCiv.Leaders[_currentCiv.LeaderTypes.Count - 1].Add(leader);
                    }
                    else if (leaders.Count == 0)
                    {
                        _world.ParsingErrors.Report($"Couldn\'t Find Leader:\n{leaderName}, Leader of {_currentCiv.Name}");
                    }
#if DEBUG
                    else
                    {
                        _world.ParsingErrors.Report($"Ambiguous ({leaders.Count}) Leader Name:\n{leaderName}, Leader of {_currentCiv.Name}");
                    }
#endif
                }
                ReadLine();
            }
        }
    }

    public string Parse()
    {
        string? name = _history.ReadLine();
        _world.Name = Formatting.ReplaceNonAscii(name ?? "Unknown World");
        _world.AlternativeName = _history.ReadLine() ?? "";
        ReadLine();
        SkipAnimalPeople();

        while (!_history.EndOfStream)
        {
            if (ReadCiv())
            {
                ReadWorships();
                ReadLeaders();
            }
            else
            {
                SkipToNextCiv();
            }
        }
        if (!string.IsNullOrEmpty(_currentLine) && CivStart())
        {
            ReadCiv();
        }

        _history.Close();

        return _log.ToString();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _history.Dispose();
        }
    }
}


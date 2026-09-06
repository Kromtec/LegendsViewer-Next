using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.EventCollections;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.WorldObjects;
using LegendsViewer.Backend.Utilities;

namespace LegendsViewer.Backend.Legends.Various;

public static class WorldRecordsCalculator
{
    public static WorldRecordsDto Calculate(World world)
    {
        var dto = new WorldRecordsDto();

        // 1. WARFARE
        var warfareCategory = new RecordCategoryDto
        {
            Id = "warfare",
            Title = "Warfare & Combat",
            Icon = "mdi-sword-cross",
            Cards =
            [
                CalculateTopSlayers(world),
                CalculateBloodiestBattles(world),
                CalculateFiercestDuelists(world),
                CalculateDevastatingRampages(world)
            ]
        };

        // 2. SOVEREIGNS
        var sovereignsCategory = new RecordCategoryDto
        {
            Id = "sovereigns",
            Title = "Sovereigns & Empires",
            Icon = "mdi-crown",
            Cards =
            [
                CalculateLongestReigns(world),
                CalculateShortestReigns(world),
                CalculateMostContestedSites(world),
                CalculateLargestEmpires(world)
            ]
        };

        // 3. LORE & ARTS
        var loreCategory = new RecordCategoryDto
        {
            Id = "lore",
            Title = "Lore, Science & Arts",
            Icon = "mdi-book-open-page-variant",
            Cards =
            [
                CalculateGreatestScholars(world),
                CalculateProlificAuthors(world),
                CalculateMostCopiedWorks(world)
            ]
        };

        // 4. INTRIGUE
        var intrigueCategory = new RecordCategoryDto
        {
            Id = "intrigue",
            Title = "Conspiracies & Crime",
            Icon = "mdi-incognito",
            Cards =
            [
                CalculateMastersOfDisguise(world),
                CalculateNotoriousSyndicates(world)
            ]
        };

        // 5. LONGEVITY
        var longevityCategory = new RecordCategoryDto
        {
            Id = "longevity",
            Title = "Longevity & Curiosities",
            Icon = "mdi-timelapse",
            Cards =
            [
                CalculateOldestLiving(world),
                CalculateMostTravelled(world),
                CalculateOldestArtifacts(world)
            ]
        };

        // 6. GEOGRAPHY & WORLD WONDERS
        var geographyCategory = new RecordCategoryDto
        {
            Id = "geography",
            Title = "Geography & World Wonders",
            Icon = "mdi-earth",
            Cards =
            [
                CalculateLargestRegions(world),
                CalculateLargestUndergroundRegions(world),
                CalculateBloodiestRegions(world),
                CalculateLongestRivers(world),
                CalculateHighestPeaks(world)
            ]
        };

        dto.Categories = [warfareCategory, sovereignsCategory, loreCategory, intrigueCategory, longevityCategory, geographyCategory];

        // Highlights for top banner
        dto.Highlights = GenerateHighlights(dto);

        return dto;
    }

    private static List<RecordHighlightDto> GenerateHighlights(WorldRecordsDto dto)
    {
        var highlights = new List<RecordHighlightDto>();

        var topSlayer = dto.Categories.FirstOrDefault(c => c.Id == "warfare")
            ?.Cards.FirstOrDefault(c => c.Id == "top_slayers")
            ?.Entries.FirstOrDefault();

        if (topSlayer != null)
        {
            highlights.Add(new RecordHighlightDto
            {
                Title = "Deadliest Creature",
                Value = topSlayer.Value,
                Icon = "mdi-skull",
                LinkHtml = topSlayer.LinkHtml
            });
        }

        var topReign = dto.Categories.FirstOrDefault(c => c.Id == "sovereigns")
            ?.Cards.FirstOrDefault(c => c.Id == "longest_reigns")
            ?.Entries.FirstOrDefault();

        if (topReign != null)
        {
            highlights.Add(new RecordHighlightDto
            {
                Title = "Longest Reign",
                Value = topReign.Value,
                Icon = "mdi-crown",
                LinkHtml = topReign.LinkHtml
            });
        }

        var bloodiestBattle = dto.Categories.FirstOrDefault(c => c.Id == "warfare")
            ?.Cards.FirstOrDefault(c => c.Id == "bloodiest_battles")
            ?.Entries.FirstOrDefault();

        if (bloodiestBattle != null)
        {
            highlights.Add(new RecordHighlightDto
            {
                Title = "Bloodiest Battle",
                Value = bloodiestBattle.Value,
                Icon = "mdi-sword-cross",
                LinkHtml = bloodiestBattle.LinkHtml
            });
        }

        var oldestLiving = dto.Categories.FirstOrDefault(c => c.Id == "longevity")
            ?.Cards.FirstOrDefault(c => c.Id == "oldest_living")
            ?.Entries.FirstOrDefault();

        if (oldestLiving != null)
        {
            highlights.Add(new RecordHighlightDto
            {
                Title = "Oldest Living Being",
                Value = oldestLiving.Value,
                Icon = "mdi-timelapse",
                LinkHtml = oldestLiving.LinkHtml
            });
        }

        return highlights;
    }

    private static RecordCardDto CalculateTopSlayers(World world)
    {
        var card = new RecordCardDto
        {
            Id = "top_slayers",
            Title = "Deadliest Slayers & Megabeasts",
            Icon = "mdi-skull",
            MetricName = "Notable Kills"
        };

        var topHfs = world.HistoricalFigures
            .Where(hf => hf.NotableKills.Count > 0)
            .OrderByDescending(hf => hf.NotableKills.Count)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var hf in topHfs)
        {
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = hf.Id,
                Name = hf.Name,
                ObjectType = "hf",
                LinkHtml = hf.ToLink(),
                Value = $"{hf.NotableKills.Count} kills",
                NumericValue = hf.NotableKills.Count,
                Subtitle = GetHistoricalFigureSubtitle(hf),
                DetailText = $"Slain notable figures: {string.Join(", ", hf.NotableKills.Take(3).Select(k => k.HistoricalFigure?.Name ?? "Unknown"))}"
            });
        }

        return card;
    }

    private static RecordCardDto CalculateBloodiestBattles(World world)
    {
        var card = new RecordCardDto
        {
            Id = "bloodiest_battles",
            Title = "Bloodiest Battles & Sieges",
            Icon = "mdi-sword-cross",
            MetricName = "Casualties"
        };

        var topBattles = world.Battles
            .Where(b => b.DeathCount > 0)
            .OrderByDescending(b => b.DeathCount)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var b in topBattles)
        {
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = b.Id,
                Name = b.Name,
                ObjectType = "battle",
                LinkHtml = b.ToLink(),
                Value = $"{b.DeathCount} deaths",
                NumericValue = b.DeathCount,
                Subtitle = $"{b.Attacker?.Name ?? "Attacker"} vs {b.Defender?.Name ?? "Defender"}",
                DetailText = $"Attacker deaths: {b.AttackerDeathCount}, Defender deaths: {b.DefenderDeathCount}"
            });
        }

        return card;
    }

    private static RecordCardDto CalculateFiercestDuelists(World world)
    {
        var card = new RecordCardDto
        {
            Id = "fiercest_duelists",
            Title = "Fiercest Duelists",
            Icon = "mdi-sword",
            MetricName = "Duels Fought"
        };

        var duelsByHf = world.Duels
            .SelectMany(d => d.GetSubEvents().OfType<HfDied>().Select(e => e.Slayer).Where(hf => hf != null))
            .GroupBy(hf => hf!)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var group in duelsByHf)
        {
            var hf = group.Key;
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = hf.Id,
                Name = hf.Name,
                ObjectType = "hf",
                LinkHtml = hf.ToLink(),
                Value = $"{group.Count()} duel victories",
                NumericValue = group.Count(),
                Race = hf.GetRaceString(),
                Subtitle = GetHistoricalFigureSubtitle(hf)
            });
        }

        return card;
    }

    private static RecordCardDto CalculateDevastatingRampages(World world)
    {
        var card = new RecordCardDto
        {
            Id = "devastating_rampages",
            Title = "Most Devastating Rampages",
            Icon = "mdi-fire",
            MetricName = "Victims"
        };

        var rampages = world.BeastAttacks
            .OrderByDescending(b => b.DeathCount)
            .ThenByDescending(b => b.EventCount)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var r in rampages)
        {
            int val = r.DeathCount > 0 ? r.DeathCount : r.EventCount;
            string valStr = r.DeathCount > 0 ? $"{r.DeathCount} kills" : $"{r.EventCount} events";

            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = r.Id,
                Name = r.Name,
                ObjectType = "beastattack",
                LinkHtml = r.ToLink(),
                Value = valStr,
                NumericValue = val,
                Subtitle = r.StartDate
            });
        }

        return card;
    }

    private static RecordCardDto CalculateLongestReigns(World world)
    {
        var card = new RecordCardDto
        {
            Id = "longest_reigns",
            Title = "Longest Reigning Leaders",
            Icon = "mdi-crown",
            MetricName = "Reign Duration"
        };

        var hfReigns = new List<(HistoricalFigure hf, HfPosition pos, int duration)>();

        foreach (var hf in world.HistoricalFigures)
        {
            if (hf.Positions == null) continue;
            foreach (var pos in hf.Positions)
            {
                if (IsMonarchOrCivLeader(pos) && pos.StartYear.HasValue)
                {
                    int end = pos.EndYear ?? (hf.DeathYear != -1 ? hf.DeathYear : world.CurrentYear);
                    int duration = end - pos.StartYear.Value;
                    if (duration > 0)
                    {
                        hfReigns.Add((hf, pos, duration));
                    }
                }
            }
        }

        var topReigns = hfReigns
            .OrderByDescending(r => r.duration)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var (hf, pos, duration) in topReigns)
        {
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = hf.Id,
                Name = hf.Name,
                ObjectType = "hf",
                LinkHtml = hf.ToLink(),
                Value = $"{duration} years",
                NumericValue = duration,
                Race = hf.GetRaceString(),
                Subtitle = GetPositionSubtitle(pos, hf),
                DetailText = pos.PrintReign(hf)
            });
        }

        return card;
    }

    private static RecordCardDto CalculateShortestReigns(World world)
    {
        var card = new RecordCardDto
        {
            Id = "shortest_reigns",
            Title = "Shortest Reigning Leaders",
            Icon = "mdi-emoticon-sad-outline",
            MetricName = "Reign Duration"
        };

        var hfReigns = new List<(HistoricalFigure hf, HfPosition pos, int duration)>();

        foreach (var hf in world.HistoricalFigures)
        {
            if (hf.Positions == null) continue;
            foreach (var pos in hf.Positions)
            {
                if (IsMonarchOrCivLeader(pos) && pos.StartYear.HasValue)
                {
                    int? end = pos.EndYear ?? (hf.DeathYear != -1 ? hf.DeathYear : null);
                    if (end.HasValue)
                    {
                        int duration = end.Value - pos.StartYear.Value;
                        if (duration >= 0)
                        {
                            hfReigns.Add((hf, pos, duration));
                        }
                    }
                }
            }
        }

        var shortestReigns = hfReigns
            .OrderBy(r => r.duration)
            .ThenBy(r => r.hf.Id)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var (hf, pos, duration) in shortestReigns)
        {
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = hf.Id,
                Name = hf.Name,
                ObjectType = "hf",
                LinkHtml = hf.ToLink(),
                Value = duration == 0 ? "< 1 year" : $"{duration} year(s)",
                NumericValue = duration,
                Race = hf.GetRaceString(),
                Subtitle = GetPositionSubtitle(pos, hf),
                DetailText = pos.PrintReign(hf)
            });
        }

        return card;
    }

    private static RecordCardDto CalculateMostContestedSites(World world)
    {
        var card = new RecordCardDto
        {
            Id = "contested_sites",
            Title = "Most Contested Sites & Cities",
            Icon = "mdi-castle",
            MetricName = "Conquests"
        };

        var sites = world.Sites
            .Select(s => new { Site = s, Conquerings = s.Events.OfType<SiteTakenOver>().Count() + s.Events.OfType<CreatedSite>().Count() })
            .Where(s => s.Conquerings > 0)
            .OrderByDescending(s => s.Conquerings)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var s in sites)
        {
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = s.Site.Id,
                Name = s.Site.Name,
                ObjectType = "site",
                LinkHtml = s.Site.ToLink(),
                Value = $"{s.Conquerings} conquest(s)",
                NumericValue = s.Conquerings,
                Subtitle = s.Site.Type
            });
        }

        return card;
    }

    private static RecordCardDto CalculateLargestEmpires(World world)
    {
        var card = new RecordCardDto
        {
            Id = "largest_empires",
            Title = "Largest Civilizations & Empires",
            Icon = "mdi-shield-crown-outline",
            MetricName = "Sites Controlled"
        };

        var civs = world.Entities
            .Where(e => e.IsCiv && e.SiteHistory.Count > 0)
            .OrderByDescending(e => e.SiteHistory.Count)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var c in civs)
        {
            string raceStr = c.Race != null && c.Race != CreatureInfo.Unknown ? $" • {c.Race.NameSingular}" : string.Empty;
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = c.Id,
                Name = c.Name,
                ObjectType = "entity",
                LinkHtml = c.ToLink(),
                Value = $"{c.SiteHistory.Count} sites",
                NumericValue = c.SiteHistory.Count,
                Race = c.Race?.NameSingular ?? string.Empty,
                Subtitle = $"{c.Type}{raceStr}"
            });
        }

        return card;
    }

    private static RecordCardDto CalculateMostCopiedWorks(World world)
    {
        var card = new RecordCardDto
        {
            Id = "most_copied_works",
            Title = "Most Copied Manuscripts & Books",
            Icon = "mdi-book-multiple",
            MetricName = "Copies / References"
        };

        var works = world.WrittenContents
            .Select(w => new { Work = w, EventCount = w.EventCount })
            .Where(w => w.EventCount > 0)
            .OrderByDescending(w => w.EventCount)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var w in works)
        {
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = w.Work.Id,
                Name = w.Work.Name,
                ObjectType = "writtencontent",
                LinkHtml = w.Work.ToLink(),
                Value = $"{w.EventCount} recorded events",
                NumericValue = w.EventCount,
                Subtitle = w.Work.Author?.Name != null ? $"By {w.Work.Author.Name}" : "Anonymous"
            });
        }

        return card;
    }

    private static RecordCardDto CalculateProlificAuthors(World world)
    {
        var card = new RecordCardDto
        {
            Id = "prolific_authors",
            Title = "Most Prolific Authors",
            Icon = "mdi-feather",
            MetricName = "Written Works"
        };

        var authors = world.WrittenContents
            .Where(w => w.Author != null)
            .GroupBy(w => w.Author!)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var g in authors)
        {
            var author = g.Key;
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = author.Id,
                Name = author.Name,
                ObjectType = "hf",
                LinkHtml = author.ToLink(),
                Value = $"{g.Count()} works written",
                NumericValue = g.Count(),
                Race = author.GetRaceString(),
                Subtitle = GetHistoricalFigureSubtitle(author)
            });
        }

        return card;
    }

    private static RecordCardDto CalculateGreatestScholars(World world)
    {
        var card = new RecordCardDto
        {
            Id = "greatest_scholars",
            Title = "Greatest Scholars & Discoverers",
            Icon = "mdi-school",
            MetricName = "Discoveries"
        };

        var scholars = world.Events.OfType<KnowledgeDiscovered>()
            .Where(k => k.HistoricalFigure != null)
            .GroupBy(k => k.HistoricalFigure!)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var g in scholars)
        {
            var hf = g.Key;
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = hf.Id,
                Name = hf.Name,
                ObjectType = "hf",
                LinkHtml = hf.ToLink(),
                Value = $"{g.Count()} discoveries",
                NumericValue = g.Count(),
                Race = hf.GetRaceString(),
                Subtitle = GetHistoricalFigureSubtitle(hf)
            });
        }

        return card;
    }

    private static RecordCardDto CalculateMastersOfDisguise(World world)
    {
        var card = new RecordCardDto
        {
            Id = "masters_of_disguise",
            Title = "Masters of Disguise & Impostors",
            Icon = "mdi-incognito",
            MetricName = "Assumed Identities"
        };

        var tricksters = world.HistoricalFigures
            .Where(hf => hf.UsedIdentityIds.Count > 0 || hf.Identities.Count > 0)
            .OrderByDescending(hf => Math.Max(hf.UsedIdentityIds.Count, hf.Identities.Count))
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var hf in tricksters)
        {
            int count = Math.Max(hf.UsedIdentityIds.Count, hf.Identities.Count);
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = hf.Id,
                Name = hf.Name,
                ObjectType = "hf",
                LinkHtml = hf.ToLink(),
                Value = $"{count} identity disguises",
                NumericValue = count,
                Race = hf.GetRaceString(),
                Subtitle = GetHistoricalFigureSubtitle(hf)
            });
        }

        return card;
    }

    private static RecordCardDto CalculateNotoriousSyndicates(World world)
    {
        var card = new RecordCardDto
        {
            Id = "notorious_syndicates",
            Title = "Most Active Crime Syndicates",
            Icon = "mdi-handcuffs",
            MetricName = "Heists & Crimes"
        };

        var crimeCounts = new Dictionary<int, (Entity Entity, int Crimes)>();

        void AddCrime(Entity? attacker)
        {
            if (attacker == null) return;
            if (!crimeCounts.TryGetValue(attacker.Id, out var tuple))
            {
                crimeCounts[attacker.Id] = (attacker, 1);
            }
            else
            {
                crimeCounts[attacker.Id] = (attacker, tuple.Crimes + 1);
            }
        }

        // 1. Theft event collections (Attacker is offender entity)
        foreach (var theft in world.Thefts)
        {
            if (theft.Attacker != null)
            {
                AddCrime(theft.Attacker);
            }
        }

        // 2. Abduction event collections (Attacker is offender entity)
        foreach (var abduction in world.Abductions)
        {
            if (abduction.Attacker != null)
            {
                AddCrime(abduction.Attacker);
            }
        }

        // 3. ItemStolen events not in a Theft collection (Thief's entity is offender)
        foreach (var itemStolen in world.Events.OfType<ItemStolen>())
        {
            if (itemStolen.ParentCollection is not Theft && itemStolen.Thief != null)
            {
                var offenderEntity = GetPerpetratorEntity(itemStolen.Thief);
                if (offenderEntity != null)
                {
                    AddCrime(offenderEntity);
                }
            }
        }

        // 4. Intrigue & corruption events (Corruptor's entity is offender)
        foreach (var intrigue in world.Events.OfType<FailedIntrigueCorruption>())
        {
            var offenderEntity = GetPerpetratorEntity(intrigue.CorruptorHf);
            if (offenderEntity != null)
            {
                AddCrime(offenderEntity);
            }
        }

        foreach (var intrigue in world.Events.OfType<HfsFormedIntrigueRelationship>())
        {
            var offenderEntity = GetPerpetratorEntity(intrigue.CorruptorHf);
            if (offenderEntity != null)
            {
                AddCrime(offenderEntity);
            }
        }

        var topCrimeEntities = crimeCounts.Values
            .Where(x => x.Crimes > 0)
            .OrderByDescending(x => x.Crimes)
            .ThenBy(x => x.Entity.Id)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var (entity, crimeCount) in topCrimeEntities)
        {
            string raceStr = entity.Race != null && entity.Race != CreatureInfo.Unknown ? $" • {entity.Race.NameSingular}" : string.Empty;
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = entity.Id,
                Name = entity.Name,
                ObjectType = "entity",
                LinkHtml = entity.ToLink(),
                Value = $"{crimeCount} crime(s)",
                NumericValue = crimeCount,
                Subtitle = $"{entity.Type}{raceStr}"
            });
        }

        return card;
    }

    private static Entity? GetPerpetratorEntity(HistoricalFigure? hf)
    {
        if (hf == null) return null;

        var posEntity = hf.Positions?.FirstOrDefault(p => p.Entity != null)?.Entity;
        if (posEntity != null) return posEntity;

        var linkEntity = hf.RelatedEntities?.FirstOrDefault(l => l.Entity != null)?.Entity;
        if (linkEntity != null) return linkEntity;

        return null;
    }

    private static RecordCardDto CalculateOldestLiving(World world)
    {
        var card = new RecordCardDto
        {
            Id = "oldest_living",
            Title = "Venerable Ancients (Oldest Living)",
            Icon = "mdi-timelapse",
            MetricName = "Age"
        };

        var ancients = world.HistoricalFigures
            .Where(hf => hf.IsAlive && hf.Age > 0 && !hf.IsDeity)
            .OrderByDescending(hf => hf.Age)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var hf in ancients)
        {
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = hf.Id,
                Name = hf.Name,
                ObjectType = "hf",
                LinkHtml = hf.ToLink(),
                Value = $"{hf.Age} years old",
                NumericValue = hf.Age,
                Race = hf.GetRaceString(),
                IsSupernatural = hf.IsDeity || hf.Force || HistoricalFigureExtensions.IsVampire(hf) || HistoricalFigureExtensions.IsNecromancer(hf),
                Subtitle = GetHistoricalFigureSubtitle(hf)
            });
        }

        return card;
    }

    private static RecordCardDto CalculateMostTravelled(World world)
    {
        var card = new RecordCardDto
        {
            Id = "most_travelled",
            Title = "Most Travelled Adventurers",
            Icon = "mdi-compass",
            MetricName = "Journeys / Travels"
        };

        var travellers = world.Events.OfType<HfTravel>()
            .Where(t => t.HistoricalFigure != null)
            .GroupBy(t => t.HistoricalFigure!)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var g in travellers)
        {
            var hf = g.Key;
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = hf.Id,
                Name = hf.Name,
                ObjectType = "hf",
                LinkHtml = hf.ToLink(),
                Value = $"{g.Count()} travel journeys",
                NumericValue = g.Count(),
                Race = hf.GetRaceString(),
                Subtitle = GetHistoricalFigureSubtitle(hf)
            });
        }

        return card;
    }

    private static RecordCardDto CalculateOldestArtifacts(World world)
    {
        var card = new RecordCardDto
        {
            Id = "oldest_artifacts",
            Title = "Most Historic Relics & Artifacts",
            Icon = "mdi-diamond-stone",
            MetricName = "Event Count"
        };

        var artifacts = world.Artifacts
            .Where(a => a.EventCount > 0)
            .OrderByDescending(a => a.EventCount)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var a in artifacts)
        {
            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = a.Id,
                Name = a.Name,
                ObjectType = "artifact",
                LinkHtml = a.ToLink(),
                Value = $"{a.EventCount} events",
                NumericValue = a.EventCount,
                Subtitle = GetArtifactSubtitle(a)
            });
        }

        return card;
    }

    private static string GetArtifactSubtitle(Artifact a)
    {
        string typeStr = !string.IsNullOrWhiteSpace(a.Subtype) ? a.Subtype : (!string.IsNullOrWhiteSpace(a.Type) ? a.Type : "Artifact");

        if (a.Holder != null)
        {
            return $"{typeStr} held by {a.Holder.Name}";
        }
        if (a.Structure != null)
        {
            return $"{typeStr} stored in {a.Structure.Name}";
        }
        if (a.Site != null)
        {
            return $"{typeStr} stored in {a.Site.Name}";
        }
        if (a.Region != null)
        {
            return $"{typeStr} located in {a.Region.Name}";
        }

        return typeStr;
    }

    private static string GetHistoricalFigureSubtitle(HistoricalFigure hf)
    {
        var topSkill = hf.SkillDescriptions.FirstOrDefault();

        string specialRole = string.Empty;
        if (HistoricalFigureExtensions.IsVampire(hf)) specialRole = "Vampire";
        else if (HistoricalFigureExtensions.IsNecromancer(hf)) specialRole = "Necromancer";
        else if (hf.IsDeity) specialRole = "Deity";
        else if (hf.Force) specialRole = "Force of Nature";
        else if (!string.IsNullOrWhiteSpace(hf.AssociatedType) && !string.Equals(hf.AssociatedType, "Standard", StringComparison.OrdinalIgnoreCase))
        {
            specialRole = hf.AssociatedType;
        }

        string skillStr = (topSkill != null && topSkill.Name != "None" && topSkill.Points > 0)
            ? $"{topSkill.Rank} {topSkill.Name}"
            : string.Empty;

        string positionStr = (hf.Positions?.Count > 0)
            ? hf.Positions.First().PrintTitle(false, hf)
            : string.Empty;

        string spheresStr = string.Empty;
        if (hf.Spheres?.Count > 0)
        {
            var formattedSpheres = hf.Spheres.Select(s => Formatting.InitCaps(s));
            spheresStr = hf.Spheres.Count == 1
                ? $"Sphere: {formattedSpheres.First()}"
                : $"Spheres: {string.Join(", ", formattedSpheres)}";
        }

        List<string> parts = new();

        if (!string.IsNullOrEmpty(specialRole))
        {
            parts.Add(specialRole);
        }

        if (!string.IsNullOrEmpty(positionStr))
        {
            if (string.IsNullOrEmpty(specialRole) || !positionStr.Equals(specialRole, StringComparison.OrdinalIgnoreCase))
            {
                parts.Add(positionStr);
            }
        }

        if (!string.IsNullOrEmpty(skillStr))
        {
            parts.Add(skillStr);
        }

        if (!string.IsNullOrEmpty(spheresStr))
        {
            parts.Add(spheresStr);
        }

        return parts.Count > 0 ? string.Join(" • ", parts) : string.Empty;
    }

    private static bool IsMonarchOrCivLeader(HfPosition pos)
    {
        if (pos.PositionId == 0) return true;
        if (pos.Entity != null)
        {
            var ep = pos.Entity.EntityPositions.FirstOrDefault(p => string.Equals(p.Name, pos.Title, StringComparison.OrdinalIgnoreCase));
            if (ep != null && ep.Id == 0) return true;
        }
        string title = pos.Title.ToLower();
        return title.Contains("monarch") || title.Contains("king") || title.Contains("queen") ||
               title.Contains("emperor") || title.Contains("empress") || title.Contains("ruler") ||
               title.Contains("overlord") || title.Contains("high priest") || title.Contains("law-giver") ||
               title.Contains("lawgiver");
    }

    private static string GetPositionSubtitle(HfPosition pos, HistoricalFigure hf)
    {
        string title = pos.PrintTitle(false, hf);
        if (pos.Entity != null && !pos.Entity.IsCiv && pos.Entity.CurrentCiv != null && !string.Equals(pos.Entity.CurrentCiv.Name, pos.Entity.Name, StringComparison.OrdinalIgnoreCase))
        {
            title += $" ({pos.Entity.CurrentCiv.Name})";
        }
        return title;
    }

    private static RecordCardDto CalculateHighestPeaks(World world)
    {
        var card = new RecordCardDto
        {
            Id = "highest_peaks",
            Title = "Highest Mountain Peaks & Volcanoes",
            Icon = "mdi-image-filter-hdr",
            MetricName = "Height (meters)"
        };

        var peaks = world.MountainPeaks
            .OrderByDescending(p => p.Height)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var p in peaks)
        {
            string regionStr = p.Region != null ? p.Region.Name : string.Empty;
            string typeStr = p.IsVolcano ? "Volcano" : "Mountain Peak";
            string subtitle = !string.IsNullOrEmpty(regionStr) ? $"{typeStr} • in {regionStr}" : typeStr;

            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = p.Id,
                Name = p.Name,
                ObjectType = "mountainpeak",
                LinkHtml = p.ToLink(),
                Value = p.HeightMeter,
                NumericValue = p.Height,
                Subtitle = subtitle
            });
        }

        return card;
    }

    private static RecordCardDto CalculateLongestRivers(World world)
    {
        var card = new RecordCardDto
        {
            Id = "longest_rivers",
            Title = "Longest Rivers",
            Icon = "mdi-waves",
            MetricName = "Length (tiles)"
        };

        var rivers = world.Rivers
            .Where(r => r.Coordinates.Count > 0)
            .OrderByDescending(r => r.Coordinates.Count)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var r in rivers)
        {
            var regionNames = new List<string>();
            foreach (var loc in r.Coordinates)
            {
                if (world.WorldGrid.TryGetValue(loc, out var region) && region != null)
                {
                    if (regionNames.Count == 0 || regionNames[^1] != region.Name)
                    {
                        regionNames.Add(region.Name);
                    }
                }
            }

            string subtitle = regionNames.Count > 0
                ? $"Flows through {string.Join(", ", regionNames)}"
                : "River";

            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = r.Id,
                Name = r.Name,
                ObjectType = "river",
                LinkHtml = r.ToLink(),
                Value = $"{r.Coordinates.Count} tiles",
                NumericValue = r.Coordinates.Count,
                Subtitle = subtitle
            });
        }

        return card;
    }

    private static RecordCardDto CalculateLargestRegions(World world)
    {
        var card = new RecordCardDto
        {
            Id = "largest_regions",
            Title = "Vastest Biomes & Surface Regions",
            Icon = "mdi-map-legend",
            MetricName = "Area (tiles)"
        };

        var regions = world.Regions
            .Where(r => r.SquareTiles > 0)
            .OrderByDescending(r => r.SquareTiles)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var r in regions)
        {
            string evilnessStr = r.Evilness.GetDescription();
            string typeStr = r.RegionType.GetDescription();
            string subtitle = $"{typeStr} • {evilnessStr}";

            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = r.Id,
                Name = r.Name,
                ObjectType = "region",
                LinkHtml = r.ToLink(),
                Value = $"{r.SquareTiles} tiles",
                NumericValue = r.SquareTiles,
                Subtitle = subtitle
            });
        }

        return card;
    }

    private static RecordCardDto CalculateLargestUndergroundRegions(World world)
    {
        var card = new RecordCardDto
        {
            Id = "largest_underground_regions",
            Title = "Vastest Underground Caverns & Deep Realms",
            Icon = "mdi-tunnel",
            MetricName = "Area (tiles)"
        };

        var uregions = world.UndergroundRegions
            .Where(u => u.SquareTiles > 0)
            .OrderByDescending(u => u.SquareTiles)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var u in uregions)
        {
            string typeStr = u.RegionType.GetDescription();
            string depthStr = u.Depth.HasValue ? $"Depth {u.Depth.Value}" : string.Empty;
            string subtitle = !string.IsNullOrEmpty(depthStr) ? $"{typeStr} • {depthStr}" : typeStr;

            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = u.Id,
                Name = u.Name,
                ObjectType = "uregion",
                LinkHtml = u.ToLink(),
                Value = $"{u.SquareTiles} tiles",
                NumericValue = u.SquareTiles,
                Subtitle = subtitle
            });
        }

        return card;
    }

    private static RecordCardDto CalculateBloodiestRegions(World world)
    {
        var card = new RecordCardDto
        {
            Id = "bloodiest_regions",
            Title = "Bloodiest Conflict Regions",
            Icon = "mdi-map-marker-path",
            MetricName = "Battles Fought"
        };

        var regions = world.Regions
            .Where(r => r.Battles != null && r.Battles.Count > 0)
            .OrderByDescending(r => r.Battles.Count)
            .Take(10)
            .ToList();

        int rank = 1;
        foreach (var r in regions)
        {
            string evilnessStr = r.Evilness.GetDescription();
            string typeStr = r.RegionType.GetDescription();
            string subtitle = $"{typeStr} • {evilnessStr}";

            card.Entries.Add(new RecordEntryDto
            {
                Rank = rank++,
                Id = r.Id,
                Name = r.Name,
                ObjectType = "region",
                LinkHtml = r.ToLink(),
                Value = $"{r.Battles.Count} battle(s)",
                NumericValue = r.Battles.Count,
                Subtitle = subtitle
            });
        }

        return card;
    }
}

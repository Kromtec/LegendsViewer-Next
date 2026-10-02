using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;
using System.Drawing;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Maps;
using SkiaSharp;

namespace LegendsViewer.Backend.Contracts;

public class WorldDto(IWorld worldDataService, IWorldMapImageGenerator worldMapImageGenerator)
{
    public string Name { get; set; } = worldDataService.Name;
    public string AlternativeName { get; set; } = worldDataService.AlternativeName;
    public int Width { get; set; } = worldDataService.Width;
    public int Height { get; set; } = worldDataService.Height;
    public int CurrentYear { get; set; } = worldDataService.CurrentYear;
    public int CurrentMonth { get; set; } = worldDataService.CurrentMonth;
    public int CurrentDay { get; set; } = worldDataService.CurrentDay;

    public List<MainCivilizationDto> MainCivilizations { get; set; } = worldDataService.Entities
        .Where(e => IsDisplayedCivilization(e) && e.EntityPopulation?.Count > 0)
        .OrderBy(e => e.Race.NamePlural)
        .Select(civ => civ.GetCivilizationInfo(worldMapImageGenerator))
        .ToList();

    public List<MainCivilizationDto> MainCivilizationsLost { get; set; } = worldDataService.Entities
        .Where(e => IsDisplayedCivilization(e) && (e.EntityPopulation?.Count ?? 0) == 0)
        .OrderBy(e => e.Race.NamePlural)
        .Select(civ => civ.GetCivilizationInfo(worldMapImageGenerator))
        .ToList();

    public List<SiteMarkerDto> SiteMarkers { get; set; } = worldDataService.Sites.ConvertAll(s => new SiteMarkerDto(s));
    public List<Population> CivilizedPopulations { get; set; } = worldDataService.CivilizedPopulations;
    public List<Population> SitePopulations { get; set; } = worldDataService.SitePopulations
        .GroupBy(population => population.Race)
        .Select(group => new Population(worldDataService, group.Key, group.Count()))
        .OrderByDescending(population => population.Count)
        .ToList();
    public List<Population> UncivilizedSitePopulations { get; set; } = worldDataService.SitePopulations
        .Where(population => !population.IsMainRace)
        .GroupBy(population => population.Race)
        .Select(group => new Population(worldDataService, group.Key, group.Sum(population => population.Count)))
        .OrderByDescending(population => population.Count)
        .ToList();
    public List<Population> OutdoorPopulations { get; set; } = worldDataService.OutdoorPopulations;
    public List<Population> UndergroundPopulations { get; set; } = worldDataService.UndergroundPopulations;
    public List<ListItemDto> PlayerRelatedObjects
    {
        get
        {
            var list = new List<ListItemDto>();
            foreach (var item in worldDataService.PlayerRelatedObjects)
            {
                if (item is HistoricalFigure hf)
                {
                    list.Add(new ListItemDto
                    {
                        Title = "Adventurer",
                        Subtitle = hf.ToLink()
                    });
                }
                else if (item is Entity entity)
                {
                    list.Add(new ListItemDto
                    {
                        Title = entity.Type,
                        Subtitle = entity.ToLink()
                    });
                }
                else if (item is Site site)
                {
                    list.Add(new ListItemDto
                    {
                        Title = site.Type,
                        Subtitle = site.ToLink()
                    });
                }
            }
            return list;
        }
    }

    private static bool IsDisplayedCivilization(Entity entity) =>
        entity.IsCiv && !entity.Race.NamePlural.EndsWith("men", StringComparison.OrdinalIgnoreCase);

    public ChartDataDto EntityPopulationsByRace
    {
        get
        {
            Dictionary<CreatureInfo, int> entityPopsByRaceDict = [];

            foreach (var entityPopulation in worldDataService.EntityPopulations)
            {
                if (entityPopulation.Entity == null || !IsDisplayedCivilization(entityPopulation.Entity))
                {
                    continue;
                }
                if (entityPopsByRaceDict.TryGetValue(entityPopulation.Race, out int popCount))
                {
                    entityPopsByRaceDict[entityPopulation.Race] = popCount + entityPopulation.Count;
                }
                else
                {
                    entityPopsByRaceDict[entityPopulation.Race] = entityPopulation.Count;
                }
            }
            ChartDataDto entityPopsByRace = new();
            ChartDatasetDto entityPopsByRaceDataset = new();
            foreach (var populationOfRace in entityPopsByRaceDict)
            {
                entityPopsByRace.Labels.Add(populationOfRace.Key.NamePlural);
                entityPopsByRaceDataset.Data.Add(populationOfRace.Value);
                if (worldDataService != null && worldDataService.MainRaces.TryGetValue(populationOfRace.Key, out var raceColor))
                {
                    entityPopsByRaceDataset.BorderColor.Add(raceColor.ToRgbaString());
                    entityPopsByRaceDataset.BackgroundColor.Add(raceColor.ToRgbaString(0.2f));
                }
                else
                {
                    entityPopsByRaceDataset.BorderColor.Add(Color.SlateGray.ToRgbaString());
                    entityPopsByRaceDataset.BackgroundColor.Add(Color.SlateGray.ToRgbaString(0.2f));
                }
            }
            entityPopsByRace.Datasets.Add(entityPopsByRaceDataset);
            return entityPopsByRace;
        }
    }

    public ChartDataDto AreaByOverworldRegions
    {
        get
        {
            Dictionary<RegionType, int> areaByRegionsDict = [];

            foreach (var region in worldDataService.Regions)
            {
                if (areaByRegionsDict.TryGetValue(region.RegionType, out int areaOfType))
                {
                    areaByRegionsDict[region.RegionType] = areaOfType + region.Coordinates.Count;
                }
                else
                {
                    areaByRegionsDict[region.RegionType] = region.Coordinates.Count;
                }
            }
            ChartDataDto areaByRegions = new();
            ChartDatasetDto areaByRegionsDataset = new();
            foreach (var populationOfRace in areaByRegionsDict)
            {
                areaByRegions.Labels.Add(populationOfRace.Key.GetDescription());
                areaByRegionsDataset.Data.Add(populationOfRace.Value);
                SKColor baseColor = WorldMapImageGenerator.GetRegionColor(populationOfRace.Key, null);
                areaByRegionsDataset.BorderColor.Add(baseColor.ToRgbaString());
                areaByRegionsDataset.BackgroundColor.Add(baseColor.ToRgbaString(0.2f));
            }
            areaByRegions.Datasets.Add(areaByRegionsDataset);
            return areaByRegions;
        }
    }
}

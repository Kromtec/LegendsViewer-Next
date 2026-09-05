using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Legends;
using LegendsViewer.Backend.Legends.EventCollections;
using LegendsViewer.Backend.Extensions;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Maps;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;
using Microsoft.AspNetCore.Mvc;

namespace LegendsViewer.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorldMapController(IWorld worldDataService, IWorldMapImageGenerator worldMapImageGenerator) : ControllerBase
{
    private readonly IWorld _worldDataService = worldDataService;
    private readonly IWorldMapImageGenerator _worldMapImageGenerator = worldMapImageGenerator;

    [HttpGet("coordinates/{type}/{id}")]
    public ActionResult<MapCoordinatesDto?> GetObjectCoordinates(string type, int id)
    {
        string lowerType = type.ToLower();

        if (lowerType == "war")
        {
            var war = _worldDataService.EventCollections.OfType<War>().FirstOrDefault(w => w.Id == id);
            if (war == null) return NotFound();

            var overlay = CreateWarMapOverlayDto(war);
            var coordsList = new List<LocationDto>();
            if (overlay.AttackerCoordinates != null) coordsList.Add(overlay.AttackerCoordinates);
            if (overlay.DefenderCoordinates != null) coordsList.Add(overlay.DefenderCoordinates);

            foreach (var b in war.Battles)
            {
                if (b.Coordinates != null)
                {
                    coordsList.Add(new LocationDto(b.Coordinates.X, b.Coordinates.Y));
                }
            }

            coordsList = coordsList.DistinctBy(c => (c.X, c.Y)).ToList();

            if (coordsList.Count == 0) return NotFound();

            int minX = coordsList.Min(c => c.X);
            int maxX = coordsList.Max(c => c.X);
            int minY = coordsList.Min(c => c.Y);
            int maxY = coordsList.Max(c => c.Y);

            return new MapCoordinatesDto
            {
                CenterX = (minX + maxX) / 2.0,
                CenterY = (minY + maxY) / 2.0,
                MinX = minX,
                MaxX = maxX,
                MinY = minY,
                MaxY = maxY,
                Coordinates = coordsList
            };
        }

        if (lowerType == "battle")
        {
            var battle = _worldDataService.EventCollections.OfType<Battle>().FirstOrDefault(b => b.Id == id);
            if (battle == null || battle.Coordinates == null) return NotFound();

            int x = battle.Coordinates.X;
            int y = battle.Coordinates.Y;

            return new MapCoordinatesDto
            {
                CenterX = x,
                CenterY = y,
                MinX = x,
                MaxX = x,
                MinY = y,
                MaxY = y,
                Coordinates = [new LocationDto(x, y)]
            };
        }

        WorldObject? obj = lowerType switch
        {
            "site" => _worldDataService.GetSite(id),
            "entity" => _worldDataService.GetEntity(id),
            "region" => _worldDataService.GetRegion(id),
            "undergroundregion" => _worldDataService.GetUndergroundRegion(id),
            "landmass" => _worldDataService.GetLandmass(id),
            "river" => _worldDataService.GetRiver(id),
            "construction" => _worldDataService.GetWorldConstruction(id),
            "mountainpeak" => _worldDataService.GetMountainPeak(id),
            "structure" => _worldDataService.GetStructure(id),
            "artifact" => _worldDataService.GetArtifact(id),
            _ => null
        };

        if (obj is not IHasCoordinates item)
        {
            return NotFound();
        }

        var dto = new MapCoordinatesDto
        {
            CenterX = item.CenterX(),
            CenterY = item.CenterY(),
            MinX = item.MinX(),
            MaxX = item.MaxX(),
            MinY = item.MinY(),
            MaxY = item.MaxY(),
            Coordinates = item.Coordinates.Select(c => new LocationDto(c.X, c.Y)).ToList()
        };

        if (obj is Entity entity && entity.CurrentSites != null)
        {
            dto.SiteIds = entity.CurrentSites.Select(s => s.Id).ToList();
        }

        return dto;
    }

    [HttpGet("warfare")]
    public ActionResult<WarfareMapDto> GetWarfareMap([FromQuery] bool activeOnly = true)
    {
        var dto = new WarfareMapDto();

        var warsQuery = _worldDataService.EventCollections.OfType<War>();
        if (activeOnly)
        {
            warsQuery = warsQuery.Where(w => w.EndYear == -1);
        }

        foreach (var war in warsQuery)
        {
            dto.Wars.Add(CreateWarMapOverlayDto(war));
        }

        var battlesQuery = _worldDataService.EventCollections.OfType<Battle>();
        if (activeOnly)
        {
            battlesQuery = battlesQuery.Where(b => b.EndYear == -1 || ((b.ParentCollection as War)?.EndYear == -1));
        }

        foreach (var battle in battlesQuery)
        {
            dto.Battles.Add(CreateBattleMapMarkerDto(battle));
        }

        return dto;
    }

    [HttpGet("war/{id}/overlay")]
    public ActionResult<WarfareMapDto> GetWarOverlay(int id)
    {
        var war = _worldDataService.EventCollections.OfType<War>().FirstOrDefault(w => w.Id == id);
        if (war == null)
        {
            return NotFound();
        }

        var dto = new WarfareMapDto
        {
            Wars = [CreateWarMapOverlayDto(war)]
        };

        foreach (var battle in war.Battles)
        {
            dto.Battles.Add(CreateBattleMapMarkerDto(battle));
        }

        return dto;
    }

    [HttpGet("battle/{id}/marker")]
    public ActionResult<WarfareMapDto> GetBattleMarker(int id)
    {
        var battle = _worldDataService.EventCollections.OfType<Battle>().FirstOrDefault(b => b.Id == id);
        if (battle == null)
        {
            return NotFound();
        }

        var dto = new WarfareMapDto
        {
            Battles = [CreateBattleMapMarkerDto(battle)]
        };

        if (battle.ParentCollection is War war)
        {
            dto.Wars.Add(CreateWarMapOverlayDto(war));
        }

        return dto;
    }

    private WarMapOverlayDto CreateWarMapOverlayDto(War war)
    {
        LocationDto? attackerCoords = null;
        var attackerSite = war.Attacker?.CurrentSites.FirstOrDefault() ?? war.Attacker?.Sites.FirstOrDefault();
        if (attackerSite?.Coordinates.Count > 0)
        {
            var c = attackerSite.Coordinates[0];
            attackerCoords = new LocationDto(c.X, c.Y);
        }
        else if (war.Attacker?.Coordinates.Count > 0)
        {
            var c = war.Attacker.Coordinates[0];
            attackerCoords = new LocationDto(c.X, c.Y);
        }

        LocationDto? defenderCoords = null;
        var defenderSite = war.Defender?.CurrentSites.FirstOrDefault() ?? war.Defender?.Sites.FirstOrDefault();
        if (defenderSite?.Coordinates.Count > 0)
        {
            var c = defenderSite.Coordinates[0];
            defenderCoords = new LocationDto(c.X, c.Y);
        }
        else if (war.Defender?.Coordinates.Count > 0)
        {
            var c = war.Defender.Coordinates[0];
            defenderCoords = new LocationDto(c.X, c.Y);
        }

        if (attackerCoords == null && war.Battles.Count > 0 && war.Battles[0].Coordinates != null)
        {
            var c = war.Battles[0].Coordinates!;
            attackerCoords = new LocationDto(c.X, c.Y);
        }

        if (defenderCoords == null && war.Battles.Count > 0 && war.Battles.Last().Coordinates != null)
        {
            var c = war.Battles.Last().Coordinates!;
            defenderCoords = new LocationDto(c.X, c.Y);
        }

        return new WarMapOverlayDto
        {
            Id = war.Id,
            Name = war.Name,
            Link = war.ToLink(true),
            AttackerId = war.Attacker?.Id,
            AttackerName = war.Attacker?.Name ?? string.Empty,
            AttackerColor = war.Attacker != null ? war.Attacker.LineColor.ToRgbaString() : "#666666",
            AttackerLink = war.Attacker?.ToLink(true),
            DefenderId = war.Defender?.Id,
            DefenderName = war.Defender?.Name ?? string.Empty,
            DefenderLink = war.Defender?.ToLink(true),
            StartYear = war.StartYear,
            EndYear = war.EndYear,
            IsActive = war.EndYear == -1,
            DeathCount = war.DeathCount,
            AttackerCoordinates = attackerCoords,
            DefenderCoordinates = defenderCoords,
            BattleIds = war.Battles.Select(b => b.Id).ToList()
        };
    }

    private BattleMapMarkerDto CreateBattleMapMarkerDto(Battle battle)
    {
        var parentWar = battle.ParentCollection as War;
        return new BattleMapMarkerDto
        {
            Id = battle.Id,
            Name = battle.Name,
            Link = battle.ToLink(true),
            WarId = parentWar?.Id,
            WarName = parentWar?.Name ?? string.Empty,
            WarLink = parentWar?.ToLink(true),
            AttackerId = battle.Attacker?.Id,
            AttackerName = battle.Attacker?.Name ?? string.Empty,
            AttackerColor = battle.Attacker != null ? battle.Attacker.LineColor.ToRgbaString() : "#666666",
            AttackerLink = battle.Attacker?.ToLink(true),
            DefenderId = battle.Defender?.Id,
            DefenderName = battle.Defender?.Name ?? string.Empty,
            DefenderLink = battle.Defender?.ToLink(true),
            VictorId = battle.Victor?.Id,
            VictorName = battle.Victor?.Name ?? string.Empty,
            VictorLink = battle.Victor?.ToLink(true),
            StartYear = battle.StartYear,
            EndYear = battle.EndYear,
            IsActive = battle.EndYear == -1 || (parentWar != null && parentWar.EndYear == -1),
            DeathCount = battle.DeathCount,
            AttackerDeathCount = battle.AttackerDeathCount,
            DefenderDeathCount = battle.DefenderDeathCount,
            Coordinates = battle.Coordinates != null ? new LocationDto(battle.Coordinates.X, battle.Coordinates.Y) : null
        };
    }

    [HttpGet("world/{size}")]
    public ActionResult<byte[]?> GetWorldMap(MapSize size = MapSize.Default)
    {
        var imageData = _worldMapImageGenerator.GenerateMapByteArray(GetTileSizeByEnum(size));
        if (imageData == null)
        {
            return NotFound();
        }
        return imageData;
    }

    [HttpGet("underworld/{size}/{depth}")]
    public ActionResult<byte[]?> GetUnderworldMap([FromRoute] MapSize size = MapSize.Default, [FromRoute] int? depth = null)
    {
        var imageData = _worldMapImageGenerator.GenerateMapByteArray(GetTileSizeByEnum(size), depth);
        if (imageData == null)
        {
            return NotFound();
        }
        return imageData;
    }

    [HttpGet("landmass/{id}/{size}")]
    public ActionResult<byte[]?> GetLandmassMap(int id, MapSize size = MapSize.Default)
    {
        return GetWorldObjectMap(GetTileSizeByEnum(size), _worldDataService.GetLandmass(id));
    }

    [HttpGet("mountainpeak/{id}/{size}")]
    public ActionResult<byte[]?> GetMountainPeakMap(int id, MapSize size = MapSize.Default)
    {
        return GetWorldObjectMap(GetTileSizeByEnum(size), _worldDataService.GetMountainPeak(id));
    }

    [HttpGet("region/{id}/{size}")]
    public ActionResult<byte[]?> GetRegionMap(int id, MapSize size = MapSize.Default)
    {
        return GetWorldObjectMap(GetTileSizeByEnum(size), _worldDataService.GetRegion(id));
    }

    [HttpGet("river/{id}/{size}")]
    public ActionResult<byte[]?> GetRiverMap(int id, MapSize size = MapSize.Default)
    {
        return GetWorldObjectMap(GetTileSizeByEnum(size), _worldDataService.GetRiver(id));
    }

    [HttpGet("construction/{id}/{size}")]
    public ActionResult<byte[]?> GetWorldConstructionMap(int id, MapSize size = MapSize.Default)
    {
        return GetWorldObjectMap(GetTileSizeByEnum(size), _worldDataService.GetWorldConstruction(id));
    }

    [HttpGet("undergroundregion/{id}/{size}")]
    public ActionResult<byte[]?> GetUndergroundRegionMap(int id, MapSize size = MapSize.Default)
    {
        UndergroundRegion? undergroundRegion = _worldDataService.GetUndergroundRegion(id);
        return GetWorldObjectMap(GetTileSizeByEnum(size), undergroundRegion, undergroundRegion?.Depth);
    }

    [HttpGet("site/{id}/{size}")]
    public ActionResult<byte[]?> GetSiteMap(int id, MapSize size = MapSize.Default)
    {
        return GetWorldObjectMap(GetTileSizeByEnum(size), _worldDataService.GetSite(id));
    }

    [HttpGet("structure/{id}/{size}")]
    public ActionResult<byte[]?> GetStructureMap(int id, MapSize size = MapSize.Default)
    {
        return GetWorldObjectMap(GetTileSizeByEnum(size), _worldDataService.GetStructure(id));
    }

    [HttpGet("entity/{id}/{size}")]
    public ActionResult<byte[]?> GetEntityMap(int id, MapSize size = MapSize.Default)
    {
        return GetWorldObjectMap(GetTileSizeByEnum(size), _worldDataService.GetEntity(id));
    }

    [HttpGet("artifact/{id}/{size}")]
    public ActionResult<byte[]?> GetArtifactMap(int id, MapSize size = MapSize.Default)
    {
        return GetWorldObjectMap(GetTileSizeByEnum(size), _worldDataService.GetArtifact(id));
    }

    [HttpGet("war/{id}/{size}")]
    public ActionResult<byte[]?> GetWarMap(int id, MapSize size = MapSize.Default)
    {
        var war = _worldDataService.EventCollections.OfType<War>().FirstOrDefault(w => w.Id == id);
        if (war == null) return NotFound();

        var coords = new List<Location>();
        var attackerSite = war.Attacker?.CurrentSites.FirstOrDefault() ?? war.Attacker?.Sites.FirstOrDefault();
        if (attackerSite?.Coordinates != null) coords.AddRange(attackerSite.Coordinates);
        else if (war.Attacker?.Coordinates != null) coords.AddRange(war.Attacker.Coordinates);

        var defenderSite = war.Defender?.CurrentSites.FirstOrDefault() ?? war.Defender?.Sites.FirstOrDefault();
        if (defenderSite?.Coordinates != null) coords.AddRange(defenderSite.Coordinates);
        else if (war.Defender?.Coordinates != null) coords.AddRange(war.Defender.Coordinates);

        var battles = war.EventCollections.OfType<Battle>().ToList();
        foreach (var battle in battles)
        {
            if (battle.Coordinates != null) coords.Add(battle.Coordinates);
        }

        if (coords.Count == 0) return NotFound();

        var imageData = _worldMapImageGenerator.GenerateMapByteArray(GetTileSizeByEnum(size), null, new WorldCoordinatesWrapper(coords));
        if (imageData == null) return NotFound();

        return imageData;
    }

    [HttpGet("battle/{id}/{size}")]
    public ActionResult<byte[]?> GetBattleMap(int id, MapSize size = MapSize.Default)
    {
        var battle = _worldDataService.EventCollections.OfType<Battle>().FirstOrDefault(b => b.Id == id);
        if (battle == null || battle.Coordinates == null) return NotFound();

        var imageData = _worldMapImageGenerator.GenerateMapByteArray(GetTileSizeByEnum(size), null, new WorldCoordinatesWrapper([battle.Coordinates]));
        if (imageData == null) return NotFound();

        return imageData;
    }

    private ActionResult<byte[]?> GetWorldObjectMap(int tileSize, WorldObject? worldObject, int? depth = null)
    {
        if (worldObject is not IHasCoordinates item)
        {
            return NotFound();
        }
        var imageData = _worldMapImageGenerator.GenerateMapByteArray(tileSize, depth, item);
        if (imageData == null)
        {
            return NotFound();
        }
        return imageData;
    }

    private static int GetTileSizeByEnum(MapSize size)
    {
        return size switch
        {
            MapSize.Small => WorldMapImageGenerator.DefaultTileSizeMin,
            MapSize.Large => WorldMapImageGenerator.DefaultTileSizeMax,
            _ => WorldMapImageGenerator.DefaultTileSizeMid,
        };
    }
}

public class WorldCoordinatesWrapper(List<Location> coordinates) : IHasCoordinates
{
    public List<Location> Coordinates { get; } = coordinates;
}


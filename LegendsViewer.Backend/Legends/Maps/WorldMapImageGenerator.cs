namespace LegendsViewer.Backend.Legends.Maps;

using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Interfaces;
using SkiaSharp;
using System.Globalization;
using System.Text.RegularExpressions;

public class WorldMapImageGenerator(IWorld worldDataService) : IWorldMapImageGenerator
{
    public const int DefaultTileSizeMin = 2;
    public const int DefaultTileSizeMid = 4;
    public const int DefaultTileSizeMax = 10;
    private const int ThicklineInterval = 16;
    private const int ThinlineInterval = 4;
    private readonly IWorld _worldDataService = worldDataService;
    private byte[]? _worldMapMin;
    private byte[]? _worldMapMid;
    private byte[]? _worldMapMax;

    private SKBitmap? _exportedWorldMapBitmap;

    public async Task LoadExportedWorldMapAsync(string? legendsFilePath)
    {
        if (string.IsNullOrEmpty(legendsFilePath) || !File.Exists(legendsFilePath)) return;

        var directory = Path.GetDirectoryName(legendsFilePath)!;
        var name = Path.GetFileName(legendsFilePath);
        var suffix = name.EndsWith("-legends_plus.xml", StringComparison.OrdinalIgnoreCase)
            ? "-legends_plus.xml" : "-legends.xml";
        if (!name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) return;
        var companion = Path.Combine(directory, name[..^suffix.Length] + "-world_map.csv");

        var graphics = FindDfFile("data/vanilla/vanilla_world_map/graphics/graphics_world_map.txt");
        string?[] sprites = new[] { "tiles", "forests", "mountains", "details" }
            .Select(name => FindDfFile($"data/vanilla/vanilla_world_map/graphics/images/world_map_{name}.png"))
            .ToArray();
        if (!File.Exists(companion) || new FileInfo(companion).Length > 10_000_000
            || graphics == null || sprites.Any(path => path == null)) return;

        _exportedWorldMapBitmap = await Task.Run(() => LoadPremiumMap(companion, graphics,
            sprites.Select(path => path!).ToArray()));
    }

    public byte[]? GenerateMapByteArray(int tileSize = DefaultTileSizeMid, int? depth = null, IHasCoordinates? objectWithCoordinates = null)
    {
        if (objectWithCoordinates == null && depth == null && TryGetCachedMap(tileSize, out byte[]? imageData))
        {
            return imageData;
        }

        int pixelWidth = _worldDataService.Width * tileSize;
        int pixelHeight = _worldDataService.Height * tileSize;

        SKBitmap worldImage = GenerateBaseMapImage(tileSize, pixelWidth, pixelHeight);

        DrawRegionsAndObjects(worldImage, tileSize, objectWithCoordinates, depth);

        using (var stream = new SKDynamicMemoryWStream())
        {
            worldImage.Encode(stream, SKEncodedImageFormat.Png, 100);
            imageData = stream.DetachAsData().ToArray();
            if (imageData != null)
            {
                if (objectWithCoordinates == null && depth == null)
                {
                    CacheDefaultMap(tileSize, imageData);
                }
                return imageData;
            }
        }
        return null;
    }

    public void Clear()
    {
        _worldMapMin = null;
        _worldMapMid = null;
        _worldMapMax = null;
        _exportedWorldMapBitmap?.Dispose();
        _exportedWorldMapBitmap = null;
    }

    private void CacheDefaultMap(int tileSize, byte[] imageData)
    {
        switch (tileSize)
        {
            case DefaultTileSizeMin:
                _worldMapMin = imageData;
                break;
            case DefaultTileSizeMid:
                _worldMapMid = imageData;
                break;
            case DefaultTileSizeMax:
                _worldMapMax = imageData;
                break;
        }
    }

    private SKBitmap GenerateBaseMapImage(int tileSize, int pixelWidth, int pixelHeight)
    {
        SKBitmap worldImage = new(pixelWidth, pixelHeight);
        if (_exportedWorldMapBitmap != null)
        {
            using (var canvas = new SKCanvas(worldImage))
            {
                // Draw the resized BMP as the base image
                canvas.DrawBitmap(_exportedWorldMapBitmap, new SKRect(0, 0, pixelWidth, pixelHeight));
            }
        }
        else
        {
            using (var canvas = new SKCanvas(worldImage))
            {
                DrawGrid(canvas, _worldDataService.Width, _worldDataService.Height, tileSize);
            }
        }

        return worldImage;
    }

    private void DrawRegionsAndObjects(SKBitmap worldImage, int tileSize, IHasCoordinates? objectWithCoordinates = null, int? depth = null)
    {
        IRegion[,] worldTiles = GetWorldTiles(depth);
        bool keepPremiumTerrain = _exportedWorldMapBitmap != null && depth == null;

        int width = worldTiles.GetLength(0);
        int height = worldTiles.GetLength(1);

        // Create a hash set of the object's coordinates for faster lookup
        HashSet<(int, int)> objectCoordinates = new();
        if (objectWithCoordinates != null)
        {
            foreach (var coord in objectWithCoordinates.Coordinates)
            {
                objectCoordinates.Add((coord.X, coord.Y));  // Assuming Location has X, Y properties
            }
        }

        using (var canvas = new SKCanvas(worldImage))
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SKColor tileColor;

                    // Check if this tile belongs to the objectWithCoordinates
                    if (objectCoordinates.Count > 0 && objectCoordinates.Contains((x, y)))
                    {
                        // Color the tile magenta if it belongs to the object
                        tileColor = SKColors.Magenta;
                    }
                    else
                    {
                        if (keepPremiumTerrain) continue;
                        RegionType regionType = worldTiles[x, y]?.RegionType ?? RegionType.Default;
                        tileColor = GetRegionColor(regionType, worldTiles[x, y]?.Id);
                    }

                    using (var paint = new SKPaint { Color = tileColor })
                    {
                        canvas.DrawRect(x * tileSize, y * tileSize, tileSize, tileSize, paint);
                    }
                }
            }

            // Then, draw a circle around the object for better visibility
            if (objectWithCoordinates?.Coordinates.Count > 0)
            {
                EncircleObject(tileSize, objectWithCoordinates, canvas);
            }
        }
    }

    // Method to get a region color with variation based on the region's Id
    public static SKColor GetRegionColor(RegionType regionType, int? regionId)
    {
        SKColor baseColor = RegionTypeColors.BaseRegionColors[regionType];
        if (regionId == null)
        {
            return baseColor;
        }
        // Apply a color variation based on the region Id
        return ApplyIdBasedVariation(baseColor, regionId.Value);
    }

    // Apply a slight color variation based on region Id
    private static SKColor ApplyIdBasedVariation(SKColor baseColor, int regionId)
    {
        // Generate a seed based on the regionId to introduce a consistent variation
        Random random = new(regionId);

        // Slightly adjust each color component using the seed
        byte r = (byte)Math.Clamp(baseColor.Red + random.Next(-15, 15), 0, 255);
        byte g = (byte)Math.Clamp(baseColor.Green + random.Next(-15, 15), 0, 255);
        byte b = (byte)Math.Clamp(baseColor.Blue + random.Next(-15, 15), 0, 255);

        return new SKColor(r, g, b);
    }

    private static void EncircleObject(int tileSize, IHasCoordinates objectWithCoordinates, SKCanvas canvas)
    {
        if (objectWithCoordinates == null || objectWithCoordinates.Coordinates == null || objectWithCoordinates.Coordinates.Count == 0)
        {
            return;
        }

        // Calculate the center of the object
        var centerX = objectWithCoordinates.CenterX();
        var centerY = objectWithCoordinates.CenterY();

        // Convert the center to pixel coordinates
        float pixelCenterX = (float)(centerX * tileSize + tileSize / 2.0f);
        float pixelCenterY = (float)(centerY * tileSize + tileSize / 2.0f);

        // Convert the width and height to pixel coordinates
        float pixelWidth = objectWithCoordinates.Width() * tileSize;
        float pixelHeight = objectWithCoordinates.Height() * tileSize;

        // Ensure a clear minimum radius relative to canvas dimensions (at least 3.5% of canvas width or 16px)
        float baseRadiusX = (float)pixelWidth / 2.0f + (tileSize * 3.0f);
        float baseRadiusY = (float)pixelHeight / 2.0f + (tileSize * 3.0f);
        float minRadius = Math.Max(16.0f, canvas.DeviceClipBounds.Width * 0.035f);
        float pixelRadiusX = Math.Max(baseRadiusX, minRadius);
        float pixelRadiusY = Math.Max(baseRadiusY, minRadius);

        // Define stroke thickness (minimum 3px for high visibility)
        float strokeThickness = Math.Max(3.0f, tileSize * 0.8f);

        // 1. Outer dashed radar ring for enhanced target acquisition area
        using (var dashEffect = SKPathEffect.CreateDash(new float[] { 8.0f, 6.0f }, 0))
        {
            float outerRadiusX = pixelRadiusX * 1.7f;
            float outerRadiusY = pixelRadiusY * 1.7f;

            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = strokeThickness + 3.0f,
                PathEffect = dashEffect,
                IsAntialias = true
            })
            {
                canvas.DrawOval(pixelCenterX, pixelCenterY, outerRadiusX, outerRadiusY, paint);
            }

            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Yellow,
                StrokeWidth = strokeThickness,
                PathEffect = dashEffect,
                IsAntialias = true
            })
            {
                canvas.DrawOval(pixelCenterX, pixelCenterY, outerRadiusX, outerRadiusY, paint);
            }
        }

        // 2. Black outer shadow ring for maximum contrast against any terrain background
        using (var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = strokeThickness + 4.0f,
            IsAntialias = true
        })
        {
            canvas.DrawOval(pixelCenterX, pixelCenterY, pixelRadiusX, pixelRadiusY, paint);
        }

        // 3. Bright Yellow primary ring
        using (var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Yellow,
            StrokeWidth = strokeThickness + 1.0f,
            IsAntialias = true
        })
        {
            canvas.DrawOval(pixelCenterX, pixelCenterY, pixelRadiusX, pixelRadiusY, paint);
        }

        // 4. Inner Red accent ring
        using (var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            StrokeWidth = strokeThickness * 0.6f,
            IsAntialias = true
        })
        {
            canvas.DrawOval(pixelCenterX, pixelCenterY, pixelRadiusX, pixelRadiusY, paint);
        }

        // 5. Full-canvas Edge-to-Edge Target Crosshair lines (Top, Bottom, Left, Right)
        float gap = 4.0f;
        float canvasWidth = canvas.DeviceClipBounds.Width;
        float canvasHeight = canvas.DeviceClipBounds.Height;

        // Black outline for full-canvas crosshairs
        using (var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = strokeThickness + 3.0f,
            IsAntialias = true
        })
        {
            // Top segment (from canvas top edge down to ring gap)
            canvas.DrawLine(pixelCenterX, 0, pixelCenterX, Math.Max(0, pixelCenterY - pixelRadiusY - gap), paint);
            // Bottom segment (from ring gap down to canvas bottom edge)
            canvas.DrawLine(pixelCenterX, Math.Min(canvasHeight, pixelCenterY + pixelRadiusY + gap), pixelCenterX, canvasHeight, paint);
            // Left segment (from canvas left edge to ring gap)
            canvas.DrawLine(0, pixelCenterY, Math.Max(0, pixelCenterX - pixelRadiusX - gap), pixelCenterY, paint);
            // Right segment (from ring gap to canvas right edge)
            canvas.DrawLine(Math.Min(canvasWidth, pixelCenterX + pixelRadiusX + gap), pixelCenterY, canvasWidth, pixelCenterY, paint);
        }

        // Bright Yellow inner lines for full-canvas crosshairs
        using (var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Yellow,
            StrokeWidth = strokeThickness,
            IsAntialias = true
        })
        {
            canvas.DrawLine(pixelCenterX, 0, pixelCenterX, Math.Max(0, pixelCenterY - pixelRadiusY - gap), paint);
            canvas.DrawLine(pixelCenterX, Math.Min(canvasHeight, pixelCenterY + pixelRadiusY + gap), pixelCenterX, canvasHeight, paint);
            canvas.DrawLine(0, pixelCenterY, Math.Max(0, pixelCenterX - pixelRadiusX - gap), pixelCenterY, paint);
            canvas.DrawLine(Math.Min(canvasWidth, pixelCenterX + pixelRadiusX + gap), pixelCenterY, canvasWidth, pixelCenterY, paint);
        }

        // 6. Target Center Dot
        using (var paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Red,
            IsAntialias = true
        })
        {
            canvas.DrawCircle(pixelCenterX, pixelCenterY, Math.Max(2.5f, strokeThickness * 0.8f), paint);
        }
        using (var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Yellow,
            StrokeWidth = 1.5f,
            IsAntialias = true
        })
        {
            canvas.DrawCircle(pixelCenterX, pixelCenterY, Math.Max(2.5f, strokeThickness * 0.8f), paint);
        }
    }

    public static string? FindDfFile(string relativePath)
    {
        return GetPotentialDfInstallDirectories()
            .Select(root => Path.Combine(root, relativePath))
            .FirstOrDefault(File.Exists);
    }

    private static IEnumerable<string> GetPotentialDfInstallDirectories()
    {
        var roots = new List<string>();

        string? envDir = Environment.GetEnvironmentVariable("DF_INSTALL_DIR");
        if (!string.IsNullOrWhiteSpace(envDir))
        {
            roots.Add(envDir);
        }

        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        var vdfPaths = new List<string>
        {
            Path.Combine(userProfile, ".local/share/Steam/steamapps/libraryfolders.vdf"),
            Path.Combine(userProfile, ".steam/steam/steamapps/libraryfolders.vdf"),
            Path.Combine(userProfile, ".steam/root/steamapps/libraryfolders.vdf"),
            Path.Combine(userProfile, ".var/app/com.valvesoftware.Steam/data/Steam/steamapps/libraryfolders.vdf"),
            Path.Combine(userProfile, ".var/app/com.valvesoftware.Steam/.local/share/Steam/steamapps/libraryfolders.vdf"),
            Path.Combine(userProfile, "Library/Application Support/Steam/steamapps/libraryfolders.vdf")
        };

        if (!string.IsNullOrWhiteSpace(programFilesX86))
        {
            vdfPaths.Add(Path.Combine(programFilesX86, "Steam", "steamapps", "libraryfolders.vdf"));
        }
        if (!string.IsNullOrWhiteSpace(programFiles))
        {
            vdfPaths.Add(Path.Combine(programFiles, "Steam", "steamapps", "libraryfolders.vdf"));
        }

        foreach (var vdfPath in vdfPaths)
        {
            if (!File.Exists(vdfPath)) continue;
            try
            {
                foreach (var line in File.ReadLines(vdfPath))
                {
                    var match = Regex.Match(line, @"^\s*""path""\s*""([^""]+)""");
                    if (match.Success)
                    {
                        string libPath = match.Groups[1].Value.Replace(@"\\", @"/");
                        roots.Add(Path.Combine(libPath, "steamapps", "common", "Dwarf Fortress"));
                        roots.Add(Path.Combine(libPath, "common", "Dwarf Fortress"));
                    }
                }
            }
            catch
            {
                // Ignore read errors
            }
        }

        roots.Add(Path.Combine(userProfile, ".local/share/Steam/steamapps/common/Dwarf Fortress"));
        roots.Add(Path.Combine(userProfile, ".steam/steam/steamapps/common/Dwarf Fortress"));
        roots.Add(Path.Combine(userProfile, ".steam/root/steamapps/common/Dwarf Fortress"));
        roots.Add(Path.Combine(userProfile, ".var/app/com.valvesoftware.Steam/data/Steam/steamapps/common/Dwarf Fortress"));
        roots.Add(Path.Combine(userProfile, ".local/share/Bay 12 Games/Dwarf Fortress"));

        if (!string.IsNullOrWhiteSpace(programFilesX86))
        {
            roots.Add(Path.Combine(programFilesX86, "Steam", "steamapps", "common", "Dwarf Fortress"));
        }
        if (!string.IsNullOrWhiteSpace(programFiles))
        {
            roots.Add(Path.Combine(programFiles, "Steam", "steamapps", "common", "Dwarf Fortress"));
        }

        return roots.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct();
    }

    private static SKBitmap LoadPremiumMap(string companion, string graphicsPath, string[] spritePaths)
    {
        var entries = File.ReadLines(graphicsPath)
            .Select(line => Regex.Match(line,
                @"^\[TILE_GRAPHICS:WORLD_MAP_(TILES|FORESTS|MOUNTAINS|DETAILS):(\d+):(\d+):([^:\]]+)(?::(\d+))?\]$"))
            .Where(match => match.Success).ToArray();
        var graphics = entries.ToDictionary(match => match.Groups[4].Value + ":" +
                (match.Groups[5].Success ? match.Groups[5].Value : "1"),
            match => (Sheet: match.Groups[1].Value, X: int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
                      Y: int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture)));
        var variantCounts = entries.GroupBy(match => match.Groups[4].Value)
            .ToDictionary(group => group.Key, group => group.Count());

        using var reader = new StreamReader(companion);
        var header = (reader.ReadLine() ?? "").Split(',');
        if (header.Length != 4 || header[0] != "DFHACK_WORLD_MAP" || header[1] != "3"
            || !int.TryParse(header[2], out int width) || !int.TryParse(header[3], out int height)
            || width is < 1 or > 300 || height is < 1 or > 300)
            throw new InvalidDataException("Invalid world map companion header");
        if (reader.ReadLine() != "x,y,biome_type,evilness,savagery,elevation,flags,volcanism,peak,river_dirs")
            throw new InvalidDataException("Invalid world map companion columns");

        using var tiles = SKBitmap.Decode(spritePaths[0]) ?? throw new InvalidDataException("Cannot decode terrain sprites");
        using var forests = SKBitmap.Decode(spritePaths[1]) ?? throw new InvalidDataException("Cannot decode forest sprites");
        using var mountains = SKBitmap.Decode(spritePaths[2]) ?? throw new InvalidDataException("Cannot decode mountain sprites");
        using var details = SKBitmap.Decode(spritePaths[3]) ?? throw new InvalidDataException("Cannot decode detail sprites");
        var sheets = new Dictionary<string, SKBitmap> { ["TILES"] = tiles, ["FORESTS"] = forests,
            ["MOUNTAINS"] = mountains, ["DETAILS"] = details };
        var result = new SKBitmap(width * 16, height * 16);
        using var canvas = new SKCanvas(result);
        var seen = new bool[width, height];
        var map = new int[width, height][];
        int count = 0;
        for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine())
        {
            var fields = line.Split(',');
            if (fields.Length != 10 || !fields.All(f => int.TryParse(f, NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out _)))
                throw new InvalidDataException("Invalid world map companion row");
            var values = fields.Select(f => int.Parse(f, CultureInfo.InvariantCulture)).ToArray();
            int x = values[0], y = values[1];
            if (x < 0 || x >= width || y < 0 || y >= height || seen[x, y])
                throw new InvalidDataException("Invalid or duplicate world map coordinate");
            seen[x, y] = true;
            map[x, y] = values;
            count++;

            string token = GetPremiumSpriteToken(values[2], values[3], values[4], values[5], values[6],
                values[7], values[8]);
            DrawSprite(token);

            void DrawSprite(string spriteToken)
            {
                int variant = Math.Abs(x * 31 + y * 17) % variantCounts[spriteToken] + 1;
                if (!graphics.TryGetValue(spriteToken + ":" + variant, out var source))
                    throw new InvalidDataException($"DF graphics do not define {spriteToken}:{variant}");
                if (source.Sheet is "FORESTS" or "MOUNTAINS")
                    DrawSprite(GetPremiumBaseSpriteToken(values[2], values[3], values[4]));
                var sheet = sheets[source.Sheet];
                canvas.DrawBitmap(sheet, new SKRect(source.X * 16, source.Y * 16,
                        source.X * 16 + 16, source.Y * 16 + 16),
                    new SKRect(x * 16, y * 16, x * 16 + 16, y * 16 + 16));
            }
        }
        if (count != width * height) throw new InvalidDataException("Incomplete world map companion");
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            int flags = map[x, y][6];
            if ((flags & 4) != 0)
            {
                DrawDetail(x, y, "ROAD_DIRT_" + GetDirectionSuffix(GetConnectedDirections(
                    width, height, x, y, (nx, ny) => (map[nx, ny][6] & 4) != 0)));
            }
        }
        return result;

        void DrawDetail(int x, int y, string token)
        {
            if (!graphics.TryGetValue(token + ":1", out var source))
                throw new InvalidDataException($"DF graphics do not define {token}");
            var sheet = sheets[source.Sheet];
            canvas.DrawBitmap(sheet, new SKRect(source.X * 16, source.Y * 16, source.X * 16 + 16, source.Y * 16 + 16),
                new SKRect(x * 16, y * 16, x * 16 + 16, y * 16 + 16));
        }
    }

    public static string GetDirectionSuffix(int directions) => directions switch
    {
        0 => "0", 1 => "N", 2 => "S", 3 => "NS", 4 => "W", 5 => "NW", 6 => "SW", 7 => "NSW",
        8 => "E", 9 => "NE", 10 => "SE", 11 => "NSE", 12 => "WE", 13 => "NWE", 14 => "SWE", 15 => "NSWE",
        _ => throw new ArgumentOutOfRangeException(nameof(directions))
    };

    public static int GetConnectedDirections(int width, int height, int x, int y, Func<int, int, bool> connected)
    {
        int directions = 0;
        if (y > 0 && connected(x, y - 1)) directions |= 1;
        if (y + 1 < height && connected(x, y + 1)) directions |= 2;
        if (x > 0 && connected(x - 1, y)) directions |= 4;
        if (x + 1 < width && connected(x + 1, y)) directions |= 8;
        return directions;
    }

    private static string GetPremiumBaseSpriteToken(int biome, int evilness, int savagery)
    {
        string token = biome == 0 ? "HILLS" : "SHRUBLAND";
        if (evilness >= 66) token += savagery >= 66 ? "_EVILSAV" : "_EVIL";
        else if (evilness < 33) token += savagery >= 66 ? "_GOODSAV" : "_GOOD";
        return token;
    }

    public static string GetPremiumSpriteToken(int biome, int evilness, int savagery,
        int elevation, int flags, int volcanism, int peak)
    {
        string token = peak == 2 || (biome == 0 && volcanism >= 100) ? "VOLCANO"
            : (flags & 1) != 0 || biome is >= 30 and <= 41 ? "LAKE" : biome switch
        {
            0 when peak == 1 => "MOUNTAIN_PEAK",
            0 when elevation >= 250 => "MOUNTAIN_HIGH",
            0 when elevation >= 200 => "MOUNTAIN_MID",
            0 => "MOUNTAIN_LOW",
            1 => "GLACIER",
            2 => "TUNDRA",
            >= 3 and <= 4 or >= 7 and <= 9 => "SWAMP",
            >= 5 and <= 6 or >= 10 and <= 11 => "MARSH",
            12 => "FOREST_TAIGA",
            13 => "FOREST_CONIFER_TEMP",
            14 => "FOREST_BROADLEAF_TEMP",
            15 => "FOREST_CONIFER_TROP",
            16 => "FOREST_BROADLEAF_TROP_DRY",
            17 => "FOREST_BROADLEAF_TROP_MOIST",
            18 => "GRASSLAND_TEMP",
            19 => "SAVANNA_TEMP",
            20 => "SHRUBLAND",
            21 => "GRASSLAND_TROP",
            22 => "SAVANNA_TROP",
            23 => "SHRUBLAND",
            24 => "BADLANDS",
            25 => "ROCKY_PLAINS",
            26 => "SAND_DESERT",
            29 => "FROZEN_OCEAN",
            >= 27 and <= 28 when elevation < 50 => "OCEAN_DEEP",
            >= 27 and <= 29 => "OCEAN",
            _ => throw new InvalidDataException($"Unsupported biome type {biome}")
        };
        if (evilness >= 66) token += savagery >= 66 ? "_EVILSAV" : "_EVIL";
        else if (evilness < 33) token += savagery >= 66 ? "_GOODSAV" : "_GOOD";
        return token;
    }

    private bool TryGetCachedMap(int tileSize, out byte[]? imageData)
    {
        switch (tileSize)
        {
            case DefaultTileSizeMin:
                if (_worldMapMin != null)
                {
                    imageData = _worldMapMin;
                    return true;
                }
                break;
            case DefaultTileSizeMid:
                if (_worldMapMid != null)
                {
                    imageData = _worldMapMid;
                    return true;
                }
                break;
            case DefaultTileSizeMax:
                if (_worldMapMax != null)
                {
                    imageData = _worldMapMax;
                    return true;
                }
                break;
        }
        imageData = null;
        return false;
    }

    private IRegion[,] GetWorldTiles(int? depth = null)
    {
        IRegion[,] worldTiles = new IRegion[_worldDataService.Width + 1, _worldDataService.Height + 1];

        if (depth == null)
        {
            GetTilesBasedOnDepth(worldTiles, _worldDataService.Regions.OfType<IRegion>());
        }
        else
        {
            GetTilesBasedOnDepth(worldTiles, _worldDataService.UndergroundRegions.OfType<IRegion>(), depth);
        }

        return worldTiles;
    }

    private static void GetTilesBasedOnDepth(IRegion[,] worldTiles, IEnumerable<IRegion> regions, int? depth = null)
    {
        foreach (var region in regions.Where(r => r.Depth == depth))
        {
            foreach (var location in region.Coordinates)
            {
                worldTiles[location.X, location.Y] = region;
            }
        }
    }

    private void DrawGrid(SKCanvas canvas, int width, int height, int tileSize)
    {
        using (var thinPaint = new SKPaint { Color = SKColors.DarkSlateGray, StrokeWidth = 0 })
        using (var thickPaint = new SKPaint { Color = SKColors.DarkSlateBlue, StrokeWidth = 0 })
        {
            for (int x = tileSize; x <= (width * tileSize) - tileSize; x += tileSize)
            {
                if ((x / tileSize) % ThicklineInterval == 0)
                {
                    canvas.DrawLine(x, 0, x, height * tileSize, thickPaint);
                }
                else if ((x / tileSize) % ThinlineInterval == 0)
                {
                    canvas.DrawLine(x, 0, x, height * tileSize, thinPaint);
                }
            }

            for (int y = tileSize; y <= (height * tileSize) - tileSize; y += tileSize)
            {
                if ((y / tileSize) % ThicklineInterval == 0)
                {
                    canvas.DrawLine(0, y, width * tileSize, y, thickPaint);
                }
                else if ((y / tileSize) % ThinlineInterval == 0)
                {
                    canvas.DrawLine(0, y, width * tileSize, y, thinPaint);
                }
            }
        }
    }
}

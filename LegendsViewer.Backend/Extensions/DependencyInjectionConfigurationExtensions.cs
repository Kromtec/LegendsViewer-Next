using LegendsViewer.Backend.DataAccess.Repositories.Interfaces;
using LegendsViewer.Backend.DataAccess.Repositories;
using LegendsViewer.Backend.Legends.EventCollections;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Extensions;

public static class DependencyInjectionConfigurationExtensions
{
    public static IServiceCollection AddClassicRepositories(this IServiceCollection services)
    {
        // DanceForm
        services.AddTransient<IWorldObjectRepository<DanceForm>, WorldObjectClassicRepository<DanceForm>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<DanceForm>(worldDataService.DanceForms, worldDataService.GetDanceForm);
        });

        // MusicalForm
        services.AddTransient<IWorldObjectRepository<MusicalForm>, WorldObjectClassicRepository<MusicalForm>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<MusicalForm>(worldDataService.MusicalForms, worldDataService.GetMusicalForm);
        });

        // PoeticForm
        services.AddTransient<IWorldObjectRepository<PoeticForm>, WorldObjectClassicRepository<PoeticForm>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<PoeticForm>(worldDataService.PoeticForms, worldDataService.GetPoeticForm);
        });

        // WrittenContent
        services.AddTransient<IWorldObjectRepository<WrittenContent>, WorldObjectClassicRepository<WrittenContent>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<WrittenContent>(worldDataService.WrittenContents, worldDataService.GetWrittenContent);
        });

        // Landmass
        services.AddTransient<IWorldObjectRepository<Landmass>, WorldObjectClassicRepository<Landmass>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Landmass>(worldDataService.Landmasses, worldDataService.GetLandmass);
        });

        // River
        services.AddTransient<IWorldObjectRepository<River>, WorldObjectClassicRepository<River>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<River>(worldDataService.Rivers, worldDataService.GetRiver);
        });

        // Site
        services.AddTransient<IWorldObjectRepository<Site>, WorldObjectClassicRepository<Site>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Site>(worldDataService.Sites, worldDataService.GetSite);
        });

        // WorldRegion
        services.AddTransient<IWorldObjectRepository<WorldRegion>, WorldObjectClassicRepository<WorldRegion>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<WorldRegion>(worldDataService.Regions, worldDataService.GetRegion);
        });

        // UndergroundRegion
        services.AddTransient<IWorldObjectRepository<UndergroundRegion>, WorldObjectClassicRepository<UndergroundRegion>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<UndergroundRegion>(worldDataService.UndergroundRegions, worldDataService.GetUndergroundRegion);
        });

        // Artifact
        services.AddTransient<IWorldObjectRepository<Artifact>, WorldObjectClassicRepository<Artifact>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Artifact>(worldDataService.Artifacts, worldDataService.GetArtifact);
        });

        // Entity
        services.AddTransient<IWorldObjectRepository<Entity>, WorldObjectClassicRepository<Entity>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Entity>(worldDataService.Entities, worldDataService.GetEntity);
        });

        // HistoricalFigure
        services.AddTransient<IWorldObjectRepository<HistoricalFigure>, WorldObjectClassicRepository<HistoricalFigure>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<HistoricalFigure>(worldDataService.HistoricalFigures, worldDataService.GetHistoricalFigure);
        });

        // MountainPeak
        services.AddTransient<IWorldObjectRepository<MountainPeak>, WorldObjectClassicRepository<MountainPeak>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<MountainPeak>(worldDataService.MountainPeaks, worldDataService.GetMountainPeak);
        });

        // Structure
        services.AddTransient<IWorldObjectRepository<Structure>, WorldObjectClassicRepository<Structure>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Structure>(worldDataService.Structures, worldDataService.GetStructure);
        });

        // WorldConstruction
        services.AddTransient<IWorldObjectRepository<WorldConstruction>, WorldObjectClassicRepository<WorldConstruction>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<WorldConstruction>(worldDataService.WorldConstructions, worldDataService.GetWorldConstruction);
        });

        // Era
        services.AddTransient<IWorldObjectRepository<Era>, WorldObjectClassicRepository<Era>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Era>(worldDataService.Eras, worldDataService.GetEra);
        });

        // War
        services.AddTransient<IWorldObjectRepository<War>, WorldObjectClassicRepository<War>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<War>(worldDataService.Wars, worldDataService.GetEventCollection<War>);
        });

        // Battle
        services.AddTransient<IWorldObjectRepository<Battle>, WorldObjectClassicRepository<Battle>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Battle>(worldDataService.Battles, worldDataService.GetEventCollection<Battle>);
        });

        // Duel
        services.AddTransient<IWorldObjectRepository<Duel>, WorldObjectClassicRepository<Duel>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Duel>(worldDataService.Duels, worldDataService.GetEventCollection<Duel>);
        });

        // Raid
        services.AddTransient<IWorldObjectRepository<Raid>, WorldObjectClassicRepository<Raid>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Raid>(worldDataService.Raids, worldDataService.GetEventCollection<Raid>);
        });

        // SiteConquered
        services.AddTransient<IWorldObjectRepository<SiteConquered>, WorldObjectClassicRepository<SiteConquered>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<SiteConquered>(worldDataService.SiteConquerings, worldDataService.GetEventCollection<SiteConquered>);
        });

        // Insurrection
        services.AddTransient<IWorldObjectRepository<Insurrection>, WorldObjectClassicRepository<Insurrection>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Insurrection>(worldDataService.Insurrections, worldDataService.GetEventCollection<Insurrection>);
        });

        // Persecution
        services.AddTransient<IWorldObjectRepository<Persecution>, WorldObjectClassicRepository<Persecution>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Persecution>(worldDataService.Persecutions, worldDataService.GetEventCollection<Persecution>);
        });

        // Purge
        services.AddTransient<IWorldObjectRepository<Purge>, WorldObjectClassicRepository<Purge>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Purge>(worldDataService.Purges, worldDataService.GetEventCollection<Purge>);
        });

        // EntityOverthrownCollection
        services.AddTransient<IWorldObjectRepository<EntityOverthrownCollection>, WorldObjectClassicRepository<EntityOverthrownCollection>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<EntityOverthrownCollection>(worldDataService.Coups, worldDataService.GetEventCollection<EntityOverthrownCollection>);
        });

        // BeastAttack
        services.AddTransient<IWorldObjectRepository<BeastAttack>, WorldObjectClassicRepository<BeastAttack>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<BeastAttack>(worldDataService.BeastAttacks, worldDataService.GetEventCollection<BeastAttack>);
        });

        // Abduction
        services.AddTransient<IWorldObjectRepository<Abduction>, WorldObjectClassicRepository<Abduction>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Abduction>(worldDataService.Abductions, worldDataService.GetEventCollection<Abduction>);
        });

        // Theft
        services.AddTransient<IWorldObjectRepository<Theft>, WorldObjectClassicRepository<Theft>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Theft>(worldDataService.Thefts, worldDataService.GetEventCollection<Theft>);
        });

        // ProcessionCollection
        services.AddTransient<IWorldObjectRepository<ProcessionCollection>, WorldObjectClassicRepository<ProcessionCollection>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<ProcessionCollection>(worldDataService.Processions, worldDataService.GetEventCollection<ProcessionCollection>);
        });

        // PerformanceCollection
        services.AddTransient<IWorldObjectRepository<PerformanceCollection>, WorldObjectClassicRepository<PerformanceCollection>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<PerformanceCollection>(worldDataService.Performances, worldDataService.GetEventCollection<PerformanceCollection>);
        });

        // Journey
        services.AddTransient<IWorldObjectRepository<Journey>, WorldObjectClassicRepository<Journey>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Journey>(worldDataService.Journeys, worldDataService.GetEventCollection<Journey>);
        });

        // CompetitionCollection
        services.AddTransient<IWorldObjectRepository<CompetitionCollection>, WorldObjectClassicRepository<CompetitionCollection>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<CompetitionCollection>(worldDataService.Competitions, worldDataService.GetEventCollection<CompetitionCollection>);
        });

        // CeremonyCollection
        services.AddTransient<IWorldObjectRepository<CeremonyCollection>, WorldObjectClassicRepository<CeremonyCollection>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<CeremonyCollection>(worldDataService.Ceremonies, worldDataService.GetEventCollection<CeremonyCollection>);
        });

        // Occasion
        services.AddTransient<IWorldObjectRepository<Occasion>, WorldObjectClassicRepository<Occasion>>(x =>
        {
            IWorld worldDataService = x.GetRequiredService<IWorld>();
            return new WorldObjectClassicRepository<Occasion>(worldDataService.Occasions, worldDataService.GetEventCollection<Occasion>);
        });

        return services;
    }
}

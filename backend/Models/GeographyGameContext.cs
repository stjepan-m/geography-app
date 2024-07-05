using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GeographyGame.Models;

public partial class GeographyGameContext : DbContext
{
    public GeographyGameContext()
    {
    }

    public GeographyGameContext(DbContextOptions<GeographyGameContext> options)
        : base(options)
    {
    }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GameLocation> GameLocations { get; set; }

    public virtual DbSet<GameType> GameTypes { get; set; }

    public virtual DbSet<GameTypeScoringType> GameTypeScoringTypes { get; set; }
    
    public virtual DbSet<GameTypeTimeLimitType> GameTypeTimeLimitTypes { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<PlayerGame> PlayerGames { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<RegionLocation> RegionLocations { get; set; }

    public virtual DbSet<Round> Rounds { get; set; }

    public virtual DbSet<ScoringType> ScoringTypes { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<TimeLimitType> TimeLimitTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("city_pkey");

            entity.ToTable("city");

            entity.Property(e => e.LocationId)
                .ValueGeneratedNever()
                .HasColumnName("location_id");
            entity.Property(e => e.Country).HasColumnName("country");
            entity.Property(e => e.Latitude)
                .HasPrecision(8, 5)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(8, 5)
                .HasColumnName("longitude");

            entity.HasOne(d => d.Location).WithOne(p => p.City)
                .HasForeignKey<City>(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("city_location_id_fkey");

            entity.HasOne(d => d.CountryNavigation).WithMany(p => p.Cities)
                .HasForeignKey(d => d.Country)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("city_country_fkey");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("country_pkey");

            entity.ToTable("country");

            entity.Property(e => e.LocationId)
                .ValueGeneratedNever()
                .HasColumnName("location_id");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(3)
                .HasColumnName("country_code");
            entity.Property(e => e.LandAndWaterCoordinates)
                .HasMaxLength(10000000)
                .HasColumnName("land_and_water_coordinates");

            entity.HasOne(d => d.Location).WithOne(p => p.Country)
                .HasForeignKey<Country>(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("country_location_id_fkey");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("game_pkey");

            entity.ToTable("game");


            entity.Property(e => e.AllowSkip).HasColumnName("allow_skip");
            entity.Property(e => e.AllowRetry).HasColumnName("allow_retry");
            entity.Property(e => e.Id)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsFinished).HasColumnName("is_finished");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.NumberOfRounds).HasColumnName("number_of_rounds");
            entity.Property(e => e.Region).HasColumnName("region");
            entity.Property(e => e.ScoringType).HasColumnName("scoring_type");
            entity.Property(e => e.TimeLimitSeconds).HasColumnName("time_limit_seconds");
            entity.Property(e => e.TimeLimitType).HasColumnName("time_limit_type");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Games)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("game_created_by_fkey");

            entity.HasOne(d => d.RegionNavigation).WithMany(p => p.Games)
                .HasForeignKey(d => d.Region)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("game_region_fkey");

            entity.HasOne(d => d.ScoringTypeNavigation).WithMany(p => p.Games)
                .HasForeignKey(d => d.ScoringType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("game_scoring_type_fkey");

            entity.HasOne(d => d.TimeLimitTypeNavigation).WithMany(p => p.Games)
                .HasForeignKey(d => d.TimeLimitType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("game_time_limit_type_fkey");

            entity.HasOne(d => d.TypeNavigation).WithMany(p => p.Games)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("game_type_fkey");
        });

        modelBuilder.Entity<GameLocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("game_location_pkey");

            entity.ToTable("game_location");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.GameId)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("game_id");

            entity.HasOne(d => d.Location).WithMany(p => p.GameLocations)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("game_location_location_id_fkey");

            entity.HasOne(d => d.Game).WithMany(p => p.GameLocations)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("game_location_game_id_fkey");
        });

        modelBuilder.Entity<GameType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("game_type_pkey");

            entity.ToTable("game_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LocationType)
                .HasMaxLength(50)
                .HasColumnName("location_type");
            entity.Property(e => e.InteractionType)
                .HasMaxLength(50)
                .HasColumnName("interaction_type");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.IsSequential).HasColumnName("is_sequential");
            entity.Property(e => e.FeatureType)
                .HasMaxLength(50)
                .HasColumnName("feature_type");
            entity.Property(e => e.LabelEn)
                .HasMaxLength(50)
                .HasColumnName("label_en");
            entity.Property(e => e.LabelHr)
                .HasMaxLength(50)
                .HasColumnName("label_hr");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<GameTypeScoringType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("game_type_scoring_type_pkey");

            entity.ToTable("game_type_scoring_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GameTypeId).HasColumnName("game_type_id");
            entity.Property(e => e.ScoringTypeId).HasColumnName("scoring_type_id");

            entity.HasOne(d => d.GameType).WithMany(p => p.GameTypeScoringTypes)
                .HasForeignKey(d => d.GameTypeId)
                .HasConstraintName("game_type_scoring_type_game_type_id_fkey");

            entity.HasOne(d => d.ScoringType).WithMany(p => p.GameTypeScoringTypes)
                .HasForeignKey(d => d.ScoringTypeId)
                .HasConstraintName("game_type_scoring_type_scoring_type_id_fkey");
        });

        modelBuilder.Entity<GameTypeTimeLimitType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("game_type_time_limit_type_pkey");

            entity.ToTable("game_type_time_limit_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GameTypeId).HasColumnName("game_type_id");
            entity.Property(e => e.TimeLimitTypeId).HasColumnName("time_limit_type_id");

            entity.HasOne(d => d.GameType).WithMany(p => p.GameTypeTimeLimitTypes)
                .HasForeignKey(d => d.GameTypeId)
                .HasConstraintName("game_type_time_limit_type_game_type_id_fkey");

            entity.HasOne(d => d.TimeLimitType).WithMany(p => p.GameTypeTimeLimitTypes)
                .HasForeignKey(d => d.TimeLimitTypeId)
                .HasConstraintName("game_type_time_limit_type_time_limit_type_id_fkey");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("location_pkey");

            entity.ToTable("location");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsCustom).HasColumnName("is_custom");
            entity.Property(e => e.LabelEn)
                .HasMaxLength(50)
                .HasColumnName("label_en");
            entity.Property(e => e.LabelHr)
                .HasMaxLength(50)
                .HasColumnName("label_hr");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Locations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("location_created_by_fkey");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("player_pkey");

            entity.ToTable("player");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nickname)
                .HasMaxLength(50)
                .HasColumnName("nickname");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Players)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("player_user_id_fkey");
        });

        modelBuilder.Entity<PlayerGame>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("player_game_pkey");

            entity.ToTable("player_game");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GameId)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("game_id");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.RoundsCompleted).HasColumnName("rounds_completed");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Not Started'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TimeLeft).HasColumnName("time_left");
            entity.Property(e => e.TotalScore).HasColumnName("total_score");

            entity.HasOne(d => d.Game).WithMany(p => p.PlayerGames)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("player_game_game_id_fkey");

            entity.HasOne(d => d.Player).WithMany(p => p.PlayerGames)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("player_game_player_id_fkey");
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("region_pkey");

            entity.ToTable("region");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.LabelEn)
                .HasMaxLength(50)
                .HasColumnName("label_en");
            entity.Property(e => e.LabelHr)
                .HasMaxLength(50)
                .HasColumnName("label_hr");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.StartLatitude)
                .HasPrecision(8, 5)
                .HasColumnName("start_latitude");
            entity.Property(e => e.StartLongitude)
                .HasPrecision(8, 5)
                .HasColumnName("start_longitude");
            entity.Property(e => e.StartZoom).HasColumnName("start_zoom");
            entity.Property(e => e.MaxDistance).HasColumnName("max_distance");
        });

        modelBuilder.Entity<RegionLocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("region_location_pkey");

            entity.ToTable("region_location");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.RegionId).HasColumnName("region_id");

            entity.HasOne(d => d.Location).WithMany(p => p.RegionLocations)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("region_location_location_id_fkey");

            entity.HasOne(d => d.Region).WithMany(p => p.RegionLocations)
                .HasForeignKey(d => d.RegionId)
                .HasConstraintName("region_location_region_id_fkey");
        });

        modelBuilder.Entity<Round>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("round_pkey");

            entity.ToTable("round");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.PlayerGameId).HasColumnName("player_game_id");
            entity.Property(e => e.RoundNumber).HasColumnName("round_number");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.TimeLeft).HasColumnName("time_left");

            entity.HasOne(d => d.Location).WithMany(p => p.Rounds)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("round_location_id_fkey");

            entity.HasOne(d => d.PlayerGame).WithMany(p => p.Rounds)
                .HasForeignKey(d => d.PlayerGameId)
                .HasConstraintName("round_player_game_id_fkey");
        });

        modelBuilder.Entity<ScoringType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("scoring_type_pkey");

            entity.ToTable("scoring_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Formula)
                .HasMaxLength(1000)
                .HasColumnName("formula");
            entity.Property(e => e.MaxScore).HasColumnName("max_score");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.LabelEn)
                .HasMaxLength(50)
                .HasColumnName("label_en");
            entity.Property(e => e.LabelHr)
                .HasMaxLength(50)
                .HasColumnName("label_hr");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("session_pkey");

            entity.ToTable("session");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.SessionToken)
                .HasMaxLength(255)
                .HasColumnName("session_token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("session_user_id_fkey");
        });

        modelBuilder.Entity<TimeLimitType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("time_limit_type_pkey");

            entity.ToTable("time_limit_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.LabelEn)
                .HasMaxLength(50)
                .HasColumnName("label_en");
            entity.Property(e => e.LabelHr)
                .HasMaxLength(50)
                .HasColumnName("label_hr");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(50)
                .HasColumnName("google_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UserType)
                .HasMaxLength(50)
                .HasColumnName("user_type");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

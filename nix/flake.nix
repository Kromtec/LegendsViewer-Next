{
  description = "LegendsViewer-Next";

  inputs = {
    flake-parts = {
      url = "github:hercules-ci/flake-parts";
      inputs.nixpkgs-lib.follows = "nixpkgs";
    };

    nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
    systems.url = "github:nix-systems/default-linux";
  };

  outputs = {
    flake-parts,
    systems,
    nixpkgs,
    self,
    ...
  } @ inputs:
    flake-parts.lib.mkFlake {inherit inputs;} {
      systems = import systems;

      perSystem = {
        config,
        pkgs,
        system,
        lib,
        ...
      }: let
        inherit (pkgs) stdenv;
        nativeBuildInputs = with pkgs; [
          dotnetCorePackages.sdk_8_0
          dotnetCorePackages.aspnetcore_8_0
          copyDesktopItems
          (writeShellScriptBin "npm" "")
        ];
        src = ../.;

        # # Setup Release Info
        repoOwner = "Kromtec";
        repoName = "LegendsViewer-Next";
        semVer = "1.0.0";
        version = "${semVer}-source";
        frontend = pkgs.callPackage ./frontend.nix {
          inherit src self version;
        };
      in {
        formatter = pkgs.nixfmt-rfc-style;
        packages.default = pkgs.buildDotnetModule rec {
          inherit src version nativeBuildInputs;
          pname = "legendsviewer-next";

          dotnet-sdk = pkgs.dotnetCorePackages.sdk_8_0;
          dotnet-runtime = pkgs.dotnetCorePackages.aspnetcore_8_0;
          nugetDeps = ./deps.json;

          projectFile = "./LegendsViewer.Backend/LegendsViewer.Backend.csproj";
          testProjectFile = "./LegendsViewer.Backend.Tests/LegendsViewer.Backend.Tests.csproj";
          executables = [meta.mainProgram];
          desktopItems = [
            (pkgs.makeDesktopItem {
              name = pname;
              desktopName = "Legends Viewer";
              genericName = "DF Legends Export Browser";
              comment = meta.description;
              icon = "${frontend}/dist/ceretelina.png";
              tryExec = meta.mainProgram;
              exec = meta.mainProgram;

              # Remove when https://github.com/Kromtec/LegendsViewer-Next/pull/55 gets
              # released
              terminal = true;

              categories = [
                "Game"
                "RolePlaying"
                "Simulation"
              ];
              keywords = [
                "df"
                "dwarf"
                "fortress"
                "legends"
                "viewer"
              ];
            })
          ];

          installPhase = ''
            runHook preInstall

            lib=$out/lib/${pname}

            mkdir -p $lib

            cp ./LegendsViewer.Backend/bin/Release/*/*/* $lib
            ln -s ${frontend} $lib/${frontend.pname}

            runHook postInstall
          '';

          doInstallCheck = true;
          nativeInstallCheckInputs = [
            pkgs.curl
          ];

          installCheckPhase = ''
            runHook preInstallCheck

            timeout 10 $out/bin/LegendsViewer &
            sleep 5

            # Static server is up
            curl -f http://localhost:8081 | grep "<!doctype html>"

            # # Version matches expected
            # curl -f http://localhost:5054/api/version | grep version

            runHook postInstallCheck
          '';

          meta = {
            homepage = "https://github.com/${repoOwner}/${repoName}";
            description = "Recreates Dwarf Fortress' Legends Mode from exported files. ";
            license = lib.licenses.mit;
            mainProgram = "LegendsViewer";
            platforms = with lib.platforms; lib.intersectLists (darwin ++ linux) (x86_64 ++ aarch64);
          };
        };
      };
    };
}

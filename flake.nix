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

  outputs = { flake-parts, systems, nixpkgs, ... }@inputs:
    flake-parts.lib.mkFlake { inherit inputs; } {

      systems = import systems;

      perSystem = { config, pkgs, system, ... }:
        let
          inherit (pkgs) stdenv;
          nativeBuildInputs = with pkgs; [
            zlib
            makeWrapper
            icu
            glib
            fontconfig
            autoPatchelfHook
          ];

          # # Setup Release Info
          # repoOwner = "Kromtec";
          # repoName = "LegendsViewer-Next";
          # # Grab latest release from Github API
          # latestReleaseJson = builtins.fetchurl {
          #   url = "https://api.github.com/repos/${repoOwner}/${repoName}/releases/latest";
          #   sha256 = lib.fakeHash;
          # };
          # releaseData = builtins.fromJSON latestReleaseJson;
          # releaseVersion = lib.strings.removePrefix "v" releaseData.tag_name; # Strip v out of version
        in {

          formatter = pkgs.nixfmt-rfc-style;
          packages.default = stdenv.mkDerivation rec {
            inherit nativeBuildInputs;
            pname = "LegendsViewer-Next";
            version = "1.2.0";

            src = pkgs.fetchzip {
              url =
                "https://github.com/Kromtec/LegendsViewer-Next/releases/download/v${version}/LegendsViewer-${version}-linux-x64.zip";
              hash = "sha256:1dxxbf2rag6ma3amjrw51f4vajl8hgxrplg5lzgw27mg2wif5svz";
              stripRoot = false;
            };

            # This is probably a horrendous way of doing this, open to better methods
            installPhase = ''
              runHook preInstall
              mkdir -p $out/share/legendsviewer/
              mkdir -p $out/bin
              mv * $out/share/legendsviewer
              ln -s  $out/share/legendsviewer/LegendsViewer $out/bin/LegendsViewer
              runHook postInstall
            '';
            # Even with icu installed dotnet still whines, so I turned this off
            postInstall = ''
              wrapProgram "$out/bin/LegendsViewer" --set 'DOTNET_SYSTEM_GLOBALIZATION_INVARIANT' '1'
            '';

            meta = {
              homepage = "https://github.com/Kromtec/LegendsViewer-Next";
              description =
                "Recreates Dwarf Fortress' Legends Mode from exported files. ";
            };
          };
        };
    };
}

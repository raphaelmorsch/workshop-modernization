#!/usr/bin/env python3
"""Baixa o targeting pack do .NET Framework 4.8 e os pacotes NuGet da solution.

O SDK do .NET no Linux não inclui os reference assemblies do net48 nem o
nuget.exe. O OmniSharp usa os arquivos baixados aqui para carregar a solution.
"""

import subprocess
import sys
import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path

REF_VERSION = "1.0.3"
ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "src"
PACKAGES = SRC / "packages"
PACKAGES_CONFIG = SRC / "PortalOrdens.Web" / "packages.config"


def download(url: str, destination: Path) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    subprocess.run(
        ["curl", "-fsSL", "--retry", "3", "--retry-delay", "2", "-o", str(destination), url],
        check=True,
    )


def extract_nupkg(nupkg: Path, destination: Path) -> None:
    destination.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(nupkg) as archive:
        archive.extractall(destination)


def restore_reference_assemblies() -> None:
    home = Path.home()
    ref_root = home / ".nuget" / "net48-refs"
    marker = ref_root / "build" / ".NETFramework" / "v4.8" / "mscorlib.dll"
    if marker.is_file():
        print(f"Reference assemblies do net48 já estão em {marker.parent}")
        return

    url = (
        "https://api.nuget.org/v3-flatcontainer/"
        f"microsoft.netframework.referenceassemblies.net48/{REF_VERSION}/"
        f"microsoft.netframework.referenceassemblies.net48.{REF_VERSION}.nupkg"
    )
    nupkg = ref_root / f"microsoft.netframework.referenceassemblies.net48.{REF_VERSION}.nupkg"
    print(f"Baixando reference assemblies do .NET Framework 4.8 para {ref_root}")
    download(url, nupkg)
    extract_nupkg(nupkg, ref_root)
    if not marker.is_file():
        raise SystemExit(f"mscorlib.dll não foi encontrado em {marker}")
    print(f"Reference assemblies prontos em {marker.parent}")


def restore_packages() -> None:
    tree = ET.parse(PACKAGES_CONFIG)
    for package in tree.getroot().findall("package"):
        package_id = package.attrib["id"]
        version = package.attrib["version"]
        destination = PACKAGES / f"{package_id}.{version}"
        if (destination / ".restored").is_file():
            print(f"Pacote {package_id}.{version} já restaurado")
            continue

        package_name = package_id.lower()
        url = (
            "https://api.nuget.org/v3-flatcontainer/"
            f"{package_name}/{version}/{package_name}.{version}.nupkg"
        )
        nupkg = destination / f"{package_name}.{version}.nupkg"
        print(f"Baixando {package_id}.{version}")
        download(url, nupkg)
        extract_nupkg(nupkg, destination)
        (destination / ".restored").write_text(package_id + "\n", encoding="utf-8")


def main() -> None:
    restore_reference_assemblies()
    restore_packages()
    print("Restauração concluída.")


if __name__ == "__main__":
    try:
        main()
    except subprocess.CalledProcessError as error:
        sys.exit(error.returncode)

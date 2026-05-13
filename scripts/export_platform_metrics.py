#!/usr/bin/env python3
"""Emit platform testing JSON for the .NET fixture (publish output size, deps, build time, NuGet SCCs, unused imports)."""

from __future__ import annotations

import json
import re
import subprocess
import sys
import time
import xml.etree.ElementTree as ET
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SLN = ROOT / "MetricsDemo.sln"
WEB = ROOT / "src" / "MetricsDemo.Web" / "MetricsDemo.Web.csproj"


def _run(args: list[str], cwd: Path | None = None) -> subprocess.CompletedProcess[str]:
    return subprocess.run(args, cwd=cwd or ROOT, text=True, capture_output=True)


def _tarjan_circular_sccs(edges: dict[str, set[str]]) -> list[list[str]]:
    nodes = set(edges) | {v for vs in edges.values() for v in vs}
    index = 0
    stack: list[str] = []
    on_stack: set[str] = set()
    indices: dict[str, int] = {}
    lowlink: dict[str, int] = {}
    sccs: list[list[str]] = []

    def strongconnect(v: str) -> None:
        nonlocal index
        indices[v] = index
        lowlink[v] = index
        index += 1
        stack.append(v)
        on_stack.add(v)
        for w in edges.get(v, ()):
            if w not in indices:
                strongconnect(w)
                lowlink[v] = min(lowlink[v], lowlink[w])
            elif w in on_stack:
                lowlink[v] = min(lowlink[v], indices[w])
        if lowlink[v] == indices[v]:
            comp: list[str] = []
            while True:
                w = stack.pop()
                on_stack.remove(w)
                comp.append(w)
                if w == v:
                    break
            sccs.append(comp)

    for v in sorted(nodes):
        if v not in indices:
            strongconnect(v)

    circular: list[list[str]] = []
    for comp in sccs:
        if len(comp) > 1:
            circular.append(sorted(comp))
        elif comp and comp[0] in edges.get(comp[0], ()):
            circular.append(comp)
    return circular


def _nuget_cycle_count_from_assets(assets_path: Path) -> int:
    if not assets_path.is_file():
        return -1
    data = json.loads(assets_path.read_text(encoding="utf-8"))
    targets = data.get("targets") or {}
    first_target = next(iter(targets.values()), None)
    if not isinstance(first_target, dict):
        return -1
    edges: dict[str, set[str]] = defaultdict(set)
    for pkg_id, meta in first_target.items():
        if not isinstance(meta, dict):
            continue
        deps = meta.get("dependencies") or {}
        if not isinstance(deps, dict):
            continue
        for dep_name, dep_ver in deps.items():
            dep_id = f"{dep_name}/{dep_ver}"
            edges[pkg_id].add(dep_id)
    return len(_tarjan_circular_sccs({k: set(v) for k, v in edges.items()}))


def _package_refs(csproj: Path) -> list[str]:
    tree = ET.parse(csproj)
    root = tree.getroot()
    refs: list[str] = []
    for node in root.iter():
        tag = node.tag.split("}")[-1]
        if tag == "PackageReference":
            incl = node.attrib.get("Include")
            if incl:
                refs.append(incl)
    return refs


def _unused_package_refs(csproj: Path, src_root: Path) -> int:
    refs = _package_refs(csproj)
    cs_text = ""
    for p in src_root.rglob("*.cs"):
        cs_text += p.read_text(encoding="utf-8", errors="ignore") + "\n"

    def _search_token(pkg: str) -> str:
        if pkg.startswith("Newtonsoft"):
            return "Newtonsoft"
        if "Humanizer" in pkg:
            return "Humanizer"
        return pkg.split(".")[0]

    unused = 0
    for r in refs:
        if _search_token(r) not in cs_text:
            unused += 1
    return unused


def _unused_import_warnings(build_log: str) -> int:
    patterns = [
        r"\bIDE0005\b",
        r"\bCS8019\b",
        r"Using directive is unnecessary",
        r"unnecessary using directive",
    ]
    lines = []
    for ln in build_log.splitlines():
        if any(re.search(pat, ln, re.IGNORECASE) for pat in patterns):
            lines.append(ln)
    return len(lines)


def _dir_size_bytes(path: Path) -> int:
    total = 0
    for f in path.rglob("*"):
        if f.is_file():
            total += f.stat().st_size
    return total


def main() -> int:
    out = ROOT / "metrics" / "platform_dependency_report.json"
    out.parent.mkdir(parents=True, exist_ok=True)

    categories = {
        "performance_testing": {
            "dependency_analysis": {
                "bundle_size_analysis": True,
                "unused_dependency_detection": True,
                "unused_import_count": True,
                "build_performance": True,
                "dependency_graph_analysis": True,
            }
        }
    }

    if not WEB.is_file():
        print(f"Missing {WEB}", file=sys.stderr)
        return 2

    t0 = time.perf_counter()
    b = _run(["dotnet", "build", str(SLN), "-c", "Release", "-v", "m"])
    blog = (b.stdout or "") + (b.stderr or "")
    if b.returncode != 0:
        print(blog[-4000:], file=sys.stderr)
        return b.returncode
    build_s = round(time.perf_counter() - t0, 3)

    pub = ROOT / "artifacts" / "publish"
    if pub.exists():
        for child in pub.iterdir():
            if child.is_dir():
                for sub in child.rglob("*"):
                    if sub.is_file():
                        sub.unlink()
            elif child.is_file():
                child.unlink()
    pub.mkdir(parents=True, exist_ok=True)

    pb = _run(
        [
            "dotnet",
            "publish",
            str(WEB),
            "-c",
            "Release",
            "-o",
            str(pub),
            "--no-build",
        ]
    )
    if pb.returncode != 0:
        print((pb.stdout or "") + (pb.stderr or ""), file=sys.stderr)
        return pb.returncode

    bundle = _dir_size_bytes(pub)
    assets = ROOT / "src" / "MetricsDemo.Web" / "obj" / "project.assets.json"
    cycles = _nuget_cycle_count_from_assets(assets)
    unused_pkg = _unused_package_refs(WEB, ROOT / "src" / "MetricsDemo.Web")
    unused_imp = _unused_import_warnings(blog)

    report = {
        "schema": "platform_dependency_metrics/v1",
        "language": "csharp",
        "repo_root": str(ROOT),
        "metrics": {
            "bundle_size_analysis": {
                "bundle_size_bytes": bundle,
                "publish_output_dir": str(pub.relative_to(ROOT)),
            },
            "unused_dependency_detection": {
                "unused_dependency_count": unused_pkg,
            },
            "unused_import_count": {
                "unused_import_count": unused_imp,
            },
            "build_performance": {
                "build_duration_seconds": build_s,
                "build_kind": "dotnet_build_release",
            },
            "dependency_graph_analysis": {
                "circular_dependency_count": cycles,
                "dependency_graph_kind": "nuget_resolved_target_graph",
            },
        },
        "categories": categories,
    }

    out.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

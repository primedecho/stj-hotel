#!/usr/bin/env python3
"""Convert docs/openapi/openapi.json to docs/openapi/openapi.yaml."""

from __future__ import annotations

import json
import sys
from pathlib import Path

try:
    import yaml
except ImportError:
    print("PyYAML is required: pip install pyyaml", file=sys.stderr)
    sys.exit(1)


def main() -> int:
    root = Path(__file__).resolve().parents[1]
    json_path = root / "docs" / "openapi" / "openapi.json"
    yaml_path = root / "docs" / "openapi" / "openapi.yaml"

    data = json.loads(json_path.read_text(encoding="utf-8"))
    yaml_path.write_text(
        yaml.dump(data, sort_keys=False, allow_unicode=True, default_flow_style=False),
        encoding="utf-8",
    )
    print(f"Wrote {yaml_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

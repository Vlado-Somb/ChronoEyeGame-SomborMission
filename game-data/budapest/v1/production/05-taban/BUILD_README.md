# Taban church 3D — prototype build

This branch preserves `Assets/Model/Cathedral.FBX` unchanged. The FBX is an unrelated third-party generic cathedral according to its Unity AssetOrigin metadata. The new procedural geometry is original and independent.

Run `python -m pip install trimesh numpy` then `python build_taban.py` to generate a GLB and QA JSON in `generated/`. To create the editable Blender project and revised FBX, install Blender and run `blender -b --python export_taban_blender.py`.

The feature-branch GitHub Action builds all three formats, uploads an artifact and, when permitted, commits generated outputs to this feature branch. This is a **prototype**, not a historically verified reconstruction. No public AR release. The locally generated review package includes four rendered views and a more detailed offline generator.

Photographic evidence: 34576.jpg through 34580.jpg (provided 2026-10-09). Only photo-supported massing is labelled DOC; all scale, hidden facades, colors and placement are unverified. See `RECONSTRUCTION_STATUS.md`.

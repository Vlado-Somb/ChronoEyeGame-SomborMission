import bpy
import json
from pathlib import Path
here = Path(__file__).resolve().parent
source = Path("Assets/Model/Cathedral.FBX")
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(source))
items = []
for obj in bpy.data.objects:
    item = {"name":obj.name,"type":obj.type}
    if obj.type == "MESH":
        item["vertices"] = len(obj.data.vertices)
        item["faces"] = len(obj.data.polygons)
        item["materials"] = [m.name for m in obj.data.materials if m]
    items.append(item)
out = here / "generated"
out.mkdir(exist_ok=True)
(out / "legacy_fbx_inspection.json").write_text(json.dumps(items,indent=2))
print("Legacy FBX imported:",len(items))

# Blender headless conversion of original GLB prototype into editable BLEND and FBX.
# Invoke: blender -b --python export_taban_blender.py
from pathlib import Path
import bpy
here=Path(__file__).resolve().parent
out=here/"generated"
source=out/"TabanChurch_reconstruction_v01.glb"
if not source.exists(): raise RuntimeError("GLB missing: run build_taban.py first")
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.gltf(filepath=str(source))
meshes=[o for o in bpy.data.objects if o.type=="MESH"]
if not meshes: raise RuntimeError("No mesh after GLB import")
bpy.context.scene.unit_settings.system="METRIC"
bpy.context.scene.unit_settings.scale_length=1
bpy.ops.wm.save_as_mainfile(filepath=str(out/"TabanChurch_reconstruction_v01.blend"))
bpy.ops.object.select_all(action="DESELECT")
for o in meshes:o.select_set(True)
bpy.context.view_layer.objects.active=meshes[0]
bpy.ops.export_scene.fbx(filepath=str(out/"TabanChurch_reconstruction_v01.fbx"),
    use_selection=True,apply_unit_scale=True,axis_forward="-Z",axis_up="Y",add_leaf_bones=False)
print("TABAN_BLENDER_EXPORT_OK",len(meshes))

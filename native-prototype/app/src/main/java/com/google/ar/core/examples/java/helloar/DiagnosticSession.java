package com.google.ar.core.examples.java.helloar;

import android.app.Activity;
import android.app.ActivityManager;
import android.content.ClipData;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.BatteryManager;
import android.os.Build;
import android.os.Debug;
import android.os.PowerManager;
import android.os.SystemClock;
import androidx.core.content.FileProvider;
import org.json.JSONArray;
import org.json.JSONObject;
import java.io.*;
import java.nio.charset.StandardCharsets;
import java.time.Instant;
import java.util.*;
import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.atomic.AtomicLong;
import java.util.zip.ZipEntry;
import java.util.zip.ZipOutputStream;

/** Bounded producer queue. All filesystem/system sampling/ZIP work belongs to worker. */
final class DiagnosticSession {
  final String id = UUID.randomUUID().toString();
  final long startNs = SystemClock.elapsedRealtimeNanos();
  final String utc = Instant.now().toString();
  private final Context context;
  private final ArrayBlockingQueue<Runnable> queue = new ArrayBlockingQueue<>(128);
  final AtomicLong dropped = new AtomicLong();
  volatile boolean detailed = true, active = true, closed;
  volatile String status = "иницијализација";
  private volatile boolean closing;
  private File directory;
  private BufferedWriter events, csv;
  private long bytes, lastSystem, lastCpu = android.os.Process.getElapsedCpuTime(), lastWall = SystemClock.elapsedRealtime();
  private volatile int snapshots;
  private long baselinePssKiB = -1;
  private final java.util.concurrent.atomic.AtomicBoolean sharing=new java.util.concurrent.atomic.AtomicBoolean();
  private final java.util.concurrent.atomic.AtomicInteger pendingSnapshots=new java.util.concurrent.atomic.AtomicInteger();
  private static final long MAX_BYTES = 64L*1024*1024, MAX_MS = 30*60*1000;
  private final Thread worker;

  DiagnosticSession(Context context) {
    this.context = context.getApplicationContext();
    worker = new Thread(this::run, "chrono-diagnostics");
    worker.start();
  }
  long elapsedMs() { return (SystemClock.elapsedRealtimeNanos()-startNs)/1_000_000; }
  static JSONObject json(Object... kv) {
    JSONObject j = new JSONObject();
    try { for(int i=0;i<kv.length;i+=2) j.put((String)kv[i], kv[i+1] == null ? JSONObject.NULL : kv[i+1]); }
    catch(org.json.JSONException e) { throw new IllegalArgumentException(e); }
    return j;
  }
  static JSONArray array(float[] values) { JSONArray a=new JSONArray(); try { for(float v:values) a.put((double)v); } catch(org.json.JSONException e) { throw new IllegalArgumentException(e); } return a; }
  private boolean offer(Runnable r) {
    if(closing || !queue.offer(r)) { dropped.incrementAndGet(); return false; }
    return true;
  }
  void event(String type, JSONObject data) {
    long t=elapsedMs();
    String line=json("sessionId",id,"elapsedMs",t,"type",type,"data",data).toString();
    offer(() -> writeEvent(line));
  }
  void sample(JSONObject data, String row) {
    long t=elapsedMs();
    String line=json("sessionId",id,"elapsedMs",t,"type","sample","data",data).toString();
    offer(() -> { if(writeEvent(line)) try { csv.write(t+","+row+"\n"); csv.flush(); } catch(IOException e) { fail(e); } });
  }
  boolean acceptsSnapshot() { return !closing && snapshots<200 && pendingSnapshots.get()<2 && elapsedMs()<=MAX_MS; }
  void snapshot(byte[] data, JSONObject metadata) {
    long t=elapsedMs();
    if(pendingSnapshots.incrementAndGet()>2) { pendingSnapshots.decrementAndGet(); event("snapshot_rejected",json("reason","snapshot_queue_full")); return; }
    if(!offer(() -> {
      try {
      if(snapshots>=200 || !canWrite(data.length)) { writeEvent(json("type","snapshot_rejected","elapsedMs",t,"data",json("reason","storage_or_duration_limit")).toString()); return; }
      String stem=String.format(Locale.ROOT,"depth-%08d-%03d",t,snapshots++);
      try {
        try(FileOutputStream f=new FileOutputStream(new File(directory,stem+".u16"))) { f.write(data); }
        bytes+=data.length;
        put(metadata,"file",stem+".u16"); put(metadata,"elapsedMs",t); put(metadata,"sessionId",id);
        writeFile(new File(directory,stem+".json"),metadata.toString(2));
        writeEvent(json("type","snapshot","elapsedMs",t,"data",json("metadata",stem+".json")).toString());
      } catch(Exception e) { fail(e); }
      } finally { pendingSnapshots.decrementAndGet(); }
    })) pendingSnapshots.decrementAndGet();
  }
  static void put(JSONObject j,String k,Object v) { try { j.put(k,v==null?JSONObject.NULL:v); } catch(Exception e) { throw new IllegalArgumentException(e); } }
  private boolean canWrite(long size) { return events!=null && bytes+size<=MAX_BYTES && elapsedMs()<=MAX_MS; }
  private boolean writeEvent(String line) {
    if(!canWrite(line.length()*3L+1)) { status="лимит лога (30 min / 64 MB)"; return false; }
    try { events.write(line); events.newLine(); events.flush(); bytes+=line.length()*3L+1; return true; }
    catch(IOException e) { fail(e); return false; }
  }
  private void fail(Exception e) { status="грешка лога: "+e.getClass().getSimpleName(); }
  private void writeFile(File f,String s) throws IOException { try(Writer w=new OutputStreamWriter(new FileOutputStream(f),StandardCharsets.UTF_8)) { w.write(s); } }
  private void run() {
    try {
      File root=new File(context.getFilesDir(),"diagnostics"); root.mkdirs();
      File[] previous=root.listFiles(File::isDirectory);
      if(previous!=null) { Arrays.sort(previous,Comparator.comparingLong(File::lastModified)); for(int i=0;i<previous.length-4;i++) delete(previous[i]); }
      directory=new File(root,id); if(!directory.mkdirs()) throw new IOException("session directory");
      events=new BufferedWriter(new OutputStreamWriter(new FileOutputStream(new File(directory,"events.jsonl")),StandardCharsets.UTF_8));
      csv=new BufferedWriter(new OutputStreamWriter(new FileOutputStream(new File(directory,"series.csv")),StandardCharsets.UTF_8));
      csv.write("elapsedMs,fps,frameMeanMs,frameMaxMs,slowOver50Ms,depthAgeMs,depthState,occlusion,centerMm,objectZMm,objects,missingFrames,staleFrames,detailed,producerMs,depthTimestampNs,acquisitionStatus,repeatedCameraFrames,repeatedDepthFrames,newDepthFrames,acquisitionFailures,acquisitionMs\n");
      String appVersion="unknown"; long versionCode=-1;
      try {
        android.content.pm.PackageInfo info=context.getPackageManager().getPackageInfo(context.getPackageName(),0);
        appVersion=info.versionName; versionCode=Build.VERSION.SDK_INT>=28?info.getLongVersionCode():info.versionCode;
      } catch(Exception ignored) {}
      JSONObject manifest=json("schemaVersion",2,"sessionId",id,"startedUtc",utc,"startMonotonicNs",startNs,
          "appVersion",appVersion,"versionCode",versionCode,"buildId","ar-lab-v4-depth-20261009","manufacturer",Build.MANUFACTURER,"model",Build.MODEL,"android",Build.VERSION.RELEASE,"sdk",Build.VERSION.SDK_INT,
          "depth","filtered DEPTH16 little-endian uint16 mm; 0=missing; camera-axis Z, not ray distance",
          "confidence",null,"confidenceReason","filtered depth has no raw confidence in this build",
          "cpuNormalization","100 * delta process CPU ms / delta elapsed ms; 100%=one full core; may exceed 100",
          "totalCpuLoad",null,"gpuLoad",null,"cpuTemperatureC",null,"unavailableReason","not exposed by supported public APIs used here",
          "maxSessionMs",MAX_MS,"maxBytes",MAX_BYTES,"maxSnapshots",200,"queueCapacity",128,"retainedSessions",5,
          "depthFreshnessMs",100,"depthWatchdogMs",1000,"autoSnapshotIntervalSeconds",10,
          "snapshotFormat","DEPTH16 u16 uncompressed on disk; ZIP deflate on export",
          "occlusionBiasMm",-80,"rawVideo",false,"audio",false,"upload","not_configured",
          "units",json("time","ms unless Ns suffix","poseTranslation","m","quaternion","qx qy qz qw","memory","KiB","batteryTemperature","C; battery, not CPU"));
      try { put(manifest,"arcoreVersion",context.getPackageManager().getPackageInfo("com.google.ar.core",0).versionName); } catch(Exception e) { put(manifest,"arcoreVersion",null); }
      writeFile(new File(directory,"manifest.json"),manifest.toString(2)); status="пише локално";
      while(!closing || !queue.isEmpty()) {
        Runnable task=queue.poll(250,java.util.concurrent.TimeUnit.MILLISECONDS); if(task!=null) task.run();
        if(active && detailed && elapsedMs()-lastSystem>=2000) { lastSystem=elapsedMs(); systemSample(); }
      }
      writeFile(new File(directory,"end.json"),json("elapsedMs",elapsedMs(),"droppedQueueItems",dropped.get(),"approxBytes",bytes,"snapshots",snapshots,"status",status).toString(2));
    } catch(Exception e) { fail(e); }
    finally { try { if(events!=null) events.close(); if(csv!=null) csv.close(); } catch(IOException ignored) {} closed=true; }
  }
  private void systemSample() {
    long now=SystemClock.elapsedRealtime(), cpu=android.os.Process.getElapsedCpuTime();
    double percent=100.0*(cpu-lastCpu)/Math.max(1,now-lastWall); lastCpu=cpu; lastWall=now;
    Debug.MemoryInfo memory=new Debug.MemoryInfo(); Debug.getMemoryInfo(memory);
    if(baselinePssKiB<0 && elapsedMs()>=15_000) baselinePssKiB=memory.getTotalPss();
    Intent battery=context.registerReceiver(null,new IntentFilter(Intent.ACTION_BATTERY_CHANGED));
    PowerManager power=(PowerManager)context.getSystemService(Context.POWER_SERVICE);
    int level=battery==null?-1:battery.getIntExtra(BatteryManager.EXTRA_LEVEL,-1), scale=battery==null?-1:battery.getIntExtra(BatteryManager.EXTRA_SCALE,-1);
    JSONObject data=json("pssKiB",memory.getTotalPss(),"privateDirtyKiB",memory.getTotalPrivateDirty(),"nativeHeapKiB",Debug.getNativeHeapAllocatedSize()/1024,
      "javaHeapKiB",(Runtime.getRuntime().totalMemory()-Runtime.getRuntime().freeMemory())/1024,
      "processCpuTimeMs",cpu,"processCpuOneCorePercent",percent,
      "thermalStatus",Build.VERSION.SDK_INT>=29?power.getCurrentThermalStatus():null,
      "thermalReason",Build.VERSION.SDK_INT>=29?"Android thermal severity; not temperature":"API<29",
      "batteryPercent",level>=0&&scale>0?100.0*level/scale:null,
      "batteryTemperatureC",battery!=null&&battery.hasExtra(BatteryManager.EXTRA_TEMPERATURE)?battery.getIntExtra(BatteryManager.EXTRA_TEMPERATURE,0)/10.0:null,
      "droppedQueueItems",dropped.get(),"pssDeltaSinceWarmupKiB",baselinePssKiB<0?null:memory.getTotalPss()-baselinePssKiB);
    writeEvent(json("sessionId",id,"elapsedMs",elapsedMs(),"type","system","data",data).toString());
  }
  private File archive(boolean stillRunning) throws IOException {
    if(directory==null) throw new IOException("session directory not initialized");
    if(stillRunning) { events.flush(); csv.flush(); }
    File outDir=new File(context.getCacheDir(),"diagnostic-exports"); outDir.mkdirs();
    File[] old=outDir.listFiles();
    if(old!=null) {
      Arrays.sort(old,Comparator.comparingLong(File::lastModified));
      for(int i=0;i<old.length-2;i++) old[i].delete();
    }
    File zip=new File(outDir,"ChronoEye-"+id+"-"+elapsedMs()+".zip");
    writeFile(new File(directory,"export.json"),json("exportElapsedMs",elapsedMs(),
      "droppedQueueItems",dropped.get(),"status",status,"sessionContinues",stillRunning,
      "buildId","ar-lab-v4-depth-20261009").toString());
    try(ZipOutputStream z=new ZipOutputStream(new FileOutputStream(zip))) {
      File[] files=directory.listFiles(); if(files==null) throw new IOException("no session files");
      byte[] buffer=new byte[8192];
      for(File f:files) {
        if(!f.isFile()) continue;
        z.putNextEntry(new ZipEntry(f.getName()));
        try(InputStream in=new FileInputStream(f)) { int n; while((n=in.read(buffer))!=-1) z.write(buffer,0,n); }
        z.closeEntry();
      }
    }
    return zip;
  }
  private void openShareChooser(Activity activity,File zip) {
    activity.runOnUiThread(() -> {
      if(activity.isFinishing()) return;
      android.net.Uri uri=FileProvider.getUriForFile(activity,activity.getPackageName()+".diagnostics",zip);
      Intent send=new Intent(Intent.ACTION_SEND).setType("application/zip")
          .putExtra(Intent.EXTRA_STREAM,uri).addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
      send.setClipData(ClipData.newRawUri("AR diagnostics",uri));
      activity.startActivity(Intent.createChooser(send,"Подели дијагностику"));
    });
  }
  void share(Activity activity) {
    if(closing) { shareCompleted(activity); return; }
    if(!sharing.compareAndSet(false,true)) return;
    if(!offer(() -> {
      try { openShareChooser(activity,archive(true)); }
      catch(Exception e) { fail(e); activity.runOnUiThread(() ->
          android.widget.Toast.makeText(activity,status,android.widget.Toast.LENGTH_LONG).show()); }
      finally { sharing.set(false); }
    })) { sharing.set(false); activity.runOnUiThread(() ->
        android.widget.Toast.makeText(activity,"Ред је пун; покушај поново",android.widget.Toast.LENGTH_SHORT).show()); }
  }
  void finishAndShare(Activity activity) {
    close("user_finished");
    shareCompleted(activity);
  }
  private void shareCompleted(Activity activity) {
    if(!sharing.compareAndSet(false,true)) return;
    new Thread(() -> {
      try {
        worker.join(); // Off the UI thread: wait until session_end and end.json are durable.
        if(!closed) throw new IOException("session close incomplete");
        openShareChooser(activity,archive(false));
      } catch(Exception e) {
        fail(e); activity.runOnUiThread(() ->
            android.widget.Toast.makeText(activity,status,android.widget.Toast.LENGTH_LONG).show());
      } finally { sharing.set(false); }
    },"chrono-final-export").start();
  }
  synchronized void close(String reason) {
    if(closing) return;
    active=false;
    event("session_end",json("reason",reason,"closedElapsedMs",elapsedMs()));
    closing=true;
  }
  void close() { close("activity_destroyed"); }
  private static void delete(File f) { File[] children=f.listFiles(); if(children!=null) for(File c:children) delete(c); f.delete(); }
}

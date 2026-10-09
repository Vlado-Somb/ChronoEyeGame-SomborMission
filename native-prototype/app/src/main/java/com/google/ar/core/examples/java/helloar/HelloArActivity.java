/*
 * Copyright 2017 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.google.ar.core.examples.java.helloar;
// ChronoEye AR Lab: diagnostic depth, bounded telemetry, five local anchors.
import android.content.Intent;
import org.json.JSONObject;
import org.json.JSONArray;
import static com.google.ar.core.examples.java.helloar.DiagnosticSession.json;
import android.os.SystemClock;
import android.widget.Button;
import android.widget.TextView;
import com.google.ar.core.Coordinates2d;
import java.nio.ByteOrder;

import android.content.DialogInterface;
import android.content.res.Resources;
import android.media.Image;
import android.opengl.GLES30;
import android.opengl.GLSurfaceView;
import android.opengl.Matrix;
import android.os.Bundle;
import android.util.Log;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.view.View;
import android.widget.ImageButton;
import android.widget.PopupMenu;
import android.widget.Toast;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import com.google.ar.core.Anchor;
import com.google.ar.core.ArCoreApk;
import com.google.ar.core.ArCoreApk.Availability;
import com.google.ar.core.Camera;
import com.google.ar.core.Config;
import com.google.ar.core.Config.InstantPlacementMode;
import com.google.ar.core.DepthPoint;
import com.google.ar.core.Frame;
import com.google.ar.core.HitResult;
import com.google.ar.core.InstantPlacementPoint;
import com.google.ar.core.LightEstimate;
import com.google.ar.core.Plane;
import com.google.ar.core.Point;
import com.google.ar.core.Point.OrientationMode;
import com.google.ar.core.PointCloud;
import com.google.ar.core.Session;
import com.google.ar.core.Trackable;
import com.google.ar.core.TrackingFailureReason;
import com.google.ar.core.TrackingState;
import com.google.ar.core.examples.java.common.helpers.CameraPermissionHelper;
import com.google.ar.core.examples.java.common.helpers.DepthSettings;
import com.google.ar.core.examples.java.common.helpers.DisplayRotationHelper;
import com.google.ar.core.examples.java.common.helpers.FullScreenHelper;
import com.google.ar.core.examples.java.common.helpers.InstantPlacementSettings;
import com.google.ar.core.examples.java.common.helpers.SnackbarHelper;
import com.google.ar.core.examples.java.common.helpers.TapHelper;
import com.google.ar.core.examples.java.common.helpers.TrackingStateHelper;
import com.google.ar.core.examples.java.common.samplerender.Framebuffer;
import com.google.ar.core.examples.java.common.samplerender.GLError;
import com.google.ar.core.examples.java.common.samplerender.Mesh;
import com.google.ar.core.examples.java.common.samplerender.SampleRender;
import com.google.ar.core.examples.java.common.samplerender.Shader;
import com.google.ar.core.examples.java.common.samplerender.Texture;
import com.google.ar.core.examples.java.common.samplerender.VertexBuffer;
import com.google.ar.core.examples.java.common.samplerender.arcore.BackgroundRenderer;
import com.google.ar.core.examples.java.common.samplerender.arcore.PlaneRenderer;
import com.google.ar.core.examples.java.common.samplerender.arcore.SpecularCubemapFilter;
import com.google.ar.core.exceptions.CameraNotAvailableException;
import com.google.ar.core.exceptions.NotYetAvailableException;
import com.google.ar.core.exceptions.UnavailableApkTooOldException;
import com.google.ar.core.exceptions.UnavailableArcoreNotInstalledException;
import com.google.ar.core.exceptions.UnavailableDeviceNotCompatibleException;
import com.google.ar.core.exceptions.UnavailableSdkTooOldException;
import com.google.ar.core.exceptions.UnavailableUserDeclinedInstallationException;
import java.io.IOException;
import java.io.InputStream;
import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

/**
 * This is a simple example that shows how to create an augmented reality (AR) application using the
 * ARCore API. The application will display any detected planes and will allow the user to tap on a
 * plane to place a 3D model.
 */
public class HelloArActivity extends AppCompatActivity implements SampleRender.Renderer {

  private static final String TAG = HelloArActivity.class.getSimpleName();

  private static final String SEARCHING_PLANE_MESSAGE = "Полако помери телефон да препозна површину.";
  private static final String WAITING_FOR_TAP_MESSAGE = "Додирни под или сто да поставиш предмет.";

  // See the definition of updateSphericalHarmonicsCoefficients for an explanation of these
  // constants.
  private static final float[] sphericalHarmonicFactors = {
    0.282095f,
    -0.325735f,
    0.325735f,
    -0.325735f,
    0.273137f,
    -0.273137f,
    0.078848f,
    -0.273137f,
    0.136569f,
  };

  private static final float Z_NEAR = 0.1f;
  private static final float Z_FAR = 100f;
  private static final int MAX_CRYSTALS = 5;
  private static final float CRYSTAL_SCALE = 2.0f;
  private static final float CRYSTAL_PICK_RADIUS_METERS = .28f;
  private static final long DEPTH_FRESHNESS_NS = 100_000_000L;

  private static final int CUBEMAP_RESOLUTION = 16;
  private static final int CUBEMAP_NUMBER_OF_IMPORTANCE_SAMPLES = 32;

  // Rendering. The Renderers are created here, and initialized when the GL surface is created.
  private GLSurfaceView surfaceView;

  private boolean installRequested;
  private TextView diagnostics, availableCrystalsLabel;
  private Button depthButton;
  private volatile boolean depthWanted = true;
  private volatile boolean resetRequested;
  private volatile int crystalsAvailable = MAX_CRYSTALS;
  private boolean depthSupported, completing, renderReady;
  private long statsStart;
  private int statsFrames;
  private float lastFps;
  private final float[] tapView = new float[16], tapProjection = new float[16];
  private final float[] tapVp = new float[16], tapInverse = new float[16];
  private final float[] rayClip = new float[4], rayWorld = new float[4];
  private final float[] rayOrigin = new float[3], objectCenter = new float[3], cameraPoint = new float[3];
  private final float[] localCenter = {0f, .11f * CRYSTAL_SCALE, 0f};
  private final float[] depthInput = new float[2], depthUv = new float[2];

  private DiagnosticSession log;
  private volatile int viewMode, snapshotInterval;
  private volatile boolean markRequested, collectRequested;
  private boolean touchDepthVerified;
  private int selectedId=-1, nextId=1;
  private String depthState="no_data", lastTracking="";
  private double depthAge=Double.NaN, centerMm=-1, selectedZ=-1, selectedRealMm=-1;
  private long lastFrameNs, sampleNs, missingFrames, staleFrames, slowFrames, lastSnapshotMs;
  private double frameSum, frameMax, producerMs;
  private int frameCount;
  private final float[][] objectColors={{1f,.6f,.05f},{.05f,.7f,1f},{.2f,1f,.3f},{1f,.2f,.5f},{.7f,.35f,1f}};
  private final String[] colorNames={"златни","плави","зелени","ружичасти","љубичасти"};

  private Session session;
  private final SnackbarHelper messageSnackbarHelper = new SnackbarHelper();
  private DisplayRotationHelper displayRotationHelper;
  private final TrackingStateHelper trackingStateHelper = new TrackingStateHelper(this);
  private TapHelper tapHelper;
  private SampleRender render;

  private PlaneRenderer planeRenderer;
  private BackgroundRenderer backgroundRenderer;
  private Framebuffer virtualSceneFramebuffer;
  private boolean hasSetTextureNames = false;

  private final DepthSettings depthSettings = new DepthSettings();
  private boolean[] depthSettingsMenuDialogCheckboxes = new boolean[2];

  private final InstantPlacementSettings instantPlacementSettings = new InstantPlacementSettings();
  private boolean[] instantPlacementSettingsMenuDialogCheckboxes = new boolean[1];
  // Assumed distance from the device camera to the surface on which user will try to place objects.
  // This value affects the apparent scale of objects while the tracking method of the
  // Instant Placement point is SCREENSPACE_WITH_APPROXIMATE_DISTANCE.
  // Values in the [0.2, 2.0] meter range are a good choice for most AR experiences. Use lower
  // values for AR experiences where users are expected to place objects on surfaces close to the
  // camera. Use larger values for experiences where the user will likely be standing and trying to
  // place an object on the ground or floor in front of them.
  private static final float APPROXIMATE_DISTANCE_METERS = 2.0f;

  // Point Cloud
  private VertexBuffer pointCloudVertexBuffer;
  private Mesh pointCloudMesh;
  private Shader pointCloudShader;
  // Keep track of the last point cloud rendered to avoid updating the VBO if point cloud
  // was not changed.  Do this using the timestamp since we can't compare PointCloud objects.
  private long lastPointCloudTimestamp = 0;

  // Virtual object (ARCore pawn)
  private Mesh virtualObjectMesh;
  private Shader virtualObjectShader;
  private Texture virtualObjectAlbedoTexture;
  private Texture virtualObjectAlbedoInstantPlacementTexture;

  private final List<WrappedAnchor> wrappedAnchors = new ArrayList<>();

  // Environmental HDR
  private Texture dfgTexture;
  private SpecularCubemapFilter cubemapFilter;

  // Temporary matrix allocated here to reduce number of allocations for each frame.
  private final float[] modelMatrix = new float[16];
  private final float[] viewMatrix = new float[16];
  private final float[] projectionMatrix = new float[16];
  private final float[] modelViewMatrix = new float[16]; // view x model
  private final float[] modelViewProjectionMatrix = new float[16]; // projection x view x model
  private final float[] sphericalHarmonicsCoefficients = new float[9 * 3];
  private final float[] viewInverseMatrix = new float[16];
  private final float[] worldLightDirection = {0.0f, 0.0f, 0.0f, 0.0f};
  private final float[] viewLightDirection = new float[4]; // view x world light direction

  @Override
  protected void onCreate(Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);
    setContentView(R.layout.activity_main);
    log=new DiagnosticSession(this);
    log.event("session_start",json());
    surfaceView = findViewById(R.id.surfaceview);
    displayRotationHelper = new DisplayRotationHelper(/* context= */ this);

    // Set up touch listener.
    tapHelper = new TapHelper(/* context= */ this);
    surfaceView.setOnTouchListener(tapHelper);

    // Set up renderer.
    render = new SampleRender(surfaceView, this, getAssets());

    installRequested = false;

    depthSettings.onCreate(this);
    instantPlacementSettings.onCreate(this);
    diagnostics = findViewById(R.id.diagnostics);
    availableCrystalsLabel = findViewById(R.id.available_crystals);
    depthButton = findViewById(R.id.depth_button);
    depthButton.setOnClickListener(v -> {
      depthWanted = !depthWanted;
      depthButton.setText(depthWanted ? "Заклањање: ДА" : "Заклањање: НЕ");
      log.event("occlusion_toggle",json("enabled",depthWanted));
    });
    findViewById(R.id.reset_button).setOnClickListener(v -> resetRequested = true);
    findViewById(R.id.return_button).setOnClickListener(v -> {
      log.event("test_finished", json("remaining", crystalsAvailable));
      getSharedPreferences("chrono_progress", MODE_PRIVATE).edit().putBoolean("testFinished", true).apply();
      setResult(RESULT_OK, new Intent().putExtra("event", "ar_test_finished").putExtra("sessionId", log.id));
      finish();
    });
    findViewById(R.id.mode_button).setOnClickListener(v -> {
      viewMode=(viewMode+1)%3;
      ((Button)v).setText(new String[]{"Камера","Дубина","Преклапање"}[viewMode]);
      log.event("view_mode",json("mode",viewMode));
    });
    findViewById(R.id.mark_button).setOnClickListener(v -> {
      log.event("problem_mark",json("requestedElapsedMs",log.elapsedMs())); markRequested=true;
      Toast.makeText(this,"Проблем обележен",Toast.LENGTH_SHORT).show();
    });
    findViewById(R.id.share_button).setOnClickListener(v -> log.share(this));
    findViewById(R.id.collect_button).setOnClickListener(v -> collectRequested=true);
    findViewById(R.id.sample_button).setOnClickListener(v -> {
      snapshotInterval=snapshotInterval==0?10:snapshotInterval==10?30:0;
      ((Button)v).setText("Снимци: "+(snapshotInterval==0?"ручни":snapshotInterval+" s"));
      log.event("snapshot_interval",json("seconds",snapshotInterval));
    });
    findViewById(R.id.log_button).setOnClickListener(v -> {
      log.detailed=!log.detailed; ((Button)v).setText(log.detailed?"Лог: пун":"Лог: основни");
      log.event("logging_mode",json("detailed",log.detailed));
    });
  }

  /** Menu button to launch feature specific settings. */
  protected boolean settingsMenuClick(MenuItem item) {
    if (item.getItemId() == R.id.depth_settings) {
      launchDepthSettingsMenuDialog();
      return true;
    } else if (item.getItemId() == R.id.instant_placement_settings) {
      launchInstantPlacementSettingsMenuDialog();
      return true;
    }
    return false;
  }

  @Override
  protected void onDestroy() {
    if (session != null) {
      // Explicitly close ARCore Session to release native resources.
      // Review the API reference for important considerations before calling close() in apps with
      // more complicated lifecycle requirements:
      // https://developers.google.com/ar/reference/java/arcore/reference/com/google/ar/core/Session#close()
      for (WrappedAnchor wrapped : wrappedAnchors) wrapped.getAnchor().detach();
      wrappedAnchors.clear();
      session.close();
      session = null;
    }

    if(log!=null) log.close();
    super.onDestroy();
  }

  @Override
  protected void onResume() {
    super.onResume();
    if(log!=null) { log.active=true; log.event("resume",json()); }
    lastFrameNs=0; sampleNs=0; frameSum=0; frameMax=0; frameCount=0; slowFrames=0;

    if (session == null) {
      Exception exception = null;
      String message = null;
      try {
        // Always check the latest availability.
        Availability availability = ArCoreApk.getInstance().checkAvailability(this);

        // In all other cases, try to install ARCore and handle installation failures.
        if (availability != Availability.SUPPORTED_INSTALLED) {
          switch (ArCoreApk.getInstance().requestInstall(this, !installRequested)) {
            case INSTALL_REQUESTED:
              installRequested = true;
              return;
            case INSTALLED:
              break;
          }
        }

        // ARCore requires camera permissions to operate. If we did not yet obtain runtime
        // permission on Android M and above, now is a good time to ask the user for it.
        if (!CameraPermissionHelper.hasCameraPermission(this)) {
          CameraPermissionHelper.requestCameraPermission(this);
          return;
        }

        // Create the session.
        session = new Session(/* context= */ this);
      } catch (UnavailableArcoreNotInstalledException
          | UnavailableUserDeclinedInstallationException e) {
        message = "Please install ARCore";
        exception = e;
      } catch (UnavailableApkTooOldException e) {
        message = "Please update ARCore";
        exception = e;
      } catch (UnavailableSdkTooOldException e) {
        message = "Please update this app";
        exception = e;
      } catch (UnavailableDeviceNotCompatibleException e) {
        message = "This device does not support AR";
        exception = e;
      } catch (Exception e) {
        message = "Failed to create AR session";
        exception = e;
      }

      if (message != null) {
        messageSnackbarHelper.showError(this, message);
        Log.e(TAG, "Exception creating session", exception);
        log.event("session_error",json("error",String.valueOf(exception)));
        return;
      }
    }

    // Note that order matters - see the note in onPause(), the reverse applies here.
    try {
      depthSupported = session.isDepthModeSupported(Config.DepthMode.AUTOMATIC);
      depthButton.setEnabled(depthSupported);
      depthButton.setText(depthSupported ? (depthWanted ? "Заклањање: ДА" : "Заклањање: НЕ") : "Depth: НЕМА");
      getSharedPreferences("chrono_progress", MODE_PRIVATE).edit().putBoolean("depthSupported", depthSupported).apply();
      configureSession();
      // To record a live camera session for later playback, call
      // `session.startRecording(recordingConfig)` at anytime. To playback a previously recorded AR
      // session instead of using the live camera feed, call
      // `session.setPlaybackDatasetUri(Uri)` before calling `session.resume()`. To
      // learn more about recording and playback, see:
      // https://developers.google.com/ar/develop/java/recording-and-playback
      session.resume();
    } catch (CameraNotAvailableException e) {
      messageSnackbarHelper.showError(this, "Camera not available. Try restarting the app.");
      session.close();
      session = null;
      return;
    }
    statsStart = 0;
    statsFrames = 0;
    surfaceView.onResume();
    displayRotationHelper.onResume();
  }

  @Override
  public void onPause() {
    super.onPause();
    if(log!=null) { log.active=false; log.event("pause",json()); }
    if (session != null) {
      // Note that the order matters - GLSurfaceView is paused first so that it does not try
      // to query the session. If Session is paused before GLSurfaceView, GLSurfaceView may
      // still call session.update() and get a SessionPausedException.
      displayRotationHelper.onPause();
      surfaceView.onPause();
      session.pause();
    }
    tapHelper.clear();
    getSharedPreferences("chrono_progress", MODE_PRIVATE).edit().putFloat("lastFps", lastFps).apply();
  }

  @Override
  public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] results) {
    super.onRequestPermissionsResult(requestCode, permissions, results);
    if (!CameraPermissionHelper.hasCameraPermission(this)) {
      // Use toast instead of snackbar here since the activity will exit.
      Toast.makeText(this, "Camera permission is needed to run this application", Toast.LENGTH_LONG)
          .show();
      if (!CameraPermissionHelper.shouldShowRequestPermissionRationale(this)) {
        // Permission denied with checking "Do not ask again".
        CameraPermissionHelper.launchPermissionSettings(this);
      }
      finish();
    }
  }

  @Override
  public void onWindowFocusChanged(boolean hasFocus) {
    super.onWindowFocusChanged(hasFocus);
    FullScreenHelper.setFullScreenOnWindowFocusChanged(this, hasFocus);
  }

  @Override
  public void onSurfaceCreated(SampleRender render) {
    hasSetTextureNames = false;
    renderReady = false;
    // Prepare the rendering objects. This involves reading shaders and 3D model files, so may throw
    // an IOException.
    try {
      planeRenderer = new PlaneRenderer(render);
      backgroundRenderer = new BackgroundRenderer(render);
      backgroundRenderer.setUseDepthVisualization(render,false);
      virtualSceneFramebuffer = new Framebuffer(render, /* width= */ 1, /* height= */ 1);

      cubemapFilter =
          new SpecularCubemapFilter(
              render, CUBEMAP_RESOLUTION, CUBEMAP_NUMBER_OF_IMPORTANCE_SAMPLES);
      // Load DFG lookup table for environmental lighting
      dfgTexture =
          new Texture(
              render,
              Texture.Target.TEXTURE_2D,
              Texture.WrapMode.CLAMP_TO_EDGE,
              /* useMipmaps= */ false);
      // The dfg.raw file is a raw half-float texture with two channels.
      final int dfgResolution = 64;
      final int dfgChannels = 2;
      final int halfFloatSize = 2;

      ByteBuffer buffer =
          ByteBuffer.allocateDirect(dfgResolution * dfgResolution * dfgChannels * halfFloatSize);
      try (InputStream is = getAssets().open("models/dfg.raw")) {
        byte[] data = new byte[buffer.capacity()];
        new java.io.DataInputStream(is).readFully(data);
        buffer.put(data).rewind();
      }
      // SampleRender abstraction leaks here.
      GLES30.glBindTexture(GLES30.GL_TEXTURE_2D, dfgTexture.getTextureId());
      GLError.maybeThrowGLException("Failed to bind DFG texture", "glBindTexture");
      GLES30.glTexImage2D(
          GLES30.GL_TEXTURE_2D,
          /* level= */ 0,
          GLES30.GL_RG16F,
          /* width= */ dfgResolution,
          /* height= */ dfgResolution,
          /* border= */ 0,
          GLES30.GL_RG,
          GLES30.GL_HALF_FLOAT,
          buffer);
      GLError.maybeThrowGLException("Failed to populate DFG texture", "glTexImage2D");

      // Virtual object to render (ARCore pawn)
      virtualObjectAlbedoTexture =
          Texture.createFromAsset(
              render,
              "models/pawn_albedo.png",
              Texture.WrapMode.CLAMP_TO_EDGE,
              Texture.ColorFormat.SRGB);
      virtualObjectAlbedoInstantPlacementTexture =
          Texture.createFromAsset(
              render,
              "models/pawn_albedo_instant_placement.png",
              Texture.WrapMode.CLAMP_TO_EDGE,
              Texture.ColorFormat.SRGB);
      Texture virtualObjectPbrTexture =
          Texture.createFromAsset(
              render,
              "models/pawn_roughness_metallic_ao.png",
              Texture.WrapMode.CLAMP_TO_EDGE,
              Texture.ColorFormat.LINEAR);

      virtualObjectMesh = Mesh.createFromAsset(render, "models/pawn.obj");
      virtualObjectShader =
          Shader.createFromAssets(
                  render,
                  "shaders/environmental_hdr.vert",
                  "shaders/environmental_hdr.frag",
                  /* defines= */ new HashMap<String, String>() {
                    {
                      put(
                          "NUMBER_OF_MIPMAP_LEVELS",
                          Integer.toString(cubemapFilter.getNumberOfMipmapLevels()));
                    }
                  })
              .setTexture("u_AlbedoTexture", virtualObjectAlbedoTexture)
              .setTexture("u_RoughnessMetallicAmbientOcclusionTexture", virtualObjectPbrTexture)
              .setTexture("u_Cubemap", cubemapFilter.getFilteredCubemapTexture())
              .setTexture("u_DfgTexture", dfgTexture);
      renderReady = true;
    } catch (IOException e) {
      Log.e(TAG, "Failed to read a required asset file", e);
      messageSnackbarHelper.showError(this, "Failed to read a required asset file: " + e);
    }
  }

  @Override
  public void onSurfaceChanged(SampleRender render, int width, int height) {
    if (!renderReady) return;
    displayRotationHelper.onSurfaceChanged(width, height);
    virtualSceneFramebuffer.resize(width, height);
  }

  @Override
  public void onDrawFrame(SampleRender render) {
    if (session == null || !renderReady || completing) return;
    if (resetRequested) {
      log.event("reset",json("objects",wrappedAnchors.size())); selectedId=-1;
      for (WrappedAnchor wrapped : wrappedAnchors) wrapped.getAnchor().detach();
      wrappedAnchors.clear();
      resetRequested = false;
      updateAvailableCrystals();
    }

    // Texture names should only be set once on a GL thread unless they change. This is done during
    // onDrawFrame rather than onSurfaceCreated since the session is not guaranteed to have been
    // initialized during the execution of onSurfaceCreated.
    if (!hasSetTextureNames) {
      session.setCameraTextureNames(
          new int[] {backgroundRenderer.getCameraColorTexture().getTextureId()});
      hasSetTextureNames = true;
    }

    // -- Update per-frame state

    // Notify ARCore session that the view size changed so that the perspective matrix and
    // the video background can be properly adjusted.
    displayRotationHelper.updateSessionIfNeeded(session);

    // Obtain the current frame from the AR Session. When the configuration is set to
    // UpdateMode.BLOCKING (it is by default), this will throttle the rendering to the
    // camera framerate.
    Frame frame;
    try {
      frame = session.update();
    } catch (CameraNotAvailableException e) {
      Log.e(TAG, "Camera not available during onDrawFrame", e);
      log.event("camera_error",json("error",e.toString()));
      messageSnackbarHelper.showError(this, "Camera not available. Try restarting the app.");
      return;
    }
    Camera camera = frame.getCamera();

    backgroundRenderer.updateDisplayGeometry(frame);
    long producerStart=SystemClock.elapsedRealtimeNanos();
    boolean freshDepth=false;
    depthAge=Double.NaN; centerMm=-1; selectedZ=-1; selectedRealMm=-1;
    depthState=!depthSupported?"unsupported":camera.getTrackingState()!=TrackingState.TRACKING?"not_tracking":"no_data";
    for(WrappedAnchor w:wrappedAnchors) if(w.id==selectedId && w.getAnchor().getTrackingState()==TrackingState.TRACKING) {
      w.getAnchor().getPose().transformPoint(localCenter,0,objectCenter,0);
      camera.getPose().inverse().transformPoint(objectCenter,0,cameraPoint,0); selectedZ=-cameraPoint[2]*1000.0;
    }
    boolean snapshotDue=markRequested || (log.detailed && snapshotInterval>0 && log.elapsedMs()-lastSnapshotMs>=snapshotInterval*1000L);
    if(camera.getTrackingState()==TrackingState.TRACKING && depthSupported) {
      try(Image image=frame.acquireDepthImage16Bits()) {
        depthAge=(frame.getTimestamp()-image.getTimestamp())/1_000_000.0;
        // ARCore camera/depth timestamps can differ slightly in either direction.
        freshDepth=Math.abs(depthAge)<=DEPTH_FRESHNESS_NS/1_000_000.0;
        depthState=freshDepth?"active":"stale";
        if(!freshDepth) staleFrames++;
        if(freshDepth) backgroundRenderer.updateCameraDepthTexture(image);
        centerMm=depthAt(frame,image,surfaceView.getWidth()/2f,surfaceView.getHeight()/2f);
        for(WrappedAnchor w:wrappedAnchors) if(w.id==selectedId && w.getAnchor().getTrackingState()==TrackingState.TRACKING) {
          w.getAnchor().getPose().transformPoint(localCenter,0,objectCenter,0);
          camera.getPose().inverse().transformPoint(objectCenter,0,cameraPoint,0);
          selectedZ=-cameraPoint[2]*1000.0;
          float[] point={objectCenter[0],objectCenter[1],objectCenter[2],1}, clip=new float[4];
          camera.getViewMatrix(tapView,0); camera.getProjectionMatrix(tapProjection,0,Z_NEAR,Z_FAR);
          Matrix.multiplyMM(tapVp,0,tapProjection,0,tapView,0); Matrix.multiplyMV(clip,0,tapVp,0,point,0);
          if(clip[3]>0) selectedRealMm=depthAt(frame,image,(clip[0]/clip[3]+1)*surfaceView.getWidth()/2f,(1-clip[1]/clip[3])*surfaceView.getHeight()/2f);
        }
        if(snapshotDue) { captureDepth(frame,camera,image,markRequested?"problem":"periodic"); lastSnapshotMs=log.elapsedMs(); }
      } catch(NotYetAvailableException e) { missingFrames++; }
      catch(RuntimeException e) { log.event("depth_error",json("error",e.toString())); depthState="error"; }
    } else missingFrames++;
    if(snapshotDue && (depthState.equals("no_data") || depthState.equals("not_tracking") || depthState.equals("unsupported") || depthState.equals("error"))) {
      log.event("snapshot_unavailable",json("reason",depthState,"problem",markRequested)); lastSnapshotMs=log.elapsedMs();
    }
    markRequested=false;
    try { backgroundRenderer.setUseOcclusion(render,freshDepth && depthWanted); }
    catch(IOException e) { log.event("shader_error",json("error",e.toString())); return; }
    backgroundRenderer.setDiagnosticView(viewMode,freshDepth);
    producerMs=(SystemClock.elapsedRealtimeNanos()-producerStart)/1_000_000.0;
    updateDiagnostics(frame,camera,freshDepth && depthWanted);
    if(collectRequested) {
      collectRequested=false;
      WrappedAnchor selected=null;
      for(WrappedAnchor w:wrappedAnchors) if(w.id==selectedId) { selected=w; break; }
      if(selected==null) {
        log.event("collection_rejected",json("id",selectedId,"reason","no_selected_crystal"));
        runOnUiThread(() -> Toast.makeText(this,"Прво постави дијамант",Toast.LENGTH_SHORT).show());
      } else {
        // Confirmed foreground geometry blocks collection, but missing depth does not.
        boolean knownOccluded=depthWanted && depthSupported && freshDepth &&
            selectedZ>0 && selectedRealMm>0 &&
            selectedRealMm < selectedZ-CRYSTAL_PICK_RADIUS_METERS*1000.0;
        if(knownOccluded) {
          log.event("collection_rejected",json("id",selected.id,"reason","confirmed_occlusion"));
          runOnUiThread(() -> Toast.makeText(this,"Дијамант је заклоњен",Toast.LENGTH_SHORT).show());
        } else {
          collectCrystal(selected,"button",freshDepth && selectedRealMm>0);
        }
      }
    }

    // Handle one tap per frame.
    handleTap(frame, camera);

    // Keep the screen unlocked while tracking, but allow it to lock when tracking stops.
    trackingStateHelper.updateKeepScreenOnFlag(camera.getTrackingState());

    // Show a message based on whether tracking has failed, if planes are detected, and if the user
    // has placed any objects.
    String message = null;
    if (camera.getTrackingState() == TrackingState.PAUSED) {
      if (camera.getTrackingFailureReason() == TrackingFailureReason.NONE) {
        message = SEARCHING_PLANE_MESSAGE;
      } else {
        message = TrackingStateHelper.getTrackingFailureReasonString(camera);
      }
    } else if (!wrappedAnchors.isEmpty()) {
      message = null;
    } else if (hasTrackingPlane()) {
      if (wrappedAnchors.isEmpty()) {
        message = WAITING_FOR_TAP_MESSAGE;
      }
    } else {
      message = SEARCHING_PLANE_MESSAGE;
    }
    if (message == null) {
      messageSnackbarHelper.hide(this);
    } else {
      messageSnackbarHelper.showMessage(this, message);
    }

    // -- Draw background

    if (frame.getTimestamp() != 0) {
      // Suppress rendering if the camera did not produce the first frame yet. This is to avoid
      // drawing possible leftover data from previous sessions if the texture is reused.
      backgroundRenderer.drawBackground(render);
    }

    // If not tracking, don't draw 3D objects.
    if (camera.getTrackingState() == TrackingState.PAUSED) {
      return;
    }

    // -- Draw non-occluded virtual objects (planes, point cloud)

    // Get projection matrix.
    camera.getProjectionMatrix(projectionMatrix, 0, Z_NEAR, Z_FAR);

    // Get camera matrix and draw.
    camera.getViewMatrix(viewMatrix, 0);

    if (wrappedAnchors.isEmpty()) {
      planeRenderer.drawPlanes(render, session.getAllTrackables(Plane.class), camera.getDisplayOrientedPose(), projectionMatrix);
    }

    // -- Draw occluded virtual objects

    // Update lighting parameters in the shader
    updateLightEstimation(frame.getLightEstimate(), viewMatrix);

    // Visualize anchors created by touch.
    render.clear(virtualSceneFramebuffer, 0f, 0f, 0f, 0f);
    for (WrappedAnchor wrappedAnchor : wrappedAnchors) {
      Anchor anchor = wrappedAnchor.getAnchor();
      Trackable trackable = wrappedAnchor.getTrackable();
      if (anchor.getTrackingState() != TrackingState.TRACKING) {
        continue;
      }

      // Get the current pose of an Anchor in world space. The Anchor pose is updated
      // during calls to session.update() as ARCore refines its estimate of the world.
      anchor.getPose().toMatrix(modelMatrix, 0);
      Matrix.scaleM(modelMatrix, 0, CRYSTAL_SCALE, CRYSTAL_SCALE, CRYSTAL_SCALE);

      // Calculate model/view/projection matrices
      Matrix.multiplyMM(modelViewMatrix, 0, viewMatrix, 0, modelMatrix, 0);
      Matrix.multiplyMM(modelViewProjectionMatrix, 0, projectionMatrix, 0, modelViewMatrix, 0);

      // Update shader properties and draw
      virtualObjectShader.setVec3("u_ObjectTint",objectColors[wrappedAnchor.color]);
      virtualObjectShader.setMat4("u_ModelView", modelViewMatrix);
      virtualObjectShader.setMat4("u_ModelViewProjection", modelViewProjectionMatrix);

      if (trackable instanceof InstantPlacementPoint
          && ((InstantPlacementPoint) trackable).getTrackingMethod()
              == InstantPlacementPoint.TrackingMethod.SCREENSPACE_WITH_APPROXIMATE_DISTANCE) {
        virtualObjectShader.setTexture(
            "u_AlbedoTexture", virtualObjectAlbedoInstantPlacementTexture);
      } else {
        virtualObjectShader.setTexture("u_AlbedoTexture", virtualObjectAlbedoTexture);
      }

      render.draw(virtualObjectMesh, virtualObjectShader, virtualSceneFramebuffer);
    }

    // Compose the virtual scene with the background.
    backgroundRenderer.drawVirtualScene(render, virtualSceneFramebuffer, Z_NEAR, Z_FAR);
  }

  private void updateAvailableCrystals() {
    crystalsAvailable=Math.max(0,MAX_CRYSTALS-wrappedAnchors.size());
    final int count=crystalsAvailable;
    runOnUiThread(() -> {
      if(!isFinishing()) availableCrystalsLabel.setText("Преостало: "+count+"/"+MAX_CRYSTALS+" дијаманата");
    });
  }

  private void collectCrystal(WrappedAnchor crystal,String method,boolean visibilityVerified) {
    if(!wrappedAnchors.remove(crystal)) return;
    crystal.getAnchor().detach();
    selectedId=-1;
    updateAvailableCrystals();
    log.event("collected",json("id",crystal.id,"method",method,
        "visibilityVerified",visibilityVerified,"remaining",crystalsAvailable));
    runOnUiThread(() -> {
      android.content.SharedPreferences prefs=getSharedPreferences("chrono_progress",MODE_PRIVATE);
      prefs.edit().putInt("collected",prefs.getInt("collected",0)+1).apply();
      setResult(RESULT_OK,new Intent().putExtra("event","artifact_collected").putExtra("sessionId",log.id));
      Toast.makeText(this,"Дијамант сакупљен!",Toast.LENGTH_SHORT).show();
    });
  }

  // Handle only one tap per frame, as taps are usually low frequency compared to frame rate.
  private void handleTap(Frame frame, Camera camera) {
    MotionEvent tap = tapHelper.poll();
    if (tap == null) return;
    try {
      if (camera.getTrackingState() != TrackingState.TRACKING || completing) return;
      for(WrappedAnchor wrapped:wrappedAnchors) {
        Anchor anchor=wrapped.getAnchor();
        if(anchor.getTrackingState()==TrackingState.TRACKING && intersectsObject(tap,camera,anchor)) {
          selectedId=wrapped.id;
          log.event("selected",json("id",selectedId));
          if(visibleAtTap(frame,camera,tap)) collectCrystal(wrapped,"tap",touchDepthVerified);
          else {
            log.event("collection_rejected",json("id",selectedId,"reason","confirmed_occlusion"));
            runOnUiThread(() -> Toast.makeText(this,"Дијамант је заклоњен",Toast.LENGTH_SHORT).show());
          }
          return;
        }
      }
      if(wrappedAnchors.size()>=MAX_CRYSTALS) return;
      for (HitResult hit : frame.hitTest(tap)) {
        Trackable t = hit.getTrackable();
        if ((t instanceof Plane && ((Plane)t).isPoseInPolygon(hit.getHitPose())
             && PlaneRenderer.calculateDistanceToPlane(hit.getHitPose(), camera.getPose()) > 0)
             || t instanceof DepthPoint
             || (t instanceof Point && ((Point)t).getOrientationMode() == OrientationMode.ESTIMATED_SURFACE_NORMAL)) {
          boolean[] used=new boolean[MAX_CRYSTALS]; for(WrappedAnchor w:wrappedAnchors) used[w.color]=true;
          int color=0; while(color<MAX_CRYSTALS-1 && used[color]) color++;
          WrappedAnchor created=new WrappedAnchor(hit.createAnchor(),t,nextId++,color);
          wrappedAnchors.add(created); selectedId=created.id;
          updateAvailableCrystals();
          log.event("placed",json("id",created.id,"color",colorNames[color],"remaining",crystalsAvailable,"pose",pose(created.getAnchor().getPose())));
          break;
        }
      }
    } finally { tap.recycle(); }
  }

  /** Ray / sphere picking for the doubled 44 cm prototype crystal. */
  private boolean intersectsObject(MotionEvent tap, Camera camera, Anchor anchor) {
    camera.getViewMatrix(tapView, 0);
    camera.getProjectionMatrix(tapProjection, 0, Z_NEAR, Z_FAR);
    Matrix.multiplyMM(tapVp, 0, tapProjection, 0, tapView, 0);
    if (!Matrix.invertM(tapInverse, 0, tapVp, 0)) return false;
    rayClip[0] = 2f*tap.getX()/surfaceView.getWidth()-1f;
    rayClip[1] = 1f-2f*tap.getY()/surfaceView.getHeight(); rayClip[2]=1; rayClip[3]=1;
    Matrix.multiplyMV(rayWorld, 0, tapInverse, 0, rayClip, 0);
    if (Math.abs(rayWorld[3]) < 1e-6f) return false;
    camera.getPose().getTranslation(rayOrigin, 0);
    anchor.getPose().transformPoint(localCenter, 0, objectCenter, 0);
    float dx=rayWorld[0]/rayWorld[3]-rayOrigin[0], dy=rayWorld[1]/rayWorld[3]-rayOrigin[1], dz=rayWorld[2]/rayWorld[3]-rayOrigin[2];
    float len=(float)Math.sqrt(dx*dx+dy*dy+dz*dz);
    if (len < 1e-6f) return false;
    dx/=len; dy/=len; dz/=len;
    float cx=objectCenter[0]-rayOrigin[0], cy=objectCenter[1]-rayOrigin[1], cz=objectCenter[2]-rayOrigin[2];
    float t=cx*dx+cy*dy+cz*dz;
    if (t < Z_NEAR || t > Z_FAR) return false;
    float ex=cx-t*dx, ey=cy-t*dy, ez=cz-t*dz;
    return ex*ex+ey*ey+ez*ez <= CRYSTAL_PICK_RADIUS_METERS*CRYSTAL_PICK_RADIUS_METERS;
  }

  private boolean visibleAtTap(Frame frame, Camera camera, MotionEvent tap) {
    touchDepthVerified=false;
    if (!depthSupported || !depthWanted) return true;
    try (Image image = frame.acquireDepthImage16Bits()) {
      long skewNs=frame.getTimestamp()-image.getTimestamp();
      if (Math.abs(skewNs)>DEPTH_FRESHNESS_NS) {
        log.event("touch_depth_unverified",json("reason","stale","ageMs",skewNs/1_000_000.0));
        return true;
      }
      depthInput[0]=tap.getX(); depthInput[1]=tap.getY();
      frame.transformCoordinates2d(Coordinates2d.VIEW, depthInput, Coordinates2d.TEXTURE_NORMALIZED, depthUv);
      int x=Math.max(0,Math.min(image.getWidth()-1,(int)(depthUv[0]*image.getWidth())));
      int y=Math.max(0,Math.min(image.getHeight()-1,(int)(depthUv[1]*image.getHeight())));
      Image.Plane plane=image.getPlanes()[0];
      int mm=plane.getBuffer().order(ByteOrder.LITTLE_ENDIAN).getShort(y*plane.getRowStride()+x*plane.getPixelStride()) & 0xffff;
      if (mm==0) {
        log.event("touch_depth_unverified",json("reason","missing_pixel"));
        return true;
      }
      touchDepthVerified=true;
      camera.getPose().inverse().transformPoint(objectCenter,0,cameraPoint,0);
      return mm/1000f >= -cameraPoint[2]-CRYSTAL_PICK_RADIUS_METERS;
    } catch (NotYetAvailableException e) {
      log.event("touch_depth_unverified",json("reason","not_yet_available"));
      return true;
    } catch (RuntimeException e) {
      log.event("touch_depth_unverified",json("reason","depth_error","error",e.toString()));
      return true;
    }
  }

  private static JSONObject pose(com.google.ar.core.Pose p) {
    return json("translation",DiagnosticSession.array(p.getTranslation()),"quaternion",DiagnosticSession.array(p.getRotationQuaternion()));
  }
  private int depthAt(Frame frame,Image image,float x,float y) {
    if(x<0 || y<0 || x>=surfaceView.getWidth() || y>=surfaceView.getHeight()) return -1;
    float[] uv=new float[2];
    frame.transformCoordinates2d(Coordinates2d.VIEW,new float[]{x,y},Coordinates2d.TEXTURE_NORMALIZED,uv);
    if(uv[0]<0 || uv[0]>=1 || uv[1]<0 || uv[1]>=1) return -1;
    Image.Plane p=image.getPlanes()[0];
    return p.getBuffer().order(ByteOrder.LITTLE_ENDIAN).getShort((int)(uv[1]*image.getHeight())*p.getRowStride()+(int)(uv[0]*image.getWidth())*p.getPixelStride())&0xffff;
  }
  private void captureDepth(Frame frame,Camera camera,Image image,String reason) {
    if(!log.acceptsSnapshot()) { log.event("snapshot_rejected",json("reason","queue_or_session_limit")); return; }
    int w=image.getWidth(),h=image.getHeight(); Image.Plane p=image.getPlanes()[0];
    byte[] bytes=new byte[w*h*2]; ByteBuffer buffer=p.getBuffer();
    for(int y=0;y<h;y++) for(int x=0;x<w;x++) {
      int from=y*p.getRowStride()+x*p.getPixelStride(), to=(y*w+x)*2;
      bytes[to]=buffer.get(from); bytes[to+1]=buffer.get(from+1);
    }
    float[] ndc={-1,-1,1,-1,-1,1,1,1}, uv=new float[8];
    frame.transformCoordinates2d(Coordinates2d.OPENGL_NORMALIZED_DEVICE_COORDINATES,ndc,Coordinates2d.TEXTURE_NORMALIZED,uv);
    float[] v=new float[16],proj=new float[16]; camera.getViewMatrix(v,0); camera.getProjectionMatrix(proj,0,Z_NEAR,Z_FAR);
    log.snapshot(bytes,json("reason",reason,"width",w,"height",h,"format","uint16_le","unit","mm","invalidValue",0,
        "sourceFormat",image.getFormat(),"sourceRowStride",p.getRowStride(),"sourcePixelStride",p.getPixelStride(),"packedRowStride",w*2,
        "frameTimestampNs",frame.getTimestamp(),"depthTimestampNs",image.getTimestamp(),"depthAgeMs",depthAge,
        "captureElapsedMs",log.elapsedMs(),"fresh",depthState.equals("active"),"cameraPose",pose(camera.getPose()),
        "viewMatrixColumnMajor",DiagnosticSession.array(v),"projectionMatrixColumnMajor",DiagnosticSession.array(proj),
        "ndcQuad",DiagnosticSession.array(ndc),"textureUvQuad",DiagnosticSession.array(uv),
        "displayRotation",getWindowManager().getDefaultDisplay().getRotation(),"viewportWidth",surfaceView.getWidth(),"viewportHeight",surfaceView.getHeight(),
        "imageIntrinsics",json("focalLength",DiagnosticSession.array(camera.getImageIntrinsics().getFocalLength()),"principalPoint",DiagnosticSession.array(camera.getImageIntrinsics().getPrincipalPoint())),
        "confidence",null,"confidenceReason","filtered depth; raw confidence not acquired"));
  }
  private void updateDiagnostics(Frame frame,Camera camera,boolean occlusion) {
    long now=SystemClock.elapsedRealtimeNanos();
    if(lastFrameNs!=0) { double ms=(now-lastFrameNs)/1_000_000.0; frameSum+=ms; frameMax=Math.max(frameMax,ms); frameCount++; if(ms>50) slowFrames++; }
    lastFrameNs=now;
    String tracking=camera.getTrackingState()+":"+camera.getTrackingFailureReason();
    if(!tracking.equals(lastTracking)) { log.event("tracking",json("state",camera.getTrackingState().toString(),"reason",camera.getTrackingFailureReason().toString())); lastTracking=tracking; }
    if(sampleNs==0) { sampleNs=now; return; }
    if(now-sampleNs<1_000_000_000L) return;
    lastFps=(float)(frameCount*1e9/(now-sampleNs));
    JSONArray anchors=new JSONArray(),pairs=new JSONArray();
    if(log.detailed) {
      for(WrappedAnchor a:wrappedAnchors) anchors.put(json("id",a.id,"color",colorNames[a.color],"tracking",a.getAnchor().getTrackingState().toString(),"pose",pose(a.getAnchor().getPose())));
      for(int i=0;i<wrappedAnchors.size();i++) for(int j=i+1;j<wrappedAnchors.size();j++) {
        WrappedAnchor a=wrappedAnchors.get(i),b=wrappedAnchors.get(j);
        float[] at=a.getAnchor().getPose().getTranslation(),bt=b.getAnchor().getPose().getTranslation(); double d=0;
        for(int k=0;k<3;k++) d+=(at[k]-bt[k])*(at[k]-bt[k]);
        pairs.put(json("a",a.id,"b",b.id,"meters",Math.sqrt(d),"bothTracking",a.getAnchor().getTrackingState()==TrackingState.TRACKING&&b.getAnchor().getTrackingState()==TrackingState.TRACKING));
      }
    }
    double mean=frameCount==0?0:frameSum/frameCount;
    JSONObject data=json("fps",lastFps,"frameMeanMs",mean,"frameMaxMs",frameMax,"slowOver50Ms",slowFrames,
      "frameTimestampNs",frame.getTimestamp(),"depthAgeMs",Double.isFinite(depthAge)?depthAge:null,"depthState",depthState,
      "occlusion",occlusion,"occlusionReason",!depthWanted?"user_disabled":depthState,"centerMm",centerMm<=0?null:centerMm,
      "selectedId",selectedId,"objectZMm",selectedZ<=0?null:selectedZ,"realAtObjectMm",selectedRealMm<=0?null:selectedRealMm,
      "cameraPose",log.detailed?pose(camera.getPose()):null,"anchors",anchors,"pairs",pairs,
      "tracking",tracking,"missingFrames",missingFrames,"staleFrames",staleFrames,"detailed",log.detailed,"producerMs",producerMs);
    String row=String.format(java.util.Locale.ROOT,"%.2f,%.3f,%.3f,%d,%s,%s,%s,%s,%s,%d,%d,%d,%s,%.3f",lastFps,mean,frameMax,slowFrames,
      Double.isFinite(depthAge)?Double.toString(depthAge):"",depthState,occlusion,centerMm<=0?"":Double.toString(centerMm),selectedZ<=0?"":Double.toString(selectedZ),wrappedAnchors.size(),missingFrames,staleFrames,log.detailed,producerMs);
    log.sample(data,row);
    String selected="нема"; for(WrappedAnchor w:wrappedAnchors) if(w.id==selectedId) selected="#"+w.id+" "+colorNames[w.color];
    String text=String.format(java.util.Locale.ROOT,
      "%s · %.1f s · %.0f FPS\n%s\nДубина: %s · старост %s ms · заклањање %s\nЦентар +: %s mm | избор %s\nZ предмета: %s mm | стварно ту: %s mm\n0 m плаво → 2.5 m зелено → ≥5 m црвено\nШаховница: нема важеће дубине · %d/5 предмета\n%s · губитак реда %d",
      log.id.substring(0,8),log.elapsedMs()/1000.0,lastFps,tracking,depthState,Double.isFinite(depthAge)?String.format(java.util.Locale.ROOT,"%.2f",depthAge):"—",occlusion?"ДА":"НЕ",
      centerMm<=0?"—":String.format(java.util.Locale.ROOT,"%.0f",centerMm),selected,
      selectedZ<=0?"—":String.format(java.util.Locale.ROOT,"%.0f",selectedZ),selectedRealMm<=0?"—":String.format(java.util.Locale.ROOT,"%.0f",selectedRealMm),wrappedAnchors.size(),log.status,log.dropped.get());
    runOnUiThread(() -> { if(!isFinishing()) diagnostics.setText(text); });
    sampleNs=now; frameCount=0; frameSum=0; frameMax=0; slowFrames=0;
  }

  /**
   * Shows a pop-up dialog on the first call, determining whether the user wants to enable
   * depth-based occlusion. The result of this dialog can be retrieved with useDepthForOcclusion().
   */
  private void showOcclusionDialogIfNeeded() {
    boolean isDepthSupported = session.isDepthModeSupported(Config.DepthMode.AUTOMATIC);
    if (!depthSettings.shouldShowDepthEnableDialog() || !isDepthSupported) {
      return; // Don't need to show dialog.
    }

    // Asks the user whether they want to use depth-based occlusion.
    new AlertDialog.Builder(this)
        .setTitle(R.string.options_title_with_depth)
        .setMessage(R.string.depth_use_explanation)
        .setPositiveButton(
            R.string.button_text_enable_depth,
            (DialogInterface dialog, int which) -> {
              depthSettings.setUseDepthForOcclusion(true);
            })
        .setNegativeButton(
            R.string.button_text_disable_depth,
            (DialogInterface dialog, int which) -> {
              depthSettings.setUseDepthForOcclusion(false);
            })
        .show();
  }

  private void launchInstantPlacementSettingsMenuDialog() {
    resetSettingsMenuDialogCheckboxes();
    Resources resources = getResources();
    new AlertDialog.Builder(this)
        .setTitle(R.string.options_title_instant_placement)
        .setMultiChoiceItems(
            resources.getStringArray(R.array.instant_placement_options_array),
            instantPlacementSettingsMenuDialogCheckboxes,
            (DialogInterface dialog, int which, boolean isChecked) ->
                instantPlacementSettingsMenuDialogCheckboxes[which] = isChecked)
        .setPositiveButton(
            R.string.done,
            (DialogInterface dialogInterface, int which) -> applySettingsMenuDialogCheckboxes())
        .setNegativeButton(
            android.R.string.cancel,
            (DialogInterface dialog, int which) -> resetSettingsMenuDialogCheckboxes())
        .show();
  }

  /** Shows checkboxes to the user to facilitate toggling of depth-based effects. */
  private void launchDepthSettingsMenuDialog() {
    // Retrieves the current settings to show in the checkboxes.
    resetSettingsMenuDialogCheckboxes();

    // Shows the dialog to the user.
    Resources resources = getResources();
    if (session.isDepthModeSupported(Config.DepthMode.AUTOMATIC)) {
      // With depth support, the user can select visualization options.
      new AlertDialog.Builder(this)
          .setTitle(R.string.options_title_with_depth)
          .setMultiChoiceItems(
              resources.getStringArray(R.array.depth_options_array),
              depthSettingsMenuDialogCheckboxes,
              (DialogInterface dialog, int which, boolean isChecked) ->
                  depthSettingsMenuDialogCheckboxes[which] = isChecked)
          .setPositiveButton(
              R.string.done,
              (DialogInterface dialogInterface, int which) -> applySettingsMenuDialogCheckboxes())
          .setNegativeButton(
              android.R.string.cancel,
              (DialogInterface dialog, int which) -> resetSettingsMenuDialogCheckboxes())
          .show();
    } else {
      // Without depth support, no settings are available.
      new AlertDialog.Builder(this)
          .setTitle(R.string.options_title_without_depth)
          .setPositiveButton(
              R.string.done,
              (DialogInterface dialogInterface, int which) -> applySettingsMenuDialogCheckboxes())
          .show();
    }
  }

  private void applySettingsMenuDialogCheckboxes() {
    depthSettings.setUseDepthForOcclusion(depthSettingsMenuDialogCheckboxes[0]);
    depthSettings.setDepthColorVisualizationEnabled(depthSettingsMenuDialogCheckboxes[1]);
    instantPlacementSettings.setInstantPlacementEnabled(
        instantPlacementSettingsMenuDialogCheckboxes[0]);
    configureSession();
  }

  private void resetSettingsMenuDialogCheckboxes() {
    depthSettingsMenuDialogCheckboxes[0] = depthSettings.useDepthForOcclusion();
    depthSettingsMenuDialogCheckboxes[1] = depthSettings.depthColorVisualizationEnabled();
    instantPlacementSettingsMenuDialogCheckboxes[0] =
        instantPlacementSettings.isInstantPlacementEnabled();
  }

  /** Checks if we detected at least one plane. */
  private boolean hasTrackingPlane() {
    for (Plane plane : session.getAllTrackables(Plane.class)) {
      if (plane.getTrackingState() == TrackingState.TRACKING) {
        return true;
      }
    }
    return false;
  }

  /** Update state based on the current frame's light estimation. */
  private void updateLightEstimation(LightEstimate lightEstimate, float[] viewMatrix) {
    if (lightEstimate.getState() != LightEstimate.State.VALID) {
      virtualObjectShader.setBool("u_LightEstimateIsValid", false);
      return;
    }
    virtualObjectShader.setBool("u_LightEstimateIsValid", true);

    Matrix.invertM(viewInverseMatrix, 0, viewMatrix, 0);
    virtualObjectShader.setMat4("u_ViewInverse", viewInverseMatrix);

    updateMainLight(
        lightEstimate.getEnvironmentalHdrMainLightDirection(),
        lightEstimate.getEnvironmentalHdrMainLightIntensity(),
        viewMatrix);
    updateSphericalHarmonicsCoefficients(
        lightEstimate.getEnvironmentalHdrAmbientSphericalHarmonics());
    cubemapFilter.update(lightEstimate.acquireEnvironmentalHdrCubeMap());
  }

  private void updateMainLight(float[] direction, float[] intensity, float[] viewMatrix) {
    // We need the direction in a vec4 with 0.0 as the final component to transform it to view space
    worldLightDirection[0] = direction[0];
    worldLightDirection[1] = direction[1];
    worldLightDirection[2] = direction[2];
    Matrix.multiplyMV(viewLightDirection, 0, viewMatrix, 0, worldLightDirection, 0);
    virtualObjectShader.setVec4("u_ViewLightDirection", viewLightDirection);
    virtualObjectShader.setVec3("u_LightIntensity", intensity);
  }

  private void updateSphericalHarmonicsCoefficients(float[] coefficients) {
    // Pre-multiply the spherical harmonics coefficients before passing them to the shader. The
    // constants in sphericalHarmonicFactors were derived from three terms:
    //
    // 1. The normalized spherical harmonics basis functions (y_lm)
    //
    // 2. The lambertian diffuse BRDF factor (1/pi)
    //
    // 3. A <cos> convolution. This is done to so that the resulting function outputs the irradiance
    // of all incoming light over a hemisphere for a given surface normal, which is what the shader
    // (environmental_hdr.frag) expects.
    //
    // You can read more details about the math here:
    // https://google.github.io/filament/Filament.html#annex/sphericalharmonics

    if (coefficients.length != 9 * 3) {
      throw new IllegalArgumentException(
          "The given coefficients array must be of length 27 (3 components per 9 coefficients");
    }

    // Apply each factor to every component of each coefficient
    for (int i = 0; i < 9 * 3; ++i) {
      sphericalHarmonicsCoefficients[i] = coefficients[i] * sphericalHarmonicFactors[i / 3];
    }
    virtualObjectShader.setVec3Array(
        "u_SphericalHarmonicsCoefficients", sphericalHarmonicsCoefficients);
  }

  /** Configures the session with feature settings. */
  private void configureSession() {
    Config config = session.getConfig();
    config.setLightEstimationMode(Config.LightEstimationMode.ENVIRONMENTAL_HDR);
    if (session.isDepthModeSupported(Config.DepthMode.AUTOMATIC)) {
      config.setDepthMode(Config.DepthMode.AUTOMATIC);
    } else {
      config.setDepthMode(Config.DepthMode.DISABLED);
    }
    if (instantPlacementSettings.isInstantPlacementEnabled()) {
      config.setInstantPlacementMode(InstantPlacementMode.LOCAL_Y_UP);
    } else {
      config.setInstantPlacementMode(InstantPlacementMode.DISABLED);
    }
    session.configure(config);
  }
}

/**
 * Associates an Anchor with the trackable it was attached to. This is used to be able to check
 * whether or not an Anchor originally was attached to an {@link InstantPlacementPoint}.
 */
class WrappedAnchor {
  private Anchor anchor;
  private Trackable trackable;

  final int id, color;
  public WrappedAnchor(Anchor anchor, Trackable trackable,int id,int color) {
    this.id=id; this.color=color;
    this.anchor = anchor;
    this.trackable = trackable;
  }

  public Anchor getAnchor() {
    return anchor;
  }

  public Trackable getTrackable() {
    return trackable;
  }
}

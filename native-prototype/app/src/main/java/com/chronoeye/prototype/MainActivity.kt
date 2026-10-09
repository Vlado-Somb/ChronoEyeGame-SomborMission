package com.chronoeye.prototype

import android.content.Intent
import android.os.Bundle
import android.webkit.JavascriptInterface
import android.webkit.WebResourceRequest
import android.webkit.WebResourceResponse
import android.webkit.WebView
import android.webkit.WebViewClient
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.webkit.WebViewAssetLoader
import com.google.ar.core.examples.java.helloar.HelloArActivity
import org.json.JSONObject
import java.io.ByteArrayInputStream

/** Packaged web UI. No arbitrary navigation, remote content, or file access. */
class MainActivity : AppCompatActivity() {
 private lateinit var web: WebView
 private var arOpen = false
 private val arLauncher = registerForActivityResult(ActivityResultContracts.StartActivityForResult()) {
  arOpen = false
  refresh()
 }
 override fun onCreate(savedInstanceState: Bundle?) {
  super.onCreate(savedInstanceState)
  arOpen = savedInstanceState?.getBoolean("arOpen") ?: false
  web = WebView(this)
  setContentView(web)
  val loader = WebViewAssetLoader.Builder().addPathHandler("/assets/", WebViewAssetLoader.AssetsPathHandler(this)).build()
  web.settings.apply { javaScriptEnabled = true; allowFileAccess = false; allowContentAccess = false; setSupportMultipleWindows(false) }
  web.webViewClient = object : WebViewClient() {
   override fun shouldOverrideUrlLoading(view: WebView, request: WebResourceRequest) = true
   override fun shouldInterceptRequest(view: WebView, request: WebResourceRequest): WebResourceResponse =
    loader.shouldInterceptRequest(request.url) ?: WebResourceResponse("text/plain", "UTF-8", 403, "Forbidden", emptyMap(), ByteArrayInputStream(byteArrayOf()))
   override fun onPageFinished(view: WebView, url: String) { refresh() }
  }
  web.addJavascriptInterface(Bridge(), "ChronoNative")
  web.loadUrl("https://appassets.androidplatform.net/assets/web/index.html")
 }
 override fun onSaveInstanceState(outState: Bundle) { outState.putBoolean("arOpen", arOpen); super.onSaveInstanceState(outState) }
 private fun refresh() { if (::web.isInitialized) web.evaluateJavascript("window.refreshState && window.refreshState()", null) }
 override fun onResume() { super.onResume(); if (::web.isInitialized) { web.onResume(); refresh() } }
 override fun onPause() { if (::web.isInitialized) web.onPause(); super.onPause() }
 override fun onDestroy() { web.removeJavascriptInterface("ChronoNative"); web.destroy(); super.onDestroy() }
 inner class Bridge {
  @JavascriptInterface fun openAr() { runOnUiThread { if (!arOpen && !isFinishing) { arOpen = true; getSharedPreferences("chrono_progress", MODE_PRIVATE).edit().putBoolean("testFinished", false).apply(); arLauncher.launch(Intent(this@MainActivity, HelloArActivity::class.java)) } } }
  @JavascriptInterface fun getState(): String {
   val prefs = getSharedPreferences("chrono_progress", MODE_PRIVATE)
   return JSONObject().put("collected", prefs.getInt("collected", 0)).put("lastFps", prefs.getFloat("lastFps", 0f).toDouble())
    .put("depthSupported", prefs.getBoolean("depthSupported", false)).put("tested", prefs.contains("depthSupported"))
    .put("testFinished", prefs.getBoolean("testFinished", false)).toString()
  }
  @JavascriptInterface fun resetProgress() { getSharedPreferences("chrono_progress", MODE_PRIVATE).edit().clear().apply(); runOnUiThread { refresh() } }
 }
}

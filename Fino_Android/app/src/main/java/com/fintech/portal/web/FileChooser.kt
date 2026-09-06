package com.fintech.portal.web

import android.Manifest
import android.app.Activity
import android.content.Intent
import android.content.pm.PackageManager
import android.net.Uri
import android.provider.MediaStore
import android.webkit.ValueCallback
import android.webkit.WebChromeClient
import androidx.activity.ComponentActivity
import androidx.activity.result.contract.ActivityResultContracts
import androidx.core.content.ContextCompat
import androidx.core.content.FileProvider
import com.fintech.portal.util.toast
import java.io.File
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

/**
 * `<input type="file">` ka poora support — KYC documents, fund-request slip,
 * profile photo and so on. Camera and gallery, with multi-select.
 */
class FileChooser(
    private val activity: ComponentActivity,
    private val requestPermission: (String, (Boolean) -> Unit) -> Unit
) {

    private var pending: ValueCallback<Array<Uri>>? = null
    private var cameraOutput: Uri? = null

    private val launcher = activity.registerForActivityResult(
        ActivityResultContracts.StartActivityForResult()
    ) { result ->
        val callback = pending
        pending = null

        if (result.resultCode != Activity.RESULT_OK) {
            callback?.onReceiveValue(null)
            cameraOutput = null
            return@registerForActivityResult
        }

        val data = result.data
        val uris: Array<Uri>? = when {
            data?.clipData != null -> {
                val clip = data.clipData!!
                Array(clip.itemCount) { clip.getItemAt(it).uri }
            }

            data?.data != null -> arrayOf(data.data!!)

            cameraOutput != null -> arrayOf(cameraOutput!!)

            else -> null
        }

        callback?.onReceiveValue(uris)
        cameraOutput = null
    }

    fun open(
        callback: ValueCallback<Array<Uri>>,
        params: WebChromeClient.FileChooserParams
    ): Boolean {
        // Close any still-open callback first, otherwise the WebView hangs
        pending?.onReceiveValue(null)
        pending = callback

        val acceptsImage = params.acceptTypes.orEmpty().any {
            it.isBlank() || it.contains("image") || it.contains("*/*")
        }

        if (acceptsImage && hasCamera()) {
            if (ContextCompat.checkSelfPermission(activity, Manifest.permission.CAMERA)
                == PackageManager.PERMISSION_GRANTED
            ) {
                launchChooser(params, withCamera = true)
            } else {
                requestPermission(Manifest.permission.CAMERA) { granted ->
                    launchChooser(params, withCamera = granted)
                }
            }
        } else {
            launchChooser(params, withCamera = false)
        }
        return true
    }

    private fun launchChooser(params: WebChromeClient.FileChooserParams, withCamera: Boolean) {
        val content = Intent(Intent.ACTION_GET_CONTENT).apply {
            addCategory(Intent.CATEGORY_OPENABLE)
            type = params.acceptTypes.orEmpty().firstOrNull { it.isNotBlank() } ?: "*/*"
            val extra = params.acceptTypes.orEmpty().filter { it.isNotBlank() }
            if (extra.size > 1) putExtra(Intent.EXTRA_MIME_TYPES, extra.toTypedArray())
            putExtra(
                Intent.EXTRA_ALLOW_MULTIPLE,
                params.mode == WebChromeClient.FileChooserParams.MODE_OPEN_MULTIPLE
            )
        }

        val cameraIntent = if (withCamera) buildCameraIntent() else null

        val chooser = Intent.createChooser(content, "Choose file").apply {
            if (cameraIntent != null) {
                putExtra(Intent.EXTRA_INITIAL_INTENTS, arrayOf(cameraIntent))
            }
        }

        try {
            launcher.launch(chooser)
        } catch (_: Throwable) {
            activity.toast("Could not open the file picker")
            pending?.onReceiveValue(null)
            pending = null
        }
    }

    private fun buildCameraIntent(): Intent? = try {
        val dir = File(activity.cacheDir, "captures").apply { mkdirs() }
        val stamp = SimpleDateFormat("yyyyMMdd_HHmmss", Locale.US).format(Date())
        val file = File(dir, "IMG_$stamp.jpg")
        val uri = FileProvider.getUriForFile(
            activity,
            "${activity.packageName}.fileprovider",
            file
        )
        cameraOutput = uri
        Intent(MediaStore.ACTION_IMAGE_CAPTURE).apply {
            putExtra(MediaStore.EXTRA_OUTPUT, uri)
            addFlags(Intent.FLAG_GRANT_WRITE_URI_PERMISSION or Intent.FLAG_GRANT_READ_URI_PERMISSION)
        }
    } catch (_: Throwable) {
        cameraOutput = null
        null
    }

    private fun hasCamera(): Boolean =
        activity.packageManager.hasSystemFeature(PackageManager.FEATURE_CAMERA_ANY)

    /** Release the pending callback when the activity is destroyed. */
    fun release() {
        pending?.onReceiveValue(null)
        pending = null
    }
}

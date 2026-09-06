# WebView <-> JavaScript bridge ke saare @JavascriptInterface methods bachao,
# warna release build me web se native calls fail ho jayengi.
-keepclassmembers class com.fintech.portal.bridge.NativeBridge {
    public *;
}
-keepattributes JavascriptInterface
-keepattributes *Annotation*

# Kotlin metadata
-keep class kotlin.Metadata { *; }

# Crash stack traces padhne layak rahein
-keepattributes SourceFile,LineNumberTable
-renamesourcefileattribute SourceFile

import groovy.json.JsonSlurper
import java.io.FileInputStream
import java.net.URI
import java.util.Properties

plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
}

// ---------------------------------------------------------------------------
// White-label registry: product flavors are GENERATED from whitelabel/clients.json.
// A new client is a new block in that JSON — this Gradle file never needs editing.
// ---------------------------------------------------------------------------
@Suppress("UNCHECKED_CAST")
private val whitelabel = JsonSlurper()
    .parse(rootProject.file("whitelabel/clients.json")) as Map<String, Any>

@Suppress("UNCHECKED_CAST")
private val wlDefaults = whitelabel["defaults"] as Map<String, Any>

@Suppress("UNCHECKED_CAST")
private val wlClients = whitelabel["clients"] as List<Map<String, Any>>

fun Map<String, Any>.wlString(key: String, fallback: String = ""): String =
    (this[key] ?: wlDefaults[key] ?: fallback).toString()

fun Map<String, Any>.wlBool(key: String, fallback: Boolean = false): Boolean =
    (this[key] ?: wlDefaults[key] ?: fallback) as Boolean

fun Map<String, Any>.wlInt(key: String, fallback: Int = 0): Int =
    ((this[key] ?: wlDefaults[key] ?: fallback) as Number).toInt()

// Release signing: put keystore.properties in the project root (never commit it).
private val keystoreFile = rootProject.file("keystore.properties")
private val keystoreProps = Properties().apply {
    if (keystoreFile.exists()) FileInputStream(keystoreFile).use { load(it) }
}

android {
    namespace = "com.fintech.portal"
    compileSdk = 35

    defaultConfig {
        minSdk = 24
        targetSdk = 35
        versionCode = wlDefaults.wlInt("versionCode", 1)
        versionName = wlDefaults.wlString("versionName", "1.0.0")
        vectorDrawables.useSupportLibrary = true
    }

    signingConfigs {
        if (keystoreProps.getProperty("storeFile") != null) {
            create("release") {
                storeFile = rootProject.file(keystoreProps.getProperty("storeFile"))
                storePassword = keystoreProps.getProperty("storePassword")
                keyAlias = keystoreProps.getProperty("keyAlias")
                keyPassword = keystoreProps.getProperty("keyPassword")
            }
        }
    }

    buildTypes {
        debug {
            applicationIdSuffix = ".debug"
            isMinifyEnabled = false
            buildConfigField("boolean", "DEV_TOOLS", "true")
        }
        release {
            isMinifyEnabled = true
            isShrinkResources = true
            buildConfigField("boolean", "DEV_TOOLS", "false")
            proguardFiles(getDefaultProguardFile("proguard-android-optimize.txt"), "proguard-rules.pro")
            if (keystoreProps.getProperty("storeFile") != null) {
                signingConfig = signingConfigs.getByName("release")
            }
        }
    }

    flavorDimensions += "client"
    productFlavors {
        wlClients.forEach { client ->
            create(client.wlString("id")) {
                dimension = "client"
                applicationId = client.wlString("applicationId")
                versionCode = client.wlInt("versionCode", 1)
                versionName = client.wlString("versionName", "1.0.0")

                val portal = client.wlString("portalUrl").trimEnd('/')
                val remoteConfig = client["remoteConfigUrl"]?.toString()
                    ?: (portal + client.wlString("remoteConfigPath", "/app-config.json"))
                val portalHost = runCatching { URI(portal).host }.getOrNull().orEmpty()

                // Deep links open only for this client's own domain
                manifestPlaceholders["deepLinkHost"] = portalHost

                // Branding -> resources (a per-client res/ folder can override these)
                resValue("string", "app_name", client.wlString("appName"))
                resValue("color", "brand_primary", client.wlString("colorPrimary"))
                resValue("color", "brand_accent", client.wlString("colorAccent"))
                resValue("color", "brand_splash", client.wlString("colorSplash"))

                // Branding -> BuildConfig (runtime defaults; the remote config overrides them)
                buildConfigField("String", "CLIENT_ID", "\"" + client.wlString("id") + "\"")
                buildConfigField("String", "BRAND_NAME", "\"" + client.wlString("appName") + "\"")
                buildConfigField("String", "PORTAL_URL", "\"" + portal + "\"")
                buildConfigField("String", "PORTAL_HOST", "\"" + portalHost + "\"")
                buildConfigField("String", "START_PATH", "\"" + client.wlString("startPath", "/") + "\"")
                buildConfigField("String", "REMOTE_CONFIG_URL", "\"" + remoteConfig + "\"")
                buildConfigField("String", "COLOR_PRIMARY", "\"" + client.wlString("colorPrimary") + "\"")
                buildConfigField("String", "COLOR_ACCENT", "\"" + client.wlString("colorAccent") + "\"")
                buildConfigField("String", "COLOR_SPLASH", "\"" + client.wlString("colorSplash") + "\"")
                buildConfigField("boolean", "BLOCK_SCREENSHOTS", client.wlBool("blockScreenshots", true).toString())
                buildConfigField("boolean", "PULL_TO_REFRESH", client.wlBool("pullToRefresh", true).toString())
                buildConfigField("String", "SUPPORT_WHATSAPP", "\"" + client.wlString("supportWhatsApp") + "\"")
                buildConfigField("String", "SUPPORT_EMAIL", "\"" + client.wlString("supportEmail") + "\"")
                buildConfigField("String", "SUPPORT_PHONE", "\"" + client.wlString("supportPhone") + "\"")
            }
        }
    }

    buildFeatures {
        viewBinding = true
        buildConfig = true
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    kotlinOptions {
        jvmTarget = "17"
    }

    packaging {
        resources.excludes += setOf("META-INF/*.kotlin_module", "DebugProbesKt.bin")
    }
}

dependencies {
    implementation("androidx.core:core-ktx:1.15.0")
    implementation("androidx.core:core-splashscreen:1.0.1")
    implementation("androidx.appcompat:appcompat:1.7.0")
    implementation("androidx.activity:activity-ktx:1.9.3")
    implementation("androidx.constraintlayout:constraintlayout:2.2.0")
    implementation("androidx.swiperefreshlayout:swiperefreshlayout:1.1.0")
    implementation("androidx.webkit:webkit:1.12.1")
    implementation("androidx.browser:browser:1.8.0")
    implementation("androidx.biometric:biometric:1.1.0")
    implementation("com.google.android.material:material:1.12.0")
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-android:1.9.0")
}

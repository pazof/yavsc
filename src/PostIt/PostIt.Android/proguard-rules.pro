# Keep the LibVLC Android classes and JNI/native bindings intact in Release builds.
# R8/ProGuard otherwise can strip or rename the Java-side entry points that load
# the native libvlc and libc++_shared libraries bundled by VideoLAN.LibVLC.Android.
-keep class org.videolan.libvlc.** { *; }
-keep class org.videolan.libvlc.** { native *; }
-keep class org.videolan.medialibrary.** { *; }
-keepattributes *Annotation*
-dontwarn org.videolan.libvlc.**
-dontwarn org.videolan.medialibrary.**

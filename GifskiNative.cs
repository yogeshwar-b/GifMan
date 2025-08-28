using System;
using System.Runtime.InteropServices;

public static class GifskiNative
{
    private const string DllName = "gifski.dll";

    // Opaque handle
    public struct Gifski { }

    // =========================
    // Structs
    // =========================
    [StructLayout(LayoutKind.Sequential)]
    public struct GifskiSettings
    {
        public uint width;
        public uint height;
        public byte quality;     // 1-100
        [MarshalAs(UnmanagedType.I1)]
        public bool fast;
        public short repeat;     // -1 disable looping, 0 loop forever
    }

    // =========================
    // Errors enum
    // =========================
    public enum GifskiError
    {
        GIFSKI_OK = 0,
        GIFSKI_NULL_ARG,
        GIFSKI_INVALID_STATE,
        GIFSKI_QUANT,
        GIFSKI_GIF,
        GIFSKI_THREAD_LOST,
        GIFSKI_NOT_FOUND,
        GIFSKI_PERMISSION_DENIED,
        GIFSKI_ALREADY_EXISTS,
        GIFSKI_INVALID_INPUT,
        GIFSKI_TIMED_OUT,
        GIFSKI_WRITE_ZERO,
        GIFSKI_INTERRUPTED,
        GIFSKI_UNEXPECTED_EOF,
        GIFSKI_ABORTED,
        GIFSKI_OTHER,
    }

    // =========================
    // Core functions
    // =========================
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr gifski_new(ref GifskiSettings settings);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern GifskiError gifski_set_file_output(IntPtr handle, string destinationPath);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern GifskiError gifski_add_frame_rgba(
        IntPtr handle,
        uint frameNumber,
        uint width,
        uint height,
        byte[] pixels,    // width*height*4 RGBA
        double presentationTimestamp
    );

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern GifskiError gifski_finish(IntPtr handle);

    // =========================
    // Optional config
    // =========================
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern GifskiError gifski_set_motion_quality(IntPtr handle, byte quality);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern GifskiError gifski_set_lossy_quality(IntPtr handle, byte quality);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern GifskiError gifski_set_extra_effort(IntPtr handle, bool extra);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern GifskiError gifski_add_fixed_color(IntPtr handle, byte r, byte g, byte b);
}

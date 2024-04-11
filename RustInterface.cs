using System.Runtime.InteropServices;

namespace RustConnectionTest;

public class RustInterface
{
    [DllImport("rust_test.dll", EntryPoint = "hello_world")]
    public static extern void HelloWorld();

    [DllImport("rust_test.dll")]
    public static extern Int32 square(Int32 val);

    [DllImport("rust_test.dll")]
    public static extern bool invert(bool val);
    

    [DllImport("rust_test.dll")]
    public static extern bool isOdd(Int64 val);

    [DllImport("rust_test.dll")]
    public static extern float sqrt(float val);

    [DllImport("rust_test.dll")]
    public static extern float length(EulerCoordinates vec);

    [DllImport("rust_test.dll")]
    public static extern EulerCoordinates toEuler(PolarCoordinates vec);

    [DllImport("rust_test.dll")]
    private static extern void inverse_with_second_array(byte[] dataIn, byte[] dataOut, UIntPtr len);

    [DllImport("rust_test.dll")]
    public static extern void inverse_in_place(byte[] data, UIntPtr len);

    [DllImport("rust_test.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    public static extern void printc(byte[] buf);

    [DllImport("rust_test.dll")]
    private static extern void free_string();

    [DllImport("rust_test.dll")]
    private static extern IntPtr format_string(UInt32 number);

    public static byte[] InverseWithSecondArray(byte[] data)
    {
        var dataOut = (byte[]) data.Clone();
        inverse_with_second_array(data, dataOut, (UIntPtr) data.Length);
        return dataOut;
    }

    public static string FormatString(UInt32 number)
    {
        var ptr = format_string(number);
        var str = Marshal.PtrToStringUTF8(ptr) ?? "null";
        free_string();

        return str;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct PolarCoordinates
{
    public float Length;
    public float Angle;
}

[StructLayout(LayoutKind.Sequential)]
public struct EulerCoordinates
{
    public float X;
    public float Y;
}
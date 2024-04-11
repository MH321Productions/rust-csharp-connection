using System.Text;
using RustConnectionTest;

//Setup Console
Console.OutputEncoding = Encoding.Default;

//Hello World
Console.WriteLine("Hello from C#!");
RustInterface.HelloWorld();

//Int32 -> Int32
var num = 20;
var squared = RustInterface.square(num);
Console.WriteLine($"{num}^2 is {squared}");

//Bool -> Bool
var b = true;
var inverted = RustInterface.invert(b);
Console.WriteLine($"The inverse of {b} is {inverted}");

//Int64 -> Bool
const long oddNumber = (1L << 40) + 1;
var isOdd = RustInterface.isOdd(oddNumber);
Console.WriteLine($"Is {oddNumber} odd? {isOdd}");

//Float -> Float
var f = 6.25f;
var sqrt = RustInterface.sqrt(f);
Console.WriteLine($"The square root of {f} is {sqrt}");

//EulerCoordinates -> Float
var vecEuler = new EulerCoordinates { X = 3, Y = 4 };
var len = RustInterface.length(vecEuler);
Console.WriteLine($"The length of euler vector ({vecEuler.X}, {vecEuler.Y}) is {len}");

//PolarCoordinates -> EulerCoordinates
var vecPolar = new PolarCoordinates {Length = 5, Angle = MathF.PI / 4}; // pi/4 = 45°
vecEuler = RustInterface.toEuler(vecPolar);
Console.WriteLine($"The polar vector ({vecPolar.Length}, {vecPolar.Angle}) is equivalent to the euler vector ({vecEuler.X}, {vecEuler.Y})");

//byte[] -> byte[] (Using second array)
byte[] data = [1, 2, 3, 4, 5];
var reverseData = RustInterface.InverseWithSecondArray(data);
Console.WriteLine($"The reverse of {data.Print()} is {reverseData.Print()}");

//Array reverse in-place
RustInterface.inverse_in_place(data, (UIntPtr) data.Length);
Console.WriteLine($"After in-place reverse, data is {data.Print()}");

//String: C# -> Rust
var str = "This is a test äöü 😎";
Console.WriteLine($"C# String: \"{str}\"");
RustInterface.printc(Encoding.UTF8.GetBytes(str));

//String: Rust -> C#
var number = 42u;
var formattedString = RustInterface.FormatString(number);
Console.WriteLine($"Created Rust string: \"{formattedString}\"");
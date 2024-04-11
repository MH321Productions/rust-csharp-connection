# C#/Rust Connection Prototype

I wrote a small prototype, which shows how Rust code can be called from C#.
It consists of a small Rust library and a C# console application.

## Basic structure and compilation

The root folder contains the C# project, the subfolder "rusttest" contains the
rust cargo crate. This crate has the following Definition in its Cargo.toml file,
which tells the compiler to create a DLL:

```toml
[lib]
name = "rust_test"
crate-type = ["dylib"]
```

Compile it by running

```bash
cd rusttest
cargo build
```

You can then run the C# App normally through VS/Rider. The project has a
post-build build event which copies the compiled rust DLL into the C# build
directory.

## Basic function syntax

Every exposed Rust function has to have the following signature:

```rust
#[no_mangle]
pub extern fn hello_world() {
    println!("Hello from Rust 😎");
}
```

- The ```#[no_mangle]``` ensures that the rust compiler doesn't change the
  function name, so that it can be called externally
- The function has to be labeled ```pub``` and ```extern``` to be fully exposed
  in the DLL

The corresponding C# function has to have the following signature:

```c#
[DllImport("rust_test.dll")]
public static extern void hello_world();
```

> **Note**: The C# function name can differ (and conform to the C# conventions) by
using the ```EndPoint``` property in the ```DllImport``` Attribute. The above
snippet is equivalent to this one:
>
> ```c#
> [DllImport("rust_test.dll", EntryPoint = "hello_world")]
> public static extern void HelloWorld();
> ```

## Type conversion

### Scalar types

The basic scalar types can be (with the exception of chars) easily converted to
C# types. A conversion table can be found [here](https://microsoft.github.io/rust-for-dotnet-devs/latest/language/scalar-types.html)

> **Note**: For integer types it is recommended to use the .NET classes
> (e.g. Int32) instead of the primitive types (e.g. int) to specify the bit width
> explicitly.

```rust
pub extern fn square(val: i32) -> i32 {}
pub extern fn invert(val: bool) -> bool {}
pub extern fn isOdd(val: i64) -> bool {}
pub extern fn sqrt(val: f32) -> f32 {}
```

```c#
public static extern Int32 square(Int32 val);
public static extern bool invert(bool val);
public static extern bool isOdd(Int64 val);
public static extern float sqrt(float val);
```

### Structs

Structs can be used to easily transfer multiple values at once. In Rust, they
have to have the following syntax:

```rust
#[repr(C)]
pub struct PolarCoordinates {
    length: f32,
    angle: f32
}
```

- The #[repr(C)] saves the struct in a c-like structure (i.e the entries are
saved sequentially in the given order)

The corresponding C# struct looks like this:

```c#
[StructLayout(LayoutKind.Sequential)]
public struct PolarCoordinates
{
    public float Length;
    public float Angle;
}
```

- The ```[StructLayout(LayoutKind.Sequential)]``` ensures, like in Rust, that the
structure is identical

> **Note**: The variable names can differ, but the types and order have to match.

### Arrays

Methods with array parameters can be called in the c-style: a (mutable) pointer
to the array and the length. The best tactic is, for our purpose, to let C#
allocate the array and give it to Rust. Therefore, there are two approaches
(given an input array):

1. Allocate a second array (with compile-time known size). Pass a constant
pointer to the input array and a mutable pointer to the output array, along with
their sizes

    ```rust
    pub extern fn inverse_with_second_array(data_in: *const u8, data_out: *mut u8, len: usize)
    ```

    ```c#
    private static extern void inverse_with_second_array(byte[] dataIn, byte[] dataOut, UIntPtr len);
    ```

2. Let Rust modify the input array in-place, i.e. pass a mutable pointer to the
input array, along with its size

    ```rust
    pub extern fn inverse_in_place(data: *mut u8, len: usize)
    ```

    ```c#
    public static extern void inverse_in_place(byte[] data, UIntPtr len);
    ```

You can then convert the raw pointer into a (mutable) rust array slice and work
with it normally:

```rust
let input: &[u8] = unsafe { std::slice::from_raw_parts(data_in, len)};
let output: &mut[u8] = unsafe { std::slice::from_raw_parts_mut(data_out, len) };
```

### Strings

Strings are just char arrays:

![Meme here](https://i.redd.it/aokujz0p78u91.png)

But they are a bit complicated over language barriers, especially if the one uses
UTF-16 with 2-Byte chars and the other uses UTF-32 with 4-byte chars. But both
support UTF-8 and 1-byte chars (aka c-chars). Therefore the strategy for sending
Strings to Rust is:

1. Encode the C# String as a null-terminated UTF-8 byte array and send it to Rust

    ```c#
    RustInterface.printc(Encoding.UTF8.GetBytes(str));
    ```

2. Let Rust decode the C-String and convert it to a normal Rust String

    ```rust
    let c_str = unsafe { CStr::from_ptr(buf) };
    let str = c_str.to_str().unwrap();
    ```

  The reverse way is not so simple: You could let C# provide a predefined buffer
  and copy the formatted bytes into it, but there is a more elegant way: Rust has
  a way to transfer the ownership to the C caller (aka trust that there is no
  memory leak). This opens up the following way:

1. Decode the Rust String into a C-String
2. Pass the ownership to C and save the pointer in a static variable

    ```rust
    let ptr = CString::new(str).unwrap().into_raw();
    unsafe { STRING_POINTER = ptr; }
    ```

3. In C#: Copy the data into a managed String and notify Rust to free the String

    ```c#
    var ptr = format_string(number);
    var str = Marshal.PtrToStringUTF8(ptr) ?? "null";
    free_string();
    ```

4. In Rust: Take back ownership of the string, clear the static variable and
deallocate the data

    ```rust
    unsafe {
        let _ = CString::from_raw(STRING_POINTER);
        STRING_POINTER = 0 as *mut c_char;
    }
    ```

## Example Output

The C# console app tests every function type I’ve listed here (and some more).
The output should look something like this:

```text
Hello from C#!
Hello from Rust 😎
20^2 is 400
The inverse of True is False
Is 1099511627777 odd? True
The square root of 6,25 is 2,5
The length of euler vector (3, 4) is 5
The polar vector (5, 0,7853982) is equivalent to the euler vector (3,535534, 3,535534)
The reverse of [1, 2, 3, 4, 5] is [5, 4, 3, 2, 1]
After in-place reverse, data is [5, 4, 3, 2, 1]
C# String: "This is a test äöü 😎"
Rust decoded string: "This is a test äöü 😎"
Created Rust string: "The number is 42 äöü 😎"
```

## Useful links and tutorials

I mainly used [this](https://dev.to/living_syn/calling-rust-from-c-6hk) guide
from the DEV Community and the [Rust for C#/.NET developers](https://microsoft.github.io/rust-for-dotnet-devs/latest/)
reference by .NET.

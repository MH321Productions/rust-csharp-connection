use std::ffi::{c_char, CStr, CString};

static mut STRING_POINTER: *mut c_char = 0 as *mut c_char;

#[repr(C)]
pub struct PolarCoordinates {
    length: f32,
    angle: f32
}

#[repr(C)]
pub struct EulerCoordinates {
    x: f32,
    y: f32
}

#[no_mangle]
pub extern fn hello_world() {
    println!("Hello from Rust 😎");
}

#[no_mangle]
pub extern fn square(val: i32) -> i32 {
    val * val
}

#[no_mangle]
pub extern fn invert(val: bool) -> bool {
    !val
}

#[no_mangle]
pub extern fn isOdd(val: i64) -> bool {
    val % 2 != 0
}

#[no_mangle]
pub extern fn sqrt(val: f32) -> f32 {
    val.sqrt()
}

#[no_mangle]
pub extern fn length(vec: EulerCoordinates) -> f32 {
    (vec.x * vec.x + vec.y * vec.y).sqrt()
}

#[no_mangle]
pub extern fn toEuler(vec: PolarCoordinates) -> EulerCoordinates {
    EulerCoordinates {
        x: vec.length * vec.angle.sin(),
        y: vec.length * vec.angle.cos()
    }
}

#[no_mangle]
pub extern fn inverse_with_second_array(data_in: *const u8, data_out: *mut u8, len: usize) {
    let input = unsafe { std::slice::from_raw_parts(data_in, len)};
    let output = unsafe { std::slice::from_raw_parts_mut(data_out, len) };

    for i in 0..len {
        output[len - i - 1] = input[i]
    }
}

#[no_mangle]
pub extern fn inverse_in_place(data: *mut u8, len: usize) {
    let arr = unsafe { std::slice::from_raw_parts_mut(data, len) };
    arr.reverse();
}

#[no_mangle]
pub extern fn printc(buf: *const c_char) {
    let c_str = unsafe { CStr::from_ptr(buf) };
    let str = c_str.to_str().unwrap();
    println!("Rust decoded string: \"{str}\"")
}

#[no_mangle]
pub extern fn format_string(number: u32) -> *mut c_char {
    let str = format!("The number is {} äöü 😎", number);

    store_string_on_heap(str.as_str())
}

fn store_string_on_heap(str: &str) -> *mut c_char {
    let ptr = CString::new(str).unwrap().into_raw();
    unsafe { STRING_POINTER = ptr; }

    ptr
}

#[no_mangle]
pub extern fn free_string() {
    unsafe {
        let _ = CString::from_raw(STRING_POINTER);
        STRING_POINTER = 0 as *mut c_char;
    }
}
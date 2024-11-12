module Dithering

open SkiaSharp

let private clamp = fun v -> System.Math.Clamp(v, 0, 255)

let private difference (a: SKColor) (b: SKColor) =
    float a.Red - float b.Red,
    float a.Green - float b.Green,
    float a.Blue - float b.Blue

let private findClosestPaletteColor (pixel: SKColor) =
    // convert to greyscale and calculate luminance 
    let luminance =
        0.2126 * float pixel.Red +
        0.7152 * float pixel.Green +
        0.0722 * float pixel.Blue |> int

    if luminance > 128 then
        new SKColor(255uy, 255uy, 255uy)
    else
        new SKColor(0uy, 0uy, 0uy)

// let private findClosestPaletteColor (pixel: SKColor) =
//     let round = fun v -> if v > 128.0 then 255 else 0

//     let r, g, b =
//         round (float pixel.Red) |> byte,
//         round (float pixel.Green) |> byte,
//         round (float pixel.Blue) |> byte

//     new SKColor(r, g, b)

let private addError (bitmap: SKBitmap) (x: int) (y: int) (error: float * float * float) (factor: float) =
    if x >= 0 && x < bitmap.Width && y >= 0 && y < bitmap.Height then
        let pixel = bitmap.GetPixel(x, y)
        let errorRed, errorGreen, errorBlue = error
        let r,g,b =
            clamp (float pixel.Red + errorRed * factor |> int) |> byte,
            clamp (float pixel.Green + errorGreen * factor |> int) |> byte,
            clamp (float pixel.Blue + errorBlue * factor |> int) |> byte

        bitmap.SetPixel(x, y, new SKColor(r, g, b))

let atkinson (bitmap: SKBitmap) =
    for y in 0 .. bitmap.Height - 1 do
        for x in 0 .. bitmap.Width - 1 do
            let oldPixel = bitmap.GetPixel(x, y)
            let newPixel = findClosestPaletteColor oldPixel
            bitmap.SetPixel(x, y, newPixel)
            let quantError = difference oldPixel newPixel
            let factor = 1.0 / 8.0

            [ x + 1,  y,      factor
              x + 2,  y,      factor
              x - 1,  y + 1,  factor
              x,      y + 1,  factor
              x + 1,  y + 1,  factor
              x,      y + 2,  factor ]
            |> List.iter (fun (x, y, factor) -> addError bitmap x y quantError factor)

let floydSteinberg (bitmap: SKBitmap) =
    // requires going left to right, from top to bottom
    for y in 0 .. bitmap.Height - 1 do
        for x in 0 .. bitmap.Width - 1 do
            let oldPixel = bitmap.GetPixel(x, y)
            let newPixel = findClosestPaletteColor oldPixel
            bitmap.SetPixel(x, y, newPixel)
            let quantError = difference oldPixel newPixel

            // distribute the "error" to neighbouring unvisited pixels to deal with later
            [ x + 1,  y,      7.0 / 16.0
              x - 1,  y + 1,  3.0 / 16.0
              x,      y + 1,  5.0 / 16.0
              x + 1,  y + 1,  1.0 / 16.0 ]
            |> List.iter (fun (x, y, factor) -> addError bitmap x y quantError factor)

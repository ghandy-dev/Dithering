module Arguments

open Argu

type Dithering = 
    | FloydSteinberg = 1 
    | Atkinson = 2
    | Jarvis = 3
    | Sierra = 4
    | Bayer = 4

let parseAlgorithm (algorithm: Dithering) = 
    match int algorithm with
    | 1 -> Dithering.floydSteinberg
    | 2 -> Dithering.atkinson
    | 3 -> Dithering.jarvis
    | 4 -> Dithering.sierra
    | 5 -> Dithering.bayer
    | _ -> failwith "invalid dithering algorithm"

type CliArguments =
    | [<Mandatory; AltCommandLine("-i")>] Input_File of PATH: string
    | [<Unique; AltCommandLine("-da")>] Dithering_Algorithm of Dithering // optional
    | [<Unique; AltCommandLine("-o")>] Output_Path of PATH: string  // optional

    interface IArgParserTemplate with
        member arg.Usage =
            match arg with
            | Input_File _ -> "specify an input file to dither."
            | Dithering_Algorithm _ -> "specify a dithering algorith to use (default: floydsteinberg)."
            | Output_Path _ -> "specify a custom output path for the dithered image (default: My Pictures)."

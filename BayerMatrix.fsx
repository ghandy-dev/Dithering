let generateBayerMatrix size =
    let baseMatrix = [|
        [| 0 ; 2 |]
        [| 3 ; 1 |]
    |]

    let rec generate (matrix: int array array) (n: int) =
        match n with 
        | 1 -> matrix
        | _ -> 
            let l = matrix.Length
            let size = matrix.Length * 2

            let newMatrix = 
                Array.init size (fun y ->
                    Array.init size (fun x ->
                        if y < l then
                            if x < l then 
                                matrix.[y % l][x % l] * 4 + 0
                            else 
                                matrix.[y % l][x % l] * 4 + 2
                        else
                            if x < l then 
                                matrix.[y % l][x % l] * 4 + 3
                            else 
                                matrix.[y % l][x % l] * 4 + 1
                        
                    ) 
                ) 
                
            generate newMatrix (n-1)

    generate baseMatrix size

let matrix = generateBayerMatrix 3
matrix 
|> Array.iteri (fun i row -> 
    if i = 0 then printf "[" 
    printf "\n    ["

    row 
    |> Array.iteri (fun i -> 
        if i = 0 then printf "" else printf " ;"
        printf "%3d"
    ) 
    printf " ]"
) 
    
printfn "\n]\n"
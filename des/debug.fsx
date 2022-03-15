open System.Collections

module debug =

    let bools2hex bin =
        bin
        |> Array.map (fun i -> if i then 1 else 0)
        |> Array.reduce (fun a b -> (a * 2) + b)
        |> sprintf "%x"


    let hex2bools str =
        let num =
            System.Int32.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier)

        [| 3..-1..0 |]
        |> Array.map (fun i -> (num >>> i) % 2 = 1)


    let toStr (bits: BitArray) =
        let inter = (Array.replicate bits.Length false)
        bits.CopyTo(inter, 0)

        inter
        |> Array.chunkBySize 4
        |> Array.map bools2hex
        |> Array.reduce (+)

    let toBA (str: string) =
        let bools =
            Seq.toArray str
            |> Array.map string
            |> Array.collect hex2bools

        BitArray bools

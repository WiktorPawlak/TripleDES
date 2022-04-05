module utils
// why does a functional language not have this in standard library???
let memoize f =
    let cache = System.Collections.Generic.Dictionary<_, _>()

    fun c ->
        let exist, value = cache.TryGetValue(c)

        match exist with
        | true -> value
        | _ ->
            let value = f c
            cache.Add(c, value)
            value

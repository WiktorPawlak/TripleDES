module tdea

#if INTERACTIVE
#load "des.fs"
#load "conv.fs"
#endif

let encryptBlock (k1, k2, k3) block =
    block
    |> des.encrypt k1
    |> des.decrypt k2
    |> des.encrypt k3


let decryptBlock (k1, k2, k3) block =
    block
    |> des.decrypt k3
    |> des.encrypt k2
    |> des.decrypt k1

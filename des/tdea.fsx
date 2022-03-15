module tdea

#if INTERACTIVE
#load "des.fs"
#load "conv.fs"
#endif

let encryptBlock (k1, k2, k3) block =
    block
    |> des.encryptBlock k1
    |> des.decryptBlock k2
    |> des.encryptBlock k3


let decryptBlock (k1, k2, k3) block =
    block
    |> des.decryptBlock k3
    |> des.encryptBlock k2
    |> des.decryptBlock k1

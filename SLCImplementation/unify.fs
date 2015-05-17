﻿//unification and resolution theorem prover (pp.108-109)
module unify

type term =
    | TVar of int
    | TFun of string * (term list)
and subst = (int->term)

let emptysubst = TVar

let rec unify (x1,x2) =
    if x1=x2 || x1=TVar 0 || x2=TVar 0 then emptysubst
    else match (x1,x2) with
            | ((TVar x1), t2) -> bind x1 t2
            | (t1, (TVar x2)) -> bind x2 t1
            | (TFun(f1,l1), TFun(f2,l2)) ->
                if f1=f2 then unifylist (l1,l2) else failwith "functor"
and unifylist p =
    match p with
    | ([],[]) -> emptysubst
    | (x1::l1, x2::l2) ->
        let sl=unifylist (l1,l2) in
            let s=unify(substitute sl x1, substitute sl x2) in
                (substitute s) << sl
    | _ -> failwith "arity"
and bind x t =
    if occurs x t then failwith "occur"
    else (fun x1->if x1=x then t else TVar x1)
and substitute s t =
    match t with
    | TVar x -> s x
    | TFun (f,l) -> TFun(f, List.map (substitute s) l)
and occurs x t =
    match t with
    | TVar x1 -> x1=x
    | TFun (_,l) -> foldor (occurs x) l
and foldor f l =
    match l with
    | [] -> false
    | h::t -> (f h) || (foldor f t)

let instance n = substitute (fun k->if k=0 then TVar 0 else TVar (k+n))

let tCst t = TFun(t, [])
let V_ = TVar 0
let V1 = TVar 1
let V2 = TVar 2
let V3 = TVar 3
let V4 = TVar 4
let V5 = TVar 5

type ProofTree = PT of int * term * ((ProofTree * term) list)

let rec solve (PT(n, t1, l)) t x s =
    let s1=(substitute (unify(instance x t1, substitute s t))) << s in
        let l1=List.map (fun (p,t)->(p,(instance x t))) l in
            solvelist l1 (x+n) s1
and solvelist l x s =
        match l with
        | [] -> (s, x)
        | (p,t)::tr -> let (s1,x1)=solve p t x s in solvelist tr x1 s1

(* generalize term to clause *)

let rec addl x l =
    match l with
    | [] -> (1,[x])
    | h::t -> if x=h then (1,h::t) else let (n,t1)=addl x t in (n+1,h::t1)
and gen vl t =
    match t with
    | TVar v -> let (n,vl1) = addl v vl in (vl1,TVar n)
    | TFun (f,a) -> let (vl1,a1) = genl vl a in (vl1,TFun(f,a1))
and genl vl l =
    match l with
    | [] -> (vl,[])
    | h::t -> let (vl1,h1)=gen vl h in let (vl2,t1)=genl vl1 t in (vl2,h1::t1)

namespace Book_A_Desk.Domain.Reservation.Commands.Instructions

open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain

module ReservationInstructions =
    type ReservationInstruction<'next> =
        | AppendEvents of DomainEvent seq * 'next
        | GetReservationAggregate of ReservationId * (ReservationAggregate -> 'next)
        | GetOffices of (Office list -> 'next)
        
    type ReservationProgram<'a> =
        | Stop of 'a
        | Next of ReservationInstruction<ReservationProgram<'a>>
    
    let private mapT f instruction =
        match instruction with
        | AppendEvents (events, next) -> AppendEvents (events, f next)
        | GetReservationAggregate (aggregateId, next) -> GetReservationAggregate (aggregateId, next >> f)
        | GetOffices next -> GetOffices (next >> f)       
        
    let private returnT x =
        Stop x
    
    let rec private bindT f program =
        match program with
        | Next instruction -> 
            Next (mapT (bindT f) instruction)
        | Stop x -> 
            f x
        
    type ReservationInstructionsBuilder() =
        member this.Return(x) = returnT x
        member this.Bind(x,f) = bindT f x
        member this.Zero(x) = returnT ()        
    
module Reservation =
    open ReservationInstructions
    let stop = Stop()
    let appendEvents events = Next(AppendEvents (events, stop))
    let getReservationAggregate aggregateId =Next(GetReservationAggregate (aggregateId, Stop))
    let getOffices = Next((GetOffices Stop))
                          
    let reservationInstructions = ReservationInstructionsBuilder()
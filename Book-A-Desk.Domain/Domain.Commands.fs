namespace Book_A_Desk.Domain.Commands

open Book_A_Desk.Domain.Reservation.Commands

type DomainCommand =
    | ReservationCommand of ReservationCommand

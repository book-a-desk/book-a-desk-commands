namespace Book_A_Desk.Domain.Domain

open Book_A_Desk.Domain.Reservation

type DomainError =
    | ReservationError of ReservationError
    | AggregatedError of DomainError list

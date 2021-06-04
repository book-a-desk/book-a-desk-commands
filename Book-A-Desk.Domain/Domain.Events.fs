namespace Book_A_Desk.Domain.Events

open Book_A_Desk.Domain.Notification_Events
open Book_A_Desk.Domain.Reservation.Events

type DomainEvent =
    | ReservationEvent of ReservationEvent
    | NotificationEvent of NotificationEvent

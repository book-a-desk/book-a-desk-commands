namespace Book_A_Desk.Domain.Errors

open System
open Book_A_Desk.Domain


type GenericError =
    | GetEventsException of string


type UserBookingParam =
    {
        Date: DateTime
        EmailAddress: EmailAddress        
    }
    
type ReservationError =
    | InvalidEmailAddress
    | DateLowerThanToday
    | InvalidOfficeId
    | OfficeHasNoAvailability of DateTime
    | UserHadBookedBefore of UserBookingParam
    | UserHasNotBookedBefore of UserBookingParam

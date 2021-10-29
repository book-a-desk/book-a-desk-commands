namespace Book_A_Desk.Domain.Errors

open System
open Book_A_Desk.Domain


type GenericError =
    | GetEventsException of string


type UserHadBookedBeforeParam =
    {
        Date: DateTime
        EmailAddress: EmailAddress        
    }

type UserHasNotBookedBeforeParam =
    {
        Date: DateTime
        EmailAddress: EmailAddress        
    }
    
type ReservationError =
    | InvalidEmailAddress
    | DateLowerThanToday
    | InvalidOfficeId
    | OfficeHasNoAvailability of DateTime
    | UserHadBookedBefore of UserHadBookedBeforeParam
    | UserHasNotBookedBefore of UserHasNotBookedBeforeParam

namespace Book_A_Desk.Api

open System
open Microsoft.AspNetCore.Http

open Giraffe
open GiraffeExtensions

open Book_A_Desk.Domain.Domain
open Book_A_Desk.Domain.Reservation
open Microsoft.Extensions.Logging

type BookADeskError =
    | InvalidEmailAddress
    | DateLowerThanToday
    | InvalidOfficeId
    | OfficeHasNoAvailability of DateTime
    | UserHadBookedBefore of UserHadBookedBeforeParam

type BookADeskErrorHandler =
    {
        MapDomainErrorToAssignBookADeskError: DomainError -> BookADeskError
        ConvertErrorToResponse: ILogger -> BookADeskError -> HttpHandler       
    }

module BookADeskErrorHandler =
    let initialize () =

        let mapReservationErrorToHandlerError = function
            | ReservationError.InvalidEmailAddress -> InvalidEmailAddress
            | ReservationError.DateLowerThanToday -> DateLowerThanToday
            | ReservationError.InvalidOfficeId -> InvalidOfficeId
            | ReservationError.OfficeHasNoAvailability date -> date |> OfficeHasNoAvailability
            | ReservationError.UserHadBookedBefore userHadBookedBeforeParam -> userHadBookedBeforeParam |> UserHadBookedBefore

        let mapDomainErrorToHandlerError = function
            | ReservationError campError -> mapReservationErrorToHandlerError campError
            | _ -> failwith " This error type doesn't handle here "

        let convertErrorToResponse (logger:ILogger) (error:BookADeskError) =
            error
            |> string
            |> logger.LogWarning

            match error with
            | InvalidEmailAddress ->
                setStatusCode StatusCodes.Status400BadRequest
                >=> jsonProblem
                    {
                        Title = "Invalid Email Address";
                        Detail = "The e-mail address is invalid."
                    }
            | DateLowerThanToday ->
                setStatusCode StatusCodes.Status400BadRequest
                >=> jsonProblem
                    {
                        Title = "Date Lower Than Today";
                        Detail = "Date must be greater than today."
                    }
            | InvalidOfficeId ->
                setStatusCode StatusCodes.Status400BadRequest
                >=> jsonProblem
                    {
                        Title = "Invalid Office Id";
                        Detail = "You must enter a valid office ID."
                    }
            | OfficeHasNoAvailability (date : DateTime) ->
                setStatusCode StatusCodes.Status400BadRequest
                >=> jsonProblem
                    {
                        Title = "Office Has No Availability";
                        Detail = $"The office is booked out at {date.ToShortDateString()}"
                    }
            | UserHadBookedBefore (userHadBookedBeforeParam : UserHadBookedBeforeParam) ->
                setStatusCode StatusCodes.Status400BadRequest
                >=> jsonProblem
                    {
                        Title = "User Had Booked Before";
                        Detail = $"The office is already booked out at {userHadBookedBeforeParam.Date.ToShortDateString()} for user {userHadBookedBeforeParam.EmailAddress}"
                    }

        {
            MapDomainErrorToAssignBookADeskError = mapDomainErrorToHandlerError
            ConvertErrorToResponse = convertErrorToResponse
        }


namespace Book_A_Desk.Api

open System
open Microsoft.AspNetCore.Http

open Book_A_Desk.Domain.Errors

type BookADeskError =
    | InvalidEmailAddress
    | DateLowerThanToday
    | InvalidOfficeId
    | OfficeHasNoAvailability of DateTime
    | UserHadBookedBefore of UserBookingParam
    | UserHasNotBookedBefore of UserBookingParam
    | GenericError of string

type ProblemDetailsDto =
    {
        Title: string
        Details: string
    }

type ResponseError =
    {
        StatusCode: int
        Error: ProblemDetailsDto
    }

type BookADeskErrorHandler =
    {
        MapReservationErrorToAssignBookADeskError: ReservationError -> BookADeskError
        MapStringToAssignBookADeskError: string -> BookADeskError
        ConvertErrorToResponseError: BookADeskError -> ResponseError
    }

module BookADeskErrorHandler =
    let initialize () =

        let mapReservationErrorToHandlerError = function
            | ReservationError.InvalidEmailAddress -> InvalidEmailAddress
            | ReservationError.DateLowerThanToday -> DateLowerThanToday
            | ReservationError.InvalidOfficeId -> InvalidOfficeId
            | ReservationError.OfficeHasNoAvailability date -> date |> OfficeHasNoAvailability
            | ReservationError.UserHadBookedBefore userBookingParam -> userBookingParam |> UserHadBookedBefore
            | ReservationError.UserHasNotBookedBefore userBookingParam -> userBookingParam |> UserHasNotBookedBefore

        let mapStringToAssignBookADeskError (description: string) = GenericError description

        let convertErrorToResponseError (error:BookADeskError) =
            match error with
            | InvalidEmailAddress ->
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                        Error = {
                                    ProblemDetailsDto.Title = "Invalid Email Address"
                                    Details = "The e-mail address is invalid."
                                }
                    }
            | DateLowerThanToday ->
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                        Error = {
                                    ProblemDetailsDto.Title = "Date Lower Than Today"
                                    Details = "Date must be greater than today."
                                }
                    }
            | InvalidOfficeId ->
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                        Error = {
                                    ProblemDetailsDto.Title = "Invalid Office Id"
                                    Details = "You must enter a valid office ID."
                                }
                    }
            | OfficeHasNoAvailability (date : DateTime) ->
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                        Error = {
                                    ProblemDetailsDto.Title = "Office Has No Availability"
                                    Details = $"The office is booked out at {date.ToShortDateString()}"
                                }
                    }
            | UserHadBookedBefore (userBookingParam : UserBookingParam) ->
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                        Error = {
                                    ProblemDetailsDto.Title = "User Had Booked Before"
                                    Details = $"The office is already booked out at {userBookingParam.Date.ToShortDateString()} for user {userBookingParam.EmailAddress}"
                                }
                    }
            | UserHasNotBookedBefore (userBookingParam : UserBookingParam) ->
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                        Error = {
                                    ProblemDetailsDto.Title = "User Has Not Booked Before"
                                    Details = $"The office is not booked at {userBookingParam.Date.ToShortDateString()} for user {userBookingParam.EmailAddress}"
                                }
                    }
            | GenericError (description: string) ->
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                        Error = {
                                    ProblemDetailsDto.Title = "Generic Error"
                                    Details = description
                                }
                    }
        {
            MapReservationErrorToAssignBookADeskError = mapReservationErrorToHandlerError
            MapStringToAssignBookADeskError = mapStringToAssignBookADeskError
            ConvertErrorToResponseError = convertErrorToResponseError
        }


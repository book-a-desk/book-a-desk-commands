namespace Book_A_Desk.Domain

open System

open System.ComponentModel.DataAnnotations
open System.Text.RegularExpressions
open Book_A_Desk.Core
open Book_A_Desk.Domain.Errors
open Book_A_Desk.Domain.Office.Domain

module BookADeskValidator =
    
    let validateCorporateEmail email validDomainName =
        let (EmailAddress emailToValidate) = email
        let emailValidator = EmailAddressAttribute()
        let isValidEmail = emailValidator.IsValid(emailToValidate)
        let domainName = "@" + validDomainName
        let hasCorporateDomain = Regex.Match(emailToValidate.ToLower(), domainName)
        if isValidEmail && hasCorporateDomain.Success then
            Ok()
        else
            ReservationError.InvalidEmailAddress |> Error
            
    let validateDateIsGreaterThanToday requestedDate =
        let allowedDate = DateTime.Today.AddDays(1.)
        if requestedDate < allowedDate then
            ReservationError.DateLowerThanToday |> Error
        else
            Ok ()
            
    let validateOfficeIdIsValid officeId (offices : Office list) =
        if offices |> List.exists (fun office -> office.Id = officeId) |> not then
            ReservationError.InvalidOfficeId |> Error
        else
            Ok ()

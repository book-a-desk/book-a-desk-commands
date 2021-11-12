namespace Book_A_Desk.Api

open System

type OfficeRestrictionNotifier =
    {
        Execute: DateTime -> Async<Result<unit, string>>
    }

module rec OfficeRestrictionNotifier =
    let provide apiDependencyFactory bookingNotifier eventStore getOffices =

        let execute (day:DateTime) = asyncResult {
             apiDependencyFactory

            // TODO
                return! ()
            }


        {
            Execute = execute
        }

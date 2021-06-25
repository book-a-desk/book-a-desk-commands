namespace Book_A_Desk.Api

open Book_A_Desk.Api.Models

type ApiDependencyFactory =
    {
        CreateBookingsHttpHandler: unit -> BookingsHttpHandler
        CreateOfficesHttpHandler: unit -> OfficesHttpHandler
    }

module ApiDependencyFactory =
    let provide
        getEventStore
        reservationCommandsFactory
        getOffices
        (notifySuccess: Booking->Async<unit>)
        =

        let createBookingsHttpHandler bearerToken =
            BookingsHttpHandler.initialize
                getEventStore
                reservationCommandsFactory
                notifySuccess

        let createOfficesHttpHandler bearerToken =
            OfficesHttpHandler.initialize
                getEventStore
                getOffices

        {
            CreateBookingsHttpHandler = createBookingsHttpHandler
            CreateOfficesHttpHandler = createOfficesHttpHandler
        }

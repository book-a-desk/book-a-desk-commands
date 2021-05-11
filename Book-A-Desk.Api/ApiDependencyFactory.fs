namespace Book_A_Desk.Api

open Book_A_Desk.Domain

type ApiDependencyFactory =
    {
        CreateBookingsHttpHandler: unit -> BookingsHttpHandler
        CreateOfficesHttpHandler: unit -> OfficesHttpHandler
    }

module ApiDependencyFactory =
    let provide
        eventStore
        reservationCommandsFactory
        getOffices
        =

        let createBookingsHttpHandler bearerToken =
            BookingsHttpHandler.initialize
                eventStore
                reservationCommandsFactory

        let createOfficesHttpHandler bearerToken =
            OfficesHttpHandler.initialize
                eventStore
                getOffices

        {
            CreateBookingsHttpHandler = createBookingsHttpHandler
            CreateOfficesHttpHandler = createOfficesHttpHandler
        }

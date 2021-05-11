namespace Book_A_Desk.Api

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

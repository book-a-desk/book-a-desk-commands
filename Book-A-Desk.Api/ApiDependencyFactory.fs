namespace Book_A_Desk.Api

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
        notifySuccess
        =

        let createBookingsHttpHandler bearerToken =
            BookingsHttpHandler.initialize
                getEventStore
                reservationCommandsFactory
                notifySuccess
                <| BookADeskErrorHandler.initialize()                

        let createOfficesHttpHandler bearerToken =
            OfficesHttpHandler.initialize
                getEventStore
                getOffices

        {
            CreateBookingsHttpHandler = createBookingsHttpHandler
            CreateOfficesHttpHandler = createOfficesHttpHandler
        }

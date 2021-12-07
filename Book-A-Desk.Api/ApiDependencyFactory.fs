namespace Book_A_Desk.Api

type ApiDependencyFactory =
    {
        CreateBookingsHttpHandler: unit -> BookingsHttpHandler
        CreateOfficesHttpHandler: unit -> OfficesHttpHandler
        CreateFeatureFlagsHttpHandler: unit -> FlagsHttpHandler
        CreateCancelBookingsHttpHandler: unit -> CancelBookingsHttpHandler
    }

module ApiDependencyFactory =
    let provide
        getEventStore
        reservationCommandsFactory
        getOffices
        notifySuccess
        getFeatureFlags
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
                
        let createFeatureFlagsHandler bearerToken =
            FlagsHttpHandler.initialize
                getFeatureFlags
                
        let createCancelBookingsHttpHandler bearerToken =
            CancelBookingsHttpHandler.initialize
                getEventStore
                reservationCommandsFactory
                <| BookADeskErrorHandler.initialize()

        {
            CreateBookingsHttpHandler = createBookingsHttpHandler
            CreateOfficesHttpHandler = createOfficesHttpHandler
            CreateFeatureFlagsHttpHandler = createFeatureFlagsHandler
            CreateCancelBookingsHttpHandler = createCancelBookingsHttpHandler
        }

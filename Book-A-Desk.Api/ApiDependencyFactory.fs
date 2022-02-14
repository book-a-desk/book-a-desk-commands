namespace Book_A_Desk.Api

type ApiDependencyFactory =
    {
        CreateBookingsHttpHandler: unit -> BookingsHttpHandler
        CreateOfficesHttpHandler: unit -> OfficesHttpHandler
        CreateNotifierHttpHandler: unit -> NotifierHttpHandler
        CreateFeatureFlagsHttpHandler: unit -> FlagsHttpHandler
        CreateCancelBookingsHttpHandler: unit -> CancelBookingsHttpHandler
    }

module ApiDependencyFactory =
    let provide
        getEventStore
        reservationCommandsFactory
        getOffices
        notifySuccess
        notifyOfficeRestrictions
        featureFlags
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

        let createNotifierHttpHandler bearerToken =
            NotifierHttpHandler.initialize
                getEventStore
                notifyOfficeRestrictions
                <| BookADeskErrorHandler.initialize()
                
        let createFeatureFlagsHandler bearerToken =
            FlagsHttpHandler.initialize
                featureFlags
                
        let createCancelBookingsHttpHandler bearerToken =
            CancelBookingsHttpHandler.initialize
                getEventStore
                reservationCommandsFactory
                <| BookADeskErrorHandler.initialize()

        {
            CreateBookingsHttpHandler = createBookingsHttpHandler
            CreateOfficesHttpHandler = createOfficesHttpHandler
            CreateNotifierHttpHandler = createNotifierHttpHandler
            CreateFeatureFlagsHttpHandler = createFeatureFlagsHandler
            CreateCancelBookingsHttpHandler = createCancelBookingsHttpHandler
        }

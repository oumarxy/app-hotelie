﻿Namespace Services.Persistence
	Public Interface IUnitOfWork
		ReadOnly Property RoomRepository As IRoomRepository

		Function Commit() As Integer

		Function Rollback() As Integer
	End Interface
End Namespace
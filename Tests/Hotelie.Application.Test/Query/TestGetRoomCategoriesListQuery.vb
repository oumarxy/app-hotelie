﻿Imports Hotelie.Application.Rooms.Queries.GetRoomCategoriesList
Imports Hotelie.Application.Rooms.Queries.GetRoomsList
Imports Hotelie.Domain.Rooms
Imports Hotelie.Persistence.Common
Imports Hotelie.Persistence.Rooms

Namespace Query
    <TestClass>
    Public Class TestGetRoomCategoriesListQuery
        Private _context As DatabaseContext
        Private _roomRepository As RoomRepository
        Private _roomCategoriesList As List(Of RoomCategory)
        Private _getRoomCategoriesListQuery As GetRoomCategoriesListQuery

        <TestInitialize>
        Public Sub TestInitialize()
            _context = New DatabaseContext(
                $"data source=KHUONG-ASUS\SQLEXPRESS;initial catalog=HotelieDatabase;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework")
            _roomRepository = New RoomRepository(_context)
            _getRoomCategoriesListQuery = New GetRoomCategoriesListQuery(_roomRepository)
        End Sub

        <TestCleanup>
        Public Sub TestCleanup()
            _context.Dispose()
        End Sub

        Public Sub RoomCategoriesInitialize()
            _roomCategoriesList = New List(Of RoomCategory)
            Dim roomCategory1 = New RoomCategory() With{.Id="00001",.Name="NOR01",.Price=100000}
            Dim roomCategory2 = New RoomCategory() With{.Id="00002",.Name="VIP01",.Price=200000}
            _roomCategoriesList.Add(roomCategory1)
            _roomCategoriesList.Add(roomCategory2)
            _roomRepository.AddRoomCategories(_roomCategoriesList)
            _context.SaveChanges()
        End Sub

        Public Sub DisposeRoomCategories()
            _roomCategoriesList?.Clear()
            _roomRepository.RemoveRoomCategories(_roomRepository.GetAllRoomCategories())
            _context.SaveChanges()
        End Sub

        <TestMethod>
        Public Sub TestGetRoomCategories__ReturnAllCategory()
            ' pre-act
            DisposeRoomCategories()

            ' input
            RoomCategoriesInitialize()

            ' act
            Dim roomCategories = _getRoomCategoriesListQuery.Execute().ToList()

            ' assert
            CollectionAssert.AllItemsAreNotNull(roomCategories)
            Assert.AreEqual(_roomCategoriesList.Count, roomCategories.Count)

            For Each room As RoomCategory In _roomCategoriesList
                Dim q = False
                For Each rc As RoomCategoryModel In roomCategories
                    If (room.Id= rc.Id And room.Price = rc.Price) Then 
                        q = True
                        Exit For
                    End If
                Next
                Assert.IsTrue(q)
            Next

            ' rollback
            DisposeRoomCategories()
        End Sub
    End Class
End Namespace
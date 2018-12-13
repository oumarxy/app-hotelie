﻿Imports System.ComponentModel
Imports Caliburn.Micro
Imports Hotelie.Application.Rooms.Models
Imports Hotelie.Application.Rooms.Queries
Imports Hotelie.Presentation.Common
Imports Hotelie.Presentation.Common.Controls
Imports Hotelie.Presentation.Common.Infrastructure
Imports Hotelie.Presentation.Rooms.Models

Namespace Rooms.ViewModels
	Public Class ScreenRoomsListViewModel
		Inherits AppScreen
		Implements IRoomsListPresenter,
		           INeedWindowModals,
		           IChild(Of RoomsWorkspaceViewModel)

		' Backing fields
		Private _filterRoomModel As FilterRoomModel
		Private _sortRoomModel As SortRoomModel

		' Dependencies
		Private ReadOnly _getAllRoomsQuery As IGetAllRoomsQuery
		Private ReadOnly _getAllRoomCategoriesQuery As IGetAllRoomCategoriesQuery

		' Parent
		Public Property ParentWorkspace As RoomsWorkspaceViewModel Implements IChild(Of RoomsWorkspaceViewModel).Parent
			Get
				Return TryCast(Parent, RoomsWorkspaceViewModel)
			End Get
			Set
				Parent = value
				NotifyOfPropertyChange(Function() ParentWorkspace)
			End Set
		End Property

		' Binding models

		Public ReadOnly Property Rooms As IObservableCollection(Of FilterableRoomModel)

		Public Property FilterRoomModel As FilterRoomModel
			Get
				Return _filterRoomModel
			End Get
			Set
				If IsNothing( Value ) OrElse Equals( Value, _filterRoomModel ) Then Return
				_filterRoomModel = value
				NotifyOfPropertyChange( Function() FilterRoomModel )
			End Set
		End Property

		Public Property SortRoomModel As SortRoomModel
			Get
				Return _sortRoomModel
			End Get
			Set
				If IsNothing( Value ) OrElse Equals( Value, _sortRoomModel ) Then Return
				_sortRoomModel = value
				NotifyOfPropertyChange( Function() SortRoomModel )
			End Set
		End Property

		' Binding data
		' ReSharper disable once UnassignedGetOnlyAutoProperty

		Public ReadOnly Property Categories As IObservableCollection(Of IRoomCategoryModel)

		' ReSharper disable once UnassignedGetOnlyAutoProperty

		Public ReadOnly Property States As IObservableCollection(Of Integer)

		' ReSharper disable once UnassignedGetOnlyAutoProperty

		Public ReadOnly Property MinUnitPrices As IObservableCollection(Of Decimal)

		' ReSharper disable once UnassignedGetOnlyAutoProperty

		Public ReadOnly Property MaxUnitPrices As IObservableCollection(Of Decimal)

		' Initialization

		Public Sub New( workspace As RoomsWorkspaceViewModel,
		                getAllRoomsQuery As IGetAllRoomsQuery,
		                getAllRoomCategoriesQuery As IGetAllRoomCategoriesQuery )
			MyBase.New( MaterialDesignThemes.Wpf.ColorZoneMode.PrimaryDark )

			ParentWorkspace = workspace
			_getAllRoomsQuery = getAllRoomsQuery
			_getAllRoomCategoriesQuery = getAllRoomCategoriesQuery
			RegisterInventory()

			Rooms = New BindableCollection(Of FilterableRoomModel)
			Categories = New BindableCollection(Of IRoomCategoryModel)
			States = New BindableCollection(Of Integer)
			MinUnitPrices = New BindableCollection(Of Decimal)
			MaxUnitPrices = New BindableCollection(Of Decimal)

			FilterRoomModel = New FilterRoomModel()
			AddHandler FilterRoomModel.PropertyChanged, AddressOf OnFilterRoomModelUpdated

			SortRoomModel = New SortRoomModel()
			SortRoomModel.SortingCode = - 1
			AddHandler SortRoomModel.PropertyChanged, AddressOf OnSortRoomModelUpdated
		End Sub

		Public Sub Init()
			'InitRooms()

			'InitRoomCategories()

			'InitRoomStates()

			'InitRoomUnitPrices()

			'RefreshRoomsListVisibility()
		End Sub

		Private Sub InitRooms()
			Rooms.Clear()
			Rooms.AddRange( _getAllRoomsQuery.Execute().Select( Function( r ) New FilterableRoomModel With
				                                                  {.Model=r, .IsFiltersMatch=False} ) )
		End Sub

		Private Async Function InitRoomsAsync() As Task
			Rooms.Clear()
			Rooms.AddRange( (Await _getAllRoomsQuery.ExecuteAsync()).Select( Function( r ) New FilterableRoomModel With
				                                                               {.Model=r, .IsFiltersMatch=False} ) )
		End Function

		Private Sub InitRoomCategories()
			Categories.Clear()
			Categories.AddRange( _getAllRoomCategoriesQuery.Execute() )
			Categories.Add( New FakeRoomCategoryModel() ) 'filter all
		End Sub

		Private Async Function InitRoomCategoriesAsync() As Task
			Categories.Clear()
			Categories.AddRange( Await _getAllRoomCategoriesQuery.ExecuteAsync() )
			Categories.Add( New FakeRoomCategoryModel() ) 'filter all
		End Function

		Private Sub InitRoomUnitPrices()
			Dim minPrices = New List(Of Decimal)
			Dim maxPrices = New List(Of Decimal)

			For i = 0 To Categories.Count - 2
				Dim price = Categories( i ).UnitPrice

				If Not minPrices.Contains( price ) Then minPrices.Add( price )

				If Not maxPrices.Contains( price ) Then maxPrices.Add( price )
			Next

			minPrices.Sort( Function( a,
				              b ) a < b )
			minPrices.Add( - 1 )

			maxPrices.Sort( Function( a,
				              b ) a > b )
			maxPrices.Add( - 1 )

			MinUnitPrices.Clear()
			MinUnitPrices.AddRange( minPrices )

			MaxUnitPrices.Clear()
			MaxUnitPrices.AddRange( maxPrices )
		End Sub

		Private Sub InitRoomStates()
			States.Clear()
			States.AddRange( {0, 1} )
			States.Add( 2 ) 'filter all
		End Sub

		' Loading
		Public Sub Reload() Implements IRoomsListPresenter.Reload
			Throw New NotImplementedException()
		End Sub

		Public Async Function ReloadAsync() As Task Implements IRoomsListPresenter.ReloadAsync
			Await ReloadRoomsAsync()

			Await ReloadRoomCategoriesAsync()

			ReloadRoomStates()

			ReloadRoomUnitPrices()

			RefreshRoomsListVisibility()
		End Function

		Private Async Function ReloadRoomsAsync() As Task
			Rooms.Clear()
			Rooms.AddRange( (Await _getAllRoomsQuery.ExecuteAsync()).Select( Function( r ) New FilterableRoomModel With
				                                                               {.Model=r, .IsFiltersMatch=False} ) )
		End Function

		Private Async Function ReloadRoomCategoriesAsync() As Task
			Categories.Clear()
			Categories.AddRange( Await _getAllRoomCategoriesQuery.ExecuteAsync() )
			Categories.Add( New FakeRoomCategoryModel() ) 'filter all
		End Function

		Private Sub ReloadRoomUnitPrices()
			Dim minPrices = New List(Of Decimal)
			Dim maxPrices = New List(Of Decimal)

			For i = 0 To Categories.Count - 2
				Dim price = Categories( i ).UnitPrice

				If Not minPrices.Contains( price ) Then minPrices.Add( price )

				If Not maxPrices.Contains( price ) Then maxPrices.Add( price )
			Next

			minPrices.Sort( Function( a,
				              b ) a < b )
			minPrices.Add( - 1 )

			maxPrices.Sort( Function( a,
				              b ) a > b )
			maxPrices.Add( - 1 )

			MinUnitPrices.Clear()
			MinUnitPrices.AddRange( minPrices )

			MaxUnitPrices.Clear()
			MaxUnitPrices.AddRange( maxPrices )
		End Sub

		Private Sub ReloadRoomStates()
			States.Clear()
			States.AddRange( {0, 1} )
			States.Add( 2 ) 'filter all
		End Sub

		' Business Actions

		Public Sub DoRoomAction( model As IRoomModel )
			If IsNothing( model ) Then Return

			Select Case model.State
				Case 0
					ParentWorkspace.ParentShell.NavigateToScreenAddLease( model.Id )
				Case 1
					ParentWorkspace.ParentShell.NavigateToScreenAddBillWithRoom( model.Id )
			End Select
		End Sub

		' Filtering

		Public Sub ResetFilters()
			RemoveHandler FilterRoomModel.PropertyChanged, AddressOf OnFilterRoomModelUpdated
			FilterRoomModel.NamePrefix = String.Empty
			FilterRoomModel.Category = Nothing
			FilterRoomModel.State = Nothing
			FilterRoomModel.MinUnitPrice = Nothing
			FilterRoomModel.MaxUnitPrice = Nothing
			AddHandler FilterRoomModel.PropertyChanged, AddressOf OnFilterRoomModelUpdated
			RefreshRoomsListVisibility()
		End Sub

		Public Sub FilterByRoomCategoryOf( roomModel As IRoomModel )
			Dim category = Categories.FirstOrDefault( Function( c ) String.Equals( c.Id, roomModel.Category.Id ) )
			If IsNothing( category ) Then Return

			FilterRoomModel.Category = category
		End Sub

		Public Sub FilterByRoomStateOf( roomModel As IRoomModel )
			Dim state = States.FirstOrDefault( Function( s ) Equals( s, roomModel.State ) )
			If IsNothing( state ) Then Return

			FilterRoomModel.State = state
		End Sub

		Private Sub OnFilterRoomModelUpdated( sender As Object,
		                                      e As PropertyChangedEventArgs )
			RefreshRoomsListVisibility()
		End Sub

		Public Sub RefreshRoomsListVisibility()
			For Each filterableRoomModel As FilterableRoomModel In Rooms
				If IsNothing( FilterRoomModel ) Then filterableRoomModel.IsFiltersMatch = True
				filterableRoomModel.IsFiltersMatch = FilterRoomModel.IsMatch( filterableRoomModel.Model,
				                                                              Categories,
				                                                              States )
			Next
		End Sub

		' Sorting
		Private Sub OnSortRoomModelUpdated( sender As Object,
		                                    e As PropertyChangedEventArgs )
			SortRoomsList()
		End Sub

		Public Sub SortRoomsList()
			Dim orderedRooms As IObservableCollection(Of FilterableRoomModel ) = Nothing

			Select Case SortRoomModel.SortingCode
				Case 0
					If SortRoomModel.IsDescendingSort
						orderedRooms = New BindableCollection(Of FilterableRoomModel)( Rooms.OrderByDescending( Function( r ) r.Model.Name ) )
					Else
						orderedRooms = New BindableCollection(Of FilterableRoomModel)( Rooms.OrderBy( Function( r ) r.Model.Name ) )
					End If
				Case 1
					If SortRoomModel.IsDescendingSort
						orderedRooms =
							New BindableCollection(Of FilterableRoomModel)( Rooms.OrderByDescending( Function( r ) r.Model.Category.Name ) )
					Else
						orderedRooms = New BindableCollection(Of FilterableRoomModel)( Rooms.OrderBy( Function( r ) r.Model.Category.Name ) )
					End If
				Case 2
					If SortRoomModel.IsDescendingSort
						orderedRooms =
							New BindableCollection(Of FilterableRoomModel)( Rooms.OrderByDescending( Function( r ) r.Model.Category.UnitPrice ) )
					Else
						orderedRooms =
							New BindableCollection(Of FilterableRoomModel)( Rooms.OrderBy( Function( r ) r.Model.Category.UnitPrice ) )
					End If
				Case 3
					If SortRoomModel.IsDescendingSort
						orderedRooms =
							New BindableCollection(Of FilterableRoomModel)( Rooms.OrderByDescending( Function( r ) r.Model.State ) )
					Else
						orderedRooms = New BindableCollection(Of FilterableRoomModel)( Rooms.OrderBy( Function( r ) r.Model.State ) )
					End If
			End Select

			If orderedRooms IsNot Nothing
				Rooms.Clear()
				Rooms.AddRange( orderedRooms )
			End If
		End Sub

		' Infrastructure

		Public Sub OnRoomAdded( model As IRoomModel ) _
			Implements IRoomsListPresenter.OnRoomAdded
			If IsNothing( model ) OrElse String.IsNullOrEmpty( model.Id ) Then Return

			Dim room = Rooms.FirstOrDefault( Function( r ) r.Model?.Id = model.Id )
			If room IsNot Nothing
				ShowStaticBottomNotification( Start.MainWindow.Models.StaticNotificationType.Warning,
				                              "Tìm thấy phòng cùng id trong danh sách" )
				room.Model = model
			Else
				Rooms.Add( New FilterableRoomModel With {.Model=model, .IsFiltersMatch=False} )
			End If

			RefreshRoomsListVisibility()
			SortRoomsList()
		End Sub

		Public Sub OnRoomUpdated( model As IRoomModel ) _
			Implements IRoomsListPresenter.OnRoomUpdated
			If IsNothing( model ) OrElse String.IsNullOrEmpty( model.Id ) Then Return

			' find room
			Dim room = Rooms.FirstOrDefault( Function( r ) r.Model?.Id = model.Id )
			If room Is Nothing
				ShowStaticBottomNotification( Start.MainWindow.Models.StaticNotificationType.Warning,
				                              $"Không tìm thấy phòng {model.Name} trong danh sách phòng để cập nhật" )
				room = new FilterableRoomModel With {.IsFiltersMatch=False}
				Rooms.Add( room )
			End If

			' update
			room.Model = model

			RefreshRoomsListVisibility()
			SortRoomsList()
		End Sub

		Public Sub OnRoomRemoved( id As String ) Implements IRoomsListPresenter.OnRoomRemoved
			' find room
			Dim room = Rooms.FirstOrDefault( Function( r ) r.Model.Id = id )
			If room Is Nothing Then Return

			Rooms.Remove( room )
		End Sub
	End Class
End Namespace

﻿Imports System.ComponentModel
Imports Caliburn.Micro
Imports Hotelie.Application.Leases.Models
Imports Hotelie.Application.Leases.Queries
Imports Hotelie.Application.Rooms.Models
Imports Hotelie.Presentation.Common
Imports Hotelie.Presentation.Common.Controls
Imports Hotelie.Presentation.Common.Infrastructure
Imports Hotelie.Presentation.Leases.Models

Namespace Leases.ViewModels
	Public Class ScreenLeasesListViewModel
		Inherits AppScreen
		Implements IChild(Of LeasesWorkspaceViewModel)
		Implements INeedWindowModals
		Implements ILeasesListPresenter
		Implements IRoomPresenter

		' Dependencies
		Private ReadOnly _getAllLeasesQuery As IGetAllLeasesQuery

		' Parent 
		Public Property ParentWorkspace As LeasesWorkspaceViewModel Implements IChild(Of LeasesWorkspaceViewModel).Parent
			Get
				Return TryCast(Parent, LeasesWorkspaceViewModel)
			End Get
			Set
				If IsNothing( Value ) OrElse Equals( Value, Parent ) Then Return
				Parent = value
				NotifyOfPropertyChange( Function() ParentWorkspace )
			End Set
		End Property

		Public Sub New( workspace As LeasesWorkspaceViewModel,
		                getAllLeasesQuery As IGetAllLeasesQuery )
			MyBase.New( MaterialDesignThemes.Wpf.ColorZoneMode.PrimaryDark )
			ParentWorkspace = workspace
			_getAllLeasesQuery = getAllLeasesQuery
			TryCast(Me, IRoomPresenter).RegisterInventory()
			TryCast(Me, ILeasesListPresenter).RegisterInventory()

			FilterModel = new FilterLeaseModel
			AddHandler FilterModel.PropertyChanged, AddressOf OnFilterModelUpdated

			Leases = New BindableCollection(Of UpdatableLeaseModel)()
		End Sub

		Public Sub Init()
			'InitLeases()
		End Sub

		Private Sub InitLeases()
			Leases.Clear()

			Leases.AddRange(
				_getAllLeasesQuery.Execute().
				               Where( Function( l ) Not l.IsPaid ).
				               Select( Function( l ) New UpdatableLeaseModel( l ) With {.IsFiltersMatch=False} ) )

			RefreshLeasesVisibity()
			SortLeases()
		End Sub

		Private Async Function InitLeasesAsync() As Task
			Leases.Clear()
			Leases.AddRange(
				(Await _getAllLeasesQuery.ExecuteAsync()).
				               Where( Function( l ) Not l.IsPaid ).
				               Select( Function( l ) New UpdatableLeaseModel( l ) With {.IsFiltersMatch=False} ) )
			RefreshLeasesVisibity()
			SortLeases()
		End Function

		' Loading
		Public Sub Reload() Implements ILeasesListPresenter.Reload
			Throw New NotImplementedException()
		End Sub

		Private Sub Reload_() Implements IRoomPresenter.Reload
		End Sub

		Public Function ReloadAsync_() As Task Implements IRoomPresenter.ReloadAsync
			Return Task.FromResult( True )
		End Function

		Public Async Function ReloadAsync() As Task Implements ILeasesListPresenter.ReloadAsync
			Await ReloadLeasesAsync()
		End Function

		Private Async Function ReloadLeasesAsync() As Task
			Leases.Clear()
			Leases.AddRange(
				(Await _getAllLeasesQuery.ExecuteAsync()).
				               Where( Function( l ) Not l.IsPaid ).
				               Select( Function( l ) New UpdatableLeaseModel( l ) With {.IsFiltersMatch=False} ) )
			RefreshLeasesVisibity()
			SortLeases()
		End Function

		' Bind models

		Public ReadOnly Property FilterModel As FilterLeaseModel

		Public ReadOnly Property Leases As IObservableCollection(Of UpdatableLeaseModel)

		Private Sub OnFilterModelUpdated( sender As Object,
		                                  e As PropertyChangedEventArgs )
			RefreshLeasesVisibity()
		End Sub

		Private Sub RefreshLeasesVisibity()
			For Each lease As UpdatableLeaseModel In Leases
				lease.IsFiltersMatch = FilterModel.IsMatch( lease )
			Next
		End Sub

		Private Sub SortLeases()
			Dim bakLeases = New List(Of UpdatableLeaseModel)
			bakLeases.AddRange( Leases.OrderByDescending(Function(l) l.CheckinDate) )

			Leases.Clear()
			Leases.AddRange( bakLeases )
		End Sub

		' Business actions

		Public Sub DoLeaseAction( model As ILeaseModel )
			If IsNothing( model ) Then Return

			ParentWorkspace.ParentShell.NavigateToScreenAddBillWithLease( model.Id )
		End Sub

		' Infrastructure

		Public Sub OnLeaseAdded( model As ILeaseModel ) Implements ILeasesListPresenter.OnLeaseAdded
			If IsNothing( model ) OrElse String.IsNullOrEmpty( model.Id ) Then Return
			If model.IsPaid Then Return

			Dim lease = Leases.FirstOrDefault( Function( l ) l.Id = model.Id )
			If lease IsNot Nothing
				ShowStaticTopNotification( Start.MainWindow.Models.StaticNotificationType.Warning,
				                           $"Tìm thấy phiếu thuê phòng cùng mã {model.IdEx} trong danh sách" )
				Leases( Leases.IndexOf( lease ) ) = New UpdatableLeaseModel( model )
			Else
				Leases.Add( New UpdatableLeaseModel( model ) )
			End If

			RefreshLeasesVisibity()
			SortLeases()
		End Sub

		Public Sub OnLeaseUpdated( model As ILeaseModel ) Implements ILeasesListPresenter.OnLeaseUpdated
			If IsNothing( model ) OrElse String.IsNullOrEmpty( model.Id ) Then Return

			If model.IsPaid
				Dim leaseToRemove = Leases.FirstOrDefault( Function( l ) l.Id = model.Id )
				If leaseToRemove IsNot Nothing
					Leases.Remove( leaseToRemove )
				End If

			Else
				Dim leaseToUpdate = Leases.FirstOrDefault( Function( l ) l.Id = model.Id )
				If leaseToUpdate IsNot Nothing
					Leases( Leases.IndexOf( leaseToUpdate ) ) = New UpdatableLeaseModel( model )
				Else
					Leases.Add( New UpdatableLeaseModel( model ) )
				End If
			End If

			RefreshLeasesVisibity()
			SortLeases()
		End Sub

		Public Sub OnLeaseRemoved( id As String ) Implements ILeasesListPresenter.OnLeaseRemoved
			Dim leaseToRemove = Leases.FirstOrDefault( Function( l ) l.Id = id )
			If IsNothing( leaseToRemove ) Then Return

			Leases.Remove( leaseToRemove )
		End Sub

		Public Sub OnRoomUpdated( model As IRoomModel ) Implements IRoomPresenter.OnRoomUpdated
			If IsNothing( model ) OrElse String.IsNullOrEmpty( model.Id ) Then Return

			Dim leaseToUpdate = Leases.FirstOrDefault( Function( l ) l.Room?.Id = model.Id )
			If IsNothing( leaseToUpdate ) Then Return

			leaseToUpdate.Room = model
			RefreshLeasesVisibity()
		End Sub
	End Class
End Namespace

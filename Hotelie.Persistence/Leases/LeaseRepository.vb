﻿Imports System.Data.Entity
Imports Hotelie.Application.Services.Persistence
Imports Hotelie.Domain.Leases
Imports Hotelie.Persistence.Common

Namespace Leases
    Public Class LeaseRepository
        Inherits Repository(Of Lease)
        Implements ILeaseRepository

        Private ReadOnly _databaseService As DatabaseService

        Public Sub New(databaseService As DatabaseService)
            MyBase.New(databaseService)
            _databaseService = databaseService
        End Sub

        Public Function GetCustomers(id As String) As List(Of LeaseDetail) Implements ILeaseRepository.GetCustomers
            Return _databaseService.Context.LeaseDetails.Where(Function(p)p.Lease.Id = id).ToList()
        End Function

        Public Async Function GetCustomersAsync(id As String) As Task(Of List(Of LeaseDetail)) Implements ILeaseRepository.GetCustomersAsync
            Dim curs = Await _databaseService.Context.LeaseDetails.Where(Function(p)p.Lease.Id = id).ToListAsync()
            Return curs
        End Function

        Public Overrides Function GetOne(id As Object) As Lease
            Dim idString = CType(id, String)
            If idString Is Nothing Then Throw New InvalidCastException("Id must be string")

            Return _databaseService.Context.Leases.FirstOrDefault(Function(p) String.Equals(p.Id, idString))
        End Function

        Public Overrides Async Function GetOneAsync(id As Object) As Task(Of Lease)
            Dim idString = CType(id, String)
            If idString Is Nothing Then Throw New InvalidCastException("Id must be string")
            Dim lease = Await _databaseService.Context.Leases.FirstOrDefaultAsync(Function(p) String.Equals(p.Id, idString))
            Return lease
        End Function
    End Class
End Namespace
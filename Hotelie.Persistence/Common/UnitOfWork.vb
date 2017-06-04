﻿Imports Hotelie.Application.Services.Persistence

Namespace Common
    Public Class UnitOfWork
        Implements IUnitOfWork

        Private ReadOnly _databaseService As DatabaseService

        Sub New(databaseService As DatabaseService)
            _databaseService = databaseService
            If _databaseService.Context Is Nothing Then Throw New ArgumentNullException()
        End Sub


        Public Sub Commit() Implements IUnitOfWork.Commit
            Try
                _databaseService.Context.SaveChanges()
            Catch
                Throw New DatabaseConnectionFailedException
            End Try
        End Sub

        Public Async Sub CommitAsync() Implements IUnitOfWork.CommitAsync
            Try
                Await _databaseService.Context.SaveChangesAsync()
            Catch
                Throw New DatabaseConnectionFailedException
            End Try
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            _databaseService.Context.Dispose()
        End Sub
    End Class
End Namespace
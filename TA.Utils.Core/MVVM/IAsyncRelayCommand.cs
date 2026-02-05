// This file is part of the TA.Utils project
// Copyright © 2015-2026 Timtek Systems, all rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.

using System.ComponentModel;

namespace TA.Utils.Core.MVVM;

/// <summary>
///     An asynchronous Relay Command that accepts a parameter of type <typeparamref name="TParam" />
///     and supports property change notifications for observable properties such as
///     <see cref="IAsyncRelayCommand.IsRunning" />.
/// </summary>
/// <typeparam name="TParam">The type of parameter passed to the command.</typeparam>
public interface IAsyncRelayCommand<TParam> : IRelayCommand<TParam>, IAsyncRelayCommand { }

/// <summary>
///     An asynchronous MVVM-style Relay Command that supports property change notifications
///     for observable properties such as <see cref="IsRunning" />.
/// </summary>
public interface IAsyncRelayCommand : IRelayCommand, INotifyPropertyChanged
{
    /// <summary>
    ///     Gets a value indicating whether the command is currently executing.
    /// </summary>
    bool IsRunning { get; }
}
﻿using statsd.net.shared.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace statsd.net.shared.Blocks
{
  public class TimedBufferBlock<T> : ITargetBlock<T>
  {
    private ConcurrentStack<T> _buffer;
    private Action<T[]> _flushAction;
    private IIntervalService _intervalService;
    private Task _completionTask;
    private bool _isActive;

    public TimedBufferBlock(TimeSpan flushPeriod, 
      Action<T[]> flushAction, 
      IIntervalService interval = null,
      CancellationToken? cancellationToken = null)
    {
      _buffer = new ConcurrentStack<T>();
      _isActive = true;
         
      _completionTask = new Task(() =>
      {
        _isActive = false;
      });
      _flushAction = flushAction;
      if (interval == null)
      {
        _intervalService = new IntervalService(flushPeriod, cancellationToken);
      }
      _intervalService.Elapsed += (sender, e) =>
      {
        Flush();
      };
      _intervalService.Start();
      _isActive = true;
    }

    private void Flush()
    {
      var items = _buffer.ToArray();
      _buffer.Clear();
      _flushAction(items);
    }

    public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, bool consumeToAccept)
    {
      _buffer.Push(messageValue);
      return DataflowMessageStatus.Accepted;
    }

    public void Complete()
    {
      _completionTask.RunSynchronously();
    }

    public Task Completion
    {
      get { return _completionTask; } 
    }

    public void Fault(Exception exception)
    {
      throw new NotImplementedException();
    }
  }
}

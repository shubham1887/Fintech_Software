// ============================================================================
// ConcurrencyControl.cs
// Enterprise-Grade Distributed Item-Level Locking for Financial Systems
// Target: .NET 9 | Thread-Safe | Async-Ready | Observable
// ============================================================================
// Usage:
//   await using (await ConcurrencyControl.AcquireLockAsync(repoId, "TXN-00123"))
//   {
//       // critical section — only one thread per (repoId, itemKey) at a time
//   }
//
//   // Synchronous (legacy support)
//   using (ConcurrencyControl.AcquireLock(repoId, "TXN-00123"))
//   {
//       // critical section
//   }
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FintechApp.Processing
{
    /// <summary>
    /// Provides fine-grained, item-level mutual exclusion for concurrent operations.
    /// Designed for high-throughput financial transaction processing where multiple
    /// threads may attempt to process the same logical item simultaneously.
    /// 
    /// <para><b>Thread Safety:</b> All public members are thread-safe.</para>
    /// <para><b>Reentrancy:</b> NOT reentrant — acquiring the same key on the same
    /// thread that already holds it will deadlock (by design, to prevent
    /// double-processing in financial pipelines).</para>
    /// </summary>
    public sealed class ConcurrencyControl : IDisposable
    {
        // ====================================================================
        // Singleton (backward-compatible static API)
        // ====================================================================
        private static readonly Lazy<ConcurrencyControl> _default = new(
            () => new ConcurrencyControl(ConcurrencyControlOptions.Default, NullLogger<ConcurrencyControl>.Instance),
            LazyThreadSafetyMode.ExecutionAndPublication);

        // ====================================================================
        // Instance fields
        // ====================================================================
        private readonly ConcurrentDictionary<LockKey, LockEntry> _locks = new();
        private readonly ILogger<ConcurrencyControl> _logger;
        private readonly ConcurrencyControlOptions _options;
        private readonly ConcurrencyMetrics _metrics = new();
        private volatile bool _disposed;

        // ====================================================================
        // Constructors
        // ====================================================================

        /// <summary>
        /// Creates a new <see cref="ConcurrencyControl"/> instance with the
        /// specified options and logger. Prefer DI registration over the static API
        /// in new code.
        /// </summary>
        public ConcurrencyControl(ConcurrencyControlOptions options, ILogger<ConcurrencyControl> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? NullLogger<ConcurrencyControl>.Instance;
        }

        // ====================================================================
        // Static convenience API (delegates to singleton)
        // ====================================================================

        /// <summary>Acquires an item lock synchronously (legacy/simple callers).</summary>
        public static IDisposable AcquireLock(int repoId, string itemKey)
        {
            return _default.Value.AcquireLockInstance(repoId, itemKey);
        }

        /// <summary>Acquires an item lock asynchronously with cancellation support.</summary>
        public static ValueTask<IAsyncDisposable> AcquireLockAsync(
            int repoId,
            string itemKey,
            CancellationToken cancellationToken = default)
        {
            return _default.Value.AcquireLockInstanceAsync(repoId, itemKey, cancellationToken);
        }

        // ====================================================================
        // Instance API (for DI / testability)
        // ====================================================================

        /// <summary>Acquires an item lock synchronously.</summary>
        public IDisposable AcquireLockInstance(int repoId, string itemKey)
        {
            ThrowIfDisposed();
            ValidateArguments(repoId, itemKey);

            var key = new LockKey(repoId, itemKey);
            var entry = GetOrCreateEntry(key);
            var sw = Stopwatch.StartNew();

            bool acquired = false;
            try
            {
                acquired = entry.Semaphore.Wait(_options.AcquireTimeout);
                if (!acquired)
                {
                    ReleaseEntry(key, entry);
                    throw new TimeoutException(
                        $"Failed to acquire lock for ({repoId}, \"{itemKey}\") " +
                        $"within {_options.AcquireTimeout.TotalSeconds:F1}s. " +
                        $"Possible deadlock or long-running critical section.");
                }
            }
            catch (Exception) when (!acquired)
            {
                // If Wait itself throws (e.g. ObjectDisposedException), clean up
                ReleaseEntry(key, entry);
                throw;
            }

            sw.Stop();
            RecordAcquired(key, sw.Elapsed);

            return new LockHandle(this, key, entry);
        }

        /// <summary>Acquires an item lock asynchronously with cancellation support.</summary>
        public async ValueTask<IAsyncDisposable> AcquireLockInstanceAsync(
            int repoId,
            string itemKey,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ValidateArguments(repoId, itemKey);

            var key = new LockKey(repoId, itemKey);
            var entry = GetOrCreateEntry(key);
            var sw = Stopwatch.StartNew();

            bool acquired = false;
            try
            {
                acquired = await entry.Semaphore.WaitAsync(_options.AcquireTimeout, cancellationToken)
                                                .ConfigureAwait(false);
                if (!acquired)
                {
                    ReleaseEntry(key, entry);
                    throw new TimeoutException(
                        $"Failed to acquire lock for ({repoId}, \"{itemKey}\") " +
                        $"within {_options.AcquireTimeout.TotalSeconds:F1}s. " +
                        $"Possible deadlock or long-running critical section.");
                }
            }
            catch (Exception) when (!acquired)
            {
                ReleaseEntry(key, entry);
                throw;
            }

            sw.Stop();
            RecordAcquired(key, sw.Elapsed);

            return new AsyncLockHandle(this, key, entry);
        }

        /// <summary>Returns a snapshot of current lock metrics.</summary>
        public ConcurrencyMetricsSnapshot GetMetrics() => _metrics.Snapshot(_locks.Count);

        // ====================================================================
        // Internal helpers
        // ====================================================================

        private LockEntry GetOrCreateEntry(LockKey key)
        {
            // AddOrUpdate guarantees atomicity of the factory/update delegates
            return _locks.AddOrUpdate(
                key,
                static _ => new LockEntry(),
                static (_, existing) =>
                {
                    existing.IncrementRef();
                    return existing;
                });
        }

        private void ReleaseEntry(LockKey key, LockEntry entry)
        {
            if (entry.DecrementRef() == 0)
            {
                // Only remove if no other thread has since incremented the ref count.
                // This prevents a TOCTOU race: if another thread called GetOrCreateEntry
                // between our DecrementRef and TryRemove, the entry's ref is > 0 and
                // we must NOT remove it.
                _locks.TryRemove(KeyValuePair.Create(key, entry));
            }
        }

        private void ReleaseLock(LockKey key, LockEntry entry)
        {
            try
            {
                entry.Semaphore.Release();
            }
            catch (SemaphoreFullException ex)
            {
                // Should never happen — indicates a double-dispose bug in caller
                _logger.LogError(ex, "Double-release detected for lock ({RepoId}, {ItemKey}). " +
                    "This indicates a bug in the calling code.", key.RepoId, key.ItemKey);
            }
            finally
            {
                _metrics.IncrementReleased();
                ReleaseEntry(key, entry);

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Lock released: ({RepoId}, {ItemKey}). Active locks: {Count}",
                        key.RepoId, key.ItemKey, _locks.Count);
                }
            }
        }

        private void RecordAcquired(LockKey key, TimeSpan waitTime)
        {
            _metrics.RecordAcquired(waitTime);

            if (waitTime > _options.SlowLockThreshold)
            {
                _logger.LogWarning(
                    "Slow lock acquisition: ({RepoId}, {ItemKey}) took {WaitMs:F1}ms " +
                    "(threshold: {ThresholdMs}ms). Check for contention.",
                    key.RepoId, key.ItemKey, waitTime.TotalMilliseconds,
                    _options.SlowLockThreshold.TotalMilliseconds);
            }
            else if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Lock acquired: ({RepoId}, {ItemKey}) in {WaitMs:F2}ms. Active locks: {Count}",
                    key.RepoId, key.ItemKey, waitTime.TotalMilliseconds, _locks.Count);
            }
        }

        private static void ValidateArguments(int repoId, string itemKey)
        {
            ArgumentNullException.ThrowIfNull(itemKey);
            if (string.IsNullOrWhiteSpace(itemKey))
                throw new ArgumentException("Item key cannot be empty or whitespace.", nameof(itemKey));
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        // ====================================================================
        // IDisposable — for DI container / graceful shutdown
        // ====================================================================

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // Drain all active lock entries
            foreach (var kvp in _locks)
            {
                try { kvp.Value.Semaphore.Dispose(); }
                catch { /* best-effort cleanup */ }
            }
            _locks.Clear();

            _logger.LogInformation("ConcurrencyControl disposed. Final metrics: {Metrics}", _metrics.Snapshot(0));
        }

        // ====================================================================
        // LockKey — value-type composite key (zero allocation for lookups)
        // ====================================================================

        private readonly record struct LockKey(int RepoId, string ItemKey);

        // ====================================================================
        // LockEntry — ref-counted semaphore wrapper
        // ====================================================================

        private sealed class LockEntry
        {
            public SemaphoreSlim Semaphore { get; } = new(1, 1);
            private int _refCount = 1;

            public void IncrementRef() => Interlocked.Increment(ref _refCount);

            /// <returns>The new ref count after decrement.</returns>
            public int DecrementRef() => Interlocked.Decrement(ref _refCount);
        }

        // ====================================================================
        // LockHandle — sync IDisposable (for 'using' blocks)
        // ====================================================================

        private sealed class LockHandle : IDisposable
        {
            private readonly ConcurrencyControl _owner;
            private readonly LockKey _key;
            private readonly LockEntry _entry;
            private int _disposed;

            public LockHandle(ConcurrencyControl owner, LockKey key, LockEntry entry)
            {
                _owner = owner;
                _key = key;
                _entry = entry;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 1)
                    return;

                _owner.ReleaseLock(_key, _entry);
            }
        }

        // ====================================================================
        // AsyncLockHandle — async IAsyncDisposable (for 'await using' blocks)
        // ====================================================================

        private sealed class AsyncLockHandle : IAsyncDisposable
        {
            private readonly ConcurrencyControl _owner;
            private readonly LockKey _key;
            private readonly LockEntry _entry;
            private int _disposed;

            public AsyncLockHandle(ConcurrencyControl owner, LockKey key, LockEntry entry)
            {
                _owner = owner;
                _key = key;
                _entry = entry;
            }

            public ValueTask DisposeAsync()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 1)
                    return ValueTask.CompletedTask;

                _owner.ReleaseLock(_key, _entry);
                return ValueTask.CompletedTask;
            }
        }
    }

    // ========================================================================
    // Configuration
    // ========================================================================

    /// <summary>
    /// Configuration options for <see cref="ConcurrencyControl"/>.
    /// </summary>
    public sealed class ConcurrencyControlOptions
    {
        /// <summary>
        /// Maximum time to wait for a lock before throwing <see cref="TimeoutException"/>.
        /// Default: 30 seconds. For financial transactions, keep this tight to
        /// fail fast rather than queue indefinitely.
        /// </summary>
        public TimeSpan AcquireTimeout { get; init; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Lock acquisitions taking longer than this threshold will be logged as warnings.
        /// Default: 5 seconds. Tune based on your SLA requirements.
        /// </summary>
        public TimeSpan SlowLockThreshold { get; init; } = TimeSpan.FromSeconds(5);

        /// <summary>Sensible defaults for financial processing.</summary>
        public static ConcurrencyControlOptions Default { get; } = new();
    }

    // ========================================================================
    // Metrics (lock-free counters for observability)
    // ========================================================================

    /// <summary>
    /// Thread-safe, lock-free metrics collector for lock operations.
    /// Wire this into your APM (Application Insights, Prometheus, Datadog, etc.).
    /// </summary>
    internal sealed class ConcurrencyMetrics
    {
        private long _totalAcquired;
        private long _totalReleased;
        private long _totalWaitTimeTicks;
        private long _maxWaitTimeTicks;
        private long _timeoutCount;

        public void RecordAcquired(TimeSpan waitTime)
        {
            Interlocked.Increment(ref _totalAcquired);
            Interlocked.Add(ref _totalWaitTimeTicks, waitTime.Ticks);

            // Lock-free max update
            long newTicks = waitTime.Ticks;
            long currentMax;
            do
            {
                currentMax = Interlocked.Read(ref _maxWaitTimeTicks);
                if (newTicks <= currentMax) break;
            }
            while (Interlocked.CompareExchange(ref _maxWaitTimeTicks, newTicks, currentMax) != currentMax);
        }

        public void IncrementReleased() => Interlocked.Increment(ref _totalReleased);

        public void IncrementTimeout() => Interlocked.Increment(ref _timeoutCount);

        public ConcurrencyMetricsSnapshot Snapshot(int activeCount) => new()
        {
            ActiveLockCount = activeCount,
            TotalAcquired = Interlocked.Read(ref _totalAcquired),
            TotalReleased = Interlocked.Read(ref _totalReleased),
            TotalTimeouts = Interlocked.Read(ref _timeoutCount),
            AverageWaitTime = Interlocked.Read(ref _totalAcquired) > 0
                ? TimeSpan.FromTicks(Interlocked.Read(ref _totalWaitTimeTicks) / Interlocked.Read(ref _totalAcquired))
                : TimeSpan.Zero,
            MaxWaitTime = TimeSpan.FromTicks(Interlocked.Read(ref _maxWaitTimeTicks))
        };
    }

    /// <summary>
    /// Immutable snapshot of lock metrics at a point in time.
    /// Serialize this for dashboards, health checks, and alerting.
    /// </summary>
    public sealed record ConcurrencyMetricsSnapshot
    {
        public int ActiveLockCount { get; init; }
        public long TotalAcquired { get; init; }
        public long TotalReleased { get; init; }
        public long TotalTimeouts { get; init; }
        public TimeSpan AverageWaitTime { get; init; }
        public TimeSpan MaxWaitTime { get; init; }

        public override string ToString() =>
            $"Active={ActiveLockCount}, Acquired={TotalAcquired}, Released={TotalReleased}, " +
            $"Timeouts={TotalTimeouts}, AvgWait={AverageWaitTime.TotalMilliseconds:F1}ms, " +
            $"MaxWait={MaxWaitTime.TotalMilliseconds:F1}ms";
    }

    // ========================================================================
    // DI Registration Extension
    // ========================================================================

    /// <summary>
    /// Extension method for registering ConcurrencyControl in the DI container.
    /// <code>
    /// // In Program.cs or Startup.cs:
    /// builder.Services.AddConcurrencyControl(options =>
    /// {
    ///     options.AcquireTimeout = TimeSpan.FromSeconds(15);
    ///     options.SlowLockThreshold = TimeSpan.FromSeconds(3);
    /// });
    /// </code>
    /// </summary>
    public static class ConcurrencyControlServiceExtensions
    {
        public static IServiceCollection AddConcurrencyControl(
            this IServiceCollection services,
            Action<ConcurrencyControlOptions>? configure = null)
        {
            var options = new ConcurrencyControlOptions();
            configure?.Invoke(options);

            services.AddSingleton(options);
            services.AddSingleton<ConcurrencyControl>();

            return services;
        }
    }
}